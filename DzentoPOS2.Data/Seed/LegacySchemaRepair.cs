using System.Data;
using System.Data.Common;

using DzentoPOS2.Core.Enums;

using Microsoft.Extensions.Logging;

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
///   table exists and the column is missing;</item>
///   <item>closes duplicate open cash sessions per cashbox before the unique
///   open-session index is created (see
///   <c>E3_UniqueOpenSessionIndex</c>). It keeps the newest session by
///   <c>OpenedAtUtc</c> and closes the rest without deleting any row.</item>
/// </list>
/// A brand new / empty database has no <c>__EFMigrationsHistory</c> table and is
/// left untouched (the migrations create everything from scratch). Everything
/// runs in a single transaction.
/// </summary>
public static class LegacySchemaRepair
{
    /// <summary>
    /// Name of the filtered unique index created by the
    /// <c>E3_UniqueOpenSessionIndex</c> migration. When it already exists the
    /// duplicate-session repair is skipped (idempotency).
    /// </summary>
    private const string OpenSessionIndexName = "UX_CashSessions_OpenPerCashbox";

    /// <summary>Note appended to every session the upgrade had to close.</summary>
    public const string DuplicateOpenSessionNote =
        "Auto-closed by upgrade (duplicate open session)";

    private static readonly string[] RemovedE3MigrationIds =
    {
        "20261023112454_E3_CashSessionLinking",
        "20261023131151_E3_CashSessionLinking"
    };

