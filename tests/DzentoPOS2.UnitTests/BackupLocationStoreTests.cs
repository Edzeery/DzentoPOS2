using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.Core.Configuration;
using Xunit;

namespace DzentoPOS2.UnitTests;

/// <summary>
/// Item 5: the backup folder is a preference stored outside the database and
/// read dynamically by the backup service.
/// </summary>
public sealed class BackupLocationStoreTests : IDisposable
{
    private readonly string _root =
        Path.Combine(Path.GetTempPath(), "DzentoPOS2.BackupLocation." + Guid.NewGuid().ToString("N"));

    private BackupLocationStore CreateStore(string? fileName = null, string? defaultDirectory = null)
    {
        Directory.CreateDirectory(_root);
        return new BackupLocationStore(
            Path.Combine(_root, fileName ?? "backup-location.json"),
            defaultDirectory ?? Path.Combine(_root, "default-backups"));
    }

    [Fact]
    public void GetDirectory_returns_default_when_no_selection_was_saved()
    {
        var store = CreateStore();

        Assert.Equal(
            Path.GetFullPath(Path.Combine(_root, "default-backups")),
            store.GetDirectory());
    }

    [Fact]
    public void SetDirectory_persists_the_choice_and_creates_the_folder()
    {
        var store = CreateStore();
        string chosen = Path.Combine(_root, "chosen", "backups");

        store.SetDirectory(chosen);

        Assert.True(Directory.Exists(chosen));
        Assert.True(File.Exists(store.FilePath));
        Assert.Equal(Path.GetFullPath(chosen), store.GetDirectory());
    }

    [Fact]
    public void GetDirectory_falls_back_to_default_when_the_file_is_corrupt()
    {
        var store = CreateStore();
        File.WriteAllText(store.FilePath, "{ this is not json");

        Assert.Equal(Path.GetFullPath(store.DefaultDirectory), store.GetDirectory());
    }

    [Fact]
    public void GetDirectory_falls_back_to_default_when_the_stored_path_is_relative()
    {
        var store = CreateStore();
        File.WriteAllText(
            store.FilePath,
            "{\"BackupDirectory\":\"relative\\\\folder\"}");

        Assert.Equal(Path.GetFullPath(store.DefaultDirectory), store.GetDirectory());
    }

    [Fact]
    public void GetDirectory_falls_back_to_default_when_the_stored_value_is_empty()
    {
        var store = CreateStore();
        File.WriteAllText(store.FilePath, "{\"BackupDirectory\":\"   \"}");

        Assert.Equal(Path.GetFullPath(store.DefaultDirectory), store.GetDirectory());
    }

    [Fact]
    public void SetDirectory_rejects_an_empty_path()
    {
        var store = CreateStore();

        Assert.Throws<InvalidOperationException>(() => store.SetDirectory("   "));
    }

    [Fact]
    public void SetDirectory_rejects_a_path_that_is_an_existing_file()
    {
        var store = CreateStore();
        string file = Path.Combine(_root, "not-a-folder.txt");
        File.WriteAllText(file, "x");

        Assert.Throws<InvalidOperationException>(() => store.SetDirectory(file));
    }

    [Fact]
    public void Reset_removes_the_persisted_selection()
    {
        var store = CreateStore();
        store.SetDirectory(Path.Combine(_root, "chosen"));

        store.Reset();

        Assert.False(File.Exists(store.FilePath));
        Assert.Equal(Path.GetFullPath(store.DefaultDirectory), store.GetDirectory());
    }

    [Fact]
    public void BackupService_reads_the_injected_folder_dynamically()
    {
        string first = Path.Combine(_root, "first");
        string second = Path.Combine(_root, "second");

        var service = new BackupService(contextFactory: null!, backupDirectory: first);

        Assert.Equal(Path.GetFullPath(first), service.BackupDirectory);

        service.SetBackupDirectory(second);

        Assert.Equal(Path.GetFullPath(second), service.BackupDirectory);
        Assert.True(Directory.Exists(second));
    }

    [Fact]
    public void BackupService_reads_the_persisted_location_when_no_override_is_given()
    {
        string preferenceFile = AppPaths.BackupLocationFile;
        string? previous = File.Exists(preferenceFile)
            ? File.ReadAllText(preferenceFile)
            : null;

        string chosen = Path.Combine(_root, "persisted");

        try
        {
            BackupLocation.SetDirectory(chosen);

            var service = new BackupService(contextFactory: null!, backupDirectory: null);

            Assert.Equal(Path.GetFullPath(chosen), service.BackupDirectory);
        }
        finally
        {
            if (previous is null)
                BackupLocation.Reset();
            else
                File.WriteAllText(preferenceFile, previous);
        }
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_root))
                Directory.Delete(_root, recursive: true);
        }
        catch (IOException)
        {
        }
    }
}
