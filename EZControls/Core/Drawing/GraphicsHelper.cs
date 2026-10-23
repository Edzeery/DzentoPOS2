using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace EZControls.Core.Drawing;
// ══════════════════════════════════════════════════════════════════════════════
//  GraphicsHelper (Enhanced)
//
//  Ultra-smooth rendering settings for professional appearance.
//  All quality settings are applied for maximum smoothness and clarity.
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Utility methods for configuring a <see cref="Graphics"/> context and
/// building common geometric primitives with ultra-smooth rendering.
/// </summary>
public static class GraphicsHelper
{
    /// <summary>
    /// Applies ultra-high-quality rendering hints to <paramref name="g"/>.
    /// Call once at the start of every <c>OnPaint</c> override.
    /// This ensures the smoothest possible borders, rounded corners, and text.
    /// </summary>
    /// <param name="g">The graphics context to configure.</param>
    public static void SetHighQuality(Graphics g)
    {
        // Compositing: blend source pixels over destination
        g.CompositingMode = CompositingMode.SourceOver;

        // Antialiasing: smooth all edges (borders, curves, text)
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Image interpolation: highest quality scaling
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

        // Pixel offset: align pixels perfectly for crisp rendering
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        // Compositing quality: highest available quality
        g.CompositingQuality = CompositingQuality.HighQuality;

        // Text rendering: ClearType for sharp, anti-aliased text
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
    }

    /// <summary>
    /// Ultra-enhanced version of SetHighQuality that includes additional
    /// smoothing for borders and curves. Use this for controls with borders.
    /// </summary>
    public static void SetUltraHighQuality(Graphics g)
    {
        SetHighQuality(g);

        // Additional smoothing for curves and borders
        // Note: HighQuality is already set above, but being explicit
        g.SmoothingMode = SmoothingMode.HighQuality;
    }

    /// <summary>
    /// Creates a rounded-rectangle <see cref="GraphicsPath"/> with ultra-smooth curves.
    /// </summary>
    /// <param name="bounds">Outer bounding rectangle.</param>
    /// <param name="radius">Corner arc radius in logical pixels. Clamped to half the shortest side.</param>
    /// <returns>A new <see cref="GraphicsPath"/>; the caller must dispose it.</returns>
    public static GraphicsPath CreateRoundedPath(RectangleF bounds, float radius)
    {
        GraphicsPath path = new();

        if (radius <= 0)
        {
            path.AddRectangle(bounds);
            path.CloseFigure();
            return path;
        }

        // Clamp radius so arcs don't overlap on very small controls
        float maxRadius = Math.Min(bounds.Width, bounds.Height) / 2f;
        float r = Math.Min(radius, maxRadius);
        float d = r * 2f;

        path.StartFigure();

        // Top-left corner: 180° to 270°
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);

        // Top-right corner: 270° to 360°
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);

        // Bottom-right corner: 0° to 90°
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);

        // Bottom-left corner: 90° to 180°
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);

        path.CloseFigure();
        return path;
    }

    /// <summary>
    /// Returns a <see cref="Rectangle"/> uniformly inset by <paramref name="amount"/> on each side.
    /// </summary>
    public static Rectangle Deflate(Rectangle rectangle, int amount) =>
        Rectangle.Inflate(rectangle, -amount, -amount);

    /// <summary>
    /// Returns a <see cref="RectangleF"/> uniformly inset by <paramref name="amount"/> on each side.
    /// </summary>
    public static RectangleF Deflate(RectangleF rectangle, float amount) =>
        RectangleF.Inflate(rectangle, -amount, -amount);
}
