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
public sealed class SaleFeesTests
{
    private readonly PosDatabaseFixture _fixture;

    public SaleFeesTests(PosDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task FeesAreStoredAndIncludedInTheTotalAndPayment()
    {
        await using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();

        var (product, variant) = await SeedAsync(context, listPrice: 100m);

        var sale = await sales.CreateAsync(new CreateSaleDto
        {
            TaxRate = 10,
            Fees = 5,
            PaidAmount = 115,
            Items =
            {
                new CreateSaleItemDto
                {
                    ProductId = product.Id,
                    VariantId = variant.Id,
                    Quantity = 1,
                    Price = 100
                }
            }
        });

        var stored = await context.SalesInvoices
            .AsNoTracking()
            .SingleAsync(x => x.Id == sale.Id);

        Assert.Equal(5m, stored.FeesAmount!.Amount);
        Assert.Equal(115m, stored.Total!.Amount);
        Assert.Equal(115m, stored.PaidAmount!.Amount);
        Assert.Equal(0m, stored.RemainingAmount!.Amount);
        Assert.Equal(115m, sale.Total);
        Assert.Equal(5m, sale.FeesAmount);
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
            Name = "Fees Product",
            SKU = $"FEE-{Guid.NewGuid():N}",
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
