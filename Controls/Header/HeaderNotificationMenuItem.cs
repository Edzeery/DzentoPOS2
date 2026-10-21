using DzentoPOS2.WinForms.Models;
using System.Globalization;

namespace DzentoPOS2.WinForms.Controls.Header;

internal sealed class HeaderNotificationMenuItem : ToolStripMenuItem
{
    public HeaderNotificationMenuItem(NotificationItem? notification, string title, string message)
    {
        Notification = notification;
        Text = title;
        Message = message;
        AutoSize = false;
        Height = 68;
        Width = 330;
        Padding = Padding.Empty;
        Margin = Padding.Empty;
        Enabled = notification is not null;
    }

    public NotificationItem? Notification { get; }

    public string Message { get; }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var isRtl = RightToLeft == RightToLeft.Yes;
        var hovered = Enabled && (Selected || Pressed);
        var content = new Rectangle(8, 4, Width - 16, Height - 8);

        using var backBrush = new SolidBrush(hovered ? HeaderColors.AccentSoft : HeaderColors.Surface);
        g.FillRectangle(backBrush, new Rectangle(Point.Empty, Size));

        if (hovered)
        {
            using var hoverPath = RoundedRect(content, 10);
            using var hoverBrush = new SolidBrush(HeaderColors.AccentSoft);
            g.FillPath(hoverBrush, hoverPath);
        }

        DrawStateGlyph(g, isRtl, content);
        DrawTexts(g, isRtl, content);
    }

    private void DrawStateGlyph(Graphics g, bool isRtl, Rectangle content)
    {
        var glyphRect = new Rectangle(
            isRtl ? Width - 40 : 16,
            content.Y + 15,
            24,
            24);

        var unread = Notification?.IsRead == false;
        var fill = unread ? HeaderColors.Accent : HeaderColors.SurfaceAlt;
        var border = unread ? HeaderColors.Accent : HeaderColors.Border;

        using var brush = new SolidBrush(fill);
        using var pen = new Pen(border);
        g.FillEllipse(brush, glyphRect);
        g.DrawEllipse(pen, glyphRect);

        if (!unread)
            return;

        using var inner = new SolidBrush(Color.White);
        g.FillEllipse(inner, glyphRect.X + 8, glyphRect.Y + 8, 8, 8);
    }

    private void DrawTexts(Graphics g, bool isRtl, Rectangle content)
    {
        var textRect = isRtl
            ? new Rectangle(16, content.Y + 7, Width - 66, 22)
            : new Rectangle(50, content.Y + 7, Width - 66, 22);

        var messageRect = new Rectangle(textRect.X, content.Y + 30, textRect.Width, 18);
        var dateRect = new Rectangle(textRect.X, content.Y + 48, textRect.Width, 15);

        var titleColor = Enabled ? HeaderColors.Ink : HeaderColors.MutedInk;
        var flags = TextFormatFlags.EndEllipsis |
            (isRtl ? TextFormatFlags.Right | TextFormatFlags.RightToLeft : TextFormatFlags.Left);

        using var titleFont = new Font("Segoe UI Semibold", 9.5F);
        using var bodyFont = new Font("Segoe UI", 8F);
        using var dateFont = new Font("Segoe UI", 7.5F);

        TextRenderer.DrawText(g, Text, titleFont, textRect, titleColor, flags);
        TextRenderer.DrawText(g, Message, bodyFont, messageRect, HeaderColors.MutedInk, flags);
        TextRenderer.DrawText(g, FormatDate(), dateFont, dateRect, HeaderColors.MutedInk, flags);
    }

    private string FormatDate()
    {
        if (Notification is null)
            return "";

        var date = Notification.Date;
        if (date.Date == DateTime.Today)
            return date.ToString("t", CultureInfo.CurrentUICulture);

        return date.ToString("d MMM", CultureInfo.CurrentUICulture);
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
