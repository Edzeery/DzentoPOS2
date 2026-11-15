using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Controls.Pos;

/// <summary>Quantity picked from the +/- buttons of a cart row.</summary>
public sealed class PosQuantityChangedEventArgs : EventArgs
{
    public PosQuantityChangedEventArgs(decimal quantity)
    {
        Quantity = quantity;
    }

    public decimal Quantity { get; }
}

/// <summary>
/// One cart line: product name, variant subtitle, unit price, quantity
/// stepper and line total. It raises intent events and never touches the cart
/// itself, which keeps it reusable and safe on a design surface.
/// </summary>
public partial class CartLineRow : UserControl, IThemeAware
{
    private decimal _quantity = 1;
    private decimal _unitPrice;
    private string _currency = string.Empty;
    private bool _isSelected;

    private Color _fill = SystemColors.Control;
    private Color _fillSelected = SystemColors.Highlight;
    private Color _border = SystemColors.ControlDark;

    /// <summary>Raised when the cashier changes the quantity.</summary>
    public event EventHandler<PosQuantityChangedEventArgs>? QuantityChanged;

    /// <summary>Raised when the cashier asks to remove the line.</summary>
    public event EventHandler? RemoveRequested;

    public CartLineRow()
    {
        InitializeComponent();
        DoubleBuffered = true;

        if (DesignTime.IsRuntime(this))
            ApplyTheme();

        ApplyDirection();
        ApplyCompactLayout();
    }

    /// <summary>Lowest quantity the minus button allows.</summary>
    public decimal MinimumQuantity { get; set; } = 1;

    /// <summary>Highest quantity the plus button allows.</summary>
    public decimal MaximumQuantity { get; set; } = 9999;

    public decimal Quantity => _quantity;

    public decimal LineTotal => _quantity * _unitPrice;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            UpdateFill();
        }
    }

    /// <summary>Fills the row. Call it again after any cart edit.</summary>
    public void SetLine(
        string title,
        string? subtitle,
        decimal quantity,
        decimal unitPrice,
        string? currency,
        bool canRemove = true)
    {
        if (IsDisposed) return;

        _title.Text = string.IsNullOrWhiteSpace(title) ? "-" : title;
        _subtitle.Text = subtitle ?? string.Empty;
        _subtitle.Visible = !string.IsNullOrWhiteSpace(subtitle);
        _quantity = Math.Clamp(quantity, MinimumQuantity, MaximumQuantity);
        _unitPrice = unitPrice;
        _currency = currency ?? string.Empty;

        _removeHost.Visible = canRemove;

        UpdateValues();
        AccessibleName = _title.Text;
        _toolTip.SetToolTip(this, _title.Text);
        _toolTip.SetToolTip(_title, _title.Text);
    }

    public void ApplyTheme()
    {
        if (IsDisposed) return;

        _fill = AppColors.Surface;
        _fillSelected = AppColors.PrimarySoft;
        _border = AppColors.Border;

        _title.ForeColor = AppColors.Ink;
        _subtitle.ForeColor = AppColors.InkMuted;
        _price.ForeColor = AppColors.InkMuted;
        _total.ForeColor = AppColors.Ink;
        _qty.ForeColor = AppColors.Ink;

        _minus.ApplyTheme();
        _plus.ApplyTheme();
        _remove.ApplyTheme();

        UpdateFill();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (DesignTime.IsActive(this))
            return;

        ThemeManager.ThemeChanged += OnThemeChanged;
        ApplyTheme();
    }

    protected override void OnRightToLeftChanged(EventArgs e)
    {
        base.OnRightToLeftChanged(e);
        ApplyDirection();
        ApplyTextAlignment();
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        ApplyCompactLayout();
    }

    /// <summary>
    /// At 375 px the fixed columns would leave no room for the product name,
    /// so the unit price column and the paddings shrink before anything is
    /// clipped.
    /// </summary>
    private void ApplyCompactLayout()
    {
        if (Width <= 0) return;

        bool compact = Width < 430;
        bool narrow = Width < 380;

        _price.Visible = !compact;
        _total.Width = narrow ? 74 : compact ? 82 : 88;
        _qtyHost.Width = narrow ? 84 : 96;
        _removeHost.Width = narrow ? 34 : 40;
        _textHost.Padding = new Padding(0, 2, compact ? 2 : 8, 2);

        // The stepper buttons stay 30 px tall and centred whatever height the
        // hosting list gives the row.
        int vertical = Math.Max(0, (Height - Padding.Vertical - 30) / 2);
        var stepper = new Padding(0, vertical, 0, vertical);
        _qtyHost.Padding = stepper;
        _removeHost.Padding = stepper;
    }

    /// <summary>
    /// WinForms does not mirror Dock inside this control, so the trailing
    /// columns are moved by hand when the screen is right-to-left.
    /// </summary>
    private void ApplyDirection()
    {
        DockStyle side = RightToLeft == RightToLeft.Yes ? DockStyle.Left : DockStyle.Right;
        _price.Dock = side;
        _total.Dock = side;
        _qtyHost.Dock = side;
        _removeHost.Dock = side;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
            return;

        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
        using var pen = new Pen(_isSelected ? AppColors.Primary : _border);
        using var path = PosDrawing.RoundedRect(bounds, AppStyle.RadiusMedium);
        e.Graphics.DrawPath(pen, path);
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        if (IsDisposed) return;

        if (InvokeRequired)
        {
            if (IsHandleCreated)
                BeginInvoke(new Action(ApplyTheme));
            return;
        }

        ApplyTheme();
    }

    private void UpdateFill()
    {
        BackColor = _isSelected ? _fillSelected : _fill;
        Invalidate();
    }

    private void ApplyTextAlignment()
    {
        bool rtl = RightToLeft == RightToLeft.Yes;
        _title.TextAlign = rtl ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
        _subtitle.TextAlign = rtl ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
        _total.TextAlign = rtl ? ContentAlignment.MiddleLeft : ContentAlignment.MiddleRight;
        _price.TextAlign = rtl ? ContentAlignment.MiddleLeft : ContentAlignment.MiddleRight;
    }

    private void UpdateValues()
    {
        _qty.Text = _quantity.ToString("0.##");
        _price.Text = _unitPrice.ToString("N2");
        _total.Text = LineTotal.ToString("N2");
        _toolTip.SetToolTip(_qty, $"{_quantity:0.##} × {_unitPrice:N2} {_currency}".Trim());
        _toolTip.SetToolTip(_total, $"{LineTotal:N2} {_currency}".Trim());

        _minus.Enabled = _quantity > MinimumQuantity;
        _plus.Enabled = _quantity < MaximumQuantity;
    }

    private void OnMinusClick(object? sender, EventArgs e) => Step(-1);

    private void OnPlusClick(object? sender, EventArgs e) => Step(1);

    private void Step(decimal delta)
    {
        decimal next = Math.Clamp(_quantity + delta, MinimumQuantity, MaximumQuantity);
        if (next == _quantity) return;

        _quantity = next;
        UpdateValues();
        QuantityChanged?.Invoke(this, new PosQuantityChangedEventArgs(next));
    }

    private void OnRemoveClick(object? sender, EventArgs e) => RemoveRequested?.Invoke(this, EventArgs.Empty);
}
