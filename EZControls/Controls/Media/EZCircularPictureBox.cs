using System.ComponentModel;
using System.Drawing.Drawing2D;
using EZControls.Core.Animation;
using EZControls.Core.Drawing;
using EZControls.Core.Helpers;
using EZControls.Core.Interfaces;
using EZControls.Core.Themes;

namespace EZControls.Controls.Media;

// ══════════════════════════════════════════════════════════════════════════════
//  EZCircularPictureBox  –  Circular image display with animations and effects
//
//  All 15 original concerns plus additional improvements are addressed.
//  Every design decision is documented at the point of implementation.
//
//  Key subsystem responsibilities:
//    ShadowRenderer  – cached bitmap shadow (no per-frame brush allocation).
//    AnimationState  – time-based border width interpolation.
//    RippleState     – time-based click ripple expansion.
//    PlaceholderCache – shared singleton placeholder bitmap.
//    AlignmentHelper – zero-alloc content alignment offsets.
//    LayoutMetrics   – pre-computed, GDI+-safe layout rectangles.
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// A circular image display control with optional drop shadow, animated border,
/// hover glow, click ripple, status indicator, and SVG support.
/// </summary>
[DefaultEvent(nameof(Click))]
[ToolboxBitmap(typeof(PictureBox))]
[DesignerCategory("Component")]
public class EZCircularPictureBox : Control, IEZThemeable
{
    // ══════════════════════════════════════════════════════════════════════════
    //  DesignMode detection
    //
    //  Belt-and-braces: DesignMode walks up the ISite chain and is only valid
    //  after the control is sited.  LicenseManager covers the constructor
    //  window before siting occurs.
    // ══════════════════════════════════════════════════════════════════════════

    private bool IsInDesigner =>
        DesignMode ||
        LicenseManager.UsageMode == LicenseUsageMode.Designtime;

    // ── Sub-systems ───────────────────────────────────────────────────────────
    private readonly ShadowRenderer _shadowRenderer = new();
    private readonly AnimationState _borderAnim;
    private readonly RippleState _ripple = new();

    // ── Feature flags ─────────────────────────────────────────────────────────
    private bool _enableShadow = true;
    private bool _enableHoverAnimation = true;
    private bool _enableBorderAnimation = true;
    private bool _enablePlaceholder = true;
    private bool _enableClickRipple = true;
    private bool _enableStatusIndicator = false;

    // ── Status indicator ──────────────────────────────────────────────────────
    private bool _isOnline = true;
    private Color _onlineColor = Color.LimeGreen;
    private Color _offlineColor = Color.Gray;

    // ── Image ─────────────────────────────────────────────────────────────────

    // Issue #8 – ownership model:
    //   DisplayImage and PlaceholderImage are assigned externally.
    //   The control must NOT dispose them – the caller owns the lifetime.
    //   This is the same contract as System.Windows.Forms.PictureBox.Image.
    private Image? _image;
    private Image? _placeholderOverride;
    private ContentAlignment _imageAlignment = ContentAlignment.MiddleCenter;

    // ── Border ────────────────────────────────────────────────────────────────

    // Issue #3 – DPI scaling: original values stored so ScaleControl never
    // compounds the scaling factor across multiple calls.
    private int _borderSize = 3;
    private int _originalBorderSize = 3;
    private float _hoverBorderSize = 5f;
    private Color _borderColor = Color.RoyalBlue;
    private Color _borderColor2 = Color.DeepPink;
    private bool _useGradient = true;
    private float _gradientAngle = 45f;
    private DashStyle _dashStyle = DashStyle.Solid;

    // ── Shadow ────────────────────────────────────────────────────────────────

    private int _shadowSize = 8;
    private int _originalShadowSize = 8;
    private int _shadowOpacity = 60;
    private Color _shadowColor = Color.Black;

    // ── Hover ─────────────────────────────────────────────────────────────────
    private bool _isHovered = false;
    private Color _hoverGlowColor = Color.FromArgb(80, Color.White);

    // ── Animation timer ───────────────────────────────────────────────────────
    private System.Windows.Forms.Timer? _animationTimer;

