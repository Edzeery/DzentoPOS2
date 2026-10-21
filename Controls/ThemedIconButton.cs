using DzentoPOS2.WinForms.Themes;
using FontAwesome.Sharp;

namespace DzentoPOS2.WinForms.Controls;

/// <summary>
/// Outlined icon button (toolbar action) that follows the active theme.
/// </summary>
public class ThemedIconButton : IconButton, IThemeAware
{
    public ButtonTone Tone { get; set; } = ButtonTone.Primary;

    public ThemedIconButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 1;
        UseVisualStyleBackColor = false;
        Cursor = Cursors.Hand;
        Font = AppStyle.ButtonFont;
        TextImageRelation = TextImageRelation.ImageBeforeText;
    }

    public void ApplyTheme()
    {
        if (IsDisposed) return;

        Color accent = ThemedButton.ResolveBack(Tone);

        IconColor = accent;
        ForeColor = accent;
        BackColor = AppColors.Surface;
        FlatAppearance.BorderColor = accent;
        FlatAppearance.MouseOverBackColor = AppColors.Tint(accent, 0.90f);
        FlatAppearance.MouseDownBackColor = AppColors.Tint(accent, 0.82f);
    }
}
