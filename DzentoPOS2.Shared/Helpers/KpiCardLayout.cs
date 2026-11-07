using System.Drawing;

namespace DzentoPOS2.Shared.Helpers;

/// <summary>
/// Invariant, measured inputs for <see cref="KpiCardLayout"/>. The caller fills
/// the measured values (font heights, trend text width) and the layout engine
/// only does arithmetic, so the same layout can be unit tested without WinForms.
/// </summary>
public readonly record struct KpiCardMetrics
{
    /// <summary>Width of the accent bar on the leading edge.</summary>
    public int AccentWidth { get; init; }

    /// <summary>Inner padding on every side.</summary>
    public int Padding { get; init; }

    /// <summary>Side of the square icon badge.</summary>
    public int IconSize { get; init; }

    /// <summary>Gap between the icon badge and the title.</summary>
    public int IconGap { get; init; }

    /// <summary>Height of the title line.</summary>
    public int TitleHeight { get; init; }

    /// <summary>Height of the trend / warning line.</summary>
    public int TrendHeight { get; init; }

    /// <summary>Measured width of the trend text (including its leading glyph).</summary>
    public int TrendWidth { get; init; }

    /// <summary>
    /// Measured natural width of the title. When greater than zero it caps the
    /// room the title needs before the trend has to move to a second line.
    /// </summary>
    public int TitleWidth { get; init; }

    /// <summary>Square reserved for the estimated-cost warning sign.</summary>
    public int EstimatedSize { get; init; }

    /// <summary>Gap between two trailing elements on the same line.</summary>
    public int ElementGap { get; init; }

    /// <summary>Vertical gap between stacked rows.</summary>
    public int LineGap { get; init; }

    /// <summary>Preferred height of the big value line.</summary>
    public int ValueHeight { get; init; }

    /// <summary>Height of the optional caption line.</summary>
    public int CaptionHeight { get; init; }

    /// <summary>
    /// Minimum width the title must keep on the icon line. Below this the trend
    /// (and the warning sign) move to a second line instead of overlapping it.
    /// </summary>
    public int MinTitleWidth { get; init; }

    public bool HasTrend { get; init; }

    public bool HasEstimated { get; init; }

    public bool HasCaption { get; init; }

    /// <summary>Apple-style defaults shared by every KPI surface.</summary>
    public static KpiCardMetrics Default => new()
    {
        AccentWidth = 5,
        Padding = 16,
        IconSize = 40,
        IconGap = 12,
        TitleHeight = 18,
        TrendHeight = 18,
        EstimatedSize = 18,
        ElementGap = 8,
        LineGap = 8,
        ValueHeight = 40,
        CaptionHeight = 16,
        MinTitleWidth = 48
    };
}

/// <summary>Physical rectangles produced by <see cref="KpiCardLayout"/>.</summary>
public readonly record struct KpiCardLayoutResult(
    Rectangle Icon,
    Rectangle Title,
    Rectangle Trend,
    Rectangle Estimated,
    Rectangle Value,
    Rectangle Caption)
{
    /// <summary>Every non-empty rectangle, in painting order.</summary>
    public IEnumerable<Rectangle> NonEmpty
    {
        get
        {
            if (!Icon.IsEmpty) yield return Icon;
            if (!Title.IsEmpty) yield return Title;
            if (!Trend.IsEmpty) yield return Trend;
            if (!Estimated.IsEmpty) yield return Estimated;
            if (!Value.IsEmpty) yield return Value;
            if (!Caption.IsEmpty) yield return Caption;
        }
    }
}