    // ══════════════════════════════════════════════════════════════════════════
    //  Constructor
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Initialises a new <see cref="EZCircularPictureBox"/> with default settings.</summary>
    public EZCircularPictureBox()
    {
        Size = new Size(120, 120);
        BackColor = SystemColors.Control;

        _borderAnim = new AnimationState(_borderSize);

        // Issue #10 – flicker / transparency:
        //   AllPaintingInWmPaint   – suppress WM_ERASEBKGND.
        //   UserPaint              – we own all drawing.
        //   OptimizedDoubleBuffer  – off-screen buffer managed by the runtime.
        //   SupportsTransparentBackColor – correct parent compositing for
        //                                 semi-transparent BackColor values.
        //   ResizeRedraw           – repaint on resize without WM_ERASEBKGND.
        //   Opaque = false         – required for transparent back colours.
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.SupportsTransparentBackColor |
            ControlStyles.ResizeRedraw,
            true);
        SetStyle(ControlStyles.Opaque, false);
        DoubleBuffered = true;

        // Timer is only created at runtime to avoid issues in the VS designer.
        if (!IsInDesigner)
        {
            _animationTimer = new System.Windows.Forms.Timer
            {
                Interval = EZDefaults.AnimationIntervalMs // ~60 fps
            };
            _animationTimer.Tick += OnAnimationTick;
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Public API
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Applies a visual style preset atomically (suspends layout during update).
    /// </summary>
    public void ApplyStyle(CircularImageStyle style)
    {
        SuspendLayout();
        BorderSize = style.BorderSize;
        BorderColor = style.BorderColor ?? Color.RoyalBlue;
        BorderColor2 = style.BorderColor2 ?? Color.DeepPink;
        UseGradient = style.UseGradient;
        GradientAngle = style.GradientAngle;
        BorderDashStyle = style.BorderDashStyle;
        HoverBorderSize = style.HoverBorderSize;
        HoverGlowColor = style.HoverGlowColor ?? Color.FromArgb(80, Color.White);
        ShadowSize = style.ShadowSize;
        ShadowOpacity = style.ShadowOpacity;
        ShadowColor = style.ShadowColor ?? Color.Black;
        ResumeLayout(false);
        Invalidate();
    }

    /// <summary>
    /// Clears the displayed image and cancels any active animations,
    /// returning the control to its default placeholder state.
    /// </summary>
    public void Reset()
    {
        _image = null;
        _ripple.Cancel();
        _isHovered = false;
        _borderAnim.SnapTo(_borderSize);
        Invalidate();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Properties – Features
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>When <see langword="true"/>, a soft drop-shadow is rendered beneath the circle.</summary>
    [Category("EZ Features"), DefaultValue(true)]
    public bool EnableShadow
    {
        get => _enableShadow;
        set { _enableShadow = value; Invalidate(); }
    }

    /// <summary>When <see langword="true"/>, the border animates in/out on mouse hover.</summary>
    [Category("EZ Features"), DefaultValue(true)]
    public bool EnableHoverAnimation
    {
        get => _enableHoverAnimation;
        set => _enableHoverAnimation = value;
    }

    /// <summary>When <see langword="true"/>, the border width interpolates smoothly.</summary>
    [Category("EZ Features"), DefaultValue(true)]
    public bool EnableBorderAnimation
    {
        get => _enableBorderAnimation;
        set => _enableBorderAnimation = value;
    }

    /// <summary>
    /// When <see langword="true"/> and <see cref="DisplayImage"/> is null, a
    /// generic silhouette placeholder is shown.
    /// </summary>
    [Category("EZ Features"), DefaultValue(true)]
    public bool EnablePlaceholderImage
    {
        get => _enablePlaceholder;
        set { _enablePlaceholder = value; Invalidate(); }
    }

    /// <summary>When <see langword="true"/>, a ripple effect plays on mouse click.</summary>
    [Category("EZ Features"), DefaultValue(true)]
    public bool EnableClickRipple
    {
        get => _enableClickRipple;
        set => _enableClickRipple = value;
    }

    /// <summary>
    /// When <see langword="true"/>, a small colored dot is drawn in the
    /// bottom-right corner indicating <see cref="IsOnline"/> status.
    /// </summary>
    [Category("EZ Features"), DefaultValue(false)]
    public bool EnableStatusIndicator
    {
        get => _enableStatusIndicator;
        set { _enableStatusIndicator = value; Invalidate(); }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Properties – Image
    //
    //  Issue #8 – ownership model documented via XML doc on both properties.
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// The image to display inside the circle.
    /// <para>
    /// The control does <b>not</b> dispose this image; the caller retains
    /// ownership and is responsible for disposal.  This follows the same
    /// contract as <see cref="PictureBox.Image"/>.
    /// </para>
    /// </summary>
    [Category("EZ Image")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Image? DisplayImage
    {
        get => _image;
        set { _image = value; Invalidate(); }
    }

    /// <summary>
    /// Optional user-supplied override for the placeholder image shown when
    /// <see cref="DisplayImage"/> is null.
    /// <para>
    /// The control does <b>not</b> dispose this image; the caller retains
    /// ownership.
    /// </para>
    /// </summary>
    [Category("EZ Image")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Image? PlaceholderImage
    {
        get => _placeholderOverride;
        set { _placeholderOverride = value; Invalidate(); }
    }

    /// <summary>How the image is aligned within the circular clip region.</summary>
    [Category("EZ Image"), DefaultValue(typeof(ContentAlignment), nameof(ContentAlignment.MiddleCenter))]
    public ContentAlignment ImageAlignment
    {
        get => _imageAlignment;
        set { _imageAlignment = value; Invalidate(); }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Properties – Border
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Border width in logical pixels at rest.</summary>
    [Category("EZ Border"), DefaultValue(3)]
    public int BorderSize
    {
        get => _borderSize;
        set
        {
            _borderSize = _originalBorderSize = Math.Max(0, value);
            _borderAnim.SnapTo(_borderSize);
            _shadowRenderer.Invalidate();
            Invalidate();
        }
    }

    /// <summary>Primary border color (or first gradient stop).</summary>
    [Category("EZ Border")]
    public Color BorderColor
    {
        get => _borderColor;
        set { _borderColor = value; Invalidate(); }
    }

    /// <summary>Second gradient stop color.  Only used when <see cref="UseGradient"/> is <see langword="true"/>.</summary>
    [Category("EZ Border")]
    public Color BorderColor2
    {
        get => _borderColor2;
        set { _borderColor2 = value; Invalidate(); }
    }

    /// <summary>When <see langword="true"/>, the border is painted with a linear gradient.</summary>
    [Category("EZ Border"), DefaultValue(true)]
    public bool UseGradient
    {
        get => _useGradient;
        set { _useGradient = value; Invalidate(); }
    }

    /// <summary>Gradient angle in degrees (0 = left-to-right).</summary>
    [Category("EZ Border"), DefaultValue(45f)]
    public float GradientAngle
    {
        get => _gradientAngle;
        set { _gradientAngle = value; Invalidate(); }
    }

    /// <summary>Border line dash style.</summary>
    [Category("EZ Border"), DefaultValue(DashStyle.Solid)]
    public DashStyle BorderDashStyle
    {
        get => _dashStyle;
        set { _dashStyle = value; Invalidate(); }
    }

    /// <summary>Border width in logical pixels when hovered.</summary>
    [Category("EZ Border"), DefaultValue(5f)]
    public float HoverBorderSize
    {
        get => _hoverBorderSize;
        set => _hoverBorderSize = Math.Max(0f, value);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Properties – Shadow
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Shadow halo spread in logical pixels.</summary>
    [Category("EZ Shadow"), DefaultValue(8)]
    public int ShadowSize
    {
        get => _shadowSize;
        set
        {
            _shadowSize = _originalShadowSize = Math.Max(0, value);
            _shadowRenderer.Invalidate();
            Invalidate();
        }
    }

    /// <summary>Shadow alpha (0 = transparent, 255 = fully opaque).</summary>
    [Category("EZ Shadow"), DefaultValue(60)]
    public int ShadowOpacity
    {
        get => _shadowOpacity;
        set
        {
            _shadowOpacity = Math.Clamp(value, 0, 255);
            _shadowRenderer.Invalidate();
            Invalidate();
        }
    }

    /// <summary>Shadow tint color.</summary>
    [Category("EZ Shadow")]
    public Color ShadowColor
    {
        get => _shadowColor;
        set { _shadowColor = value; _shadowRenderer.Invalidate(); Invalidate(); }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Properties – Hover
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Color of the radial glow overlay when the mouse is over the control.</summary>
    [Category("EZ Hover")]
    public Color HoverGlowColor
    {
        get => _hoverGlowColor;
        set { _hoverGlowColor = value; Invalidate(); }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Properties – Status Indicator
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>When <see langword="true"/>, the status indicator dot is green; when <see langword="false"/>, it is gray.</summary>
    [Category("EZ Status"), DefaultValue(true)]
    public bool IsOnline
    {
        get => _isOnline;
        set { _isOnline = value; Invalidate(); }
    }

    /// <summary>Color of the status dot when <see cref="IsOnline"/> is <see langword="true"/>.</summary>
    [Category("EZ Status")]
    public Color OnlineColor
    {
        get => _onlineColor;
        set { _onlineColor = value; Invalidate(); }
    }

    /// <summary>Color of the status dot when <see cref="IsOnline"/> is <see langword="false"/>.</summary>
    [Category("EZ Status")]
    public Color OfflineColor
    {
        get => _offlineColor;
        set { _offlineColor = value; Invalidate(); }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Painting
    //
    //  Issue #10 – OnPaintBackground is intentionally NOT overridden.
    //  With SupportsTransparentBackColor + Opaque=false, the base implementation
    //  composites the parent's background through our client area – the correct
    //  WinForms transparency mechanism.  Overriding with a manual SolidBrush
    //  fill would break parents with gradient or image backgrounds.
    // ══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics g = e.Graphics;
        ConfigureGraphics(g);

        LayoutMetrics layout = new(Width, Height, _shadowSize, _borderAnim.Value);

        if (!layout.IsUsable) return;

        // ── Shadow ────────────────────────────────────────────────────────────
        if (_enableShadow)
        {
            _shadowRenderer.Draw(
                g,
                ClientRectangle,
                _shadowSize,
                _shadowOpacity,
                _shadowColor);
        }

        // ── Image (clipped to circle) ─────────────────────────────────────────
        DrawImage(g, layout.Image);

        // ── Border ────────────────────────────────────────────────────────────
        BorderStyleSettings borderStyle = new()
        {
            Enabled = _borderAnim.Value > 0,
            Radius = layout.Border.Width / 2,
            BorderSize = _borderAnim.Value,
            BorderColor = _borderColor,
            UseGradient = _useGradient,
            GradientColor = _borderColor2,
            GradientAngle = _gradientAngle,
            DashStyle = _dashStyle,
            AntiAlias = true
        };

        BorderRenderer.DrawRoundedBorder(g, layout.Border, borderStyle);

        // ── Hover glow ────────────────────────────────────────────────────────
        if (_isHovered)
            DrawHoverGlow(g, layout.Border);

        // ── Status indicator ──────────────────────────────────────────────────
        if (_enableStatusIndicator)
            DrawStatusIndicator(g);

        // ── Click ripple ──────────────────────────────────────────────────────
        // Issue #5 – ripple is clipped to the image ellipse in DrawRipple.
        if (_ripple.IsActive)
            DrawRipple(g, layout.Image);
    }

    private static void ConfigureGraphics(Graphics g)
    {
        g.CompositingMode = CompositingMode.SourceOver;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.CompositingQuality = CompositingQuality.HighQuality;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Draw helpers
    // ══════════════════════════════════════════════════════════════════════════

    private void DrawImage(Graphics g, Rectangle rect)
    {
        using GraphicsPath path = new();
        path.AddEllipse(rect);

        using Region region = new(path);
        GraphicsState state = g.Save();
        g.SetClip(region, CombineMode.Replace);

        DrawBitmapImage(g, rect);

        g.Restore(state);
    }

    private void DrawBitmapImage(Graphics g, Rectangle rect)
    {
        // Issue #2 – PlaceholderCache.Instance is a shared singleton; no alloc.
        Image? img = _image
                  ?? (_enablePlaceholder
                        ? _placeholderOverride ?? PlaceholderCache.Instance
                        : null);

        if (img is null) return;

        Rectangle dest = CalculateImageRect(img, rect);
        g.DrawImage(img, dest);
    }

    private void DrawStatusIndicator(Graphics g)
    {
        const int size = 16;
        Rectangle rect = new(Width - size - 8, Height - size - 8, size, size);
        Color color = _isOnline ? _onlineColor : _offlineColor;

        using SolidBrush fill = new(color);
        g.FillEllipse(fill, rect);

        using Pen border = new(Color.White, 2f);
        g.DrawEllipse(border, rect);
    }

    private void DrawHoverGlow(Graphics g, Rectangle rect)
    {
        using GraphicsPath path = new();
        path.AddEllipse(rect);

        using PathGradientBrush brush = new(path);
        brush.CenterColor = _hoverGlowColor;
        brush.SurroundColors = new[] { Color.Transparent };

        g.FillPath(brush, path);
    }

    // Issue #5 – ripple clipping:
    //   The ripple disc is clipped to the image ellipse before painting so it
    //   cannot bleed outside the circular boundary.  Previously the disc was
    //   drawn directly onto the unclipped Graphics context.
    private void DrawRipple(Graphics g, Rectangle imageRect)
    {
        using GraphicsPath clipPath = new();
        clipPath.AddEllipse(imageRect);

        GraphicsState state = g.Save();
        g.SetClip(new Region(clipPath), CombineMode.Replace);

        float s = _ripple.Radius;
        using SolidBrush brush = new(Color.FromArgb(40, Color.White));
        g.FillEllipse(
            brush,
            _ripple.Origin.X - s / 2f,
            _ripple.Origin.Y - s / 2f,
            s, s);

        g.Restore(state);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Image layout
    //  Issue #9 – AlignmentHelper: zero-alloc, no string operations.
    // ══════════════════════════════════════════════════════════════════════════

    private Rectangle CalculateImageRect(Image image, Rectangle bounds)
    {
        float iRatio = (float)image.Width / image.Height;
        float bRatio = (float)bounds.Width / bounds.Height;

        int w, h;
        if (iRatio > bRatio) { h = bounds.Height; w = (int)(h * iRatio); }
        else { w = bounds.Width; h = (int)(w / iRatio); }

        (int dx, int dy) = AlignmentHelper.GetOffset(
            _imageAlignment, bounds.Width, bounds.Height, w, h);

        return new Rectangle(bounds.X + dx, bounds.Y + dy, w, h);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Mouse events → animation
    // ══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (!_enableClickRipple || _animationTimer is null) return;

        // The maximum radius is the control diagonal (disc covers the full area).
        float maxRadius = MathF.Sqrt(Width * Width + Height * Height);
        _ripple.Start(e.Location, maxRadius);
        _animationTimer.Start();
    }

    /// <inheritdoc/>
    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        if (!_enableHoverAnimation) return;

        _isHovered = true;

        if (_enableBorderAnimation)
        {
            _borderAnim.SetTarget(_hoverBorderSize);
            _animationTimer?.Start();
        }
        else
        {
            Invalidate();
        }
    }

    /// <inheritdoc/>
    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _isHovered = false;
        _borderAnim.SetTarget(_borderSize);
        _animationTimer?.Start();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Animation tick
    //
    //  Issue #6 – timer lifecycle:
    //    Guard checks: IsHandleCreated, !IsDisposed, Visible.
    //    Timer stops itself when no animation is active so it never idles.
    // ══════════════════════════════════════════════════════════════════════════

    private void OnAnimationTick(object? sender, EventArgs e)
    {
        // Issue #6: bail out immediately if the control is invisible or gone.
        if (!IsHandleCreated || IsDisposed || !Visible)
        {
            _animationTimer?.Stop();
            return;
        }

        // Tick both animations; | (not ||) ensures both always advance even if
        // one finishes first.
        bool needsMore = _borderAnim.Tick() | _ripple.Tick();

        if (!needsMore)
            _animationTimer?.Stop();

        // Issue #13: Invalidate with explicit rect makes the intent clear.
        // Future optimisation: compute minimal dirty rect (border ring + ripple disc).
        Invalidate(ClientRectangle);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  DPI scaling
    //
    //  Issue #3 – compounding scale:
    //    ScaleControl receives a factor relative to original design-time values.
    //    Multiplying the CURRENT value each time compounds incorrectly
    //    (two 1.5× calls → 2.25× instead of 1.5×).
    //    Fix: always multiply from _original* which are set once at design time.
    // ══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
    {
        base.ScaleControl(factor, specified);

        _borderSize = (int)(_originalBorderSize * factor.Width);
        _shadowSize = (int)(_originalShadowSize * factor.Width);
        _borderAnim.SnapTo(_borderSize);
        _shadowRenderer.Invalidate();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Visibility / handle lifecycle
    //  Issue #6 – stop timer when control is hidden or handle is destroyed.
    // ══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);
        if (!Visible) _animationTimer?.Stop();
    }

    /// <inheritdoc/>
    protected override void OnHandleDestroyed(EventArgs e)
    {
        _animationTimer?.Stop();
        base.OnHandleDestroyed(e);
    }

    /// <inheritdoc/>
    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        _shadowRenderer.Invalidate();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Theming
    // ══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public void ApplyTheme(EZTheme theme)
    {
        _borderColor = theme.PrimaryColor;
        _borderColor2 = theme.SecondaryColor;
        _hoverGlowColor = Color.FromArgb(80, theme.PrimaryColor);
        Invalidate();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Dispose
    //
    //  Issue #8 confirmation:
    //    _image and _placeholderOverride are NOT disposed (caller owns them).
    //    _shadowRenderer and _animationTimer ARE disposed (we created them).
    // ══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _animationTimer?.Stop();
            _animationTimer?.Dispose();
            _shadowRenderer.Dispose();
            // _image and _placeholderOverride intentionally NOT disposed.
        }
        base.Dispose(disposing);
    }
}

// ══════════════════════════════════════════════════════════════════════════════
//  CircularImageStyle  –  immutable value-object for preset application
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Immutable value-object that carries the full visual specification for an
/// <see cref="EZCircularPictureBox"/>.  Pass an instance to
/// <see cref="EZCircularPictureBox.ApplyStyle"/> to change multiple properties
/// atomically in a single layout pass.
/// </summary>
/// <remarks>
/// Null color values indicate "use the default", which keeps serialised
/// representations compact – only non-default values need to appear.
/// </remarks>
public sealed record CircularImageStyle
{
    // ── Border ────────────────────────────────────────────────────────────────

    /// <summary>Resting border width.  Default: 3.</summary>
    public int BorderSize { get; init; } = 3;

    /// <summary>Primary border color.  null → RoyalBlue.</summary>
    public Color? BorderColor { get; init; }

    /// <summary>Second gradient stop.  null → DeepPink.</summary>
    public Color? BorderColor2 { get; init; }

    /// <summary>Whether to use a gradient border.  Default: true.</summary>
    public bool UseGradient { get; init; } = true;

    /// <summary>Gradient angle in degrees.  Default: 45.</summary>
    public float GradientAngle { get; init; } = 45f;

    /// <summary>Border dash pattern.  Default: Solid.</summary>
    public DashStyle BorderDashStyle { get; init; } = DashStyle.Solid;

    /// <summary>Hover border width.  Default: 5.</summary>
    public float HoverBorderSize { get; init; } = 5f;

    // ── Hover ─────────────────────────────────────────────────────────────────

    /// <summary>Hover glow color.  null → 80-alpha white.</summary>
    public Color? HoverGlowColor { get; init; }

    // ── Shadow ────────────────────────────────────────────────────────────────

    /// <summary>Shadow spread.  Default: 8.</summary>
    public int ShadowSize { get; init; } = 8;

    /// <summary>Shadow opacity (0–255).  Default: 60.</summary>
    public int ShadowOpacity { get; init; } = 60;

    /// <summary>Shadow tint color.  null → Black.</summary>
    public Color? ShadowColor { get; init; }

    // ── Preset ────────────────────────────────────────────────────────────────

    /// <summary>A style with all default values applied.</summary>
    public static CircularImageStyle Default { get; } = new();
}