using System.Drawing.Drawing2D;
using DzentoPOS2.WinForms.Events;

namespace DzentoPOS2.WinForms.Controls.Header;

internal sealed class HeaderSettingsActionsMenuItem : ToolStripItem
{
    private Rectangle _settingsBounds;
    private Rectangle _upgradeBounds;
    private SettingsMenuAction? _hoveredAction;

    public HeaderSettingsActionsMenuItem(
        string settingsText,
        string upgradeText,
        bool canUpgrade,
        RightToLeft rightToLeft)
    {
        SettingsText = settingsText;
        UpgradeText = upgradeText;
        CanUpgrade = canUpgrade;
        RightToLeft = rightToLeft;
        AutoSize = false;
        Height = 52;
        Margin = new Padding(0, 4, 0, 4);
        Padding = Padding.Empty;
    }

    public string SettingsText { get; set; }

    public string UpgradeText { get; set; }

    public bool CanUpgrade { get; set; }

    public event EventHandler<SettingsMenuEventArgs>? ActionSelected;

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var bounds = new Rectangle(8, 4, Width - 16, Height - 8);
        var gap = 8;
        var buttonWidth = (bounds.Width - gap) / 2;

        if (RightToLeft == RightToLeft.Yes)
        {
            _upgradeBounds = new Rectangle(bounds.Left, bounds.Top, buttonWidth, bounds.Height);
            _settingsBounds = new Rectangle(_upgradeBounds.Right + gap, bounds.Top, buttonWidth, bounds.Height);
        }
        else
        {
            _settingsBounds = new Rectangle(bounds.Left, bounds.Top, buttonWidth, bounds.Height);
            _upgradeBounds = new Rectangle(_settingsBounds.Right + gap, bounds.Top, buttonWidth, bounds.Height);
        }

        PaintButton(g, _settingsBounds, SettingsText, SettingsMenuAction.OpenSettings, true, false);
        PaintButton(g, _upgradeBounds, UpgradeText, SettingsMenuAction.Upgrade, CanUpgrade, true);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        var hovered = HitTest(e.Location);
        if (_hoveredAction == hovered)
            return;

        _hoveredAction = hovered;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hoveredAction = null;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        var action = HitTest(e.Location);
        if (action is null)
            return;

        if (action == SettingsMenuAction.Upgrade && !CanUpgrade)
            return;

        ActionSelected?.Invoke(this, new SettingsMenuEventArgs(action.Value));
    }

    private SettingsMenuAction? HitTest(Point point)
    {
        if (_settingsBounds.Contains(point))
            return SettingsMenuAction.OpenSettings;

        if (_upgradeBounds.Contains(point))
            return SettingsMenuAction.Upgrade;

        return null;
    }

    private void PaintButton(
        Graphics g,
        Rectangle bounds,
        string text,
        SettingsMenuAction action,
        bool enabled,
        bool accent)
    {
        var isHovered = enabled && _hoveredAction == action;
        var backColor = accent
            ? (enabled ? HeaderColors.Accent : HeaderColors.SurfaceAlt)
            : (isHovered ? HeaderColors.AccentSoft : HeaderColors.SurfaceAlt);
        var foreColor = accent
            ? (enabled ? Color.White : HeaderColors.MutedInk)
            : HeaderColors.Ink;

        if (isHovered && accent)
            backColor = Color.FromArgb(13, 96, 96);

        using var path = CreateRoundRect(bounds, 9);
        using var brush = new SolidBrush(backColor);
        using var borderPen = new Pen(accent && enabled ? HeaderColors.Accent : HeaderColors.Border);

        g.FillPath(brush, path);
        g.DrawPath(borderPen, path);

        var flags = TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis |
                    TextFormatFlags.NoPrefix;

        if (RightToLeft == RightToLeft.Yes)
            flags |= TextFormatFlags.RightToLeft;

        TextRenderer.DrawText(g, text, Font, bounds, foreColor, flags);
    }

    private static GraphicsPath CreateRoundRect(Rectangle rectangle, int radius)
    {
        var diameter = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(rectangle.Left, rectangle.Top, diameter, diameter, 180, 90);
        path.AddArc(rectangle.Right - diameter, rectangle.Top, diameter, diameter, 270, 90);
        path.AddArc(rectangle.Right - diameter, rectangle.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rectangle.Left, rectangle.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}
