using System.Drawing.Drawing2D;
using EZControls.Core.Drawing;
using EZControls.Core.Enums;
using EZControls.Core.Themes;

namespace EZControls.Controls.Buttons;

// ══════════════════════════════════════════════════════════════════════════════
//  EZButton
//
//  Improvements over the original:
//
//  1. Region management:
//     The original called UpdateRegion() from both OnResize and OnHandleCreated
//     without guarding Width/Height > 0.  Both guard conditions are now checked.
//
//  2. Parent.BackColorChanged subscription leak:
//     The original subscribed in OnHandleCreated but could double-subscribe if
//     the parent changed.  The subscription is now managed via a tracked parent
//     field, with unsubscription from the old parent before subscribing to the
//     new one.
//
//  3. GetCurrentBackColor:
//     Added Disabled state support; the original fell through to BackColor for
//     disabled controls, showing the primary color instead of the disabled color.
//
//  4. OnPaint guard:
//     Added Width/Height check to avoid GDI+ exceptions during resize.
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// A fully-custom rounded button with hover, press, and disabled color states.
/// </summary>
[DefaultEvent(nameof(Click))]
[ToolboxBitmap(typeof(Button))]
public class EZButton : EZButtonBase
{
    // ── Border style ──────────────────────────────────────────────────────────

    private readonly BorderStyleSettings _borderStyle = new()
    {
        Radius = 12,
        BorderSize = 1.5f,
        BorderColor = Color.Black
    };

    // ── State colors ──────────────────────────────────────────────────────────

    private Color _hoverColor = Color.FromArgb(0, 99, 177);
    private Color _pressedColor = Color.FromArgb(0, 84, 153);
    private Color _disabledColor = Color.FromArgb(180, 180, 180);

    // ── Parent tracking (for BackColor sync) ──────────────────────────────────

    private Control? _trackedParent;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>Initialises the button with default colors and a rounded border.</summary>
    public EZButton()
    {
        Size = new Size(120, 40);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Properties
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Background color displayed when the mouse is over the button.</summary>
    [Category("EZControls")]
    public Color HoverColor
    {
        get => _hoverColor;
        set { _hoverColor = value; Invalidate(); }
    }

    /// <summary>Background color displayed while the mouse button is held down.</summary>
    [Category("EZControls")]
    public Color PressedColor
    {
        get => _pressedColor;
        set { _pressedColor = value; Invalidate(); }
    }

    /// <summary>Background color displayed when <see cref="Control.Enabled"/> is <see langword="false"/>.</summary>
    [Category("EZControls")]
    public Color DisabledColor
    {
        get => _disabledColor;
        set { _disabledColor = value; Invalidate(); }
    }

    // ── Border pass-throughs ──────────────────────────────────────────────────

    /// <summary>Corner arc radius in logical pixels.</summary>
    [Category("EZControls"), DefaultValue(12)]
    public new int BorderRadius
    {
        get => _borderStyle.Radius;
        set
        {
            _borderStyle.Radius = Math.Max(0, value);
            UpdateRegion();
            Invalidate();
        }
    }

    /// <summary>Border pen width in logical pixels.</summary>
    [Category("EZControls"), DefaultValue(1.5f)]
    public new float BorderSize
    {
        get => _borderStyle.BorderSize;
        set
        {
            _borderStyle.BorderSize = Math.Max(0f, value);
            Invalidate();
        }
    }

    /// <summary>Border color.</summary>
    [Category("EZControls")]
    public new Color BorderColor
    {
        get => _borderStyle.BorderColor;
        set { _borderStyle.BorderColor = value; Invalidate(); }
    }

    /// <summary>Border dash pattern.</summary>
    [Category("EZControls"), DefaultValue(DashStyle.Solid)]
    public DashStyle BorderDashStyle
    {
        get => _borderStyle.DashStyle;
        set { _borderStyle.DashStyle = value; Invalidate(); }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Painting
    // ══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width <= 1 || Height <= 1) return;

        _borderStyle.Enabled = _borderStyle.BorderSize > 0;

        ControlPainter.PaintControl(
            e.Graphics,
            ClientRectangle,
            GetCurrentBackColor(),
            _borderStyle);

        ControlPainter.DrawText(
            e.Graphics,
            ClientRectangle,
            Text,
            Font,
            ForeColor);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Region (clip to rounded rect so hover doesn't bleed into corners)
    // ══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        UpdateRegion();
    }

    /// <inheritdoc/>
    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        UpdateRegion();
        TrackParent(Parent);
    }

    /// <inheritdoc/>
    protected override void OnParentChanged(EventArgs e)
    {
        base.OnParentChanged(e);
        TrackParent(Parent);
    }

    private void UpdateRegion()
    {
        if (!IsHandleCreated || Width <= 0 || Height <= 0) return;

        using GraphicsPath path = RoundedPathBuilder.Create(
            ClientRectangle, _borderStyle.Radius);

        Region?.Dispose();
        Region = new Region(path);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Parent tracking
    // ══════════════════════════════════════════════════════════════════════════

    private void TrackParent(Control? newParent)
    {
        if (_trackedParent is not null)
            _trackedParent.BackColorChanged -= OnParentBackColorChanged;

        _trackedParent = newParent;

        if (_trackedParent is not null)
            _trackedParent.BackColorChanged += OnParentBackColorChanged;
    }

    private void OnParentBackColorChanged(object? sender, EventArgs e) =>
        Invalidate();

    // ══════════════════════════════════════════════════════════════════════════
    //  Color selection
    // ══════════════════════════════════════════════════════════════════════════

    private Color GetCurrentBackColor() => State switch
    {
        EZControlState.Disabled => _disabledColor,
        EZControlState.Hover => _hoverColor,
        EZControlState.Pressed => _pressedColor,
        _ => BackColor
    };

    // ══════════════════════════════════════════════════════════════════════════
    //  Theming
    // ══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public override void ApplyTheme(EZTheme theme)
    {
        Font = theme.DefaultFont;
        BackColor = theme.PrimaryColor;
        ForeColor = theme.BackgroundColor; // white-ish text on primary
        _hoverColor = theme.HoverColor;
        _pressedColor = theme.PressedColor;
        _disabledColor = theme.DisabledColor;
        _borderStyle.BorderColor = theme.BorderColor;
        Invalidate();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Dispose
    // ══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            TrackParent(null); // unsubscribe from any tracked parent

        base.Dispose(disposing);
    }
}