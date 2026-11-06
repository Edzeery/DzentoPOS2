using System.Security;
using System.Text.Json;
using DzentoPOS2.Localization.Resources;

namespace DzentoPOS2.Core.Configuration;

/// <summary>
/// Persists the folder the backup service writes to.
/// <para>
/// The choice is stored in a small JSON file under
/// <c>%LocalAppData%\DzentoPOS2</c> instead of the database: the location of
/// the backups must be readable before the database is opened (and while it is
/// being restored), so it cannot live inside the database itself.
/// </para>
/// <para>
/// The type is a normal instance so tests can point it at a temporary file.
/// The application uses the shared <see cref="BackupLocation"/> helper.
/// </para>
/// </summary>
public sealed class BackupLocationStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _filePath;
    private readonly string _defaultDirectory;

    /// <param name="filePath">Preference file that holds the location.</param>
    /// <param name="defaultDirectory">Fallback used when nothing is stored.</param>
    public BackupLocationStore(string filePath, string defaultDirectory)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("A preference file path is required.", nameof(filePath));

        if (string.IsNullOrWhiteSpace(defaultDirectory))
            throw new ArgumentException("A default directory is required.", nameof(defaultDirectory));

        _filePath = Path.GetFullPath(filePath);
        _defaultDirectory = Path.GetFullPath(defaultDirectory);
    }

    /// <summary>Folder the application uses when the user never chose one.</summary>
    public string DefaultDirectory => _defaultDirectory;

    /// <summary>File the selection is persisted to.</summary>
    public string FilePath => _filePath;

    /// <summary>
    /// Reads the stored folder. A missing, unreadable or malformed preference
    /// file never throws: the default folder is returned instead so a bad
    /// preference can not stop the application from starting.
    /// </summary>
    public string GetDirectory()
    {
        try
        {
            if (!File.Exists(_filePath))
                return _defaultDirectory;

            string json = File.ReadAllText(_filePath);
            var payload = JsonSerializer.Deserialize<BackupLocationPayload>(json);
            string? stored = payload?.BackupDirectory?.Trim();

            if (string.IsNullOrWhiteSpace(stored) || !Path.IsPathFullyQualified(stored))
                return _defaultDirectory;

            return Path.GetFullPath(stored);
        }
        catch (Exception ex) when (
            ex is IOException
                or UnauthorizedAccessException
                or JsonException
                or NotSupportedException
                or ArgumentException
                or SecurityException)
        {
            return _defaultDirectory;
        }
    }

    /// <summary>
    /// Validates the folder, creates it when needed and persists the choice.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The folder is empty, points at a file, or can not be created/written.
    /// </exception>
    public void SetDirectory(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
            throw new InvalidOperationException(AppStrings.BackupLocationInvalid);

        string normalized;

        try
        {
            normalized = Path.GetFullPath(directory.Trim());

            if (File.Exists(normalized))
                throw new InvalidOperationException(AppStrings.BackupLocationInvalid);

            Directory.CreateDirectory(normalized);
        }
        catch (Exception ex) when (
            ex is IOException
                or UnauthorizedAccessException
                or NotSupportedException
                or ArgumentException)
        {
            throw new InvalidOperationException(AppStrings.BackupLocationInvalid, ex);
        }

        string? folder = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(folder))
            Directory.CreateDirectory(folder);

        string json = JsonSerializer.Serialize(
            new BackupLocationPayload { BackupDirectory = normalized },
            SerializerOptions);

        // Write through a temporary file so an interrupted write can never
        // leave a truncated preference behind.
        string temporary = _filePath + ".tmp";
        File.WriteAllText(temporary, json);
        File.Move(temporary, _filePath, overwrite: true);
    }

    /// <summary>Clears the stored selection (the next read uses the default).</summary>
    public void Reset()
    {
        try
        {
            if (File.Exists(_filePath))
                File.Delete(_filePath);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private sealed class BackupLocationPayload
    {
        public string? BackupDirectory { get; set; }
    }
}

/// <summary>
/// Application-wide access to the backup folder preference.
/// </summary>
public static class BackupLocation
{
    private static readonly BackupLocationStore Store =
        new(AppPaths.BackupLocationFile, AppPaths.BackupsFolder);

    /// <summary>Currently configured backup folder.</summary>
    public static string GetDirectory() => Store.GetDirectory();

    /// <summary>Validates, creates and persists a new backup folder.</summary>
    public static void SetDirectory(string directory) => Store.SetDirectory(directory);

    /// <summary>Restores the default backup folder.</summary>
    public static void Reset() => Store.Reset();
}
