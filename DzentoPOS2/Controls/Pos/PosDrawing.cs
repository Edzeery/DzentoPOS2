using System.Drawing;
using System.Drawing.Drawing2D;

namespace DzentoPOS2.WinForms.Controls.Pos;

/// <summary>Small painting helpers shared by the POS controls.</summary>
internal static class PosDrawing
{
    /// <summary>
    /// Rounded rectangle path. The radius is clamped so tiny controls still
    /// produce a valid path (GDI+ rejects arcs larger than the bounds).
    /// </summary>
    public static GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();

        if (bounds.Width <= 0 || bounds.Height <= 0)
            return path;

        radius = System.Math.Max(1, System.Math.Min(radius, System.Math.Min(bounds.Width, bounds.Height) / 2));
        int diameter = radius * 2;

        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }

    /// <summary>Mixes <paramref name="accent"/> into <paramref name="background"/>.</summary>
    public static Color Blend(Color background, Color accent, float accentShare)
    {
        accentShare = System.Math.Clamp(accentShare, 0f, 1f);
        return Color.FromArgb(
            (int)(background.R + (accent.R - background.R) * accentShare),
            (int)(background.G + (accent.G - background.G) * accentShare),
            (int)(background.B + (accent.B - background.B) * accentShare));
    }

    /// <summary>Horizontal mirroring for text rendered with TextRenderer.</summary>
    public static TextFormatFlags Direction(Control control) =>
        control.RightToLeft == RightToLeft.Yes ? TextFormatFlags.RightToLeft : TextFormatFlags.Left;
}
