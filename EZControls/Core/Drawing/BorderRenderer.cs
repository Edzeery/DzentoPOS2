using System.Drawing.Drawing2D;

namespace EZControls.Core.Drawing;
// ══════════════════════════════════════════════════════════════════════════════
//  BorderRenderer (Enhanced)
//
//  Improved rendering for ultra-smooth, crisp borders with proper anti-aliasing.
//  All GDI+ resources are allocated and disposed within the single Draw call.
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Enhanced stateless utility that renders smooth, rounded, optionally-gradient borders.
/// </summary>
public static class BorderRenderer
{
    /// <summary>
    /// Draws a smooth rounded border described by <paramref name="style"/> inside
    /// <paramref name="bounds"/> with maximum quality anti-aliasing.
    /// </summary>
    /// <param name="g">Target graphics context.</param>
    /// <param name="bounds">
    /// The control's layout rectangle. The border is drawn 1 px inside this
    /// rectangle so it is not clipped by the parent.
    /// </param>
    /// <param name="style">Full rendering specification.</param>
    public static void DrawRoundedBorder(
        Graphics g,
        RectangleF bounds,
        BorderStyleSettings style)
    {
        if (!style.Enabled || style.BorderSize <= 0)
            return;

        // Apply anti-aliasing for smooth edges
        if (style.AntiAlias)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
        }

        // Deflate by 1 px so the pen centre-line is entirely within the client area.
        RectangleF rect = RectangleF.Inflate(bounds, -1, -1);

        if (rect.Width <= 0 || rect.Height <= 0)
            return;

        using GraphicsPath path = RoundedPathBuilder.Create(
            rect,
            style.Radius,
            style.RoundTopLeft,
            style.RoundTopRight,
            style.RoundBottomRight,
            style.RoundBottomLeft);

        using Pen pen = CreatePen(rect, style);

        // Ultra-smooth pen settings
        pen.Alignment = PenAlignment.Center;
        pen.LineJoin = LineJoin.Round;           // Smooth corner joins
        pen.StartCap = LineCap.Round;            // Smooth start
        pen.EndCap = LineCap.Round;              // Smooth end
        pen.MiterLimit = 4f;                     // Limit miter spikes

        g.DrawPath(pen, path);
    }

    /// <summary>
    /// Draws a smooth underline-style border (bottom edge only).
    /// Useful for Material Design / minimalist input fields.
    /// </summary>
    /// <param name="g">Target graphics context.</param>
    /// <param name="bounds">Full control bounds.</param>
    /// <param name="color">Line color.</param>
    /// <param name="thickness">Line width in logical pixels.</param>
    public static void DrawUnderlineBorder(
        Graphics g,
        Rectangle bounds,
        Color color,
        float thickness)
    {
        if (thickness <= 0)
            return;

        // Apply anti-aliasing for smooth edges
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        using Pen pen = new(color, thickness)
        {
            LineJoin = LineJoin.Round,
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            Alignment = PenAlignment.Center
        };

        g.DrawLine(
            pen,
            bounds.Left, bounds.Bottom - 1,
            bounds.Right - 1, bounds.Bottom - 1);
    }

    /// <summary>
    /// Draws a smooth rounded rectangle outline (just the border, no fill).
    /// </summary>
    /// <param name="g">Target graphics context.</param>
    /// <param name="bounds">Rectangle bounds.</param>
    /// <param name="radius">Corner radius.</param>
    /// <param name="color">Border color.</param>
    /// <param name="thickness">Border thickness.</param>
    public static void DrawSmoothRoundedRectangle(
        Graphics g,
        Rectangle bounds,
        int radius,
        Color color,
        float thickness)
    {
        // Apply anti-aliasing
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        using GraphicsPath path = RoundedPathBuilder.Create(bounds, radius);
        using Pen pen = new(color, thickness)
        {
            LineJoin = LineJoin.Round,
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            Alignment = PenAlignment.Center
        };

        g.DrawPath(pen, path);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Creates the <see cref="Pen"/> for the border, either solid or gradient.
    /// The caller is responsible for disposing the returned pen.
    /// </summary>
    private static Pen CreatePen(RectangleF rect, BorderStyleSettings style)
    {
        if (style.UseGradient)
        {
            // LinearGradientBrush is owned by the pen; disposing the pen
            // disposes the brush (Pen.Dispose calls Brush.Dispose internally).
            var brush = new LinearGradientBrush(
                rect,
                style.BorderColor,
                style.GradientColor,
                style.GradientAngle);

            return new Pen(brush, style.BorderSize)
            {
                DashStyle = style.DashStyle
            };
        }

        return new Pen(style.BorderColor, style.BorderSize)
        {
            DashStyle = style.DashStyle
        };
    }
}