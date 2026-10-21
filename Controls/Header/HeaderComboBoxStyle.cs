using EZControls.Controls.Inputs;

namespace DzentoPOS2.WinForms.Controls.Header;

internal static class HeaderComboBoxStyle
{
    public static void Apply(EZComboBox comboBox)
    {
        comboBox.BackColor = HeaderColors.Surface;
        comboBox.BorderColor = HeaderColors.Border;
        comboBox.BorderFocusColor = HeaderColors.Accent;
        comboBox.BorderRadius = 12;
        comboBox.BorderSize = 1;
        comboBox.DropDownBackColor = HeaderColors.Surface;
        comboBox.DropDownBorderColor = HeaderColors.Border;
        comboBox.DropDownForeColor = HeaderColors.Ink;
        comboBox.DropDownHoverBackColor = HeaderColors.AccentSoft;
        comboBox.DropDownSelectedBackColor = HeaderColors.Accent;
        comboBox.DropDownSelectedForeColor = Color.White;
        comboBox.DropDownBorderRadius = 10;
        comboBox.DropDownGap = HeaderPopupOptions.Gap;
        comboBox.DropDownPlacement = EZDropDownPlacement.Bottom;
        comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox.Font = new Font("Segoe UI", 9.5F);
        comboBox.ForeColor = HeaderColors.Ink;
        comboBox.IconColor = HeaderColors.MutedInk;
        comboBox.ItemHeight = 30;
        comboBox.MaxDropDownItems = 6;
        comboBox.MinimumSize = new Size(108, 38);
        comboBox.Size = new Size(118, 42);
    }
}
