using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Seed;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace DzentoPOS2.IntegrationTests;

/// <summary>
/// Upgrade-scenario coverage for <see cref="LegacySchemaRepair"/>: it must make
/// the physical schema consistent with the model for every starting state,
/// never lose data and be safe to run repeatedly.
/// </summary>
public sealed class LegacySchemaRepairTests : IDisposable
{
    private readonly string _directory = Path.Combine(
        Path.GetTempPath(),
        "DzentoPOS2.Repair",
        Guid.NewGuid().ToString("N"));

    private const string RemovedE3Id = "20261023131151_E3_CashSessionLinking";

    public LegacySchemaRepairTests()
    {
        Directory.CreateDirectory(_directory);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();

        try
        {
            Directory.Delete(_directory, recursive: true);
        }
        catch
        {
            // Best effort cleanup.
        }
    }

    [Fact]
    public async Task Fresh_database_migrates_without_foreign_key_violations()
    {
        var path = NewPath("fresh");

        await using var context = CreateContext(path);
        await LegacySchemaRepair.RepairAsync(context.Database.GetDbConnection());
        await context.Database.MigrateAsync();

        Assert.True(await ColumnExistsAsync(path, "SalesInvoices", "CashSessionId"));
        Assert.True(await ColumnExistsAsync(path, "CashTransactions", "CashSessionId"));
        Assert.True(await ColumnExistsAsync(path, "CashTransactions", "SourceType"));
        Assert.True(await ColumnExistsAsync(path, "CashTransactions", "SourceId"));
        Assert.Empty(await ForeignKeyViolationsAsync(path));

        var applied = await context.Database.GetAppliedMigrationsAsync();
        Assert.DoesNotContain(RemovedE3Id, applied);
        Assert.DoesNotContain("20261023112454_E3_CashSessionLinking", applied);
    }

    [Fact]
    public async Task Repair_and_migrate_are_idempotent_on_the_second_run()
    {
        var path = NewPath("idempotent");

        List<string> before;

        await using (var context = CreateContext(path))
        {
            await LegacySchemaRepair.RepairAsync(context.Database.GetDbConnection());
            await context.Database.MigrateAsync();
            before = (await context.Database.GetAppliedMigrationsAsync()).ToList();
        }

        await using (var context = CreateContext(path))
        {
            await LegacySchemaRepair.RepairAsync(context.Database.GetDbConnection());
            await context.Database.MigrateAsync();

            var after = (await context.Database.GetAppliedMigrationsAsync()).ToList();

            Assert.Equal(before, after);
            Assert.Empty(await ForeignKeyViolationsAsync(path));
        }
    }

    [Fact]
    public async Task Legacy_database_missing_e3_columns_is_repaired_without_data_loss()
    {
        var path = NewPath("legacy");

        // Hand-build the pre-E3 shape: the three tables exist but lack the
        // CashSessionId / SourceType / SourceId columns, and the migration
        // history still references a removed E3 migration id.
        await using (var connection = new SqliteConnection($"Data Source={path}"))
        {
            await connection.OpenAsync();
            await ExecuteAsync(connection, """
                CREATE TABLE "__EFMigrationsHistory" (
                    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
                    "ProductVersion" TEXT NOT NULL);
                CREATE TABLE "SalesInvoices" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_SalesInvoices" PRIMARY KEY,
                    "InvoiceNumber" TEXT NULL);
                CREATE TABLE "CashTransactions" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_CashTransactions" PRIMARY KEY,
                    "Amount" TEXT NULL);
                INSERT INTO "SalesInvoices" ("Id", "InvoiceNumber") VALUES ('inv-1', 'INV-1');
                INSERT INTO "CashTransactions" ("Id", "Amount") VALUES ('ct-1', '10');
                INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
                    VALUES ('20261023131151_E3_CashSessionLinking', '8.0.27');
                """);
        }

        await using (var context = CreateContext(path))
        {
            await LegacySchemaRepair.RepairAsync(context.Database.GetDbConnection());
        }

        Assert.True(await ColumnExistsAsync(path, "SalesInvoices", "CashSessionId"));
        Assert.True(await ColumnExistsAsync(path, "CashTransactions", "CashSessionId"));
        Assert.True(await ColumnExistsAsync(path, "CashTransactions", "SourceType"));
        Assert.True(await ColumnExistsAsync(path, "CashTransactions", "SourceId"));

        // Existing rows must survive untouched.
        Assert.Equal(1, await ScalarLongAsync(path, "SELECT COUNT(*) FROM \"SalesInvoices\";"));
        Assert.Equal(1, await ScalarLongAsync(path, "SELECT COUNT(*) FROM \"CashTransactions\";"));

        // The orphan E3 history row must be gone.
        Assert.Equal(0, await ScalarLongAsync(
            path,
            $"SELECT COUNT(*) FROM \"__EFMigrationsHistory\" WHERE \"MigrationId\" = '{RemovedE3Id}';"));

        // Running it again must be a no-op (idempotent).
        await using (var context = CreateContext(path))
        {
            await LegacySchemaRepair.RepairAsync(context.Database.GetDbConnection());
        }

        Assert.Equal(1, await ScalarLongAsync(path, "SELECT COUNT(*) FROM \"SalesInvoices\";"));
    }

    private string NewPath(string name)
        => Path.Combine(_directory, $"{name}.db");

