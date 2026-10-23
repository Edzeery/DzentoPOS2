using System.ComponentModel;
using System.Drawing.Drawing2D;
using EZControls.Core.Base;
using EZControls.Core.Constants;
using EZControls.Core.Drawing;
using EZControls.Core.Themes;

namespace EZControls.Controls.Progress;

// ══════════════════════════════════════════════════════════════════════════════
//  EZProgressBar
//
//  A rounded, gradient-capable, animated progress bar.
//
//  Features:
//  • Horizontal and vertical orientation.
//  • Optional gradient fill using LinearGradientBrush.
//  • Optional animated shimmer / stripes for indeterminate/busy states.
//  • Optional center percentage label.
//  • Smooth value transitions using AnimationState.
//  • Full theme integration.
//
//  Rendering layers (bottom to top):
//    1. Track background (rounded rect, TrackColor).
//    2. Progress fill (rounded rect, clipped to progress width).
//    3. Optional shimmer overlay.
//    4. Optional percentage text.
//    5. Optional border.
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// A styled, animated progress bar supporting both determinate and
/// indeterminate states.
/// </summary>
[DefaultEvent(nameof(ValueChanged))]
[ToolboxBitmap(typeof(ProgressBar))]
[DesignerCategory("Code")]
public class EZProgressBar : EZControlBase
{
    // ── Value ─────────────────────────────────────────────────────────────────
    private int _minimum = 0;
    private int _maximum = 100;
    private int _value = 0;

    // ── Colors ────────────────────────────────────────────────────────────────
    private Color _progressColor = EZDefaultColors.Primary;
    private Color _progressColor2 = EZDefaultColors.PrimaryFocus;
    private Color _trackColor = Color.FromArgb(230, 230, 230);
    private Color _textColor = Color.White;
    private bool _useGradient = true;

    // ── Border ────────────────────────────────────────────────────────────────
    private Color _borderColor = Color.Transparent;
    private int _borderSize = 0;
    private int _borderRadius = 10;

    // ── Layout ────────────────────────────────────────────────────────────────
    private Orientation _orientation = Orientation.Horizontal;

    // ── Label ─────────────────────────────────────────────────────────────────
    private bool _showPercentage = false;

    // ── Indeterminate (shimmer) ────────────────────────────────────────────────
    private bool _indeterminate = false;
    private float _shimmerOffset = 0f;
    private System.Windows.Forms.Timer? _shimmerTimer;

    // ══════════════════════════════════════════════════════════════════════════
    //  Events
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Raised when <see cref="Value"/> changes.</summary>
    [Category("EZControls")]
    public event EventHandler? ValueChanged;

