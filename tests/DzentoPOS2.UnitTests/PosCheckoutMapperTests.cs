using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.Shared.Contracts.DTOs.Sales;
using Xunit;

namespace DzentoPOS2.UnitTests;

public sealed class PosCheckoutMapperTests
{
    [Fact]
    public void ToSaleItems_KeepsVariantBatchAndSerialSelections()
    {
        var variantId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        var serialUnitId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var cart = new[]
        {
            new CreateSaleItemDto
            {
                ProductId = productId,
                VariantId = variantId,
                BatchId = batchId,
                SerialUnitId = serialUnitId,
                VariantName = "Red / 128GB",
                Quantity = 2.5m,
                Price = 1499.75m,
                Description = "line note"
            }
        };

        var result = PosCheckoutMapper.ToSaleItems(cart);

        var line = Assert.Single(result);
        Assert.Equal(productId, line.ProductId);
        Assert.Equal(variantId, line.VariantId);
        Assert.Equal(batchId, line.BatchId);
        Assert.Equal(serialUnitId, line.SerialUnitId);
        Assert.Equal("Red / 128GB", line.VariantName);
        Assert.Equal(2.5m, line.Quantity);
        Assert.Equal(1499.75m, line.Price);
        Assert.Equal("line note", line.Description);
    }

    [Fact]
    public void ToSaleItems_ReturnsAnIndependentList()
    {
        var cart = new List<CreateSaleItemDto>
        {
            new() { ProductId = Guid.NewGuid(), Quantity = 1, Price = 10 }
        };

        var result = PosCheckoutMapper.ToSaleItems(cart);
        cart.Clear();

        Assert.Single(result);
    }
}
