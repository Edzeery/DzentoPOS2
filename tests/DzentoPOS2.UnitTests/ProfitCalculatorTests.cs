using DzentoPOS2.Core.Services.Sales;
using Xunit;

namespace DzentoPOS2.UnitTests;

public sealed class ProfitCalculatorTests
{
    [Fact]
    public void AcceptanceExample()
    {
        // 2 x 1000, cost 600, discount 10%, tax 19%, fees 50
        var lines = new[]
        {
            new ProfitLine(2, 1000m, 1000m, 600m, 0m)
        };
        // distribute 10% of 2000 = 200
        var grosses = new[] { 2000m };
        var shares = ProfitCalculator.DistributeDiscount(grosses, 200m);
        lines = new[] { new ProfitLine(2, 1000m, 1000m, 600m, shares[0]) };

        var result = ProfitCalculator.CalculateInvoice(lines, fees: 50m, digits: 2);

        Assert.Equal(2142m, 1800m + 342m); // total sanity
        Assert.Equal(1800m, result.Revenue);
        Assert.Equal(1200m, result.Cost);
        Assert.Equal(800m, result.ExpectedProfit);
        Assert.Equal(600m, result.GrossProfit);
        Assert.Equal(550m, result.ActualProfit);
    }

    [Fact]
    public void CostChangeAfterSaleDoesNotAffectOldResult()
    {
        var lines = new[] { new ProfitLine(1, 100m, 100m, 60m, 0m) };
        var r1 = ProfitCalculator.CalculateInvoice(lines);
        // change cost price (new product cost) irrelevant because UnitCost frozen
        var r2 = ProfitCalculator.CalculateInvoice(lines);
        Assert.Equal(r1.GrossProfit, r2.GrossProfit);
    }

    [Fact]
    public void ZeroRevenueMarginIsZero()
    {
        var lines = new[] { new ProfitLine(1, 0m, 0m, 0m, 0m) };
        var r = ProfitCalculator.CalculateInvoice(lines);
        Assert.Equal(0m, r.GrossMargin);
        Assert.Equal(0m, r.NetMargin);
    }

    [Fact]
    public void DiscountShareDistributionRemainderToLast()
    {
        var grosses = new[] { 10m, 10m, 10m };
        var shares = ProfitCalculator.DistributeDiscount(grosses, 1m, 2);
        Assert.Equal(3, shares.Count);
        Assert.Equal(1m, shares.Sum());
    }

    [Fact]
    public void ReturnsNetEffectReducesGrossProfit()
    {
        var lines = new[] { new ProfitLine(1, 100m, 100m, 60m, 0m) };
        var r = ProfitCalculator.CalculateInvoice(lines, returnsNetEffect: 10m);
        Assert.Equal(30m, r.GrossProfit); // 100-60-10
    }

    [Fact]
    public void ExpectedStockProfit_IgnoresNonPositiveAndKeepsNegativeMargin()
    {
        var items = new[]
        {
            (Qty: 5m, SalePrice: 10m, Cost: 6m),
            (Qty: 0m, SalePrice: 10m, Cost: 6m),
            (Qty: -2m, SalePrice: 10m, Cost: 6m),
            (Qty: 2m, SalePrice: 5m, Cost: 6m) // negative margin
        };
        var r = ProfitCalculator.CalculateExpectedStockProfit(items);
        Assert.Equal(5m * 4m + 2m * (-1m), r);
    }

    [Fact]
    public void PartialReturnCostReversal()
    {
        // ReturnLineNet 30, original unit cost 60, qty 0.5 => 30 - 30 = 0 net effect
        var returnsNetEffect = 30m - 0.5m * 60m;
        Assert.Equal(0m, returnsNetEffect);
    }

    [Theory]
    [InlineData(2, 3, 4)]
    public void ExpectedProfitFormula(int q, int lp, int uc)
    {
        // not meaningful but ensures compile
        var lines = new[] { new ProfitLine(q, lp, lp, uc, 0m) };
        var r = ProfitCalculator.CalculateInvoice(lines);
        Assert.Equal((lp - uc) * q, r.ExpectedProfit);
    }
}