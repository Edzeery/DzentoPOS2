using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.App.Services.Purchases;
using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;
using DzentoPOS2.Shared.Contracts.DTOs.Purchases;
using DzentoPOS2.Shared.Contracts.DTOs.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DzentoPOS2.IntegrationTests;

[Collection(PosDatabaseCollection.Name)]
public sealed class ProfitFlowTests
{
    private readonly PosDatabaseFixture _fixture;

    public ProfitFlowTests(PosDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task PurchaseUpdatesMovingAverageCostAndSaleFreezesIt()
    {
        await using var scope = _fixture.CreateScope();
        var purchases = scope.ServiceProvider.GetRequiredService<IPurchaseService>();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();
        var products = scope.ServiceProvider.GetRequiredService<IProductService>();

        await purchases.CreateAsync(new CreatePurchaseInvoiceDto
        {
            SupplierId = Guid.NewGuid(),
            PaidAmount = 0,
            Items =
            {
                new CreatePurchaseItemDto
                {
                    ProductId = _fixture.Data.TrackedProductId,
                    Quantity = 10,
                    CostPrice = 8
                }
            }
        });

        var productAfterPurchase = await products.GetByIdAsync(_fixture.Data.TrackedProductId);
        Assert.NotNull(productAfterPurchase);
        Assert.Equal(5.27m, productAfterPurchase!.CostPrice);

        var sale = await sales.CreateAsync(new CreateSaleDto
        {
            PaidAmount = 100,
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

        Assert.Equal(5.27m, sale.Items[0].UnitCost);
        Assert.Equal(10m, sale.Items[0].ListPrice);

        await products.UpdateAsync(new UpdateProductDto
        {
            Id = _fixture.Data.TrackedProductId,
            Name = productAfterPurchase.Name,
            CategoryId = productAfterPurchase.CategoryId,
            UnitId = productAfterPurchase.UnitId,
            CostPrice = 999,
            SalePrice = productAfterPurchase.SalePrice,
            MinimumStock = productAfterPurchase.MinimumStock,
            TrackStock = true,
            IsActive = true
        });

        var reloaded = await sales.GetByIdAsync(sale.Id);
        Assert.Equal(5.27m, reloaded!.Items[0].UnitCost);
    }
}