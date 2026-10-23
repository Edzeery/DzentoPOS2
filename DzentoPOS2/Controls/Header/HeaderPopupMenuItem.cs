namespace DzentoPOS2.WinForms.Controls.Header;

internal sealed class HeaderPopupMenuItem : ToolStripMenuItem
{
    public HeaderPopupMenuItem(string title, string? description = null)
    {
        Text = title;
        Description = description;
        AutoSize = false;
        Height = string.IsNullOrWhiteSpace(description) ? 42 : 54;
        Width = 250;
        Padding = Padding.Empty;
        Margin = Padding.Empty;
    }

    public string? Description { get; set; }

    public bool IsDanger { get; set; }

    public bool IsSelectedItem { get; set; }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        var isRtl = RightToLeft == RightToLeft.Yes;

        var bounds = new Rectangle(Point.Empty, Size);
        var content = new Rectangle(8, 4, Width - 16, Height - 8);
        var hovered = Selected || Pressed;

        using var backBrush = new SolidBrush(hovered ? HeaderColors.AccentSoft : HeaderColors.Surface);
        g.FillRectangle(backBrush, bounds);

        if (hovered)
        {
            using var hoverPath = RoundedRect(content, 9);
            using var hoverBrush = new SolidBrush(HeaderColors.AccentSoft);
            g.FillPath(hoverBrush, hoverPath);
        }

        var markerX = isRtl ? Width - 40 : 16;
        var markerRect = new Rectangle(markerX, content.Y + (content.Height - 24) / 2, 24, 24);
        DrawMarker(g, markerRect);

        var textRect = isRtl
            ? new Rectangle(16, content.Y, Width - 66, content.Height)
            : new Rectangle(50, content.Y, Width - 66, content.Height);

        var titleColor = IsDanger ? HeaderColors.Danger : HeaderColors.Ink;
        using var titleBrush = new SolidBrush(titleColor);
        using var descBrush = new SolidBrush(HeaderColors.MutedInk);
        using var titleFont = new Font("Segoe UI Semibold", 9.5F);
        using var descFont = new Font("Segoe UI", 8F);
        var textFlags = TextFormatFlags.EndEllipsis |
            (isRtl ? TextFormatFlags.Right | TextFormatFlags.RightToLeft : TextFormatFlags.Left);

        if (string.IsNullOrWhiteSpace(Description))
        {
            TextRenderer.DrawText(
                g,
                Text,
                titleFont,
                textRect,
                titleColor,
                TextFormatFlags.VerticalCenter | textFlags);
            return;
        }

        TextRenderer.DrawText(
            g,
            Text,
            titleFont,
            new Rectangle(textRect.X, content.Y + 7, textRect.Width, 19),
            titleColor,
            textFlags);

        TextRenderer.DrawText(
            g,
            Description,
            descFont,
            new Rectangle(textRect.X, content.Y + 27, textRect.Width, 18),
            HeaderColors.MutedInk,
            textFlags);
    }

    private void DrawMarker(Graphics g, Rectangle rect)
    {
        var fill = IsDanger
            ? Color.FromArgb(253, 238, 238)
            : IsSelectedItem
                ? HeaderColors.Accent
                : HeaderColors.SurfaceAlt;

        var border = IsDanger
            ? Color.FromArgb(244, 190, 190)
            : IsSelectedItem
                ? HeaderColors.Accent
                : HeaderColors.Border;

        using var markerBrush = new SolidBrush(fill);
        using var markerPen = new Pen(border);
        using var path = RoundedRect(rect, 8);
        g.FillPath(markerBrush, path);
        g.DrawPath(markerPen, path);

        if (!IsSelectedItem)
            return;

        using var checkPen = new Pen(Color.White, 2F)
        {
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
            EndCap = System.Drawing.Drawing2D.LineCap.Round
        };

        g.DrawLines(
            checkPen,
            new[]
            {
                new Point(rect.X + 7, rect.Y + 12),
                new Point(rect.X + 11, rect.Y + 16),
                new Point(rect.X + 18, rect.Y + 8)
            });
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

internal sealed class HeaderPopupSeparator : ToolStripSeparator
{
    public HeaderPopupSeparator()
    {
        AutoSize = false;
        Height = 9;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using var pen = new Pen(HeaderColors.Border);
        e.Graphics.DrawLine(pen, 14, 4, Width - 14, 4);
    }
}

internal sealed class HeaderPopupRenderer : ToolStripProfessionalRenderer
{
    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        using var pen = new Pen(HeaderColors.Border);
        var rect = new Rectangle(Point.Empty, e.ToolStrip.Size - new Size(1, 1));
        e.Graphics.DrawRectangle(pen, rect);
    }

    protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
    {
    }
}
