namespace EZControls.Core.Drawing;

// ══════════════════════════════════════════════════════════════════════════════
//  ControlPainter (Enhanced)
//
//  High-level "paint a standard EZ control" helper with ultra-smooth rendering.
//  Fills the background with a rounded clip region, then delegates to BorderRenderer.
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Enhanced high-level painting routines for EZ controls with ultra-smooth rendering.
/// All methods are stateless; they allocate GDI+ resources internally and dispose them before returning.
/// </summary>
public static class ControlPainter
{
    /// <summary>
    /// Fills <paramref name="bounds"/> with a smooth rounded background and draws the
    /// border described by <paramref name="border"/> with maximum quality.
    /// </summary>
    /// <param name="g">Target graphics context.</param>
    /// <param name="bounds">Full client rectangle of the control.</param>
    /// <param name="backColor">Fill color.</param>
    /// <param name="border">Border specification.</param>
    public static void PaintControl(
        Graphics g,
        RectangleF bounds,
        Color backColor,
        BorderStyleSettings border)
    {
        // Apply ultra-high quality settings
        if (border.AntiAlias)
        {
            GraphicsHelper.SetHighQuality(g);
        }

        using GraphicsPath path = RoundedPathBuilder.Create(
            bounds,
            border.Radius,
            border.RoundTopLeft,
            border.RoundTopRight,
            border.RoundBottomRight,
            border.RoundBottomLeft);

        // Fill with smooth edges
        using SolidBrush brush = new(backColor);
        g.FillPath(brush, path);

        // Draw smooth border
        BorderRenderer.DrawRoundedBorder(g, bounds, border);
    }

    /// <summary>
    /// Fills a plain (non-rounded) rectangle with smooth edges.
    /// Faster than the rounded variant; use for large background areas.
    /// </summary>
    public static void FillBackground(Graphics g, Rectangle bounds, Color color)
    {
        using SolidBrush brush = new(color);
        g.FillRectangle(brush, bounds);
    }

    /// <summary>
    /// Draws <paramref name="text"/> inside <paramref name="bounds"/> using
    /// <see cref="TextRenderer"/> (GDI, not GDI+) for ClearType-correct
    /// sub-pixel rendering with maximum smoothness.
    /// </summary>
    /// <param name="g">Target graphics context.</param>
    /// <param name="bounds">Layout rectangle for the text.</param>
    /// <param name="text">String to render.</param>
    /// <param name="font">Font to use.</param>
    /// <param name="color">Foreground color.</param>
    /// <param name="alignment">
    /// Horizontal alignment inside <paramref name="bounds"/>.
    /// Only <c>MiddleLeft</c>, <c>MiddleCenter</c>, and <c>MiddleRight</c>
    /// are supported; other values fall back to centred.
    /// </param>
    public static void DrawText(
        Graphics g,
        Rectangle bounds,
        string text,
        Font font,
        Color color,
        ContentAlignment alignment = ContentAlignment.MiddleCenter)
    {
        TextFormatFlags flags =
            TextFormatFlags.EndEllipsis |
            TextFormatFlags.SingleLine |
            TextFormatFlags.VerticalCenter;

        flags |= alignment switch
        {
            ContentAlignment.MiddleLeft => TextFormatFlags.Left,
            ContentAlignment.MiddleRight => TextFormatFlags.Right,
            _ => TextFormatFlags.HorizontalCenter
        };

        TextRenderer.DrawText(g, text, font, bounds, color, flags);
    }

    /// <summary>
    /// Draws text with ClearType anti-aliasing and maximum quality for smooth appearance.
    /// </summary>
    public static void DrawSmoothText(
        Graphics g,
        Rectangle bounds,
        string text,
        Font font,
        Color color,
        ContentAlignment alignment = ContentAlignment.MiddleCenter)
    {
        // Apply high quality for text rendering
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        DrawText(g, bounds, text, font, color, alignment);
    }
}