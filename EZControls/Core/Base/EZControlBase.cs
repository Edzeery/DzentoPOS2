using EZControls.Core.Enums;
using EZControls.Core.Interfaces;
using EZControls.Core.Themes;

namespace EZControls.Core.Base;

// ══════════════════════════════════════════════════════════════════════════════
//  EZControlBase
//
//  Improvements over the original:
//
//  1. DesignMode detection:
//     Uses belt-and-braces: both the inherited DesignMode property and the
//     LicenseManager check.  DesignMode is only reliable after the control is
//     sited; LicenseManager covers the constructor window.
//
//  2. ThemeManager registration:
//     Controls register only at runtime (never in the designer) and only if
//     they implement IEZThemeable.  The manager holds a WeakReference so no
//     explicit unregistration is needed.
//
//  3. State machine:
//     State transitions are gated by Enabled so a disabled control never
//     enters Hover or Pressed states.
//
//  4. Style flags:
//     All painting-related style flags are set once in the constructor.
//     Subclasses must NOT clear AllPaintingInWmPaint or OptimizedDoubleBuffer.
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Base class for all EZ controls.
/// Provides:
/// <list type="bullet">
///   <item>Flicker-free double-buffered painting setup.</item>
///   <item>Automatic <see cref="EZControlState"/> transitions in response to
///         mouse and keyboard events.</item>
///   <item>Opt-in global theme registration.</item>
/// </list>
/// Subclasses should override <c>OnPaint</c> and call
/// <see cref="Drawing.GraphicsHelper.SetHighQuality"/> at the start.
/// </summary>
public abstract class EZControlBase : Control, IEZThemeable, IEZStateful
{
    // ── Designer detection ────────────────────────────────────────────────────

    /// <summary>
    /// <see langword="true"/> when the control is hosted in the Visual Studio designer.
    /// Suppresses timer creation, ThemeManager registration, and other
    /// runtime-only behaviours.
    /// </summary>
    protected bool IsInDesigner =>
        DesignMode ||
        LicenseManager.UsageMode == LicenseUsageMode.Designtime;

    // ── State ─────────────────────────────────────────────────────────────────

    private EZControlState _state = EZControlState.Normal;

    /// <summary>
    /// Current interaction state.  Setting this property calls
    /// <see cref="Control.Invalidate"/> automatically.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public EZControlState State
    {
        get => _state;
        protected set
        {
            if (_state == value) return;
            _state = value;
            Invalidate();
        }
    }

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Initialises painting style flags and, at runtime, registers with the
    /// global <see cref="Helpers.ThemeManager"/>.
    /// </summary>
    protected EZControlBase()
    {
        // Flicker-free painting:
        //   AllPaintingInWmPaint  – suppresses the WM_ERASEBKGND message.
        //   UserPaint             – we own all drawing.
        //   OptimizedDoubleBuffer – runtime manages the off-screen buffer.
        //   ResizeRedraw          – repaint on resize without a WM_ERASEBKGND.
        //   SupportsTransparentBackColor – correct parent compositing for
        //                          semi-transparent BackColor values.
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.SupportsTransparentBackColor,
            true);
        SetStyle(ControlStyles.Opaque, false);
        DoubleBuffered = true;

        // Register with the theme system only at runtime.
        if (!IsInDesigner && this is IEZThemeable themeable)
        {
            Helpers.ThemeManager.RegisterControl(themeable);
        }
    }

    // ── IEZThemeable ──────────────────────────────────────────────────────────

    /// <summary>
    /// Applies <paramref name="theme"/> to the control.
    /// The base implementation updates the font; subclasses should call
    /// <c>base.ApplyTheme(theme)</c> and then update their own color fields
    /// followed by <see cref="Control.Invalidate"/>.
    /// </summary>
    public virtual void ApplyTheme(EZTheme theme)
    {
        Font = theme.DefaultFont;
        Invalidate();
    }

    // ── Mouse & keyboard state transitions ────────────────────────────────────

    /// <inheritdoc/>
    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        if (Enabled)
            State = EZControlState.Focused;
    }

    /// <inheritdoc/>
    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        if (Enabled)
            State = EZControlState.Normal;
    }

    /// <inheritdoc/>
    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        if (Enabled &&
            State != EZControlState.Pressed &&
            State != EZControlState.Focused)
        {
            State = EZControlState.Hover;
        }
    }

    /// <inheritdoc/>
    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        if (Enabled && State != EZControlState.Focused)
            State = EZControlState.Normal;
    }

    /// <inheritdoc/>
    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (Enabled)
        {
            Focus();
            State = EZControlState.Pressed;
        }
    }

    /// <inheritdoc/>
    protected override void OnMouseUp(MouseEventArgs e)
    {
        // لا تكمل إذا الكنترول تم التخلص منه
        if (IsDisposed || Disposing)
            return;

        base.OnMouseUp(e);

        // ربما تم التخلص منه داخل Click event
        if (IsDisposed || Disposing || !IsHandleCreated)
            return;

        if (!Enabled)
            return;

        try
        {
            bool isMouseInside =
                ClientRectangle.Contains(PointToClient(Cursor.Position));

            State = isMouseInside
                ? EZControlState.Hover
                : EZControlState.Normal;
        }
        catch (ObjectDisposedException)
        {
            // Ignore:
            // Control was disposed during mouse-up processing.
        }
    }

    /// <inheritdoc/>
    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        State = Enabled ? EZControlState.Normal : EZControlState.Disabled;
    }
}