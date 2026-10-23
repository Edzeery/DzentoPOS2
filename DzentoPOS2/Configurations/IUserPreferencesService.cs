namespace DzentoPOS2.WinForms.Configurations;

public interface IUserPreferencesService
{
    AppSettings Load();

    void Save(AppSettings settings);
}