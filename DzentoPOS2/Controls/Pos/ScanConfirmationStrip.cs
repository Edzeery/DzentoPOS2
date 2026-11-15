using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Controls.Pos;

/// <summary>
/// Transient confirmation shown after a barcode scan or a quick add. It hides
/// itself after a short delay so the cashier never has to dismiss it. The
/// control reads no services and starts no timer on a design surface.
/// </summary>
public partial class ScanConfirmationStrip : UserControl, IThemeAware
{
    private Color _fill = SystemColors.ControlLight;
    private Color _accentColor = SystemColors.Highlight;
    private Color _borderColor = SystemColors.ControlDark;

    public ScanConfirmationStrip()
    {
        InitializeComponent();
        DoubleBuffered = true;

        if (DesignTime.IsRuntime(this))
            ApplyTheme();
    }

    /// <summary>
    /// Fills the strip and shows it. <paramref name="durationMs"/> is clamped
    /// so a mis-set value cannot make the strip flash or stay forever.
    /// </summary>
    public void ShowConfirmation(string title, string? detail = null, int durationMs = 3000)
    {
        if (IsDisposed) return;
        if (DesignTime.IsActive(this)) return;

        _title.Text = title ?? string.Empty;
        _detail.Text = detail ?? string.Empty;
        _detail.Visible = !string.IsNullOrWhiteSpace(detail);

        _hideTimer.Stop();
        _hideTimer.Interval = Math.Clamp(durationMs, 500, 30_000);

        Visible = true;
        BringToFront();
        _hideTimer.Start();
    }

    public void HideConfirmation()
    {
        _hideTimer.Stop();

        if (!IsDisposed)
            Visible = false;
    }

    public void ApplyTheme()
    {
        if (IsDisposed) return;

        _fill = PosDrawing.Blend(AppColors.Surface, AppColors.Success, 0.10f);
        _accentColor = AppColors.Success;
        _borderColor = PosDrawing.Blend(AppColors.Border, AppColors.Success, 0.25f);

        BackColor = _fill;
        _accent.BackColor = _accentColor;
        _icon.ForeColor = _accentColor;
        _title.ForeColor = AppColors.Ink;
        _detail.ForeColor = AppColors.InkMuted;

        Invalidate();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        // The designer must not subscribe to runtime events or start timers.
        if (DesignTime.IsActive(this))
            return;

        ThemeManager.ThemeChanged += OnThemeChanged;
        ApplyTheme();
    }

    protected override void OnRightToLeftChanged(EventArgs e)
    {
        base.OnRightToLeftChanged(e);
        ApplyTextAlignment();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
            return;

        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using var pen = new Pen(_borderColor);
        var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
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

    private void ApplyTextAlignment()
    {
        bool rtl = RightToLeft == RightToLeft.Yes;
        _title.TextAlign = rtl ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
        _detail.TextAlign = rtl ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
    }

    private void OnHideTimerTick(object? sender, EventArgs e) => HideConfirmation();
}
