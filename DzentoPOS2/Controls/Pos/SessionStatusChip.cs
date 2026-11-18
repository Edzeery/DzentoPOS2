using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Controls.Pos;

/// <summary>State of the cash session shown by <see cref="SessionStatusChip"/>.</summary>
public enum PosSessionState
{
    Unknown = 0,
    Open = 1,
    Closed = 2
}

/// <summary>
/// Compact status pill for the cash session (drawer): a coloured dot, the
/// state text and an optional action such as "Open session". The control only
/// renders what the caller passes, so it stays safe in the designer.
/// </summary>
public partial class SessionStatusChip : UserControl, IThemeAware
{
    private Color _stateColor = SystemColors.Highlight;
    private Color _fill = SystemColors.Control;
    private Color _border = SystemColors.ControlDark;

    public SessionStatusChip()
    {
        InitializeComponent();
        DoubleBuffered = true;

        if (DesignTime.IsRuntime(this))
            ApplyTheme();
    }

    /// <summary>Raised when the cashier clicks the action ("Open session").</summary>
    public event EventHandler? ActionClicked;

    public PosSessionState State { get; private set; } = PosSessionState.Unknown;

    /// <summary>
    /// Applies a state, its localized text and an optional action label.
    /// A null or blank action hides the action button.
    /// </summary>
    public void SetStatus(PosSessionState state, string text, string? actionText = null)
    {
        if (IsDisposed) return;

        State = state;
        _text.Text = text ?? string.Empty;
        AccessibleName = _text.Text;
        _toolTip.SetToolTip(this, _text.Text);
        _toolTip.SetToolTip(_text, _text.Text);

        bool hasAction = !string.IsNullOrWhiteSpace(actionText);
        _action.Visible = hasAction;
        if (hasAction)
        {
            _action.Text = actionText!;
            _action.Width = Math.Clamp(
                TextRenderer.MeasureText(_action.Text, _action.Font).Width + 46,
                84,
                190);
        }

        ApplyTheme();
    }

    public void ApplyTheme()
    {
        if (IsDisposed) return;

        _stateColor = State switch
        {
            PosSessionState.Open => AppColors.Success,
            PosSessionState.Closed => AppColors.Warning,
            _ => AppColors.Neutral
        };

        _fill = PosDrawing.Blend(AppColors.Surface, _stateColor, 0.08f);
        _border = PosDrawing.Blend(AppColors.Border, _stateColor, 0.35f);

        BackColor = _fill;
        _dot.ForeColor = _stateColor;
        _text.ForeColor = AppColors.Ink;
        _action.ApplyTheme();

        Invalidate();
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
        _text.TextAlign = RightToLeft == RightToLeft.Yes
            ? ContentAlignment.MiddleRight
            : ContentAlignment.MiddleLeft;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
            return;

        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = PosDrawing.RoundedRect(bounds, AppStyle.RadiusLarge);
        using var pen = new Pen(_border);
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

    private void OnActionClick(object? sender, EventArgs e) => ActionClicked?.Invoke(this, EventArgs.Empty);
}
