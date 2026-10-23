using System.Globalization;
using DzentoPOS2.Localization.Constants;
using DzentoPOS2.Localization.Services.Interfaces;

namespace DzentoPOS2.Localization.Services
{
    public class LanguageService : ILanguageService
    {
        public string CurrentLanguage =>
            CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        public event EventHandler<string>? LanguageChanged;

        public void SetLanguage(string culture)
        {
            var cultureInfo = ResolveCulture(culture);

            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            LanguageChanged?.Invoke(this, culture);
        }

        /// <summary>
        /// Resolves a culture code to a usable culture. An unknown code falls
        /// back to the default language instead of throwing, so a hand edited
        /// preferences file can never prevent the application from starting.
        /// The stored preference itself is left untouched by the caller.
        /// </summary>
        private static CultureInfo ResolveCulture(string? culture)
        {
            if (string.IsNullOrWhiteSpace(culture))
                return new CultureInfo(SupportedLanguages.Arabic);

            try
            {
                return new CultureInfo(culture.Trim());
            }
            catch (CultureNotFoundException)
            {
                return new CultureInfo(SupportedLanguages.Arabic);
            }
        }

        public bool IsRightToLeft()
        {
            return CurrentLanguage == SupportedLanguages.Arabic;
        }
    }
}