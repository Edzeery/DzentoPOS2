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
public sealed class PosCheckoutVariantTests
{
    private readonly PosDatabaseFixture _fixture;

    public PosCheckoutVariantTests(PosDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ExplicitVariantSelectionIsPersistedOnTheInvoice()
    {
        await using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();

        var (product, blueVariant) = await SeedProductWithAlternativeVariantAsync(context);

        var sale = await sales.CreateAsync(new CreateSaleDto
        {
            PaidAmount = 15,
            Items =
            {
                new CreateSaleItemDto
                {
                    ProductId = product.Id,
                    VariantId = blueVariant.Id,
                    VariantName = blueVariant.Name,
                    Quantity = 1,
                    Price = 15
                }
            }
        });

        var stored = await context.SalesInvoiceItems
            .AsNoTracking()
            .SingleAsync(x => x.SalesInvoiceId == sale.Id);

        Assert.Equal(blueVariant.Id, stored.VariantId);
        Assert.Equal(blueVariant.Name, stored.VariantName);
    }

    private static async Task<(Product Product, ProductVariant BlueVariant)> SeedProductWithAlternativeVariantAsync(
        AppDbContext context)
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
            Name = "Variant Checkout Product",
            SKU = $"VC-{Guid.NewGuid():N}",
            CategoryId = categoryId,
            UnitId = unitId,
            CostPrice = new Money(5, "DZD"),
            SalePrice = new Money(10, "DZD"),
            MinimumStock = new Quantity(0),
            TrackStock = false,
            IsActive = true
        };

        var red = new ProductVariant
        {
            ProductId = product.Id,
            Name = "Red",
            CostPrice = new Money(5, "DZD"),
            SalePrice = new Money(10, "DZD"),
            MinimumStock = new Quantity(0),
            TrackStock = false,
            IsActive = true,
            IsDefault = true
        };

        var blue = new ProductVariant
        {
            ProductId = product.Id,
            Name = "Blue",
            CostPrice = new Money(7, "DZD"),
            SalePrice = new Money(15, "DZD"),
            MinimumStock = new Quantity(0),
            TrackStock = false,
            IsActive = true,
            IsDefault = false
        };

        context.Products.Add(product);
        context.ProductVariants.AddRange(red, blue);
        await context.SaveChangesAsync();

        return (product, blue);
    }
}
