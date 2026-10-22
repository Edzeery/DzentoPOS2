using DzentoPOS2.Data.Seed;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.WinForms.Configurations;
using DzentoPOS2.WinForms.Infrastructure;
using DzentoPOS2.WinForms.Themes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DzentoPOS2.WinForms;

internal static class Program
{
    public static IServiceProvider? ServiceProvider { get; private set; }

    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var services = new ServiceCollection();

        IConfiguration configuration =
            new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

        services.AddWinForms(configuration);

        ServiceProvider = services.BuildServiceProvider();

        // Language: the persisted user preference always wins, the
        // appsettings value is only the fallback for a fresh installation.
        var languageService =
            ServiceProvider.GetRequiredService<ILanguageService>();

        var currentSettings =
            ServiceProvider.GetRequiredService<ICurrentSettingsService>();

        var savedLanguage = currentSettings.Current.Language;

        var defaultLanguage =
            configuration["Localization:DefaultLanguage"] ?? "ar";

        var startupLanguage =
            string.IsNullOrWhiteSpace(savedLanguage)
                ? defaultLanguage
                : savedLanguage;

        languageService.SetLanguage(startupLanguage);

        // Theme (persisted user preference)
        ThemeManager.LoadTheme();
        EZThemeBridge.Initialize();

        // DB Init
        DbInitializer.InitializeAsync(ServiceProvider)
            .GetAwaiter()
            .GetResult();

        Application.Run(
            ServiceProvider.GetRequiredService<AppApplicationContext>());
    }
}