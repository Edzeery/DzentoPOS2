using DzentoPOS2.Core.Configuration;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Localization.Resources;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.App.Services.Systems;

/// <summary>One file in the backup folder.</summary>
public sealed record BackupFile(string Path, DateTime CreatedAtUtc, long SizeBytes);

public interface IBackupService
{
    /// <summary>Folder the automatic and manual backups are written to.</summary>
    string BackupDirectory { get; }

    /// <summary>Creates a consistent snapshot with the SQLite online-backup API.</summary>
    Task<BackupFile> CreateBackupAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a backup only when none was created today (calendar day, local
    /// time). Returns <c>null</c> when today's backup already exists.
    /// </summary>
    Task<BackupFile?> CreateAutomaticBackupIfDueAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Backups newest first.</summary>
    IReadOnlyList<BackupFile> ListBackups();

    /// <summary>Deletes everything beyond the newest <paramref name="keep"/> backups.</summary>
    int ApplyRetention(int keep = 7);

    /// <summary>
    /// Verifies that the file is a readable SQLite database produced by this
    /// application. Throws <see cref="InvalidOperationException"/> otherwise.
    /// </summary>
    Task ValidateBackupAsync(string backupPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Overwrites the live database with the backup, after taking a safety
    /// snapshot of the current data. The caller must restart the application
    /// afterwards.
    /// </summary>
    /// <returns>Path of the safety backup of the data that was replaced.</returns>
    Task<string> RestoreAsync(string backupPath, CancellationToken cancellationToken = default);
}

/// <summary>
/// Backup/restore for the SQLite database. Uses the engine's online backup API
/// (never a raw file copy) so a snapshot taken while the application is running
/// is consistent, and restoring while the application is running is safe.
/// </summary>
public sealed class BackupService : IBackupService
{
    private const string FilePrefix = "DzentoPOS-";
    private const string FileExtension = ".db";

    private static readonly string[] RequiredTables =
    {
        "Users",
        "Roles",
        "SalesInvoices",
        "AppSettings",
        "__EFMigrationsHistory"
    };

    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    /// <param name="contextFactory">Factory used to reach the live database.</param>
    public BackupService(IDbContextFactory<AppDbContext> contextFactory)
        : this(contextFactory, backupDirectory: null)
    {
    }

    /// <param name="contextFactory">Factory used to reach the live database.</param>
    /// <param name="backupDirectory">
    /// Optional override of the backup folder (tests use a temp directory).
    /// </param>
    public BackupService(
        IDbContextFactory<AppDbContext> contextFactory,
        string? backupDirectory)
    {
        _contextFactory = contextFactory;

        BackupDirectory = string.IsNullOrWhiteSpace(backupDirectory)
            ? AppPaths.BackupsFolder
            : backupDirectory;
    }

    public string BackupDirectory { get; }

    public async Task<BackupFile> CreateBackupAsync(
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(BackupDirectory);

        string destinationPath = BuildUniquePath();

        await using var context = await _contextFactory
            .CreateDbContextAsync(cancellationToken);

        var source = (SqliteConnection)context.Database.GetDbConnection();

        bool wasClosed = source.State != System.Data.ConnectionState.Open;
        if (wasClosed)
            await source.OpenAsync(cancellationToken);

        try
        {
            await using var destination = new SqliteConnection(
                new SqliteConnectionStringBuilder
                {
                    DataSource = destinationPath,
                    Mode = SqliteOpenMode.ReadWriteCreate,
                    Pooling = false
                }.ToString());

            await destination.OpenAsync(cancellationToken);

            source.BackupDatabase(destination);
        }
        finally
        {
            if (wasClosed)
                await source.CloseAsync();
        }

        return ToBackupFile(destinationPath);
    }

    public async Task<BackupFile?> CreateAutomaticBackupIfDueAsync(
        CancellationToken cancellationToken = default)
    {
        var newest = ListBackups().FirstOrDefault();

        if (newest is not null &&
            newest.CreatedAtUtc.ToLocalTime().Date == DateTime.Now.Date)
        {
            return null;
        }

        var created = await CreateBackupAsync(cancellationToken);
        ApplyRetention();

        return created;
    }

    public IReadOnlyList<BackupFile> ListBackups()
    {
        if (!Directory.Exists(BackupDirectory))
            return Array.Empty<BackupFile>();

        return Directory
            .EnumerateFiles(BackupDirectory, FilePrefix + "*" + FileExtension)
            .Select(ToBackupFile)
            .OrderByDescending(f => f.CreatedAtUtc)
            .ThenByDescending(f => f.Path, StringComparer.Ordinal)
            .ToList();
    }

    public int ApplyRetention(int keep = 7)
    {
        if (keep < 1)
            throw new ArgumentOutOfRangeException(nameof(keep));

        var stale = ListBackups().Skip(keep).ToList();

        int deleted = 0;

        foreach (var backup in stale)
        {
            try
            {
                File.Delete(backup.Path);
                deleted++;
            }
            catch (IOException)
            {
                // A locked backup is retried on the next run.
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        return deleted;
    }

    public async Task ValidateBackupAsync(
        string backupPath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(backupPath) || !File.Exists(backupPath))
            throw new InvalidOperationException(AppStrings.BackupFileInvalid);

        await using var connection = new SqliteConnection(
            new SqliteConnectionStringBuilder
            {
                DataSource = backupPath,
                Mode = SqliteOpenMode.ReadOnly,
                Pooling = false
            }.ToString());

        try
        {
            await connection.OpenAsync(cancellationToken);

            await using (var integrity = connection.CreateCommand())
            {
                integrity.CommandText = "PRAGMA integrity_check;";

                var result = await integrity.ExecuteScalarAsync(cancellationToken);

                if (!string.Equals(result?.ToString(), "ok", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException(AppStrings.BackupFileInvalid);
            }

            foreach (string table in RequiredTables)
            {
                await using var probe = connection.CreateCommand();
                probe.CommandText =
                    "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $name;";
                probe.Parameters.AddWithValue("$name", table);

                var count = Convert.ToInt64(
                    await probe.ExecuteScalarAsync(cancellationToken) ?? 0L);

                if (count == 0)
                    throw new InvalidOperationException(AppStrings.BackupFileInvalid);
            }
        }
        catch (SqliteException)
        {
            throw new InvalidOperationException(AppStrings.BackupFileInvalid);
        }
    }

    public async Task<string> RestoreAsync(
        string backupPath,
        CancellationToken cancellationToken = default)
    {
        await ValidateBackupAsync(backupPath, cancellationToken);

        // The live data is replaceable, but only after it is backed up.
        var safety = await CreateBackupAsync(cancellationToken);

        await using var context = await _contextFactory
            .CreateDbContextAsync(cancellationToken);

        var destination = (SqliteConnection)context.Database.GetDbConnection();

        bool wasClosed = destination.State != System.Data.ConnectionState.Open;
        if (wasClosed)
            await destination.OpenAsync(cancellationToken);

        try
        {
            await using var source = new SqliteConnection(
                new SqliteConnectionStringBuilder
                {
                    DataSource = backupPath,
                    Mode = SqliteOpenMode.ReadOnly,
                    Pooling = false
                }.ToString());

            await source.OpenAsync(cancellationToken);

            // Page-level copy into the live database: safe while other
            // connections are open, unlike replacing the file on disk.
            source.BackupDatabase(destination);
        }
        finally
        {
            if (wasClosed)
                await destination.CloseAsync();
        }

        return safety.Path;
    }

    private string BuildUniquePath()
    {
        string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");

        string path = Path.Combine(
            BackupDirectory,
            $"{FilePrefix}{stamp}{FileExtension}");

        // Two backups in the same second must not overwrite each other.
        int suffix = 1;

        while (File.Exists(path))
        {
            path = Path.Combine(
                BackupDirectory,
                $"{FilePrefix}{stamp}-{suffix++}{FileExtension}");
        }

        return path;
    }

    private static BackupFile ToBackupFile(string path)
    {
        var info = new FileInfo(path);

        return new BackupFile(
            info.FullName,
            info.LastWriteTimeUtc,
            info.Exists ? info.Length : 0);
    }
}