/// <summary>
/// Pure layout engine for the owner-drawn KPI card. The base layout is built
/// left-to-right; when <c>rtl</c> is true every rectangle is mirrored
/// horizontally around the card centre so the icon moves to the right and the
/// value is aligned to the reading start on the right.
/// </summary>
public static class KpiCardLayout
{
    public static KpiCardLayoutResult ComputeLayout(Size size, bool rtl, KpiCardMetrics metrics)
    {
        int width = Math.Max(0, size.Width);
        int height = Math.Max(0, size.Height);

        int padding = Math.Max(0, metrics.Padding);
        int accent = Math.Clamp(metrics.AccentWidth, 0, width);
        int left = Math.Min(width, accent + padding);
        int right = Math.Max(left, width - padding);
        int top = Math.Min(height, padding);
        int bottom = Math.Max(top, height - padding);

        int contentWidth = Math.Max(0, right - left);
        int contentHeight = Math.Max(0, bottom - top);

        int iconSize = Math.Clamp(metrics.IconSize, 0, Math.Min(contentWidth, contentHeight));
        var icon = new Rectangle(left, top, iconSize, iconSize);

        int titleHeight = Math.Clamp(metrics.TitleHeight, 0, contentHeight);
        int lineGap = Math.Max(0, metrics.LineGap);
        int elementGap = Math.Max(0, metrics.ElementGap);

        int titleLeft = Math.Min(right, icon.Right + Math.Max(0, metrics.IconGap));
        int trendWidth = metrics.HasTrend ? Math.Clamp(metrics.TrendWidth, 0, contentWidth) : 0;
        int estimatedSize = metrics.HasEstimated ? Math.Clamp(metrics.EstimatedSize, 0, contentWidth) : 0;

        int trailing = 0;
        if (estimatedSize > 0) trailing += estimatedSize;
        if (trendWidth > 0) trailing += trendWidth;
        if (estimatedSize > 0 && trendWidth > 0) trailing += elementGap;

        int availableTitle = right - titleLeft;
        if (trailing > 0) availableTitle -= trailing + elementGap;

        bool inline = availableTitle > 0 && availableTitle >= RequiredTitleWidth(metrics);

        Rectangle title;
        Rectangle trend = Rectangle.Empty;
        Rectangle estimated = Rectangle.Empty;
        int rowsBottom;

        if (inline)
        {
            title = new Rectangle(titleLeft, top, Math.Max(0, availableTitle), titleHeight);

            int cursor = right;
            if (estimatedSize > 0)
            {
                estimated = new Rectangle(cursor - estimatedSize, top, estimatedSize, titleHeight);
                cursor = estimated.Left - elementGap;
            }

            if (trendWidth > 0)
            {
                int width2 = Math.Min(trendWidth, Math.Max(0, cursor - left));
                trend = new Rectangle(Math.Max(left, cursor - width2), top, width2, titleHeight);
            }

            rowsBottom = top + titleHeight;
        }
        else
        {
            title = new Rectangle(titleLeft, top, Math.Max(0, right - titleLeft), titleHeight);

            // The second line starts below both the title and the icon so it can
            // never overlap the badge, even on a very narrow card.
            int secondTop = Math.Max(top + titleHeight + lineGap, icon.Bottom + lineGap);
            int secondHeight = Math.Clamp(
                Math.Max(metrics.TrendHeight, metrics.EstimatedSize),
                0,
                Math.Max(0, bottom - secondTop));

            int cursor = right;
            if (trendWidth > 0)
            {
                int width2 = Math.Min(trendWidth, contentWidth);
                trend = new Rectangle(cursor - width2, secondTop, width2, secondHeight);
                cursor = trend.Left - (estimatedSize > 0 ? elementGap : 0);
            }

            if (estimatedSize > 0)
            {
                int width2 = Math.Min(estimatedSize, Math.Max(0, cursor - left));
                estimated = new Rectangle(Math.Max(left, cursor - width2), secondTop, width2, secondHeight);
            }

            rowsBottom = Math.Max(top + titleHeight, secondTop + secondHeight);
        }

        int captionHeight = metrics.HasCaption ? Math.Clamp(metrics.CaptionHeight, 0, contentHeight) : 0;
        var caption = Rectangle.Empty;
        int valueBottom = bottom;

        if (captionHeight > 0)
        {
            int captionTop = Math.Max(top, bottom - captionHeight);
            caption = new Rectangle(left, captionTop, contentWidth, Math.Max(0, bottom - captionTop));
            valueBottom = Math.Max(top, captionTop - lineGap);
        }

        int valueHeight = Math.Clamp(metrics.ValueHeight, 0, contentHeight);
        int minValueTop = Math.Min(
            valueBottom,
            Math.Max(icon.Bottom, rowsBottom) + lineGap);
        int valueTop = Math.Clamp(
            Math.Max(minValueTop, valueBottom - valueHeight),
            top,
            valueBottom);

        var value = new Rectangle(left, valueTop, contentWidth, Math.Max(0, valueBottom - valueTop));

        if (rtl)
        {
            icon = Mirror(icon, width);
            title = Mirror(title, width);
            trend = Mirror(trend, width);
            estimated = Mirror(estimated, width);
            value = Mirror(value, width);
            caption = Mirror(caption, width);
        }

        return new KpiCardLayoutResult(icon, title, trend, estimated, value, caption);
    }

    private static Rectangle Mirror(Rectangle rectangle, int cardWidth)
    {
        if (rectangle.Width <= 0 || rectangle.Height <= 0)
            return rectangle;

        return new Rectangle(cardWidth - rectangle.Right, rectangle.Y, rectangle.Width, rectangle.Height);
    }

    /// <summary>
    /// A short title may keep the trend on the same line with less room; a wide
    /// title is capped at <see cref="KpiCardMetrics.MinTitleWidth"/> so it is
    /// ellipsized instead of starving the whole row.
    /// </summary>
    private static int RequiredTitleWidth(KpiCardMetrics metrics)
    {
        int minimum = Math.Max(0, metrics.MinTitleWidth);
        if (metrics.TitleWidth <= 0) return minimum;
        return Math.Min(metrics.TitleWidth, minimum);
    }
}