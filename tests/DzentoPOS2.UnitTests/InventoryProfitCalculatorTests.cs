using DzentoPOS2.Core.Services.Inventory;
using Xunit;

namespace DzentoPOS2.UnitTests;

public sealed class InventoryProfitCalculatorTests
{
    [Fact]
    public void CalculatesPotentialAndRealistic()
    {
        var products = new[]
        {
            new InventoryProductInput(Guid.NewGuid(), 10, 6, true, true, Guid.Empty)
        };
        var stocks = new[] { new InventoryStockInput(products[0].VariantId, 5) };
        var r = InventoryProfitCalculator.Calculate(products, stocks, _ => 0.1m, 0.02m);
        Assert.Equal(5m * 4m, r.PotentialProfit);
        Assert.Equal(5m * (10 * 0.9m * 0.98m - 6), r.RealisticProfit);
    }

    [Fact]
    public void IgnoresNonPositiveStockAndNonTracked()
    {
        var p = new InventoryProductInput(Guid.NewGuid(), 10, 6, true, true, Guid.Empty);
        var products = new[] { p };
        var stocks = new[] { new InventoryStockInput(p.VariantId, 0) };
        var r = InventoryProfitCalculator.Calculate(products, stocks, _ => 0m, 0m);
        Assert.Equal(0m, r.PotentialProfit);
        Assert.Equal(0m, r.RealisticProfit);
    }

    [Fact]
    public void NegativeMarginNotClamped()
    {
        var p = new InventoryProductInput(Guid.NewGuid(), 5, 6, true, true, Guid.Empty);
        var products = new[] { p };
        var stocks = new[] { new InventoryStockInput(p.VariantId, 2) };
        var r = InventoryProfitCalculator.Calculate(products, stocks, _ => 0m, 0m);
        Assert.Equal(2m * (-1m), r.PotentialProfit);
        Assert.Equal(1, r.WarningNegativeMargin);
    }
}