using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.Shared.Contracts.DTOs.Sales;
using Xunit;

namespace DzentoPOS2.UnitTests;

public sealed class PosCartMergerTests
{
    [Fact]
    public void AddOrMerge_SameVariantProducesOneLine()
    {
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var cart = new List<CreateSaleItemDto>();

        PosCartMerger.AddOrMerge(cart, Line(productId, variantId, 1));
        PosCartMerger.AddOrMerge(cart, Line(productId, variantId, 2));

        var line = Assert.Single(cart);
        Assert.Equal(variantId, line.VariantId);
        Assert.Equal(3, line.Quantity);
    }

    [Fact]
    public void AddOrMerge_DifferentBatchesStaySeparate()
    {
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var cart = new List<CreateSaleItemDto>();

        PosCartMerger.AddOrMerge(cart, Line(productId, variantId, 1, batchId: Guid.NewGuid()));
        PosCartMerger.AddOrMerge(cart, Line(productId, variantId, 1, batchId: Guid.NewGuid()));

        Assert.Equal(2, cart.Count);
    }

    [Fact]
    public void AddOrMerge_DifferentVariantsStaySeparate()
    {
        var productId = Guid.NewGuid();
        var cart = new List<CreateSaleItemDto>();

        PosCartMerger.AddOrMerge(cart, Line(productId, Guid.NewGuid(), 1));
        PosCartMerger.AddOrMerge(cart, Line(productId, Guid.NewGuid(), 1));

        Assert.Equal(2, cart.Count);
    }

    [Fact]
    public void AddOrMerge_DifferentSerialUnitsStaySeparate()
    {
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var cart = new List<CreateSaleItemDto>();

        PosCartMerger.AddOrMerge(cart, Line(productId, variantId, 1, serialUnitId: Guid.NewGuid()));
        PosCartMerger.AddOrMerge(cart, Line(productId, variantId, 1, serialUnitId: Guid.NewGuid()));

        Assert.Equal(2, cart.Count);
    }

    private static CreateSaleItemDto Line(
        Guid productId,
        Guid variantId,
        decimal quantity,
        Guid? batchId = null,
        Guid? serialUnitId = null) => new()
        {
            ProductId = productId,
            VariantId = variantId,
            Quantity = quantity,
            Price = 10,
            BatchId = batchId ?? Guid.Empty,
            SerialUnitId = serialUnitId ?? Guid.Empty
        };
}
