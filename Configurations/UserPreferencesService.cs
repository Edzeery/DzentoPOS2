using System.Text.Json;

namespace DzentoPOS2.WinForms.Configurations;

public class UserPreferencesService
    : IUserPreferencesService
{
    private readonly string _filePath =
        Path.Combine(
            Application.StartupPath,
            "user-settings.json");

    public AppSettings Load()
    {
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