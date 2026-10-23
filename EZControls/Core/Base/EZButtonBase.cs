using EZControls.Core.Enums;
using EZControls.Core.Themes;

namespace EZControls.Core.Base;

// ══════════════════════════════════════════════════════════════════════════════
//  EZButtonBase
//
//  Improvements over the original:
//
//  1. Inherits from EZControlBase instead of ButtonBase.
//     ButtonBase carries its own painting pipeline that conflicts with full
//     UserPaint.  EZControlBase is a plain Control subclass with the correct
//     style flags already set.
//
//  2. Keyboard activation (Space / Enter) correctly resets state to Hover
//     rather than leaving it stuck at Pressed.
//
//  3. ApplyTheme reads all relevant color tokens from the theme so theming
//     actually affects button appearance.
//
//  4. OnMouseUp commented-out block (from original) is removed; the base class
//     already handles the Hover/Normal transition correctly.
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Abstract base for all EZ button controls.
/// Provides state-machine input handling for mouse and keyboard,
/// and delegates visual rendering to subclass <c>OnPaint</c> overrides.
/// </summary>
public abstract class EZButtonBase : EZControlBase
{
    // ── Fields ────────────────────────────────────────────────────────────────

    private int _borderRadius = 8;
    private float _borderSize = 1f;
    private Color _borderColor = Constants.EZDefaultColors.Border;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>Initialises default size, colors, and cursor.</summary>
    protected EZButtonBase()
    {
        Size = new Size(Constants.EZDefaults.DefaultControlWidth, Constants.EZDefaults.DefaultControlHeight);
        BackColor = Color.FromArgb(0, 120, 215);
        ForeColor = Color.White;
        Font = Constants.EZDefaults.DefaultFont;
        Cursor = Cursors.Hand;

        // Prevent any base-class focus rectangle drawing.
        SetStyle(ControlStyles.Selectable, true);
    }

    // ── Properties ────────────────────────────────────────────────────────────

    /// <summary>Corner arc radius in logical pixels.  Clamped to half the control height.</summary>
    [Category("EZControls"), DefaultValue(8)]
    public int BorderRadius
    {
        get => _borderRadius;
        set
        {
            _borderRadius = Math.Max(0, value);
            Invalidate();
        }
    }

    /// <summary>Border pen width in logical pixels.</summary>
    [Category("EZControls"), DefaultValue(1f)]
    public float BorderSize
    {
        get => _borderSize;
        set
        {
            _borderSize = Math.Max(0f, value);
            Invalidate();
        }
    }

    /// <summary>Border color when the control is in its <c>Normal</c> state.</summary>
    [Category("EZControls")]
    public Color BorderColor
    {
        get => _borderColor;
        set { _borderColor = value; Invalidate(); }
    }

    /// <summary>Alias for <see cref="Control.BackColor"/>; provided for API consistency.</summary>
    [Category("EZControls")]
    public Color BackgroundColor
    {
        get => BackColor;
        set => BackColor = value;
    }

    /// <summary>Alias for <see cref="Control.ForeColor"/>; provided for API consistency.</summary>
    [Category("EZControls")]
    public Color TextColor
    {
        get => ForeColor;
        set => ForeColor = value;
    }

    // ── Input handling ────────────────────────────────────────────────────────

    /// <inheritdoc/>
    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        Focus();
    }

    /// <summary>
    /// Puts the button into <c>Pressed</c> state when Space or Enter is held.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.KeyCode is Keys.Space or Keys.Enter)
            State = EZControlState.Pressed;
    }

    /// <summary>
    /// Fires <see cref="Control.Click"/> on Space / Enter release and transitions
    /// back to <c>Hover</c> state.
    /// </summary>
    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);

        if (e.KeyCode is Keys.Space or Keys.Enter)
        {
            State = EZControlState.Hover;
            OnClick(EventArgs.Empty);
        }
    }

    // ── Resize guard ──────────────────────────────────────────────────────────

    /// <inheritdoc/>
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        // Clamp radius so arcs never overlap on small buttons.
        if (_borderRadius > Height / 2)
            _borderRadius = Height / 2;
    }

    // ── Theming ───────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override void ApplyTheme(EZTheme theme)
    {
        Font = theme.DefaultFont;
        BackColor = theme.PrimaryColor;
        ForeColor = theme.TextColor.IsLight() ? Color.Black : Color.White;
        _borderColor = theme.BorderColor;
        Invalidate();
    }
}