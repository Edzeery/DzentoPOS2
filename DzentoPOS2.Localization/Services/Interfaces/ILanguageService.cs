namespace DzentoPOS2.Localization.Services.Interfaces
{
    public interface ILanguageService
    {
        string CurrentLanguage { get; }

        event EventHandler<string>? LanguageChanged;

        void SetLanguage(string culture);

        bool IsRightToLeft();
    }
}