    // ══════════════════════════════════════════════════════════════════════════
    //  Constructor
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Initialises the progress bar with default settings.</summary>
    public EZProgressBar()
    {
        Size = new Size(200, 22);
        BackColor = Color.Transparent;

        if (!IsInDesigner)
        {
            _shimmerTimer = new System.Windows.Forms.Timer
            {
                Interval = EZDefaults.AnimationIntervalMs
            };
            _shimmerTimer.Tick += OnShimmerTick;
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Properties – Value
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>The minimum value of the range.</summary>
    [Category("EZControls"), DefaultValue(0)]
    public int Minimum
    {
        get => _minimum;
        set
        {
            _minimum = value;
            _value = Math.Clamp(_value, _minimum, _maximum);
            Invalidate();
        }
    }

    /// <summary>The maximum value of the range.</summary>
    [Category("EZControls"), DefaultValue(100)]
    public int Maximum
    {
        get => _maximum;
        set
        {
            _maximum = Math.Max(value, _minimum);
            _value = Math.Clamp(_value, _minimum, _maximum);
            Invalidate();
        }
    }

    /// <summary>
    /// The current progress value, clamped to [<see cref="Minimum"/>, <see cref="Maximum"/>].
    /// Setting this raises <see cref="ValueChanged"/>.
    /// </summary>
    [Category("EZControls"), DefaultValue(0)]
    public int Value
    {
        get => _value;
        set
        {
            int clamped = Math.Clamp(value, _minimum, _maximum);
            if (_value == clamped) return;
            _value = clamped;
            ValueChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Properties – Visual
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Primary fill color (or first gradient stop).</summary>
    [Category("EZControls")]
    public Color ProgressColor
    {
        get => _progressColor;
        set { _progressColor = value; Invalidate(); }
    }

    /// <summary>Second gradient stop.  Used only when <see cref="UseGradient"/> is true.</summary>
    [Category("EZControls")]
    public Color ProgressColor2
    {
        get => _progressColor2;
        set { _progressColor2 = value; Invalidate(); }
    }

    /// <summary>Background track color.</summary>
    [Category("EZControls")]
    public Color TrackColor
    {
        get => _trackColor;
        set { _trackColor = value; Invalidate(); }
    }

    /// <summary>Percentage label text color.</summary>
    [Category("EZControls")]
    public Color TextColor
    {
        get => _textColor;
        set { _textColor = value; Invalidate(); }
    }

    /// <summary>When true, the fill uses a linear gradient.</summary>
    [Category("EZControls"), DefaultValue(true)]
    public bool UseGradient
    {
        get => _useGradient;
        set { _useGradient = value; Invalidate(); }
    }

    /// <summary>Corner arc radius in logical pixels.</summary>
    [Category("EZControls"), DefaultValue(10)]
    public int BorderRadius
    {
        get => _borderRadius;
        set { _borderRadius = Math.Max(0, value); Invalidate(); }
    }

    /// <summary>Optional border color around the track.</summary>
    [Category("EZControls")]
    public Color BorderColor
    {
        get => _borderColor;
        set { _borderColor = value; Invalidate(); }
    }

    /// <summary>Border width.  Set to 0 to hide the border.</summary>
    [Category("EZControls"), DefaultValue(0)]
    public int BorderSize
    {
        get => _borderSize;
        set { _borderSize = Math.Max(0, value); Invalidate(); }
    }

    /// <summary>Bar orientation.</summary>
    [Category("EZControls"), DefaultValue(Orientation.Horizontal)]
    public Orientation Orientation
    {
        get => _orientation;
        set { _orientation = value; Invalidate(); }
    }

    /// <summary>When true, the current percentage is drawn in the center of the bar.</summary>
    [Category("EZControls"), DefaultValue(false)]
    public bool ShowPercentage
    {
        get => _showPercentage;
        set { _showPercentage = value; Invalidate(); }
    }

    /// <summary>
    /// When true, the bar displays an animated shimmer indicating an
    /// indeterminate / busy state.  <see cref="Value"/> is ignored.
    /// </summary>
    [Category("EZControls"), DefaultValue(false)]
    public bool Indeterminate
    {
        get => _indeterminate;
        set
        {
            _indeterminate = value;
            if (_indeterminate)
                _shimmerTimer?.Start();
            else
                _shimmerTimer?.Stop();
            Invalidate();
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Painting
    // ══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (Width <= 0 || Height <= 0) return;

        Graphics g = e.Graphics;
        GraphicsHelper.SetHighQuality(g);

        Rectangle bounds = ClientRectangle;

        // ── 1. Track ──────────────────────────────────────────────────────
        using GraphicsPath trackPath = GraphicsHelper.CreateRoundedPath(bounds, _borderRadius);
        using SolidBrush trackBrush = new(_trackColor);
        g.FillPath(trackBrush, trackPath);

        // ── 2. Progress fill ──────────────────────────────────────────────
        Rectangle fillRect = ComputeFillRect(bounds);

        if (fillRect.Width > 0 && fillRect.Height > 0)
        {
            // Clip to the track shape so the fill respects rounded corners.
            GraphicsState savedState = g.Save();
            g.SetClip(trackPath, CombineMode.Replace);

            DrawFill(g, fillRect, bounds);

            // ── 3. Shimmer overlay ─────────────────────────────────────────
            if (_indeterminate)
                DrawShimmer(g, bounds);

            g.Restore(savedState);
        }

        // ── 4. Percentage text ────────────────────────────────────────────
        if (_showPercentage && !_indeterminate)
        {
            int pct = _maximum > _minimum
                ? (int)((_value - _minimum) * 100.0 / (_maximum - _minimum))
                : 0;
            ControlPainter.DrawText(
                g, bounds, $"{pct}%", Font, _textColor,
                ContentAlignment.MiddleCenter);
        }

        // ── 5. Border ─────────────────────────────────────────────────────
        if (_borderSize > 0 && _borderColor != Color.Transparent)
        {
            BorderStyleSettings borderStyle = new()
            {
                Enabled = true,
                Radius = _borderRadius,
                BorderSize = _borderSize,
                BorderColor = _borderColor,
                AntiAlias = true
            };
            BorderRenderer.DrawRoundedBorder(g, bounds, borderStyle);
        }
    }

    /// <summary>Computes the fill rectangle based on current value and orientation.</summary>
    private Rectangle ComputeFillRect(Rectangle bounds)
    {
        if (_maximum <= _minimum) return Rectangle.Empty;

        float ratio = (float)(_value - _minimum) / (_maximum - _minimum);

        if (_orientation == Orientation.Horizontal)
        {
            int fillW = (int)(bounds.Width * ratio);
            return new Rectangle(bounds.X, bounds.Y, fillW, bounds.Height);
        }
        else
        {
            int fillH = (int)(bounds.Height * ratio);
            return new Rectangle(
                bounds.X,
                bounds.Bottom - fillH,
                bounds.Width,
                fillH);
        }
    }

    private void DrawFill(Graphics g, Rectangle fillRect, Rectangle fullBounds)
    {
        if (_useGradient)
        {
            // Always create the gradient over the full track so the gradient
            // doesn't compress as progress increases.
            Rectangle gradRect = _orientation == Orientation.Horizontal
                ? new Rectangle(fullBounds.X, fullBounds.Y, fullBounds.Width, fullBounds.Height)
                : new Rectangle(fullBounds.X, fullBounds.Y, fullBounds.Width, fullBounds.Height);

            // Avoid degenerate brush on 0-size rect.
            if (gradRect.Width <= 0 || gradRect.Height <= 0) return;

            float angle = _orientation == Orientation.Horizontal ? 0f : 90f;

            using LinearGradientBrush grad = new(
                gradRect, _progressColor, _progressColor2, angle);

            g.FillRectangle(grad, fillRect);
        }
        else
        {
            using SolidBrush fill = new(_progressColor);
            g.FillRectangle(fill, fillRect);
        }
    }

    /// <summary>
    /// Draws a semi-transparent moving highlight band for indeterminate mode.
    /// </summary>
    private void DrawShimmer(Graphics g, Rectangle bounds)
    {
        int shimmerWidth = bounds.Width / 3;
        float x = bounds.X + _shimmerOffset * bounds.Width - shimmerWidth / 2f;

        RectangleF shimmerRect = new(x, bounds.Y, shimmerWidth, bounds.Height);

        // Gradient: transparent → white → transparent
        using LinearGradientBrush shimmer = new(
            new PointF(shimmerRect.Left, 0),
            new PointF(shimmerRect.Right, 0),
            Color.Transparent, Color.Transparent);

        ColorBlend blend = new(3);
        blend.Colors = new[] { Color.Transparent, Color.FromArgb(60, Color.White), Color.Transparent };
        blend.Positions = new[] { 0f, 0.5f, 1f };
        shimmer.InterpolationColors = blend;

        g.FillRectangle(shimmer, shimmerRect);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Shimmer animation
    // ══════════════════════════════════════════════════════════════════════════

    private void OnShimmerTick(object? sender, EventArgs e)
    {
        if (!IsHandleCreated || IsDisposed || !Visible) { _shimmerTimer?.Stop(); return; }

        _shimmerOffset += 0.015f;   // moves across the bar in ~1.1 s at 60 fps
        if (_shimmerOffset > 1.3f) _shimmerOffset = -0.3f;

        Invalidate();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Lifecycle
    // ══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);
        if (!Visible) _shimmerTimer?.Stop();
    }

    /// <inheritdoc/>
    protected override void OnHandleDestroyed(EventArgs e)
    {
        _shimmerTimer?.Stop();
        base.OnHandleDestroyed(e);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Theming
    // ══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public override void ApplyTheme(EZTheme theme)
    {
        Font = theme.DefaultFont;
        _progressColor = theme.PrimaryColor;
        _progressColor2 = theme.SecondaryColor;
        _trackColor = theme.SurfaceHoverColor;
        _textColor = theme.BackgroundColor;
        Invalidate();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Dispose
    // ══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _shimmerTimer?.Stop();
            _shimmerTimer?.Dispose();
        }
        base.Dispose(disposing);
    }
}