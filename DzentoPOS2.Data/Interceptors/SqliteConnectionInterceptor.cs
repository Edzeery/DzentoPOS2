using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DzentoPOS2.Data.Interceptors;

/// <summary>
/// Applies the SQLite connection pragmas that must hold for the lifetime of
/// every connection EF opens. Doing it here (instead of in
/// <c>SaveChangesAsync</c>) means reads are protected too, and the settings are
/// applied even for connections opened by repositories or the backup service.
/// <para>
/// <c>journal_mode = WAL</c> is deliberately absent: switching the journal mode
/// fails inside a transaction, so it is done once at startup
/// (<see cref="SqliteStartup.EnsureWalModeAsync"/>).
/// </para>
/// </summary>
public sealed class SqliteConnectionInterceptor : DbConnectionInterceptor
{
    public static readonly SqliteConnectionInterceptor Instance = new();

    private const string Pragmas =
        "PRAGMA foreign_keys = ON; " +
        "PRAGMA busy_timeout = 5000; " +
        "PRAGMA synchronous = NORMAL;";

    public override void ConnectionOpened(
        DbConnection connection,
        ConnectionEndEventData eventData)
    {
        if (connection is SqliteConnection)
            Execute(connection);

        base.ConnectionOpened(connection, eventData);
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        if (connection is SqliteConnection)
            await ExecuteAsync(connection, cancellationToken);

        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    private static void Execute(DbConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = Pragmas;
        command.ExecuteNonQuery();
    }

    private static async Task ExecuteAsync(
        DbConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = Pragmas;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}