using System.Data;
using DzentoPOS2.Data.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DzentoPOS2.App.Services.Systems;

public sealed class DocumentNumberService : IDocumentNumberService
{
    private const string InvoicePrefix = "INV";

    private readonly AppDbContext _context;
    private readonly IBusinessClock _clock;

    public DocumentNumberService(
        AppDbContext context,
        IBusinessClock clock)
    {
        _context = context;
        _clock = clock;
    }

    public Task<string> NextInvoiceNumberAsync(
        CancellationToken cancellationToken = default)
    {
        return NextDocumentNumberAsync(InvoicePrefix, cancellationToken);
    }

    public async Task<string> NextDocumentNumberAsync(
        string prefix,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            throw new ArgumentException("Document prefix is required", nameof(prefix));

        var dateKey = _clock.Now.ToString("yyyyMMdd");

        for (var attempt = 0; ; attempt++)
        {
            try
            {
                var lastValue = await IncrementAsync(
                    prefix.Trim(),
                    dateKey,
                    cancellationToken);

                return $"{prefix.Trim()}-{dateKey}-{lastValue:D4}";
            }
            catch (SqliteException ex) when (
                attempt < 4 &&
                (ex.SqliteErrorCode == 5 || ex.SqliteErrorCode == 6))
            {
                await Task.Delay(25 * (attempt + 1), cancellationToken);
            }
        }
    }

    private async Task<long> IncrementAsync(
        string prefix,
        string dateKey,
        CancellationToken cancellationToken)
    {
        var connection = _context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;

        if (shouldClose)
            await connection.OpenAsync(cancellationToken);

        try
        {
            await using var command = connection.CreateCommand();
            command.Transaction = _context.Database.CurrentTransaction?
                .GetDbTransaction();

            command.CommandText = """
                INSERT INTO "DocumentSequences"
                    ("Id", "Prefix", "DateKey", "LastValue", "CreatedAt", "IsDeleted")
                VALUES
                    ($id, $prefix, $dateKey, 1, $createdAt, 0)
                ON CONFLICT("Prefix", "DateKey") DO UPDATE SET
                    "LastValue" = "DocumentSequences"."LastValue" + 1,
                    "UpdatedAt" = $updatedAt
                RETURNING "LastValue";
                """;

            AddParameter(command, "$id", Guid.NewGuid().ToString());
            AddParameter(command, "$prefix", prefix);
            AddParameter(command, "$dateKey", dateKey);
            AddParameter(command, "$createdAt", _clock.UtcNow.ToString("O"));
            AddParameter(command, "$updatedAt", _clock.UtcNow.ToString("O"));

            var result = await command.ExecuteScalarAsync(cancellationToken);

            return Convert.ToInt64(result);
        }
        finally
        {
            if (shouldClose)
                await connection.CloseAsync();
        }
    }

    private static void AddParameter(
        System.Data.Common.DbCommand command,
        string name,
        object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