    public static async Task RepairAsync(
        DbConnection connection,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        var wasOpen = connection.State == ConnectionState.Open;
        if (!wasOpen)
            await connection.OpenAsync(cancellationToken);

        try
        {
            if (!await TableExistsAsync(connection, null, "__EFMigrationsHistory", cancellationToken))
                return;

            await using var transaction =
                await connection.BeginTransactionAsync(cancellationToken);

            foreach (var migrationId in RemovedE3MigrationIds)
            {
                await ExecuteAsync(
                    connection,
                    transaction,
                    "DELETE FROM \"__EFMigrationsHistory\" WHERE \"MigrationId\" = $id;",
                    migrationId,
                    cancellationToken);
            }

            await EnsureColumnAsync(
                connection, transaction, "SalesInvoices", "CashSessionId", "TEXT", cancellationToken);

            await EnsureColumnAsync(
                connection, transaction, "CashTransactions", "CashSessionId", "TEXT", cancellationToken);

            await EnsureColumnAsync(
                connection, transaction, "CashTransactions", "SourceType", "TEXT", cancellationToken);

            await EnsureColumnAsync(
                connection, transaction, "CashTransactions", "SourceId", "TEXT", cancellationToken);

            await RepairDuplicateOpenSessionsAsync(
                connection, transaction, logger, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            if (!wasOpen)
                await connection.CloseAsync();
        }
    }

    /// <summary>
    /// Closes every open cash session beyond the newest one per cashbox so the
    /// filtered unique index of <c>E3_UniqueOpenSessionIndex</c> can be created
    /// on databases that predate that migration (and allowed several open
    /// sessions per cashbox). No row is ever deleted and no invoice, payment or
    /// cash movement is touched.
    /// </summary>
    private static async Task RepairDuplicateOpenSessionsAsync(
        DbConnection connection,
        DbTransaction transaction,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        if (!await TableExistsAsync(connection, transaction, "CashSessions", cancellationToken))
            return;

        // Idempotent: once the unique index exists the data is already clean.
        if (await IndexExistsAsync(connection, transaction, OpenSessionIndexName, cancellationToken))
            return;

        var cashboxIds = await QueryStringsAsync(
            connection,
            transaction,
            "SELECT \"CashboxId\" FROM \"CashSessions\" "
            + "WHERE \"Status\" = 0 AND \"IsDeleted\" = 0 "
            + "GROUP BY \"CashboxId\" HAVING COUNT(*) > 1;",
            null,
            "cashboxId",
            cancellationToken);

        foreach (var cashboxId in cashboxIds)
        {
            var sessionIds = await QueryStringsAsync(
                connection,
                transaction,
                "SELECT \"Id\" FROM \"CashSessions\" "
                + "WHERE \"CashboxId\" = $cashboxId AND \"Status\" = 0 AND \"IsDeleted\" = 0 "
                + "ORDER BY \"OpenedAtUtc\" DESC, \"CreatedAt\" DESC, \"Id\" DESC;",
                cashboxId,
                "cashboxId",
                cancellationToken);

            // The first row is the newest session by OpenedAtUtc and is kept;
            // every other open session is closed.
            foreach (var sessionId in sessionIds.Skip(1))
            {
                await CloseDuplicateSessionAsync(
                    connection, transaction, sessionId, cancellationToken);

                logger?.LogWarning(
                    "Upgrade repair auto-closed duplicate open cash session {SessionId} on cashbox {CashboxId}.",
                    sessionId,
                    cashboxId);
            }
        }
    }

    private static async Task CloseDuplicateSessionAsync(
        DbConnection connection,
        DbTransaction transaction,
        string sessionId,
        CancellationToken cancellationToken)
    {
        var closedStatus = (int)CashSessionStatus.Closed;

        // ClosedAtUtc = max(OpenedAtUtc, latest linked movement, latest linked
        // sale). Balances and every other column are left untouched.
        var sql =
            "UPDATE \"CashSessions\" "
            + "SET \"Status\" = " + closedStatus + ", "
            + "\"ClosedAtUtc\" = MAX("
            + "\"OpenedAtUtc\", "
            + "COALESCE((SELECT MAX(t.\"TransactionDate\") FROM \"CashTransactions\" t "
            + "WHERE t.\"CashSessionId\" = \"CashSessions\".\"Id\"), \"OpenedAtUtc\"), "
            + "COALESCE((SELECT MAX(i.\"CreatedAt\") FROM \"SalesInvoices\" i "
            + "WHERE i.\"CashSessionId\" = \"CashSessions\".\"Id\"), \"OpenedAtUtc\")), "
            + "\"Notes\" = CASE "
            + "WHEN \"Notes\" IS NULL OR \"Notes\" = '' THEN $note "
            + "ELSE \"Notes\" || char(10) || $note END "
            + "WHERE \"Id\" = $id AND \"Status\" = 0 AND \"IsDeleted\" = 0;";

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        AddParameter(command, "$id", sessionId);
        AddParameter(command, "$note", DuplicateOpenSessionNote);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task EnsureColumnAsync(
        DbConnection connection,
        DbTransaction? transaction,
        string table,
        string column,
        string type,
        CancellationToken cancellationToken)
    {
        if (!await TableExistsAsync(connection, transaction, table, cancellationToken))
            return;

        if (await ColumnExistsAsync(connection, transaction, table, column, cancellationToken))
            return;

        await ExecuteAsync(
            connection,
            transaction,
            $"ALTER TABLE \"{table}\" ADD COLUMN \"{column}\" {type} NULL;",
            null,
            cancellationToken);
    }

    private static async Task<bool> TableExistsAsync(
        DbConnection connection,
        DbTransaction? transaction,
        string table,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $name;";
        AddParameter(command, "$name", table);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt64(result) > 0;
    }

    private static async Task<bool> IndexExistsAsync(
        DbConnection connection,
        DbTransaction? transaction,
        string index,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            "SELECT COUNT(*) FROM sqlite_master WHERE type = 'index' AND name = $name;";
        AddParameter(command, "$name", index);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt64(result) > 0;
    }

    private static async Task<bool> ColumnExistsAsync(
        DbConnection connection,
        DbTransaction? transaction,
        string table,
        string column,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"PRAGMA table_info('{table}');";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            if (string.Equals(reader.GetString(1), column, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static async Task<List<string>> QueryStringsAsync(
        DbConnection connection,
        DbTransaction transaction,
        string sql,
        string? parameterValue,
        string parameterName,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        if (parameterValue is not null)
            AddParameter(command, "$" + parameterName, parameterValue);

        var values = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            values.Add(reader.GetString(0));

        return values;
    }

    private static async Task ExecuteAsync(
        DbConnection connection,
        DbTransaction? transaction,
        string sql,
        string? parameterValue,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
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
