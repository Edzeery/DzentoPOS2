using System.Data;
using System.Data.Common;

namespace DzentoPOS2.Data;

/// <summary>
/// One-time SQLite settings that must not run inside a transaction. The
/// per-connection pragmas live in
/// <see cref="Interceptors.SqliteConnectionInterceptor"/>.
/// </summary>
public static class SqliteStartup
{
    /// <summary>
    /// Switches the database file to write-ahead logging. WAL survives in the
    /// file, so calling this on every startup is idempotent and cheap.
    /// </summary>
    /// <returns>The journal mode the database ended up in (<c>wal</c> for a file database).</returns>
    public static async Task<string> EnsureWalModeAsync(
        DbConnection connection,
        CancellationToken cancellationToken = default)
    {
        bool wasClosed = connection.State != ConnectionState.Open;

        if (wasClosed)
            await connection.OpenAsync(cancellationToken);

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA journal_mode = WAL;";

            var result = await command.ExecuteScalarAsync(cancellationToken);

            return result?.ToString() ?? string.Empty;
        }
        finally
        {
            if (wasClosed)
                await connection.CloseAsync();
        }
    }
}