using System.ComponentModel;
using System.Drawing.Drawing2D;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Controls.Pos;

/// <summary>
/// One rounded category pill. The designer only carries neutral colours;
/// <see cref="ApplyTheme"/> is the single source of colours at runtime.
/// </summary>
public class CategoryChip : Control, IThemeAware
{
    private bool _isSelected;
    private bool _hovered;
    private bool _pressed;

    // Neutral defaults keep the designer preview readable; ApplyTheme replaces
    // them with the active palette.
    private Color _fill = SystemColors.Control;
    private Color _fillHover = SystemColors.ControlLight;
    private Color _fillSelected = SystemColors.Highlight;
    private Color _border = SystemColors.ControlDark;
    private Color _text = SystemColors.ControlText;
    private Color _textSelected = SystemColors.HighlightText;

    /// <summary>Raised when the chip is clicked or activated from the keyboard.</summary>
    public event EventHandler? Selected;

    public CategoryChip()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
            | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw
            | ControlStyles.UserPaint
            | ControlStyles.Selectable
            | ControlStyles.SupportsTransparentBackColor,
            true);

        Height = 32;
        MinimumSize = new Size(52, 30);
        Padding = new Padding(14, 0, 14, 0);
        Font = new Font("Segoe UI Semibold", 9.5F);
        Cursor = Cursors.Hand;
        TabStop = true;
        AccessibleRole = AccessibleRole.PushButton;
        BackColor = Color.Transparent;

        if (DesignTime.IsRuntime(this))
            ApplyTheme();
    }

    /// <summary>Category represented by this chip; null is the "All" chip.</summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Guid? CategoryId { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            Invalidate();
        }
    }

    public override Size GetPreferredSize(Size proposedSize)
    {
        Size text = TextRenderer.MeasureText(Text ?? string.Empty, Font);
        int width = text.Width + Padding.Horizontal + 4;
        return new Size(Math.Max(MinimumSize.Width, width), Math.Max(MinimumSize.Height, Height));
    }

    public void ApplyTheme()
    {
        if (IsDisposed) return;

        _fill = AppColors.Surface;
        _fillHover = AppColors.PrimarySoft;
        _fillSelected = AppColors.Primary;
        _border = AppColors.Border;
        _text = AppColors.Ink;
        _textSelected = Color.White;
        ForeColor = _text;
        Invalidate();
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        AccessibleName = Text;

        if (DesignTime.IsRuntime(this))
            Width = GetPreferredSize(Size.Empty).Width;

        Invalidate();
    }

    protected override void OnRightToLeftChanged(EventArgs e)
    {
        base.OnRightToLeftChanged(e);
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _hovered = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hovered = false;
        _pressed = false;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left) return;

        _pressed = true;
        Focus();
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button != MouseButtons.Left || !_pressed) return;

        _pressed = false;
        Invalidate();

        if (ClientRectangle.Contains(e.Location))
            RaiseSelected();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.KeyCode is Keys.Space or Keys.Enter)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            RaiseSelected();
        }
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        Invalidate();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        Invalidate();
    }

    private void RaiseSelected() => Selected?.Invoke(this, EventArgs.Empty);

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
        using GraphicsPath path = PosDrawing.RoundedRect(bounds, AppStyle.RadiusLarge);

        Color fill = _isSelected
            ? _fillSelected
            : _hovered || _pressed ? _fillHover : _fill;

        using (var brush = new SolidBrush(fill))
            graphics.FillPath(brush, path);

        if (!_isSelected)
        {
            using var pen = new Pen(_border);
            graphics.DrawPath(pen, path);
        }

        if (Focused && ShowFocusCues)
        {
            Rectangle focus = Rectangle.Inflate(bounds, -3, -3);
            using GraphicsPath focusPath = PosDrawing.RoundedRect(focus, AppStyle.RadiusMedium);
            using var pen = new Pen(AppColors.Primary) { DashStyle = DashStyle.Dot };
            graphics.DrawPath(pen, focusPath);
        }

        TextRenderer.DrawText(
            graphics,
            Text ?? string.Empty,
            Font,
            bounds,
            _isSelected ? _textSelected : _text,
            TextFormatFlags.HorizontalCenter
            | TextFormatFlags.VerticalCenter
            | TextFormatFlags.SingleLine
            | TextFormatFlags.EndEllipsis
            | PosDrawing.Direction(this));
    }
}
