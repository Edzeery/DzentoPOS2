using DzentoPOS2.WinForms.Themes;
using EZControls.Controls.Inputs;
using EZControls.Core.Themes;

namespace DzentoPOS2.WinForms.Controls;

/// <summary>
/// Drop-down field used across the application forms. <para>
/// It keeps the native drop-down behaviour (keyboard support, accessibility,
/// data binding) while the chrome - border, chevron, item colours and font -
/// follows the application design tokens in both light and dark mode. The item
/// height is deliberately generous so Arabic labels are never clipped, which
/// was the root cause of the "garbled text" look of the old plain ComboBox.
/// </para>
/// </summary>
public class ThemedComboBox : EZComboBox
{
    public ThemedComboBox()
    {
        DropDownStyle = ComboBoxStyle.DropDownList;
        ItemHeight = 30;
        MinimumSize = new Size(140, 36);
        ApplyAppStyle();
    }

    /// <summary>
    /// The EZControls theme manager pushes a theme into every registered
    /// control whenever the application switches mode; the application
    /// palette is re-applied afterwards so the drop-down keeps the exact same
    /// colours as the rest of the form.
    /// </summary>
    public override void ApplyTheme(EZTheme theme)
    {
        base.ApplyTheme(theme);
        ApplyAppStyle();
    }

    /// <summary>Applies the application palette to the control.</summary>
    public void ApplyAppStyle()
    {
        if (IsDisposed) return;

        BackColor = AppColors.InputBackground;
        ForeColor = AppColors.Ink;
        Font = AppStyle.BodyFont;

        BorderColor = AppColors.InputBorder;
        BorderFocusColor = AppColors.InputBorderFocus;
        BorderRadius = AppStyle.RadiusSmall;
        BorderSize = 1;

        DropDownBackColor = AppColors.Surface;
        DropDownForeColor = AppColors.Ink;
        DropDownBorderColor = AppColors.Border;
        DropDownHoverBackColor = AppColors.PrimarySoft;
        DropDownSelectedBackColor = AppColors.Primary;
        DropDownSelectedForeColor = Color.White;
        DropDownBorderRadius = AppStyle.RadiusSmall;
        DropDownGap = 4;

        IconColor = AppColors.InkMuted;
        ItemHeight = 30;
    }

    /// <summary>
    /// Binds a list of objects to the drop-down. The display and value members
    /// are plain property names of the bound type.
    /// </summary>
    public void Bind<T>(IEnumerable<T> items, string displayMember, string valueMember)
    {
        DisplayMember = displayMember;
        ValueMember = valueMember;
        DataSource = items.ToList();
    }
}
