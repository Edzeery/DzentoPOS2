using System.Drawing.Drawing2D;

namespace EZControls.Core.Drawing;


// ══════════════════════════════════════════════════════════════════════════════
//  BorderStyleSettings (Enhanced)  –  immutable-by-convention value object
//
//  Carries the complete specification for a single border rendering pass.
//  Enhanced with better defaults for maximum smoothness.
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Enhanced settings for rendering ultra-smooth rounded borders.
/// Controls hold one instance and pass it to <see cref="BorderRenderer"/>
/// during <c>OnPaint</c>.
/// </summary>
public sealed class BorderStyleSettings
{
    // ── Visibility ────────────────────────────────────────────────────────────

    /// <summary>
    /// When <see langword="false"/> the renderer returns immediately without
    /// drawing. Use instead of setting <see cref="BorderSize"/> to zero to
    /// avoid allocating a zero-width pen.
    /// </summary>
    public bool Enabled { get; set; } = true;

    // ── Geometry ──────────────────────────────────────────────────────────────

    /// <summary>Corner arc radius in logical pixels. 0 = sharp rectangle.</summary>
    public int Radius { get; set; } = 8;

    /// <summary>Pen width in logical pixels. Recommended: 1-2 for smooth appearance.</summary>
    public float BorderSize { get; set; } = 1.5f;

    // ── Color ─────────────────────────────────────────────────────────────────

    /// <summary>Solid border color, or the first gradient stop when <see cref="UseGradient"/> is <see langword="true"/>.</summary>
    public Color BorderColor { get; set; } = Color.FromArgb(180, 180, 180);

    /// <summary>When <see langword="true"/>, a <see cref="LinearGradientBrush"/> is used for the pen.</summary>
    public bool UseGradient { get; set; } = false;

    /// <summary>Second gradient stop color. Ignored when <see cref="UseGradient"/> is <see langword="false"/>.</summary>
    public Color GradientColor { get; set; } = Color.Blue;

    /// <summary>Gradient angle in degrees (0 = left-to-right).</summary>
    public float GradientAngle { get; set; } = 90f;

    // ── Style ─────────────────────────────────────────────────────────────────

    /// <summary>Line dash pattern.</summary>
    public DashStyle DashStyle { get; set; } = DashStyle.Solid;

    /// <summary>
    /// When <see langword="true"/>, enables anti-aliasing for smooth, crisp borders.
    /// Recommended: always <see langword="true"/> for professional appearance.
    /// </summary>
    public bool AntiAlias { get; set; } = true;

    // ── Per-corner rounding ───────────────────────────────────────────────────

    /// <summary>Round the top-left corner.</summary>
    public bool RoundTopLeft { get; set; } = true;

    /// <summary>Round the top-right corner.</summary>
    public bool RoundTopRight { get; set; } = true;

    /// <summary>Round the bottom-left corner.</summary>
    public bool RoundBottomLeft { get; set; } = true;

    /// <summary>Round the bottom-right corner.</summary>
    public bool RoundBottomRight { get; set; } = true;

    // ── Advanced smoothing ─────────────────────────────────────────────────────

    /// <summary>
    /// When <see langword="true"/>, applies high-quality smoothing for ultra-crisp borders.
    /// Slightly higher CPU cost but professional appearance. Default: <see langword="true"/>.
    /// </summary>
    public bool HighQualitySmoothing { get; set; } = true;
}