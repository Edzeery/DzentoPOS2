using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Controls.Header;

/// <summary>
/// Header palette. The values are resolved on every access so the header
/// always reflects the active theme.
/// </summary>
internal static class HeaderColors
{
    public static Color Ink => AppColors.Ink;
    public static Color MutedInk => AppColors.InkMuted;
    public static Color Border => AppColors.Border;
    public static Color Surface => AppColors.Surface;
    public static Color SurfaceAlt => AppColors.SurfaceAlt;
    public static Color Accent => AppColors.Primary;
    public static Color AccentSoft => AppColors.PrimarySoft;
    public static Color Danger => AppColors.Danger;
}
