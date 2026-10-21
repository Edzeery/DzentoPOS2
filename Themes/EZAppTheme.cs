using EZControls.Core.Themes;

namespace DzentoPOS2.WinForms.Themes;

/// <summary>
/// Bridges the application design tokens into the EZControls theme system so
/// every custom input (text box, combo box, toggle) uses exactly the same
/// palette as the rest of the application in both light and dark mode.
/// </summary>
internal sealed class EZAppTheme : EZTheme
{
    public override string Name => ThemeManager.IsDark ? "Dark" : "Light";

    public override Color PrimaryColor => AppColors.Primary;
    public override Color SecondaryColor => AppColors.Primary;
    public override Color HoverColor => AppColors.PrimaryHover;
    public override Color PressedColor => AppColors.PrimaryPressed;
    public override Color FocusedColor => AppColors.Primary;

    public override Color BackgroundColor => AppColors.InputBackground;
    public override Color SurfaceColor => AppColors.Surface;
    public override Color SurfaceHoverColor => AppColors.SurfaceAlt;

    public override Color TextColor => AppColors.Ink;
    public override Color PlaceholderColor => AppColors.InkMuted;
    public override Color BorderColor => AppColors.InputBorder;
    public override Color ErrorColor => AppColors.Danger;
    public override Color DisabledColor => AppColors.DisabledBack;
}
