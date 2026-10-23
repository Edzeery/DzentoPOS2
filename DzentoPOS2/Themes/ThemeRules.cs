using System.Drawing;

namespace DzentoPOS2.WinForms.Themes;

/// <summary>
/// Color resolution rules shared by the theme engine. The goal is that a
/// control always ends up readable: text placed on a dark surface keeps a
/// light foreground, text on a light surface uses the theme ink, and colors
/// that were chosen deliberately (status colors) are preserved.
/// </summary>
internal static class ThemeRules
{
    private const int Tolerance = 14;

    public static bool IsDarkSurface(Color color)
    {
        double luminance = (0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B) / 255.0;
        return luminance < 0.45;
    }

    /// <summary>
    /// Walks up the parent chain to find the surface a control actually
    /// renders on, so transparent panels and labels resolve correctly.
    /// </summary>
    public static Color ResolveBackColor(Control? control)
    {
        Control? current = control?.Parent;

        while (current != null)
        {
            if (current.BackColor.A == 255)
                return current.BackColor;

            current = current.Parent;
        }

        return AppColors.Background;
    }

    public static Color ForeColorOn(Color background)
        => IsDarkSurface(background) ? AppColors.OnDark : AppColors.Ink;

    /// <summary>
    /// Resolves the foreground of a label, preserving deliberate colors such
    /// as status reds and muted grays.
    /// </summary>
    public static Color ResolveLabelColor(Label label)
    {
        string? tag = label.Tag?.ToString();

        switch (tag)
        {
            case "muted": return AppColors.InkMuted;
            case "primary": return AppColors.Primary;
            case "success": return AppColors.Success;
            case "danger": return AppColors.Danger;
            case "warning": return AppColors.Warning;
            case "info": return AppColors.Info;
            case "inverse": return AppColors.OnDark;
            case "keep": return label.ForeColor;
        }

        Color current = label.ForeColor;

        if (Matches(current, AppColors.Success)) return AppColors.Success;
        if (Matches(current, AppColors.Danger)) return AppColors.Danger;
        if (Matches(current, AppColors.Warning)) return AppColors.Warning;
        if (Matches(current, AppColors.Info)) return AppColors.Info;
        if (Matches(current, AppColors.Primary)) return AppColors.Primary;

        if (IsNeutralGray(current))
            return AppColors.InkMuted;

        return ForeColorOn(ResolveBackColor(label));
    }

    private static bool IsNeutralGray(Color color)
    {
        if (Math.Abs(color.R - color.G) > 10 || Math.Abs(color.G - color.B) > 10)
            return false;

        return color.R >= 90 && color.R <= 205;
    }

    public static bool Matches(Color a, Color b)
        => Math.Abs(a.R - b.R) <= Tolerance
        && Math.Abs(a.G - b.G) <= Tolerance
        && Math.Abs(a.B - b.B) <= Tolerance;
}
