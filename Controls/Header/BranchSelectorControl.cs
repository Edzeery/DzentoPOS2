using DzentoPOS2.Localization.Resources;
using DzentoPOS2.WinForms.Events;
using DzentoPOS2.WinForms.Models;
using DzentoPOS2.WinForms.Themes;
using FontAwesome.Sharp;

namespace DzentoPOS2.WinForms.Controls.Header;

public sealed class BranchSelectorControl : UserControl, IThemeAware
{
    private readonly IconPictureBox _icon = new();
    private readonly Label _caption = new();
    private readonly Label _branchName = new();
    private readonly IconPictureBox _chevron = new();
    private readonly Panel _textPanel = new();
    private readonly ContextMenuStrip _menu = new();
    private List<BranchViewModel> _branches = new();

    public BranchSelectorControl()
    {
        DoubleBuffered = true;
        BackColor = HeaderColors.SurfaceAlt;
        Cursor = Cursors.Hand;
        MinimumSize = new Size(190, 42);
        Size = new Size(230, 46);
        Padding = new Padding(12, 6, 10, 6);
        AccessibleName = "Branch selector";

        _icon.IconChar = IconChar.Store;
        _icon.IconColor = HeaderColors.Accent;
        _icon.IconFont = IconFont.Auto;
        _icon.IconSize = 24;
        _icon.BackColor = Color.Transparent;
        _icon.Dock = DockStyle.Left;
        _icon.Width = 28;

        _chevron.IconChar = IconChar.ChevronDown;
        _chevron.IconColor = HeaderColors.MutedInk;
        _chevron.IconFont = IconFont.Auto;
        _chevron.IconSize = 16;
        _chevron.BackColor = Color.Transparent;
        _chevron.Dock = DockStyle.Right;
        _chevron.Width = 20;

        _textPanel.Dock = DockStyle.Fill;
        _textPanel.BackColor = Color.Transparent;
        _textPanel.Padding = new Padding(8, 0, 4, 0);

        _caption.AutoSize = false;
        _caption.Dock = DockStyle.Top;
        _caption.Height = 15;
        _caption.Text = AppStrings.Branch;
        _caption.Font = new Font("Segoe UI", 7.8F);
        _caption.ForeColor = HeaderColors.MutedInk;

        _branchName.AutoSize = false;
        _branchName.Dock = DockStyle.Fill;
        _branchName.Text = AppStrings.SelectBranch;  
        _branchName.Font = new Font("Segoe UI Semibold", 9.5F);
        _branchName.ForeColor = HeaderColors.Ink;
        _branchName.TextAlign = ContentAlignment.MiddleLeft;
        _branchName.AutoEllipsis = true;

        _textPanel.Controls.Add(_branchName);
        _textPanel.Controls.Add(_caption);

        Controls.Add(_textPanel);
        Controls.Add(_chevron);
        Controls.Add(_icon);

        _menu.ShowImageMargin = false;
        _menu.ShowCheckMargin = false;
        _menu.BackColor = HeaderColors.Surface;
        _menu.Padding = new Padding(6);
        _menu.Renderer = new HeaderPopupRenderer();
        _menu.MinimumSize = new Size(260, 0);

        Click += (_, _) => ShowBranchMenu();
        WireAllClickEvents(this, ShowBranchMenu);

        ApplyDirection();

        ThemeManager.ThemeChanged += OnThemeChanged;
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        if (IsDisposed) return;
        if (InvokeRequired) { BeginInvoke(ApplyTheme); return; }
        ApplyTheme();
    }

    public void ApplyTheme()
    {
        if (IsDisposed) return;

        BackColor = HeaderColors.SurfaceAlt;
        _icon.IconColor = HeaderColors.Accent;
        _chevron.IconColor = HeaderColors.MutedInk;
        _caption.ForeColor = HeaderColors.MutedInk;
        _branchName.ForeColor = HeaderColors.Ink;
        _menu.BackColor = HeaderColors.Surface;
        Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            ThemeManager.ThemeChanged -= OnThemeChanged;

        base.Dispose(disposing);
    }

