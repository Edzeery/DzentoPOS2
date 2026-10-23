using EZControls.Core.Constants;
using EZControls.Core.Drawing;
using EZControls.Core.Enums;
using EZControls.Core.Interfaces;
using EZControls.Core.Themes;

namespace EZControls.Core.Base;

// ══════════════════════════════════════════════════════════════════════════════
//  EZInputBase
//
//  Improvements over the original:
//
//  1. UpdateTextBoxBounds early-exit:
//     The original had the null / IsDisposed check AFTER using InnerTextBox.
//     Guard checks now appear at the very top of every method that touches it.
//
//  2. OnPaint SolidBrush disposal:
//     The original called `new SolidBrush(BackColor)` inside FillPath without
//     a using statement, leaking the GDI object on every paint cycle.
//
//  3. GraphicsPath disposal in UpdateTextBoxBounds:
//     The original created a GraphicsPath for the region but did not dispose
//     it after assigning to InnerTextBox.Region (Region takes ownership of the
//     path, but the path itself must still be disposed by the caller).
//
//  4. SetPlaceholder / RemovePlaceholder:
//     These were not called when Text was set programmatically, leading to
//     the placeholder remaining visible after Text = "something".  The Text
//     property setter now correctly drives the placeholder state machine.
//
//  5. IEZValidatable:
//     Validation state is now surfaced via the interface so external components
//     (EZErrorProvider) can work with any input control polymorphically.
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Abstract base class for EZ text-input controls (<c>EZTextBox</c>, etc.).
/// Manages an inner <see cref="TextBox"/> with placeholder support, validation
/// state, border painting, and theme integration.
/// </summary>
public abstract class EZInputBase : EZControlBase, IEZValidatable
{
    // ── Inner control ─────────────────────────────────────────────────────────

    /// <summary>The embedded native TextBox.  Subclasses may read but must not dispose.</summary>
    protected TextBox? InnerTextBox;

    // ── Placeholder ───────────────────────────────────────────────────────────

    private string _placeholderText = string.Empty;
    private Color _placeholderColor = EZDefaultColors.Placeholder;
    private bool _isPlaceholderActive;

    // ── Password / multiline ──────────────────────────────────────────────────

    private bool _passwordChar;

    // ── Border ────────────────────────────────────────────────────────────────

    private Color _borderColor = EZDefaultColors.Border;
    private Color _borderFocusColor = EZDefaultColors.PrimaryFocus;
    private Color _borderErrorColor = EZDefaultColors.Error;
    // Expose BorderFocusColor with corrected backing field name. 
    private int _borderSize = EZDefaults.DefaultBorderSize;
    private int _borderRadius = EZDefaults.DefaultBorderRadius;
    private bool _underlinedStyle;

    // ── Validation ────────────────────────────────────────────────────────────

