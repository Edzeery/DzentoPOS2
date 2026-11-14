using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Core.ValueObjects.Common;
using DzentoPOS2.Core.ValueObjects.Inventory;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Shared.Contracts.DTOs.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DzentoPOS2.IntegrationTests;

[Collection(PosDatabaseCollection.Name)]
public sealed class PosBarcodeResolutionTests
{
    private readonly PosDatabaseFixture _fixture;

    public PosBarcodeResolutionTests(PosDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TwoBarcodesForOneVariantResolveToTheSameVariantAndOneCartLine()
    {
        await using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var resolver = scope.ServiceProvider.GetRequiredService<IPosBarcodeResolver>();

        var (product, variant, firstBarcode, secondBarcode) =
            await SeedProductWithTwoBarcodesAsync(context);

        var first = Assert.Single(await resolver.ResolveAsync(firstBarcode));
        var second = Assert.Single(await resolver.ResolveAsync(secondBarcode));

        Assert.Equal(variant.Id, first.VariantId);
        Assert.Equal(variant.Id, second.VariantId);
        Assert.Equal(product.Id, first.ProductId);

        var cart = new List<CreateSaleItemDto>();
        PosCartMerger.AddOrMerge(cart, ToLine(first));
        PosCartMerger.AddOrMerge(cart, ToLine(second));

        var line = Assert.Single(cart);
        Assert.Equal(variant.Id, line.VariantId);
        Assert.Equal(2, line.Quantity);
    }

    [Fact]
    public async Task UnknownBarcodeReturnsNoMatches()
    {
        await using var scope = _fixture.CreateScope();
        var resolver = scope.ServiceProvider.GetRequiredService<IPosBarcodeResolver>();

        var matches = await resolver.ResolveAsync($"missing-{Guid.NewGuid():N}");

        Assert.Empty(matches);
    }

    private static CreateSaleItemDto ToLine(PosProductItem item) => new()
    {
        ProductId = item.ProductId,
        VariantId = item.VariantId,
        VariantName = item.Name,
        Quantity = 1,
        Price = item.SalePrice
    };

    private static async Task<(Product Product, ProductVariant Variant, string First, string Second)>
        SeedProductWithTwoBarcodesAsync(AppDbContext context)
    {
        var categoryId = await context.ProductCategories
            .AsNoTracking()
            .Select(x => x.Id)
            .FirstAsync();
        var unitId = await context.ProductUnits
            .AsNoTracking()
            .Select(x => x.Id)
            .FirstAsync();

        var product = new Product
        {
            Name = "Two Barcode Product",
            SKU = $"TB-{Guid.NewGuid():N}",
            CategoryId = categoryId,
            UnitId = unitId,
            CostPrice = new Money(5, "DZD"),
            SalePrice = new Money(25, "DZD"),
            MinimumStock = new Quantity(0),
            TrackStock = false,
            IsActive = true
        };

        var variant = new ProductVariant
        {
            ProductId = product.Id,
            Name = "Default",
            CostPrice = new Money(5, "DZD"),
            SalePrice = new Money(25, "DZD"),
            MinimumStock = new Quantity(0),
            TrackStock = false,
            IsActive = true,
            IsDefault = true
        };

        var firstBarcode = $"BC1-{Guid.NewGuid():N}";
        var secondBarcode = $"BC2-{Guid.NewGuid():N}";

        context.Products.Add(product);
        context.ProductVariants.Add(variant);
        context.ProductBarcodes.AddRange(
            new ProductBarcode { VariantId = variant.Id, Barcode = firstBarcode, IsPrimary = true },
            new ProductBarcode { VariantId = variant.Id, Barcode = secondBarcode, IsPrimary = false });
        await context.SaveChangesAsync();

        return (product, variant, firstBarcode, secondBarcode);
    }
}
