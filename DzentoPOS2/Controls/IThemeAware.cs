namespace DzentoPOS2.WinForms.Controls;

/// <summary>
/// Implemented by controls that own their visual identity (cards, custom
/// buttons, charts...). The theme engine delegates to them instead of
/// overwriting their internal colors, so a theme switch never destroys
/// accent bars, value colors or hover states.
/// </summary>
public interface IThemeAware
{
    void ApplyTheme();
}