    [Fact]
    public async Task F2_upgrade_from_the_pre_f2_schema_preserves_legacy_rows()
    {
        var path = NewPath("pref2");

        var invoiceId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        var cashId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var cashboxId = Guid.NewGuid();

        // 1) A database at the previous head (E3) with legacy invoice / payment
        //    / cash rows that predate F2.
        await using (var context = CreateContext(path))
        {
            var migrator = context.Database.GetService<IMigrator>();
            await migrator.MigrateAsync("20261023140849_E3_CashSessionLinking");

            await context.Database.ExecuteSqlRawAsync(
                "INSERT INTO \"Branches\" (\"Id\",\"Name\",\"IsMainBranch\",\"IsActive\",\"IsDeleted\",\"CreatedAt\") VALUES ({0},{1},{2},{3},{4},{5});",
                branchId, "Legacy", 1, 1, 0, DateTime.UtcNow);

            await context.Database.ExecuteSqlRawAsync(
                "INSERT INTO \"Cashboxes\" (\"Id\",\"Name\",\"BranchId\",\"CurrentBalance\",\"IsActive\",\"IsDeleted\",\"CreatedAt\") VALUES ({0},{1},{2},{3},{4},{5},{6});",
                cashboxId, "Legacy cashbox", branchId, "0", 1, 0, DateTime.UtcNow);

            await context.Database.ExecuteSqlRawAsync(
                "INSERT INTO \"SalesInvoices\" (\"Id\",\"InvoiceNumber\",\"CustomerId\",\"BranchId\",\"TotalAmount\",\"TotalCurrency\",\"Discount\",\"TaxRate\",\"PaidAmount\",\"PaidCurrency\",\"RemainingAmount\",\"RemainingCurrency\",\"DiscountAmount\",\"DiscountCurrency\",\"SubtotalAmount\",\"SubtotalCurrency\",\"TaxAmount\",\"TaxCurrency\",\"IsVoided\",\"IsDeleted\",\"CreatedAt\") VALUES ({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20});",
                invoiceId, "LEGACY-F2-1", Guid.Empty, branchId,
                "10", "DZD", "0", "0", "10", "DZD", "0", "DZD",
                "0", "DZD", "10", "DZD", "0", "DZD",
                0, 0, DateTime.UtcNow);

            await context.Database.ExecuteSqlRawAsync(
                "INSERT INTO \"SalesPayments\" (\"Id\",\"SalesInvoiceId\",\"Amount\",\"PaymentDate\",\"PaymentMethod\",\"ReferenceNumber\",\"Notes\",\"CreatedAt\") VALUES ({0},{1},{2},{3},{4},{5},{6},{7});",
                paymentId, invoiceId, "10", DateTime.UtcNow, "Cash", "LEGACY-F2-1", string.Empty, DateTime.UtcNow);

            await context.Database.ExecuteSqlRawAsync(
                "INSERT INTO \"CashTransactions\" (\"Id\",\"CashboxId\",\"Amount\",\"TransactionType\",\"ReferenceNumber\",\"TransactionDate\",\"IsDeleted\",\"CreatedAt\") VALUES ({0},{1},{2},{3},{4},{5},{6},{7});",
                cashId, cashboxId, "10", 13, "LEGACY-F2-1", DateTime.UtcNow, 0, DateTime.UtcNow);
        }

        // 2) Upgrade to the current head (applies the F2 migration).
        await using (var context = CreateContext(path))
        {
            await context.Database.MigrateAsync();
        }

        // 3) Legacy rows survive, the new columns default to 0, no dangling FKs.
        Assert.Equal(1, await ScalarLongAsync(path, "SELECT COUNT(*) FROM \"SalesInvoices\";"));
        Assert.Equal(1, await ScalarLongAsync(path, "SELECT COUNT(*) FROM \"SalesPayments\";"));
        Assert.Equal(1, await ScalarLongAsync(path, "SELECT COUNT(*) FROM \"CashTransactions\";"));
        Assert.Equal(0, await ScalarLongAsync(path, "SELECT \"DiscountMode\" FROM \"SalesInvoices\" LIMIT 1;"));
        Assert.Equal(0, await ScalarLongAsync(path, "SELECT \"TenderedAmount\" FROM \"SalesPayments\" LIMIT 1;"));
        Assert.Empty(await ForeignKeyViolationsAsync(path));

        // 4) A second migrate is a no-op.
        await using (var context = CreateContext(path))
        {
            var before = (await context.Database.GetAppliedMigrationsAsync()).ToList();
            await context.Database.MigrateAsync();
            var after = (await context.Database.GetAppliedMigrationsAsync()).ToList();

            Assert.Equal(before, after);
        }
    }

    private static AppDbContext CreateContext(string path)
        => new(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={path}")
                .Options);

    private static async Task ExecuteAsync(SqliteConnection connection, string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<long> ScalarLongAsync(string path, string sql)
    {
        await using var connection = new SqliteConnection($"Data Source={path}");
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        var value = await command.ExecuteScalarAsync();
        if (value is null || value is DBNull)
            return 0;

        return Convert.ToInt64(Convert.ToDecimal(value, System.Globalization.CultureInfo.InvariantCulture));
    }

    private static async Task<bool> ColumnExistsAsync(string path, string table, string column)
    {
        await using var connection = new SqliteConnection($"Data Source={path}");
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info('{table}');";

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            if (string.Equals(reader.GetString(1), column, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static async Task<List<string>> ForeignKeyViolationsAsync(string path)
    {
        await using var connection = new SqliteConnection($"Data Source={path}");
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_key_check;";

        var violations = new List<string>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            violations.Add(reader.GetString(0));

        return violations;
    }
}
