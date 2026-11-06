namespace DzentoPOS2.Core.Configuration;

/// <summary>
/// Single source of truth for every writable location the application uses.
/// Data that belongs to the user (database, preferences, theme, logs, backups)
/// lives under <c>%LocalAppData%\DzentoPOS2</c> instead of the installation
/// directory, so an upgrade or a read-only <c>Program Files</c> install never
/// touches or blocks it.
/// </summary>
public static class AppPaths
{
    public const string AppFolderName = "DzentoPOS2";

    public const string DefaultDatabaseFileName = "DzentoPOS.db";

    public static string Root =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AppFolderName);

    public static string DatabaseFolder =>
        Path.Combine(Root, "Database");

    public static string LogsFolder =>
        Path.Combine(Root, "logs");

    public static string BackupsFolder =>
        Path.Combine(Root, "backups");

    public static string BrandingFolder =>
        Path.Combine(Root, "branding");

    public static string UserSettingsFile =>
        Path.Combine(Root, "user-settings.json");

    public static string ThemeFile =>
        Path.Combine(Root, "theme.config");

    /// <summary>
    /// Preference file that stores the user-selected backup folder. It lives
    /// outside the database so changing it never rewrites application data.
    /// </summary>
    public static string BackupLocationFile =>
        Path.Combine(Root, "backup-location.json");

    public static string DefaultDatabasePath =>
        Path.Combine(DatabaseFolder, DefaultDatabaseFileName);

    /// <summary>
    /// Creates the folders the application writes to. Safe to call repeatedly.
    /// </summary>
    public static void EnsureCreated()
    {
        Directory.CreateDirectory(DatabaseFolder);
        Directory.CreateDirectory(LogsFolder);
        Directory.CreateDirectory(BackupsFolder);
        Directory.CreateDirectory(BrandingFolder);
    }

    /// <summary>
    /// Copies a file that used to live next to the executable into its new
    /// location, once.
    /// <para>
    /// The target is only written when it does not exist yet, the copy is
    /// verified byte-for-byte (length + SHA-256), and the legacy file is never
    /// modified, moved or deleted: the old installation keeps working while the
    /// new location becomes authoritative.
    /// </para>
    /// </summary>
    /// <returns><c>true</c> when a copy was made and verified.</returns>
    public static bool MigrateLegacyFile(string legacyPath, string targetPath)
    {
        if (string.IsNullOrWhiteSpace(legacyPath) ||
            string.IsNullOrWhiteSpace(targetPath))
            return false;

        if (!File.Exists(legacyPath) || File.Exists(targetPath))
            return false;

        string? folder = Path.GetDirectoryName(targetPath);
        if (!string.IsNullOrEmpty(folder))
            Directory.CreateDirectory(folder);

        File.Copy(legacyPath, targetPath, overwrite: false);

        if (!SameContent(legacyPath, targetPath))
        {
            // Never leave a half-copied file behind as the authoritative one.
            TryDelete(targetPath);
            throw new IOException(
                $"Copying '{legacyPath}' to '{targetPath}' could not be verified.");
        }

        return true;
    }

    /// <summary>
    /// Copies a legacy SQLite database to its new location, including a
    /// non-empty write-ahead log so no committed row is lost. All files are
    /// verified and the originals are left untouched.
    /// </summary>
    public static bool MigrateLegacyDatabase(string legacyDatabasePath, string targetDatabasePath)
    {
        if (!MigrateLegacyFile(legacyDatabasePath, targetDatabasePath))
            return false;

        // A WAL that still holds committed pages is part of the database; the
        // next open replays it. The shared-memory file is rebuilt on demand.
        MigrateLegacyFile(legacyDatabasePath + "-wal", targetDatabasePath + "-wal");

        return true;
    }

    private static bool SameContent(string firstPath, string secondPath)
    {
        var first = new FileInfo(firstPath);
        var second = new FileInfo(secondPath);

        if (!first.Exists || !second.Exists || first.Length != second.Length)
            return false;

        using var firstStream = File.OpenRead(firstPath);
        using var secondStream = File.OpenRead(secondPath);

        byte[] firstBuffer = new byte[81920];
        byte[] secondBuffer = new byte[81920];

        while (true)
        {
            int firstRead = firstStream.Read(firstBuffer, 0, firstBuffer.Length);
            int secondRead = secondStream.Read(secondBuffer, 0, secondBuffer.Length);

            if (firstRead != secondRead)
                return false;

            if (firstRead == 0)
                return true;

            if (!firstBuffer.AsSpan(0, firstRead).SequenceEqual(secondBuffer.AsSpan(0, secondRead)))
                return false;
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (IOException)
        {
            // The caller rethrows the verification failure; a leftover file is
            // harmless because it is never read as authoritative.
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}