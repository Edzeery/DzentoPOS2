namespace DzentoPOS2.WinForms.Controls.Header;

public enum HeaderPopupPlacement
{
    Bottom,
    Top,
    Left,
    Right
}

internal static class HeaderPopupOptions
{
    public static HeaderPopupPlacement DefaultPlacement { get; set; } = HeaderPopupPlacement.Bottom;

    public static int Gap { get; set; } = 8;
}

internal static class HeaderPopupPositioner
{
    public static Point GetScreenPoint(
        Control owner,
        ToolStripDropDown popup,
        HeaderPopupPlacement placement,
        int gap = 8)
    {
        var popupWidth = Math.Max(popup.Width, popup.MinimumSize.Width);
        var popupHeight = Math.Max(popup.Height, popup.PreferredSize.Height);
        var isRtl = owner.RightToLeft == RightToLeft.Yes;

        var x = placement switch
        {
            HeaderPopupPlacement.Left => -popupWidth - gap,
            HeaderPopupPlacement.Right => owner.Width + gap,
            _ => isRtl ? owner.Width - popupWidth : 0
        };

        var y = placement switch
        {
            HeaderPopupPlacement.Top => -popupHeight - gap,
            HeaderPopupPlacement.Left or HeaderPopupPlacement.Right => 0,
            _ => owner.Height + gap
        };

        return owner.PointToScreen(new Point(x, y));
    }
}
