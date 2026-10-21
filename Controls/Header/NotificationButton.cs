using DzentoPOS2.WinForms.Events;
using DzentoPOS2.WinForms.Models;
using DzentoPOS2.WinForms.Themes;
using FontAwesome.Sharp;

namespace DzentoPOS2.WinForms.Controls.Header;

public sealed class NotificationButton : IconButton, IThemeAware
{
    private readonly ContextMenuStrip _menu = new();
    private List<NotificationItem> _notifications = new();

    public NotificationButton()
    {
        Cursor = Cursors.Hand;
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        BackColor = HeaderColors.SurfaceAlt;
        ForeColor = HeaderColors.Ink;
        IconChar = IconChar.Bell;
        IconColor = HeaderColors.Ink;
        IconFont = IconFont.Auto;
        IconSize = 20;
        Size = new Size(42, 42);
        Text = "";
        UseVisualStyleBackColor = false;
        AccessibleName = "Notifications";

        _menu.ShowImageMargin = false;
        _menu.ShowCheckMargin = false;
        _menu.BackColor = HeaderColors.Surface;
        _menu.Padding = new Padding(6);
        _menu.Renderer = new HeaderPopupRenderer();
        _menu.MinimumSize = new Size(330, 0);

        Click += (_, _) => ShowNotifications();
        MouseEnter += (_, _) => BackColor = HeaderColors.AccentSoft;
        MouseLeave += (_, _) => BackColor = HeaderColors.SurfaceAlt;

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
        ForeColor = HeaderColors.Ink;
        IconColor = HeaderColors.Ink;
        _menu.BackColor = HeaderColors.Surface;
        Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            ThemeManager.ThemeChanged -= OnThemeChanged;

        base.Dispose(disposing);
    }

    public event EventHandler<NotificationClickedEventArgs>? NotificationClicked;

    public int UnreadCount => _notifications.Count(x => !x.IsRead);

    public HeaderPopupPlacement PopupPlacement { get; set; } = HeaderPopupOptions.DefaultPlacement;

    public void SetNotifications(IEnumerable<NotificationItem> notifications)
    {
        _notifications = notifications
            .OrderByDescending(x => x.Date)
            .ToList();

        Invalidate();
    }

    protected override void OnRightToLeftChanged(EventArgs e)
    {
        base.OnRightToLeftChanged(e);
        _menu.RightToLeft = RightToLeft;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        base.OnPaint(pevent);

        pevent.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        using var borderPen = new Pen(HeaderColors.Border);
        var rect = ClientRectangle;
        rect.Width -= 1;
        rect.Height -= 1;
        using (var path = RoundedRect(rect, 12))
            pevent.Graphics.DrawPath(borderPen, path);

        if (UnreadCount <= 0)
            return;

        var badgeText = UnreadCount > 99 ? "99+" : UnreadCount.ToString();
        using var badgeBrush = new SolidBrush(HeaderColors.Danger);
        using var textBrush = new SolidBrush(Color.White);
        using var font = new Font("Segoe UI Semibold", 7.5F);

        var badgeWidth = badgeText.Length > 2 ? 22 : 18;
        var badgeX = RightToLeft == RightToLeft.Yes ? 2 : Width - badgeWidth - 2;
        var badgeRect = new Rectangle(badgeX, 4, badgeWidth, 18);
        pevent.Graphics.FillEllipse(badgeBrush, badgeRect);

        var textSize = pevent.Graphics.MeasureString(badgeText, font);
        pevent.Graphics.DrawString(
            badgeText,
            font,
            textBrush,
            badgeRect.X + (badgeRect.Width - textSize.Width) / 2F,
            badgeRect.Y + (badgeRect.Height - textSize.Height) / 2F - 0.5F);
    }

    private void ShowNotifications()
    {
        BuildMenuItems();

        _menu.Width = Math.Max(330, Width);
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

    private void BuildMenuItems()
    {
        _menu.Items.Clear();

        var summary = UnreadCount == 0 ? "All caught up" : $"{UnreadCount} unread";
        _menu.Items.Add(new HeaderPopupMenuItem("Notifications", summary)
        {
            Enabled = false,
            RightToLeft = RightToLeft
        });
        _menu.Items.Add(new HeaderPopupSeparator());

        if (_notifications.Count == 0)
        {
            _menu.Items.Add(new HeaderNotificationMenuItem(null, "No notifications", "New updates will appear here")
            {
                RightToLeft = RightToLeft
            });
            return;
        }

        foreach (var notification in _notifications.Take(8))
        {
            var menuItem = new HeaderNotificationMenuItem(
                notification,
                notification.Title,
                notification.Message)
            {
                Tag = notification,
                RightToLeft = RightToLeft
            };

            menuItem.Click += (_, _) =>
                NotificationClicked?.Invoke(this, new NotificationClickedEventArgs(notification));

            _menu.Items.Add(menuItem);
        }
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
