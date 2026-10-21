using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Controls;

/// <summary>
/// Semantic tone of a button. The concrete colors are resolved from
/// <see cref="AppColors"/> every time the theme changes.
/// </summary>
public enum ButtonTone
{
    Primary,
    Success,
    Danger,
    Warning,
    Info,
    Neutral
}

/// <summary>
/// Solid button that keeps its colors in sync with the active theme.
/// </summary>
public class ThemedButton : Button, IThemeAware
{
    public ButtonTone Tone { get; set; } = ButtonTone.Primary;

    public ThemedButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        UseVisualStyleBackColor = false;
        Cursor = Cursors.Hand;
        Font = AppStyle.ButtonFont;
        ForeColor = Color.White;
        EnabledChanged += (_, _) => ApplyTheme();
    }

    public void ApplyTheme()
    {
        if (IsDisposed) return;

        Color back = ResolveBack(Tone);
        Color hover = ResolveHover(Tone);
        Color pressed = ResolvePressed(Tone);

        if (Enabled)
        {
            BackColor = back;
            ForeColor = Color.White;
        }
        else
        {
            BackColor = AppColors.DisabledBack;
            ForeColor = AppColors.DisabledFore;
        }

        FlatAppearance.MouseOverBackColor = hover;
        FlatAppearance.MouseDownBackColor = pressed;
    }

    internal static Color ResolveBack(ButtonTone tone) => tone switch
    {
        ButtonTone.Success => AppColors.Success,
        ButtonTone.Danger => AppColors.Danger,
        ButtonTone.Warning => AppColors.Warning,
        ButtonTone.Info => AppColors.Info,
        ButtonTone.Neutral => AppColors.Neutral,
        _ => AppColors.Primary
    };

    internal static Color ResolveHover(ButtonTone tone) => tone switch
    {
        ButtonTone.Success => AppColors.SuccessHover,
        ButtonTone.Danger => AppColors.DangerHover,
        ButtonTone.Primary => AppColors.PrimaryHover,
        _ => AppColors.Hover(ResolveBack(tone))
    };

    internal static Color ResolvePressed(ButtonTone tone) => tone switch
    {
        ButtonTone.Success => AppColors.Pressed(AppColors.Success),
        ButtonTone.Danger => AppColors.Pressed(AppColors.Danger),
        ButtonTone.Primary => AppColors.PrimaryPressed,
        _ => AppColors.Pressed(ResolveBack(tone))
    };
}