    private bool _hasError;
    private string _errorMessage = string.Empty;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Initialises the embedded TextBox, wires event handlers, and sets
    /// default layout values.
    /// </summary>
    protected EZInputBase()
    {
        SetStyle(ControlStyles.ContainerControl, true);

        Size = new Size(250, 38);
        Padding = new Padding(10, 6, 10, 6);
        BackColor = Color.White;
        ForeColor = EZDefaultColors.Text;

        InnerTextBox = new TextBox
        {
            BorderStyle = BorderStyle.None,
            BackColor = BackColor,
            ForeColor = ForeColor,
            Font = EZDefaults.DefaultFont,
            Location = new Point(10, 10),
            Width = Width - 20
        };

        Controls.Add(InnerTextBox);

        // Wire inner TextBox events to outer control events / state machine.
        InnerTextBox.TextChanged += (_, e) => OnTextChanged(e);

        InnerTextBox.Enter += (_, _) =>
        {
            State = EZControlState.Focused;
            RemovePlaceholder();
            Invalidate();
        };

        InnerTextBox.Leave += (_, _) =>
        {
            State = EZControlState.Normal;
            SetPlaceholder();
            Invalidate();
        };

        if (!IsInDesigner)
            SetPlaceholder();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Properties
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// The current text value.  Returns <see cref="string.Empty"/> when the
    /// placeholder is active so callers never receive the placeholder string.
    /// </summary>
    [Category("EZControls")]
    [Browsable(true)]
    [System.Diagnostics.CodeAnalysis.AllowNull]
    public override string Text
    {
        get => _isPlaceholderActive ? string.Empty : InnerTextBox?.Text ?? string.Empty;
        set
        {
            if (InnerTextBox is null) return;
            RemovePlaceholder();
            InnerTextBox.Text = value ?? string.Empty;
            SetPlaceholder();
        }
    }

    /// <summary>Gray hint text displayed when <see cref="Text"/> is empty.</summary>
    [Category("EZControls"), DefaultValue("")]
    public string PlaceholderText
    {
        get => _placeholderText;
        set
        {
            _placeholderText = value;
            SetPlaceholder();
            Invalidate();
        }
    }

    /// <summary>Color of the placeholder text.</summary>
    [Category("EZControls")]
    public Color PlaceholderColor
    {
        get => _placeholderColor;
        set
        {
            _placeholderColor = value;
            if (_isPlaceholderActive && InnerTextBox is not null)
                InnerTextBox.ForeColor = value;
            Invalidate();
        }
    }

    /// <summary>When <see langword="true"/>, input characters are masked (password mode).</summary>
    [Category("EZControls"), DefaultValue(false)]
    public bool PasswordChar
    {
        get => _passwordChar;
        set
        {
            _passwordChar = value;
            if (InnerTextBox is not null && !_isPlaceholderActive)
                InnerTextBox.UseSystemPasswordChar = value;
        }
    }

    /// <summary>Enables multi-line text entry.</summary>
    [Category("EZControls"), DefaultValue(false)]
    public bool Multiline
    {
        get => InnerTextBox?.Multiline ?? false;
        set
        {
            if (InnerTextBox is null) return;
            InnerTextBox.Multiline = value;
            UpdateTextBoxBounds();
            UpdateControlHeight();
        }
    }

    /// <summary>Horizontal text alignment within the inner TextBox.</summary>
    [Category("EZControls"), DefaultValue(HorizontalAlignment.Left)]
    public HorizontalAlignment TextAlign
    {
        get => InnerTextBox?.TextAlign ?? HorizontalAlignment.Left;
        set { if (InnerTextBox is not null) InnerTextBox.TextAlign = value; }
    }

    // ── Border properties ─────────────────────────────────────────────────────

    /// <summary>Border color in the <c>Normal</c> state.</summary>
    [Category("EZControls")]
    public Color BorderColor
    {
        get => _borderColor;
        set { _borderColor = value; Invalidate(); }
    }

    /// <summary>Border color when the control holds focus.</summary>
    [Category("EZControls")]
    public Color BorderFocusColor
    {
        get => _borderFocusColor;
        set { _borderFocusColor = value; Invalidate(); }
    }

    /// <summary>Border color when <see cref="HasError"/> is <see langword="true"/>.</summary>
    [Category("EZControls")]
    public Color BorderErrorColor
    {
        get => _borderErrorColor;
        set { _borderErrorColor = value; Invalidate(); }
    }

    /// <summary>Border pen width in logical pixels.</summary>
    [Category("EZControls"), DefaultValue(1)]
    public int BorderSize
    {
        get => _borderSize;
        set { _borderSize = Math.Max(1, value); Invalidate(); }
    }

    /// <summary>Corner arc radius in logical pixels.</summary>
    [Category("EZControls"), DefaultValue(8)]
    public int BorderRadius
    {
        get => _borderRadius;
        set { _borderRadius = Math.Max(0, value); Invalidate(); }
    }

    /// <summary>
    /// When <see langword="true"/>, only a bottom-edge underline is drawn
    /// (Material Design style).
    /// </summary>
    [Category("EZControls"), DefaultValue(false)]
    public bool UnderlinedStyle
    {
        get => _underlinedStyle;
        set { _underlinedStyle = value; Invalidate(); }
    }

    // ── IEZValidatable ────────────────────────────────────────────────────────

    /// <inheritdoc/>
    [Browsable(false)]
    public bool HasError => _hasError;

    /// <inheritdoc/>
    [Browsable(false)]
    public string ErrorMessage => _errorMessage;



    /// <inheritdoc/>
    public virtual void SetError(string message)
    {
        _hasError = true;
        _errorMessage = message;
        Invalidate();
    }

    /// <inheritdoc/>
    public virtual void ClearError()
    {
        _hasError = false;
        _errorMessage = string.Empty;
        Invalidate();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Painting
    // ══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics g = e.Graphics;
        GraphicsHelper.SetHighQuality(g);

        Rectangle surface = ClientRectangle;
        Rectangle border = Rectangle.Inflate(surface, -1, -1);

        // Determine active border color based on priority: error > focus > normal.
        Color activeBorderColor = _hasError ? _borderErrorColor
                                : State == EZControlState.Focused ? _borderFocusColor
                                : _borderColor;

        if (_underlinedStyle)
        {
            // Fill the background flat, then draw only the bottom line.
            using SolidBrush bg = new(BackColor);
            g.FillRectangle(bg, surface);

            BorderRenderer.DrawUnderlineBorder(g, surface, activeBorderColor, _borderSize);
        }
        else
        {
            // Rounded background + parent-edge mask + rounded border.
            using GraphicsPath surfacePath = GraphicsHelper.CreateRoundedPath(surface, _borderRadius);
            using GraphicsPath borderPath = GraphicsHelper.CreateRoundedPath(border, Math.Max(0, _borderRadius - 1));

            // Fill rounded background.
            using SolidBrush bg = new(BackColor);
            g.FillPath(bg, surfacePath);

            // Mask the 1 px outer edge with the parent color so the rounded
            // corners blend correctly over any parent background.
            Color parentColor = Parent?.BackColor ?? BackColor;
            using Pen maskPen = new(parentColor, 2);
            g.DrawPath(maskPen, surfacePath);

            // Draw the rounded border.
            if (_borderSize >= 1)
            {
                using Pen borderPen = new(activeBorderColor, _borderSize);
                g.DrawPath(borderPen, borderPath);
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Layout
    // ══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        UpdateTextBoxBounds();
    }

    /// <inheritdoc/>
    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);
        if (InnerTextBox is null) return;
        InnerTextBox.Font = Font;
        UpdateControlHeight();
    }

    /// <inheritdoc/>
    protected override void OnBackColorChanged(EventArgs e)
    {
        base.OnBackColorChanged(e);
        if (InnerTextBox is not null)
            InnerTextBox.BackColor = BackColor;
    }

    /// <inheritdoc/>
    protected override void OnForeColorChanged(EventArgs e)
    {
        base.OnForeColorChanged(e);
        if (InnerTextBox is not null && !_isPlaceholderActive)
            InnerTextBox.ForeColor = ForeColor;
    }

    /// <inheritdoc/>
    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        if (InnerTextBox is null) return;
        InnerTextBox.Font = Font;
        UpdateTextBoxBounds();
        UpdateControlHeight();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Theming
    // ══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public override void ApplyTheme(EZTheme theme)
    {
        Font = theme.DefaultFont;
        BackColor = theme.BackgroundColor;
        ForeColor = theme.TextColor;
        _placeholderColor = theme.PlaceholderColor;
        _borderColor = theme.BorderColor;
        _borderFocusColor = theme.FocusedColor;
        _borderErrorColor = theme.ErrorColor;

        if (InnerTextBox is not null)
        {
            InnerTextBox.BackColor = BackColor;
            InnerTextBox.ForeColor = _isPlaceholderActive ? _placeholderColor : ForeColor;
        }

        Invalidate();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Private helpers
    // ══════════════════════════════════════════════════════════════════════════

    private void UpdateTextBoxBounds()
    {
        if (InnerTextBox is null || IsDisposed || Width <= 0 || Height <= 0)
            return;

        int padding = _borderRadius > 15 ? 12 : 10;

        InnerTextBox.Location = new Point(padding, padding - 1);
        InnerTextBox.Width = Math.Max(1, Width - padding * 2);

        if (InnerTextBox.Multiline)
            InnerTextBox.Height = Math.Max(1, Height - padding * 2);

        // Clip the TextBox to a rounded rectangle so its default white
        // background doesn't paint over the control's rounded corners.
        if (_borderRadius > 1)
        {
            using GraphicsPath path = GraphicsHelper.CreateRoundedPath(
                InnerTextBox.ClientRectangle,
                Math.Max(0, _borderRadius - 4));

            InnerTextBox.Region?.Dispose();
            InnerTextBox.Region = new Region(path);
            // path is disposed here; Region has already copied the data it needs.
        }
    }

    private void UpdateControlHeight()
    {
        if (InnerTextBox?.Multiline == true) return;

        int textHeight = Drawing.TextRendererHelper.MeasureLineHeight(Font) + 2;
        Height = textHeight + Padding.Top + Padding.Bottom + 10;
    }

    private void SetPlaceholder()
    {
        if (InnerTextBox is null) return;
        if (!string.IsNullOrWhiteSpace(InnerTextBox.Text)) return;
        if (string.IsNullOrWhiteSpace(_placeholderText)) return;

        _isPlaceholderActive = true;
        InnerTextBox.Text = _placeholderText;
        InnerTextBox.ForeColor = _placeholderColor;
        InnerTextBox.UseSystemPasswordChar = false;
    }

    private void RemovePlaceholder()
    {
        if (!_isPlaceholderActive || InnerTextBox is null) return;

        _isPlaceholderActive = false;
        InnerTextBox.Text = string.Empty;
        InnerTextBox.ForeColor = ForeColor;
        InnerTextBox.UseSystemPasswordChar = _passwordChar;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Dispose
    // ══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            InnerTextBox?.Dispose();
            InnerTextBox = null;
        }
        base.Dispose(disposing);
    }
}