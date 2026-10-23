using DzentoPOS2.WinForms.Themes;
using FontAwesome.Sharp;

namespace DzentoPOS2.WinForms.Controls;

/// <summary>
/// Outlined icon button (toolbar action) that follows the active theme.
/// </summary>
public class ThemedIconButton : IconButton, IThemeAware
{
    public ButtonTone Tone { get; set; } = ButtonTone.Primary;

    /// <summary>
    /// Filled variant: the tone color becomes the background and the label
    /// turns white. Used for the primary action of a toolbar.
    /// </summary>
    public bool Solid { get; set; }

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

        IconColor = Solid ? Color.White : accent;
        ForeColor = Solid ? Color.White : accent;

        if (Solid)
        {
            BackColor = accent;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.MouseOverBackColor = ThemedButton.ResolveHover(Tone);
            FlatAppearance.MouseDownBackColor = ThemedButton.ResolvePressed(Tone);
        }
        else
        {
            BackColor = AppColors.Surface;
            FlatAppearance.BorderSize = 1;
            FlatAppearance.BorderColor = accent;
            FlatAppearance.MouseOverBackColor = AppColors.Tint(accent, 0.90f);
            FlatAppearance.MouseDownBackColor = AppColors.Tint(accent, 0.82f);
        }
    }
}
