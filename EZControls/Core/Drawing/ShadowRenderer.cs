using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace EZControls.Core.Drawing;

// ══════════════════════════════════════════════════════════════════════════════
//  ShadowRenderer
//
//  Performance strategy:
//    The shadow shape is fully determined by four inputs:
//        control size  ×  ShadowSize  ×  ShadowOpacity  ×  ShadowColor
//
//    None of these change during hover or ripple animations.  We therefore bake
//    the shadow into an off-screen Bitmap once and reuse it via a single
//    DrawImageUnscaled call on every paint – roughly 10× cheaper than
//    recreating a PathGradientBrush each tick.
//
//    Cache invalidation: callers call Invalidate() whenever any of the four
//    inputs changes, or the control is resized.
//
//  Thread safety:
//    Not thread-safe.  Assumed to be used exclusively on the UI thread.
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Renders a soft circular drop-shadow beneath a circular control.
/// Uses an internal <see cref="Bitmap"/> cache to avoid GDI+ brush allocation
/// on every paint tick.
/// </summary>
public sealed class ShadowRenderer : IDisposable
{
    // ── Cache state ───────────────────────────────────────────────────────────
    private Bitmap? _cache;
    private Size _cachedSize;
    private int _cachedShadowSize;
    private int _cachedOpacity;
    private Color _cachedColor;

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Signals that the cached shadow bitmap is stale.
    /// Call whenever <c>ShadowSize</c>, <c>ShadowOpacity</c>, <c>ShadowColor</c>,
    /// or the control size changes.
    /// </summary>
    public void Invalidate()
    {
        _cache?.Dispose();
        _cache = null;
    }

    /// <summary>
    /// Draws the drop-shadow onto <paramref name="g"/>.
    /// Rebuilds the internal bitmap cache if any parameter has changed since
    /// the last call.
    /// </summary>
    /// <param name="g">Target graphics context (the control's client surface).</param>
    /// <param name="controlBounds">Full client rectangle of the control.</param>
    /// <param name="shadowSize">
    /// Shadow halo spread in logical pixels.  Values ≤ 0 are a no-op.
    /// </param>
    /// <param name="opacity">Shadow alpha (0 = transparent, 255 = opaque).</param>
    /// <param name="color">Shadow tint color (alpha is overridden by <paramref name="opacity"/>).</param>
    public void Draw(
        Graphics g,
        Rectangle controlBounds,
        int shadowSize,
        int opacity,
        Color color)
    {
        if (shadowSize <= 0)
            return;

        // Rebuild the cache if any input has changed.
        if (_cache is null
            || _cachedSize != controlBounds.Size
            || _cachedShadowSize != shadowSize
            || _cachedOpacity != opacity
            || _cachedColor != color)
        {
            RebuildCache(controlBounds.Size, shadowSize, opacity, color);
        }

        if (_cache is null)
            return;

        // A single unscaled blit – no brush, no path, no allocation.
        g.DrawImageUnscaled(_cache, controlBounds.Location);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void RebuildCache(Size size, int shadowSize, int opacity, Color color)
    {
        Invalidate(); // dispose any existing cache first

        if (size.Width <= 0 || size.Height <= 0)
            return;

        Bitmap bmp = new(size.Width, size.Height, PixelFormat.Format32bppArgb);

        try
        {
            using Graphics bg = Graphics.FromImage(bmp);
            bg.SmoothingMode = SmoothingMode.AntiAlias;

            // The shadow disk is inset by half the shadow size so the gradient
            // halo has room to spread outward without being clipped.
            int inset = shadowSize / 2;
            Rectangle shadowDisk = new(
                inset, inset,
                size.Width - shadowSize,
                size.Height - shadowSize);

            if (shadowDisk.Width <= 0 || shadowDisk.Height <= 0)
            {
                bmp.Dispose();
                return;
            }

            // The gradient's outer ellipse is the full halo.
            Rectangle halo = shadowDisk;
            halo.Inflate(shadowSize, shadowSize);

            using GraphicsPath path = new();
            path.AddEllipse(halo);

            using PathGradientBrush brush = new(path);
            brush.CenterColor = Color.FromArgb(opacity, color);
            brush.SurroundColors = new[] { Color.Transparent };

            // FocusScales push the full-opacity center out to the inner disk boundary.
            brush.FocusScales = new PointF(
                halo.Width > 0 ? (float)shadowDisk.Width / halo.Width : 0f,
                halo.Height > 0 ? (float)shadowDisk.Height / halo.Height : 0f);

            bg.FillPath(brush, path);
        }
        catch
        {
            // Any GDI+ failure during cache construction is non-fatal;
            // we simply skip the shadow for this paint cycle.
            bmp.Dispose();
            return;
        }

        _cache = bmp;
        _cachedSize = size;
        _cachedShadowSize = shadowSize;
        _cachedOpacity = opacity;
        _cachedColor = color;
    }

    // ── IDisposable ───────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public void Dispose()
    {
        _cache?.Dispose();
        _cache = null;
    }
}