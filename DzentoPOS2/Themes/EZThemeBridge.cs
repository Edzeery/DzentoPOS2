namespace DzentoPOS2.WinForms.Themes;

/// <summary>
/// Keeps the EZControls theme in sync with the application theme. Without this
/// bridge the custom inputs would keep the control library's own palette and
/// would not follow the light/dark switch.
/// </summary>
internal static class EZThemeBridge
{
    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        ThemeManager.ThemeChanged += OnThemeChanged;
        Apply();
    }

    private static void OnThemeChanged(object? sender, EventArgs e) => Apply();

    private static void Apply()
    {
        try
        {
            EZControls.Core.Helpers.ThemeManager.SetTheme(new EZAppTheme());
        }
        catch
        {
            // The theme must never break the application.
        }
    }
}
