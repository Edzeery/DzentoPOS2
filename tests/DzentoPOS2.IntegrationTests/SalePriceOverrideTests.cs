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
public sealed class SalePriceOverrideTests
{
    private readonly PosDatabaseFixture _fixture;

    public SalePriceOverrideTests(PosDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task OverriddenPriceIsStoredWithTheOriginalVariantPrice()
    {
        await using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();

        var (product, variant) = await SeedAsync(context, listPrice: 25m);

        var sale = await sales.CreateAsync(new CreateSaleDto
        {
            PaidAmount = 80,
            Items =
            {
                new CreateSaleItemDto
                {
                    ProductId = product.Id,
                    VariantId = variant.Id,
                    Quantity = 2,
                    Price = 40,
                    PriceOverridden = true
                }
            }
        });

        var stored = await context.SalesInvoiceItems
            .AsNoTracking()
            .SingleAsync(x => x.SalesInvoiceId == sale.Id);

        Assert.Equal(40m, stored.Price);
        Assert.Equal(25m, stored.OriginalPrice);
        Assert.Equal(80m, stored.Total);
    }

    [Fact]
    public async Task UnflaggedPriceUsesTheVariantListPrice()
    {
        await using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();

        var (product, variant) = await SeedAsync(context, listPrice: 25m);

        var sale = await sales.CreateAsync(new CreateSaleDto
        {
            PaidAmount = 25,
            Items =
            {
                new CreateSaleItemDto
                {
                    ProductId = product.Id,
                    VariantId = variant.Id,
                    Quantity = 1,
                    Price = 999
                }
            }
        });

        var stored = await context.SalesInvoiceItems
            .AsNoTracking()
            .SingleAsync(x => x.SalesInvoiceId == sale.Id);

        Assert.Equal(25m, stored.Price);
        Assert.Null(stored.OriginalPrice);
        Assert.Equal(25m, stored.Total);
    }

    private static async Task<(Product Product, ProductVariant Variant)> SeedAsync(
        AppDbContext context,
        decimal listPrice)
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
            Name = "Override Product",
            SKU = $"OV-{Guid.NewGuid():N}",
            CategoryId = categoryId,
            UnitId = unitId,
            CostPrice = new Money(10, "DZD"),
            SalePrice = new Money(listPrice, "DZD"),
            MinimumStock = new Quantity(0),
            TrackStock = false,
            IsActive = true
        };

        var variant = new ProductVariant
        {
            ProductId = product.Id,
            Name = "Default",
            CostPrice = new Money(10, "DZD"),
            SalePrice = new Money(listPrice, "DZD"),
            MinimumStock = new Quantity(0),
            TrackStock = false,
            IsActive = true,
            IsDefault = true
        };

        context.Products.Add(product);
        context.ProductVariants.Add(variant);
        await context.SaveChangesAsync();

        return (product, variant);
    }
}
