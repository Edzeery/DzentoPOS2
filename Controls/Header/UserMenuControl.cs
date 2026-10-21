using DzentoPOS2.Localization.Resources;
using DzentoPOS2.WinForms.Events;
using DzentoPOS2.WinForms.Models;
using DzentoPOS2.WinForms.Themes;
using FontAwesome.Sharp;

namespace DzentoPOS2.WinForms.Controls.Header;

public partial class UserMenuControl : UserControl, IThemeAware
{
    private readonly PictureBox _avatar = new();
    private readonly Label _name = new();
    private readonly Label _role = new();
    private readonly IconPictureBox _chevron = new();
    private readonly Panel _textPanel = new();
    private readonly ContextMenuStrip _menu = new();

    public UserMenuControl()
    {
        InitializeComponent();

        DoubleBuffered = true;
        BackColor = HeaderColors.Surface;
        Cursor = Cursors.Hand;
        MinimumSize = new Size(170, 42);
        Size = new Size(230, 46);
        Padding = new Padding(8, 5, 8, 5);
        AccessibleName = "User menu";

        _avatar.Dock = DockStyle.Left;
        _avatar.Width = 36;
        _avatar.SizeMode = PictureBoxSizeMode.Zoom;
        _avatar.BackColor = HeaderColors.AccentSoft;

        _chevron.Dock = DockStyle.Right;
        _chevron.Width = 18;
        _chevron.IconChar = IconChar.ChevronDown;
        _chevron.IconColor = HeaderColors.MutedInk;
        _chevron.IconFont = IconFont.Auto;
        _chevron.IconSize = 14;
        _chevron.BackColor = Color.Transparent;

        _textPanel.Dock = DockStyle.Fill;
        _textPanel.Padding = new Padding(9, 0, 4, 0);
        _textPanel.BackColor = Color.Transparent;

        _name.Dock = DockStyle.Top;
        _name.Height = 20;
        _name.Font = new Font("Segoe UI Semibold", 9.5F);
        _name.ForeColor = HeaderColors.Ink;
        _name.AutoEllipsis = true;
        _name.Text = AppStrings.User;

        _role.Dock = DockStyle.Fill;
        _role.Font = new Font("Segoe UI", 8F);
        _role.ForeColor = HeaderColors.MutedInk;
        _role.AutoEllipsis = true;
        _role.Text = AppStrings.Role;

        _textPanel.Controls.Add(_role);
        _textPanel.Controls.Add(_name);

        Controls.Add(_textPanel);
        Controls.Add(_chevron);
        Controls.Add(_avatar);

        _menu.ShowImageMargin = false;
        _menu.ShowCheckMargin = false;
        _menu.BackColor = HeaderColors.Surface;
        _menu.Padding = new Padding(6);
        _menu.Renderer = new HeaderPopupRenderer();
        _menu.MinimumSize = new Size(250, 0);
        AddMenuItem(AppStrings.Profile, AppStrings.ProfileDetails, UserMenuAction.Profile);
        AddMenuItem(AppStrings.ChangePassword, AppStrings.ChangePasswordDetails , UserMenuAction.ChangePassword);
        _menu.Items.Add(new HeaderPopupSeparator());
        AddMenuItem(AppStrings.Logout, AppStrings.LogoutDetails , UserMenuAction.Logout, isDanger: true);
         
        Click += (_, _) => ShowUserMenu();
        WireAllClickEvents(this, ShowUserMenu);

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

        BackColor = HeaderColors.Surface;
        _avatar.BackColor = HeaderColors.AccentSoft;
        _chevron.IconColor = HeaderColors.MutedInk;
        _name.ForeColor = HeaderColors.Ink;
        _role.ForeColor = HeaderColors.MutedInk;
        _menu.BackColor = HeaderColors.Surface;
        Invalidate();
    }

    public event EventHandler<UserMenuEventArgs>? MenuActionSelected;

    public HeaderPopupPlacement PopupPlacement { get; set; } = HeaderPopupOptions.DefaultPlacement;

    public void SetUser(UserInfoViewModel user)
    {
        _name.Text = string.IsNullOrWhiteSpace(user.FullName) ? user.UserName : user.FullName;
        _role.Text = user.RoleName;
        _avatar.Image = user.Avatar ?? CreateInitialsAvatar(_name.Text);
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

    private void AddMenuItem(
        string text,
        string description,
        UserMenuAction action,
        bool isDanger = false)
    {
        var item = new HeaderPopupMenuItem(text, description)
        {
            Tag = action,
            IsDanger = isDanger,
            RightToLeft = RightToLeft
        };

        item.Click += (_, _) => MenuActionSelected?.Invoke(this, new UserMenuEventArgs(action));
        _menu.Items.Add(item);
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

    private void ShowUserMenu()
    {
        _menu.Width = Math.Max(250, Width);
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

    protected override void OnRightToLeftChanged(EventArgs e)
    {
        base.OnRightToLeftChanged(e);
        ApplyDirection();
    }

    private void ApplyDirection()
    {
        var isRtl = RightToLeft == RightToLeft.Yes;

        _avatar.Dock = isRtl ? DockStyle.Right : DockStyle.Left;
        _chevron.Dock = isRtl ? DockStyle.Left : DockStyle.Right;
        _textPanel.Padding = isRtl
            ? new Padding(4, 0, 9, 0)
            : new Padding(9, 0, 4, 0);

        _name.TextAlign = isRtl ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
        _role.TextAlign = isRtl ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;

        Controls.SetChildIndex(_avatar, isRtl ? 0 : 2);
        Controls.SetChildIndex(_chevron, isRtl ? 2 : 0);

        foreach (ToolStripItem item in _menu.Items)
            item.RightToLeft = RightToLeft;
    }

    private static Image CreateInitialsAvatar(string displayName)
    {
        var initials = string.Join(
            "",
            displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Take(2)
                .Select(x => x[0]))
            .ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(initials))
            initials = "U";

        var bitmap = new Bitmap(36, 36);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        graphics.Clear(Color.Transparent);

        using var bg = new SolidBrush(HeaderColors.Accent);
        using var fg = new SolidBrush(Color.White);
        using var font = new Font("Segoe UI Semibold", 10F);

        graphics.FillEllipse(bg, 0, 0, 35, 35);
        var size = graphics.MeasureString(initials, font);
        graphics.DrawString(initials, font, fg, (36 - size.Width) / 2F, (36 - size.Height) / 2F - 1);

        return bitmap;
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
