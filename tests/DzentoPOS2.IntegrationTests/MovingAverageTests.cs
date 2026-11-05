using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Core.ValueObjects.Common;
using DzentoPOS2.Core.ValueObjects.Inventory;
using DzentoPOS2.Data.Context;
using DzentoPOS2.App.Services.Purchases;
using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.Shared.Contracts.DTOs.Purchases;
using DzentoPOS2.Shared.Contracts.DTOs.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DzentoPOS2.IntegrationTests;

[Collection(PosDatabaseCollection.Name)]
public sealed class MovingAverageTests
{
    private readonly PosDatabaseFixture _fixture;

    public MovingAverageTests(PosDatabaseFixture fixture) => _fixture = fixture;

    private async Task<Product> CreateTrackedProductAsync(AppDbContext context, decimal initialStock, decimal initialCost)
    {
        var category = await context.ProductCategories.FirstAsync();
        var unit = await context.ProductUnits.FirstAsync();
        var product = new Product
        {
            Name = "MA-" + Guid.NewGuid().ToString("N").Substring(0, 6),
            CategoryId = category.Id,
            UnitId = unit.Id,
            CostPrice = new Money(initialCost, "DZD"),
            SalePrice = new Money(10, "DZD"),
            MinimumStock = new Quantity(0),
            TrackStock = true,
            IsActive = true
        };
        context.Products.Add(product);
        var variant = new DzentoPOS2.Core.Entities.Inventory.ProductVariant
        {
            ProductId = product.Id,
            Name = product.Name,
            CostPrice = product.CostPrice,
            SalePrice = product.SalePrice,
            MinimumStock = product.MinimumStock,
            TrackStock = product.TrackStock,
            IsActive = product.IsActive,
            IsDefault = true
        };
        context.ProductVariants.Add(variant);
        if (initialStock != 0)
        {
            context.ProductStocks.Add(new ProductStock
            {
                ProductId = product.Id,
                VariantId = variant.Id,
                WarehouseId = _fixture.Data.WarehouseId,
                Quantity = initialStock >= 0 ? new Quantity(initialStock) : Quantity.FromSigned(initialStock)
            });
        }
        await context.SaveChangesAsync();
        return product;
    }

    [Fact]
    public async Task ZeroStockUsesNewPurchaseCost()
    {
        await using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var purchases = scope.ServiceProvider.GetRequiredService<IPurchaseService>();
        var products = scope.ServiceProvider.GetRequiredService<IProductService>();

        var product = await CreateTrackedProductAsync(context, 0, 5);

        await purchases.CreateAsync(new CreatePurchaseInvoiceDto
        {
            SupplierId = Guid.NewGuid(),
            PaidAmount = 0,
            Items = { new CreatePurchaseItemDto { ProductId = product.Id, Quantity = 10, CostPrice = 7 } }
        });

        var dto = await products.GetByIdAsync(product.Id);
        Assert.Equal(7m, dto!.CostPrice);
    }

    [Fact]
    public async Task NegativeStockUsesNewPurchaseCost()
    {
        await using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var purchases = scope.ServiceProvider.GetRequiredService<IPurchaseService>();
        var products = scope.ServiceProvider.GetRequiredService<IProductService>();

        var product = await CreateTrackedProductAsync(context, -3, 5);

        await purchases.CreateAsync(new CreatePurchaseInvoiceDto
        {
            SupplierId = Guid.NewGuid(),
            PaidAmount = 0,
            Items = { new CreatePurchaseItemDto { ProductId = product.Id, Quantity = 5, CostPrice = 8 } }
        });

        var dto = await products.GetByIdAsync(product.Id);
        Assert.Equal(8m, dto!.CostPrice);
    }

    [Fact]
    public async Task CombinesStocksAcrossWarehouses()
    {
        await using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var purchases = scope.ServiceProvider.GetRequiredService<IPurchaseService>();
        var products = scope.ServiceProvider.GetRequiredService<IProductService>();

        // second warehouse
        var wh2 = new Warehouse { Name = "WH2", BranchId = _fixture.Data.BranchId, IsActive = true };
        context.Warehouses.Add(wh2);
        var product = await CreateTrackedProductAsync(context, 100, 5);
        var v2 = await context.ProductVariants.FirstAsync(x => x.ProductId == product.Id && x.IsDefault);
        context.ProductStocks.Add(new ProductStock
        {
            ProductId = product.Id,
            VariantId = v2.Id,
            WarehouseId = wh2.Id,
            Quantity = new Quantity(50)
        });
        await context.SaveChangesAsync();

        await purchases.CreateAsync(new CreatePurchaseInvoiceDto
        {
            SupplierId = Guid.NewGuid(),
            PaidAmount = 0,
            Items = { new CreatePurchaseItemDto { ProductId = product.Id, Quantity = 10, CostPrice = 8 } }
        });

        var dto = await products.GetByIdAsync(product.Id);
        // (150*5 + 10*8)/160 = (750+80)/160 = 830/160 = 5.1875 -> 5.19
        Assert.Equal(5.19m, dto!.CostPrice);
    }

    [Fact]
    public async Task Buy10Sell4Buy10MovingAverage()
    {
        await using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var purchases = scope.ServiceProvider.GetRequiredService<IPurchaseService>();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();
        var products = scope.ServiceProvider.GetRequiredService<IProductService>();

        var product = await CreateTrackedProductAsync(context, 0, 0);
        // first purchase 10 @ 5
        await purchases.CreateAsync(new CreatePurchaseInvoiceDto
        {
            SupplierId = Guid.NewGuid(),
            PaidAmount = 0,
            Items = { new CreatePurchaseItemDto { ProductId = product.Id, Quantity = 10, CostPrice = 5 } }
        });
        // sell 4
        await sales.CreateAsync(new CreateSaleDto
        {
            PaidAmount = 100,
            Items = { new CreateSaleItemDto { ProductId = product.Id, Quantity = 4, Price = 10 } }
        });
        // second purchase 10 @ 7
        await purchases.CreateAsync(new CreatePurchaseInvoiceDto
        {
            SupplierId = Guid.NewGuid(),
            PaidAmount = 0,
            Items = { new CreatePurchaseItemDto { ProductId = product.Id, Quantity = 10, CostPrice = 7 } }
        });

        var dto = await products.GetByIdAsync(product.Id);
        // old qty after sale = 6 @5, new purchase 10 @7 => (6*5+10*7)/16 = (30+70)/16=6.25
        Assert.Equal(6.25m, dto!.CostPrice);
        // quantity: 0 +10 -4 +10 = 16
        var stock = await context.ProductStocks.Where(s => s.ProductId == product.Id).SumAsync(s => s.Quantity.Value);
        Assert.Equal(16m, stock);
    }
}