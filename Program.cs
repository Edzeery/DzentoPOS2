using DzentoPOS2.Data.Seed;
using DzentoPOS2.Localization.Services.Interfaces;
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

        // Language
        var languageService =
            ServiceProvider.GetRequiredService<ILanguageService>();

        var defaultLanguage =  configuration["Localization:DefaultLanguage"] ?? "ar";

        languageService.SetLanguage(defaultLanguage);

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