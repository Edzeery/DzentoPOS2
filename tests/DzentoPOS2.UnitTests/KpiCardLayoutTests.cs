using System.Drawing;
using DzentoPOS2.Shared.Helpers;
using Xunit;

namespace DzentoPOS2.UnitTests;

public sealed class KpiCardLayoutTests
{
    // 200-420 px wide, LTR/RTL, three title lengths, with/without trend and
    // with/without the estimated-cost warning sign.
    public static IEnumerable<object[]> Matrix()
    {
        int[] widths = [200, 240, 300, 360, 420];
        bool[] directions = [false, true];
        int[] titleWidths = [30, 90, 220];
        bool[] trends = [false, true];
        bool[] warnings = [false, true];

        foreach (var width in widths)
        foreach (var rtl in directions)
        foreach (var titleWidth in titleWidths)
        foreach (var hasTrend in trends)
        foreach (var hasWarning in warnings)
            yield return [width, rtl, titleWidth, hasTrend, hasWarning];
    }

    private static KpiCardMetrics Metrics(int titleWidth, bool hasTrend, bool hasWarning, bool hasCaption = false)
        => KpiCardMetrics.Default with
        {
            TitleWidth = titleWidth,
            HasTrend = hasTrend,
            TrendWidth = hasTrend ? 56 : 0,
            HasEstimated = hasWarning,
            HasCaption = hasCaption
        };

    private static Rectangle[] NonEmpty(KpiCardLayoutResult layout)
        => layout.NonEmpty.Where(r => r.Width > 0 && r.Height > 0).ToArray();

    [Theory]
    [MemberData(nameof(Matrix))]
    public void Rects_stay_inside_the_card(int width, bool rtl, int titleWidth, bool hasTrend, bool hasWarning)
    {
        var size = new Size(width, 150);
        var layout = KpiCardLayout.ComputeLayout(size, rtl, Metrics(titleWidth, hasTrend, hasWarning));

        foreach (var rect in NonEmpty(layout))
        {
            Assert.True(rect.Left >= 0, $"left {rect.Left} < 0 ({width}, rtl={rtl})");
            Assert.True(rect.Top >= 0, $"top {rect.Top} < 0 ({width}, rtl={rtl})");
            Assert.True(rect.Right <= size.Width, $"right {rect.Right} > {size.Width} ({width}, rtl={rtl})");
            Assert.True(rect.Bottom <= size.Height, $"bottom {rect.Bottom} > {size.Height} ({width}, rtl={rtl})");
        }
    }

    [Theory]
    [MemberData(nameof(Matrix))]
    public void Rects_never_overlap(int width, bool rtl, int titleWidth, bool hasTrend, bool hasWarning)
    {
        var layout = KpiCardLayout.ComputeLayout(
            new Size(width, 150),
            rtl,
            Metrics(titleWidth, hasTrend, hasWarning));

        var rects = NonEmpty(layout);

        for (var i = 0; i < rects.Length; i++)
        for (var j = i + 1; j < rects.Length; j++)
        {
            Assert.False(
                rects[i].IntersectsWith(rects[j]),
                $"Overlap at width={width}, rtl={rtl}, title={titleWidth}, " +
                $"trend={hasTrend}, warning={hasWarning}: {rects[i]} / {rects[j]}");
        }
    }

    [Theory]
    [InlineData(200, false)]
    [InlineData(200, true)]
    [InlineData(240, false)]
    [InlineData(240, true)]
    [InlineData(300, false)]
    [InlineData(300, true)]
    [InlineData(420, false)]
    [InlineData(420, true)]
    public void Value_is_always_below_the_icon(int width, bool rtl)
    {
        var layout = KpiCardLayout.ComputeLayout(
            new Size(width, 150),
            rtl,
            Metrics(90, true, true));

        Assert.False(layout.Value.IsEmpty);
        Assert.False(layout.Icon.IsEmpty);
        Assert.True(layout.Value.Top >= layout.Icon.Bottom);
        Assert.False(layout.Value.IntersectsWith(layout.Icon));
    }

    [Fact]
    public void Value_does_not_overlap_the_icon_even_at_minimum_height()
    {
        var layout = KpiCardLayout.ComputeLayout(
            new Size(200, 120),
            false,
            Metrics(220, true, true));

        Assert.False(layout.Value.IntersectsWith(layout.Icon));
    }

    [Fact]
    public void Rtl_mirrors_the_leading_icon_and_value_alignment()
    {
        var metrics = Metrics(90, true, true);

        var ltr = KpiCardLayout.ComputeLayout(new Size(300, 150), false, metrics);
        var rtl = KpiCardLayout.ComputeLayout(new Size(300, 150), true, metrics);

        Assert.True(ltr.Icon.Left < 300 / 2);
        Assert.True(rtl.Icon.Left > 300 / 2);
        Assert.Equal(300 - ltr.Icon.Right, rtl.Icon.Left);
        Assert.Equal(300 - ltr.Value.Right, rtl.Value.Left);
    }

    [Fact]
    public void Narrow_card_moves_the_trend_under_the_title_instead_of_overlapping_it()
    {
        var layout = KpiCardLayout.ComputeLayout(
            new Size(200, 150),
            false,
            Metrics(220, true, false));

        Assert.False(layout.Trend.IsEmpty);
        Assert.True(layout.Trend.Top >= layout.Title.Bottom);
        Assert.False(layout.Trend.IntersectsWith(layout.Title));
        Assert.False(layout.Trend.IntersectsWith(layout.Icon));
    }

    [Fact]
    public void Wide_card_keeps_the_trend_on_the_title_line()
    {
        var layout = KpiCardLayout.ComputeLayout(
            new Size(420, 150),
            false,
            Metrics(90, true, false));

        Assert.False(layout.Trend.IsEmpty);
        Assert.Equal(layout.Title.Top, layout.Trend.Top);
        Assert.True(layout.Trend.Left >= layout.Title.Right);
    }

    [Fact]
    public void Absent_trend_and_warning_have_empty_rects()
    {
        var layout = KpiCardLayout.ComputeLayout(
            new Size(300, 150),
            false,
            Metrics(90, false, false));

        Assert.True(layout.Trend.IsEmpty);
        Assert.True(layout.Estimated.IsEmpty);
        Assert.True(layout.Caption.IsEmpty);
        Assert.False(layout.Title.IsEmpty);
        Assert.False(layout.Value.IsEmpty);
    }

    [Fact]
    public void Caption_sits_under_the_value()
    {
        var layout = KpiCardLayout.ComputeLayout(
            new Size(300, 150),
            false,
            Metrics(90, true, true, hasCaption: true));

        Assert.False(layout.Caption.IsEmpty);
        Assert.True(layout.Caption.Top >= layout.Value.Bottom);
        Assert.False(layout.Caption.IntersectsWith(layout.Value));
    }
}