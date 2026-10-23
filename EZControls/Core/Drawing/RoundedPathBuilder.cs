using System.Drawing.Drawing2D;

namespace EZControls.Core.Drawing;

// ══════════════════════════════════════════════════════════════════════════════
//  RoundedPathBuilder
//
//  Per-corner variant of GraphicsHelper.CreateRoundedPath.
//  Used by buttons and cards that need one or more square corners (e.g. a
//  tab page where the selected tab has a square bottom edge).
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Builds a <see cref="GraphicsPath"/> for a rectangle with independently-
/// configurable corner radii.
/// </summary>
public static class RoundedPathBuilder
{
    /// <summary>
    /// Creates a rounded rectangle path where each corner can be independently
    /// rounded or left square.
    /// </summary>
    /// <param name="bounds">Outer bounding rectangle.</param>
    /// <param name="radius">Arc radius applied to enabled corners.</param>
    /// <param name="topLeft">Round the top-left corner.</param>
    /// <param name="topRight">Round the top-right corner.</param>
    /// <param name="bottomRight">Round the bottom-right corner.</param>
    /// <param name="bottomLeft">Round the bottom-left corner.</param>
    /// <returns>A new <see cref="GraphicsPath"/>; the caller must dispose it.</returns>
    public static GraphicsPath Create(
        RectangleF bounds,
        int radius,
        bool topLeft = true,
        bool topRight = true,
        bool bottomRight = true,
        bool bottomLeft = true)
    {
        GraphicsPath path = new();

        if (radius <= 0)
        {
            path.AddRectangle(bounds);
            return path;
        }

        // Clamp radius to fit the smallest dimension.
        float r = Math.Min(radius, Math.Min(bounds.Width, bounds.Height) / 2);
        float d = r * 2;

        // Each Arc() call sweeps 90° of the corner circle.
        // For square corners we add a degenerate line to the corner point.

        // Top-left → top-right
        if (topLeft)
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        else
            path.AddLine(bounds.X, bounds.Y, bounds.X, bounds.Y);

        // Top-right → bottom-right
        if (topRight)
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        else
            path.AddLine(bounds.Right, bounds.Y, bounds.Right, bounds.Y);

        // Bottom-right → bottom-left
        if (bottomRight)
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        else
            path.AddLine(bounds.Right, bounds.Bottom, bounds.Right, bounds.Bottom);

        // Bottom-left → top-left
        if (bottomLeft)
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        else
            path.AddLine(bounds.X, bounds.Bottom, bounds.X, bounds.Bottom);

        path.CloseFigure();
        return path;
    }
}