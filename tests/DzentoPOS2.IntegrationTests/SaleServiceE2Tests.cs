using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.App.Services.Purchases;
using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Core.Exceptions;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Shared.Contracts.DTOs.Purchases;
using DzentoPOS2.Shared.Contracts.DTOs.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DzentoPOS2.IntegrationTests;

[Collection(PosDatabaseCollection.Name)]
public sealed class SaleServiceE2Tests
{
    private readonly PosDatabaseFixture _fixture;

    public SaleServiceE2Tests(PosDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SaleDeductsTrackedStockAndWritesLedger()
    {
        await using var scope = _fixture.CreateScope();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();

        var before = await GetStockAsync(scope, _fixture.Data.TrackedProductId);

        var sale = await sales.CreateAsync(new CreateSaleDto
        {
            PaidAmount = 0,
            Items =
            {
                new CreateSaleItemDto
                {
                    ProductId = _fixture.Data.TrackedProductId,
                    Quantity = 3,
                    Price = 10
                }
            }
        });

        Assert.Equal(before - 3, await GetStockAsync(scope, _fixture.Data.TrackedProductId));

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var movement = await context.InventoryTransactions
            .AsNoTracking()
            .SingleAsync(x => x.SourceId == sale.Id && x.SourceType == "SalesInvoice");

        Assert.Equal(MovementType.SaleOut, movement.TransactionType);
        Assert.Equal(3, movement.Quantity.Value);
        Assert.Equal(sale.Id, movement.SourceId);
        Assert.Equal(before, movement.QuantityBefore);
        Assert.Equal(before - 3, movement.QuantityAfter);
    }

    [Fact]
    public async Task InsufficientStockRollsBackWholeSale()
    {
        await using var scope = _fixture.CreateScope();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var beforeStock = await GetStockAsync(scope, _fixture.Data.TrackedProductId);
        var beforeInvoices = await context.SalesInvoices.CountAsync();

        await Assert.ThrowsAsync<DomainException>(() => sales.CreateAsync(new CreateSaleDto
        {
            PaidAmount = 0,
            Items =
            {
                new CreateSaleItemDto
                {
                    ProductId = _fixture.Data.TrackedProductId,
                    Quantity = 100000,
                    Price = 10
                }
            }
        }));

        Assert.Equal(beforeStock, await GetStockAsync(scope, _fixture.Data.TrackedProductId));
        Assert.Equal(beforeInvoices, await context.SalesInvoices.CountAsync());
    }

    [Fact]
    public async Task NonTrackedProductDoesNotCreateStockMovement()
    {
        await using var scope = _fixture.CreateScope();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var sale = await sales.CreateAsync(new CreateSaleDto
        {
            PaidAmount = 0,
            Items =
            {
                new CreateSaleItemDto
                {
                    ProductId = _fixture.Data.ProductId,
                    Quantity = 5,
                    Price = 10
                }
            }
        });

        Assert.False(await context.InventoryTransactions
            .AsNoTracking()
            .AnyAsync(x => x.SourceId == sale.Id));
    }

    [Fact]
    public async Task VoidRestoresStockAndMarksInvoice()
    {
        await using var scope = _fixture.CreateScope();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var before = await GetStockAsync(scope, _fixture.Data.TrackedProductId);

        var sale = await sales.CreateAsync(new CreateSaleDto
        {
            PaidAmount = 0,
            Items =
            {
                new CreateSaleItemDto
                {
                    ProductId = _fixture.Data.TrackedProductId,
                    Quantity = 2,
                    Price = 10
                }
            }
        });

        await sales.VoidAsync(sale.Id, "Customer returned the order");

        Assert.Equal(before, await GetStockAsync(scope, _fixture.Data.TrackedProductId));

        var invoice = await context.SalesInvoices
            .AsNoTracking()
            .SingleAsync(x => x.Id == sale.Id);

        Assert.True(invoice.IsVoided);
        Assert.Equal("Customer returned the order", invoice.VoidReason);
        Assert.NotNull(invoice.VoidedAtUtc);

        Assert.True(await context.InventoryTransactions
            .AsNoTracking()
            .AnyAsync(x =>
                x.SourceId == sale.Id &&
                x.TransactionType == MovementType.SaleVoidIn));
    }

    [Fact]
    public async Task PurchaseAddsStockAndDeleteReversesIt()
    {
        await using var scope = _fixture.CreateScope();
        var purchases = scope.ServiceProvider.GetRequiredService<IPurchaseService>();

        var before = await GetStockAsync(scope, _fixture.Data.TrackedProductId);

        var purchase = await purchases.CreateAsync(new CreatePurchaseInvoiceDto
        {
            SupplierId = Guid.NewGuid(),
            PaidAmount = 0,
            Items =
            {
                new CreatePurchaseItemDto
                {
                    ProductId = _fixture.Data.TrackedProductId,
                    Quantity = 7,
                    CostPrice = 5
                }
            }
        });

        Assert.Equal(before + 7, await GetStockAsync(scope, _fixture.Data.TrackedProductId));

        await purchases.DeleteAsync(purchase.Id);

        Assert.Equal(before, await GetStockAsync(scope, _fixture.Data.TrackedProductId));
    }

    [Fact]
    public async Task StockAdjustmentUsesTargetQuantityAndWritesMovement()
    {
        await using var scope = _fixture.CreateScope();
        var products = scope.ServiceProvider.GetRequiredService<IProductService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await products.AdjustStockAsync(
            _fixture.Data.TrackedProductId,
            42,
            "Physical count");

        Assert.Equal(42, await GetStockAsync(scope, _fixture.Data.TrackedProductId));

        Assert.True(await context.InventoryTransactions
            .AsNoTracking()
            .AnyAsync(x =>
                x.ProductId == _fixture.Data.TrackedProductId &&
                x.ReferenceNumber == "Physical count" &&
                (x.TransactionType == MovementType.StockAdjustmentIncrease ||
                 x.TransactionType == MovementType.StockAdjustmentDecrease)));
    }

    private async Task<decimal> GetStockAsync(
        AsyncServiceScope scope,
        Guid productId)
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await context.ProductStocks
            .AsNoTracking()
            .Where(x =>
                x.ProductId == productId &&
                x.WarehouseId == _fixture.Data.WarehouseId)
            .Select(x => x.Quantity.Value)
            .SingleAsync();
    }
}
