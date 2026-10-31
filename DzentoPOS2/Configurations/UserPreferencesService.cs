using System.Text.Json;
using DzentoPOS2.Core.Configuration;

namespace DzentoPOS2.WinForms.Configurations;

public class UserPreferencesService
    : IUserPreferencesService
{
    private readonly string _filePath = AppPaths.UserSettingsFile;

    public AppSettings Load()
    {
        if (!File.Exists(_filePath))
        {
            // One-time move of the file an older version kept next to the
            // executable. The old copy is left in place untouched.
            AppPaths.MigrateLegacyFile(
                Path.Combine(Application.StartupPath, "user-settings.json"),
                _filePath);
        }

        if (!File.Exists(_filePath))
        {
            return new AppSettings();
        }

        var json = File.ReadAllText(_filePath);

        return JsonSerializer.Deserialize<AppSettings>(json)
               ?? new AppSettings();
    }

    public void Save(AppSettings settings)
    {
        AppPaths.EnsureCreated();

        var json =
            JsonSerializer.Serialize(
                settings,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

        File.WriteAllText(_filePath, json);
    }
}