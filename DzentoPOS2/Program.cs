using DzentoPOS2.App.Diagnostics;
using DzentoPOS2.Data.Seed;
using DzentoPOS2.Localization.Resources;
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

        RegisterGlobalExceptionHandlers();

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

        Application.Run(ServiceProvider.GetRequiredService<AppApplicationContext>());
    }

    /// <summary>
    /// Writes anything that escapes an event handler or an <c>async void</c>
    /// method to the rolling log before the process dies, so the next failure
    /// comes with a stack trace instead of only a message box.
    /// Under a debugger the default is left alone (VS keeps breaking on the
    /// exception, which is what a developer wants); the handler still runs
    /// when the app is started normally.
    /// </summary>
    private static void RegisterGlobalExceptionHandlers()
    {
        if (!System.Diagnostics.Debugger.IsAttached)
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

        // Exceptions on the UI thread: event handlers and async void
        // continuations end up here.
        Application.ThreadException += (_, e) => ReportUnhandled(e.Exception, "Unhandled UI exception");

        // Exceptions on other threads: the process is already going down, so
        // logging is all that can be done.
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception exception)
                ExceptionLogger.Write(exception, "Unhandled background exception (terminating)");
        };
    }

    private static void ReportUnhandled(Exception exception, string context)
    {
        try
        {
            var logPath = ExceptionLogger.Write(exception, context);

            var message = logPath is null
                ? $"{AppStrings.ErrorMsg}{Environment.NewLine}{Environment.NewLine}{exception.Message}"
                : $"{AppStrings.ErrorMsg}{Environment.NewLine}{Environment.NewLine}{logPath}";

            // Plain MessageBox on purpose: AppMessageBox builds its own form and
            // must not be re-entered from inside the exception handler.
            MessageBox.Show(message, AppStrings.ErrorMsg, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch
        {
            // Reporting must never take the process down on its own.
        }
    }
}