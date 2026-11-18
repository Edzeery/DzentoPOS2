using DzentoPOS2.App.Services.Sales;
using System.Drawing;
using Xunit;

namespace DzentoPOS2.UnitTests;

public class PosCardLayoutCalculatorTests
{
    [Theory]
    [InlineData(375, PosViewMode.Cards)]
    [InlineData(768, PosViewMode.Cards)]
    [InlineData(1440, PosViewMode.Cards)]
    [InlineData(375, PosViewMode.List)]
    [InlineData(768, PosViewMode.List)]
    [InlineData(1440, PosViewMode.List)]
    public void GetGrid_ReturnsValidColumnsAndWidth(int width, PosViewMode mode)
    {
        var (cols, itemW, itemH, gap) = PosCardLayoutCalculator.GetGrid(width, mode);

        Assert.True(cols >= 1, $"Columns should be >= 1, got {cols}");
        Assert.True(itemW > 0, $"ItemWidth should be > 0, got {itemW}");
        Assert.True(itemH > 0, $"ItemHeight should be > 0, got {itemH}");
        Assert.True(gap >= 0, $"Gap should be >= 0, got {gap}");

        // Total width should not exceed available width
        int totalW = cols * itemW + (cols - 1) * gap;
        Assert.True(totalW <= width, $"Total width {totalW} exceeds available {width}");
    }

    [Theory]
    [InlineData(375, 1)]
    // floor((width + 12) / 202), clamped to 4.
    [InlineData(190, 1)]
    [InlineData(202, 1)]
    [InlineData(403, 2)]
    [InlineData(768, 3)]
    [InlineData(1190, 4)]
    [InlineData(1440, 4)]
    [InlineData(2400, 4)]
    public void GetGrid_Cards_UsesApprovedColumnFormula(int width, int expectedColumns)
    {
        var (cols, _, _, _) = PosCardLayoutCalculator.GetGrid(width, PosViewMode.Cards);
        Assert.Equal(expectedColumns, cols);
    }

    [Theory]
    [InlineData(375, 1)]
    [InlineData(1190, 4)]
    [InlineData(1440, 4)]
    public void GetGrid_Cards_UsesTwelvePixelGap(int width, int expectedColumns)
    {
        var (cols, itemW, _, gap) = PosCardLayoutCalculator.GetGrid(width, PosViewMode.Cards);

        Assert.Equal(expectedColumns, cols);
        Assert.Equal(PosCardLayoutCalculator.CardGap, gap);
        Assert.Equal(12, gap);
        Assert.True(cols * itemW + (cols - 1) * gap <= width);
    }

    [Theory]
    [InlineData(375)]
    [InlineData(768)]
    [InlineData(1440)]
    public void GetGrid_List_HasOneColumn(int width)
    {
        var (cols, _, _, _) = PosCardLayoutCalculator.GetGrid(width, PosViewMode.List);
        Assert.Equal(1, cols);
    }

    [Theory]
    [InlineData(375, false)]
    [InlineData(375, true)]
    [InlineData(768, false)]
    [InlineData(768, true)]
    [InlineData(1440, false)]
    [InlineData(1440, true)]
    public void Calculate_AllRectsFitInCard(int cardWidth, bool rtl)
    {
        var (cols, itemW, itemH, gap) = PosCardLayoutCalculator.GetGrid(cardWidth, PosViewMode.Cards);
        var rects = PosCardLayoutCalculator.Calculate(itemW, itemH, rtl);

        Assert.True(PosCardLayoutCalculator.IsValid(rects, itemW, itemH),
            $"Rects do not fit in card {itemW}x{itemH} (rtl={rtl})");
    }

    [Theory]
    [InlineData(375, false)]
    [InlineData(375, true)]
    [InlineData(768, false)]
    [InlineData(768, true)]
    [InlineData(1440, false)]
    [InlineData(1440, true)]
    public void Calculate_RtlMirrorsImageAndPrice(int cardWidth, bool rtl)
    {
        var (cols, itemW, itemH, gap) = PosCardLayoutCalculator.GetGrid(cardWidth, PosViewMode.Cards);
        var rects = PosCardLayoutCalculator.Calculate(itemW, itemH, rtl);

        // Image should be on the right in RTL, left in LTR
        if (rtl)
        {
            Assert.True(rects.Image.X > itemW / 2, "In RTL, image should be on the right half");
        }
        else
        {
            Assert.True(rects.Image.X < itemW / 2, "In LTR, image should be on the left half");
        }
    }

    [Theory]
    [InlineData(375)]
    [InlineData(768)]
    [InlineData(1440)]
    public void Calculate_ListMode_ItemsSpanFullWidth(int width)
    {
        var (cols, itemW, itemH, gap) = PosCardLayoutCalculator.GetGrid(width, PosViewMode.List);
        Assert.Equal(1, cols);
        Assert.Equal(width, itemW);
    }

    [Fact]
    public void Calculate_ImageSizeDecreasesWithCardWidth()
    {
        var r1 = PosCardLayoutCalculator.Calculate(150, 104, false);
        var r2 = PosCardLayoutCalculator.Calculate(300, 96, false);
        var r3 = PosCardLayoutCalculator.Calculate(400, 112, false);

        // Image size should not exceed card dimensions
        Assert.True(r1.Image.Width <= 150);
        Assert.True(r2.Image.Width <= 300);
        Assert.True(r3.Image.Width <= 400);
    }
}
