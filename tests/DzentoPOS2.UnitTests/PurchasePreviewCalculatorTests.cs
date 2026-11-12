using DzentoPOS2.Core.Services.Inventory;
using Xunit;

namespace DzentoPOS2.UnitTests;

public sealed class PurchasePreviewCalculatorTests
{
    [Fact]
    public void UsesNrvInsteadOfListPriceForExpectedProfit()
    {
        var variantId = Guid.NewGuid();
        var lines = new[]
        {
            new PurchasePreviewLineInput(
                VariantId: variantId,
                CurrentStockQty: 10,
                CurrentCostPrice: 5,
                PurchaseQty: 10,
                PurchaseCostPrice: 8,
                SalePrice: 10)
        };

        var result = PurchasePreviewCalculator.Calculate(lines, _ => 0.1m, 0.02m);

        var line = Assert.Single(result.Lines);
        Assert.Equal(6.5m, line.NewCostPrice);
        Assert.Equal(10m - 6.5m, line.UnitMargin);
        Assert.Equal(23.2m, line.ExpectedLineProfit);
        Assert.Equal(80m, result.TotalPurchaseCost);
        Assert.Equal(23.2m, result.ExpectedProfit);
        Assert.Equal(8.2m, result.InventoryImpact);
    }

    [Fact]
    public void ClampsDiscountAndFeeFactors()
    {
        var variantId = Guid.NewGuid();
        var lines = new[]
        {
            new PurchasePreviewLineInput(variantId, 0, 0, 5, 10, 20)
        };

        var result = PurchasePreviewCalculator.Calculate(lines, _ => 0.9m, 0.9m);

        var line = Assert.Single(result.Lines);
        Assert.Equal(10m, line.NewCostPrice);
        // NRV = 20 * (1 - 0.5) * (1 - 0.25) = 7.5; margin = 7.5 - 10 = -2.5
        Assert.Equal(-12.5m, line.ExpectedLineProfit);
        Assert.Equal(-12.5m, result.ExpectedProfit);
    }

    [Fact]
    public void SkipsNonPositivePurchaseQuantities()
    {
        var variantId = Guid.NewGuid();
        var lines = new[]
        {
            new PurchasePreviewLineInput(variantId, 0, 0, 0, 10, 20)
        };

        var result = PurchasePreviewCalculator.Calculate(lines, _ => 0m, 0m);

        Assert.Empty(result.Lines);
        Assert.Equal(0m, result.ExpectedProfit);
        Assert.Equal(0m, result.TotalPurchaseCost);
    }
}
