using System.Data;
using System.Data.Common;

namespace DzentoPOS2.Data.Seed;

/// <summary>
/// Repairs the physical SQLite schema before EF migrations run.
///
/// The <c>E3_CashSessionLinking</c> migration was regenerated several times
/// while it was being developed. Its final form only adds the foreign keys; the
/// columns it depends on (<c>SalesInvoices.CashSessionId</c>,
/// <c>CashTransactions.CashSessionId/SourceType/SourceId</c>) exist in the model
/// but no migration ever adds them explicitly. On SQLite they only appear as a
/// side effect of the table rebuild EF performs for <c>AddForeignKey</c>, which
/// makes fresh installs fragile and breaks any database whose history predates
/// that rebuild.
///
/// This step makes the schema consistent with the model for every starting
/// state, is fully idempotent and never loses data:
/// <list type="bullet">
///   <item>removes the orphan history rows of the removed E3 migrations;</item>
///   <item>adds the columns above with <c>ALTER TABLE ADD COLUMN</c> when the
///   table exists and the column is missing.</item>
/// </list>
/// A brand new / empty database has no <c>__EFMigrationsHistory</c> table and is
/// left untouched (the migrations create everything from scratch).
/// </summary>
public static class LegacySchemaRepair
{
    private static readonly string[] RemovedE3MigrationIds =
    {
        "20261023112454_E3_CashSessionLinking",
        "20261023131151_E3_CashSessionLinking"
    };

    public static async Task RepairAsync(
        DbConnection connection,
        CancellationToken cancellationToken = default)
    {
        var wasOpen = connection.State == ConnectionState.Open;
        if (!wasOpen)
            await connection.OpenAsync(cancellationToken);

        try
        {
            if (!await TableExistsAsync(connection, "__EFMigrationsHistory", cancellationToken))
                return;

            foreach (var migrationId in RemovedE3MigrationIds)
            {
                await ExecuteAsync(
                    connection,
                    "DELETE FROM \"__EFMigrationsHistory\" WHERE \"MigrationId\" = $id;",
                    migrationId,
                    cancellationToken);
            }

            await EnsureColumnAsync(
                connection, "SalesInvoices", "CashSessionId", "TEXT", cancellationToken);

            await EnsureColumnAsync(
                connection, "CashTransactions", "CashSessionId", "TEXT", cancellationToken);

            await EnsureColumnAsync(
                connection, "CashTransactions", "SourceType", "TEXT", cancellationToken);

            await EnsureColumnAsync(
                connection, "CashTransactions", "SourceId", "TEXT", cancellationToken);
        }
        finally
        {
            if (!wasOpen)
                await connection.CloseAsync();
        }
    }

    private static async Task EnsureColumnAsync(
        DbConnection connection,
        string table,
        string column,
        string type,
        CancellationToken cancellationToken)
    {
        if (!await TableExistsAsync(connection, table, cancellationToken))
            return;

        if (await ColumnExistsAsync(connection, table, column, cancellationToken))
            return;

        await ExecuteAsync(
            connection,
            $"ALTER TABLE \"{table}\" ADD COLUMN \"{column}\" {type} NULL;",
            null,
            cancellationToken);
    }

    private static async Task<bool> TableExistsAsync(
        DbConnection connection,
        string table,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $name;";
        AddParameter(command, "$name", table);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt64(result) > 0;
    }

    private static async Task<bool> ColumnExistsAsync(
        DbConnection connection,
        string table,
        string column,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info('{table}');";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            if (string.Equals(reader.GetString(1), column, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static async Task ExecuteAsync(
        DbConnection connection,
        string sql,
        string? parameterValue,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        if (parameterValue is not null)
            AddParameter(command, "$id", parameterValue);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