    public event EventHandler<BranchChangedEventArgs>? BranchChanged;

    public BranchViewModel? SelectedBranch { get; private set; }

    public HeaderPopupPlacement PopupPlacement { get; set; } = HeaderPopupOptions.DefaultPlacement;

    public void SetBranches(IEnumerable<BranchViewModel> branches, Guid? selectedBranchId = null)
    {
        _branches = branches.Where(x => x.IsActive).ToList();
        SelectedBranch =
            _branches.FirstOrDefault(x => x.Id == selectedBranchId) ??
            _branches.FirstOrDefault(x => x.IsMainBranch) ??
            _branches.FirstOrDefault();

        UpdateText();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        using var borderPen = new Pen(HeaderColors.Border);
        var rect = ClientRectangle;
        rect.Width -= 1;
        rect.Height -= 1;
        using var path = RoundedRect(rect, 12);
        e.Graphics.DrawPath(borderPen, path);
    }

    private static void WireAllClickEvents(Control parent, Action showMenu)
    {
        foreach (Control child in parent.Controls)
        {
            child.Click += (_, _) => showMenu();
            if (child.HasChildren)
                WireAllClickEvents(child, showMenu);
        }
    }

    private void ShowBranchMenu()
    {
        _menu.Items.Clear();

        if (_branches.Count == 0)
        {
            _menu.Items.Add(new HeaderPopupMenuItem(AppStrings.TitelNoactivebranches, AppStrings.MsgNoactivebranches)
            {
                Enabled = false
            });
        }
        else
        {
            foreach (var branch in _branches)
            {
                var description = branch.IsMainBranch
                    ? "Main branch"
                    : string.IsNullOrWhiteSpace(branch.Address)
                        ? "Active branch"
                        : branch.Address;

                var item = new HeaderPopupMenuItem(branch.Name, description)
                {
                    IsSelectedItem = branch.Id == SelectedBranch?.Id,
                    Tag = branch,
                    RightToLeft = RightToLeft
                };

                item.Click += (_, _) => SelectBranch(branch);
                _menu.Items.Add(item);
            }
        }

        _menu.Width = Math.Max(260, Width);
        _menu.RightToLeft = RightToLeft;
        foreach (ToolStripItem item in _menu.Items)
        {
            item.Width = _menu.Width - _menu.Padding.Horizontal;
            item.RightToLeft = RightToLeft;
        }

        var screenPoint = HeaderPopupPositioner.GetScreenPoint(
            this,
            _menu,
            PopupPlacement,
            HeaderPopupOptions.Gap);
        _menu.Show(screenPoint);
    }

    private void SelectBranch(BranchViewModel branch)
    {
        SelectedBranch = branch;
        UpdateText();
        BranchChanged?.Invoke(this, new BranchChangedEventArgs(branch));
    }

    private void UpdateText()
    {
        _branchName.Text = SelectedBranch?.Name ?? AppStrings.SelectBranch;
        Invalidate();
    }

    protected override void OnRightToLeftChanged(EventArgs e)
    {
        base.OnRightToLeftChanged(e);
        ApplyDirection();
    }

    private void ApplyDirection()
    {
        var isRtl = RightToLeft == RightToLeft.Yes;

        _icon.Dock = isRtl ? DockStyle.Right : DockStyle.Left;
        _chevron.Dock = isRtl ? DockStyle.Left : DockStyle.Right;
        _textPanel.Padding = isRtl
            ? new Padding(4, 0, 8, 0)
            : new Padding(8, 0, 4, 0);

        _caption.TextAlign = isRtl ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
        _branchName.TextAlign = isRtl ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;

        Controls.SetChildIndex(_icon, isRtl ? 0 : 2);
        Controls.SetChildIndex(_chevron, isRtl ? 2 : 0);

        foreach (ToolStripItem item in _menu.Items)
            item.RightToLeft = RightToLeft;
    }

    private static System.Drawing.Drawing2D.GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        var diameter = radius * 2;
        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}
