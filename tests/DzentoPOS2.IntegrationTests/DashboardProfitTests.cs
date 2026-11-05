using DzentoPOS2.App.Services.Reports.Interfaces;
using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.Core.Entities.Sales;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Core.ValueObjects.Common;
using DzentoPOS2.Core.ValueObjects.Inventory;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Shared.Contracts.DTOs.Sales;
using DzentoPOS2.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace DzentoPOS2.IntegrationTests;

[Collection(PosDatabaseCollection.Name)]
public sealed class DashboardProfitTests
{
    private readonly PosDatabaseFixture _fixture;
    public DashboardProfitTests(PosDatabaseFixture fixture) => _fixture = fixture;

    private async Task<Product> CreateProductAsync(AppDbContext ctx, decimal cost, decimal sale, bool track = true)
    {
        var cat = await ctx.ProductCategories.FirstAsync();
        var unit = await ctx.ProductUnits.FirstAsync();
        var p = new Product
        {
            Name = "DP-" + Guid.NewGuid().ToString("N").Substring(0, 6),
            CategoryId = cat.Id,
            UnitId = unit.Id,
            CostPrice = new Money(cost, "DZD"),
            SalePrice = new Money(sale, "DZD"),
            MinimumStock = new Quantity(0),
            TrackStock = track,
            IsActive = true
        };
        ctx.Products.Add(p);
        await ctx.SaveChangesAsync();
        ctx.ProductVariants.Add(new DzentoPOS2.Core.Entities.Inventory.ProductVariant
        {
            ProductId = p.Id,
            Name = p.Name,
            CostPrice = p.CostPrice,
            SalePrice = p.SalePrice,
            MinimumStock = p.MinimumStock,
            TrackStock = p.TrackStock,
            IsActive = p.IsActive,
            IsDefault = true
        });
        await ctx.SaveChangesAsync();
        return p;
    }

    private async Task<SalesInvoice> CreateManualInvoiceAsync(AppDbContext ctx, Guid productId, decimal qty, decimal price, decimal unitCost, decimal discountAmount, decimal fees, bool estimated, DateTime createdAt, bool voided = false)
    {
        var totalsSubtotal = qty * price;
        var after = totalsSubtotal - discountAmount;
        var tax = 0m;
        var totals = new DzentoPOS2.Core.Services.Sales.InvoiceTotals(totalsSubtotal, discountAmount, after, tax, after, 0, 0, after, 0);
        var inv = SalesInvoice.Create("MAN-" + Guid.NewGuid().ToString("N").Substring(0, 6), Guid.Empty, _fixture.Data.BranchId, totals, DzentoPOS2.Core.Services.Sales.DiscountMode.FixedAmount, discountAmount, 0, "DZD", fees);
        var variantId = await ctx.ProductVariants
            .Where(v => v.ProductId == productId && v.IsDefault)
            .Select(v => v.Id)
            .FirstOrDefaultAsync();
        var item = new SalesInvoiceItem
        {
            SalesInvoiceId = inv.Id,
            ProductId = productId,
            VariantId = variantId,
            Quantity = qty,
            Price = price,
            Total = qty * price,
            UnitCost = unitCost,
            ListPrice = price,
            DiscountAmount = discountAmount,
            IsCostEstimated = estimated
        };
        inv.Items.Add(item);
        ctx.SalesInvoices.Add(inv);
        await ctx.SaveChangesAsync();
        // set CreatedAt and voided
        var entry = ctx.Entry(inv);
        entry.Property(nameof(SalesInvoice.CreatedAt)).CurrentValue = createdAt;
        if (voided) inv.Void(Guid.Empty, "void", createdAt);
        await ctx.SaveChangesAsync();
        return inv;
    }

    [Fact]
    public async Task ReturnFromPreviousMonth_UsesFrozenCost()
    {
        await using var scope = _fixture.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dashboard = scope.ServiceProvider.GetRequiredService<IDashboardService>();

        var product = await CreateProductAsync(ctx, 5, 10);
        var regional = scope.ServiceProvider.GetRequiredService<RegionalContext>();
        var localNow = regional.ToLocal(DateTime.UtcNow);
        var prevMonth = regional.ToUtcSafe(new DateTime(localNow.Year, localNow.Month, 1).AddMonths(-1).AddDays(14));
        var inv = await CreateManualInvoiceAsync(ctx, product.Id, 2, 10, 5, 0, 0, false, prevMonth);

        var before = await dashboard.GetSummaryAsync();

        var ret = new SalesReturn
        {
            ReturnNumber = "RET-" + Guid.NewGuid().ToString("N").Substring(0, 6),
            SalesInvoiceId = inv.Id,
            CustomerId = Guid.Empty,
            TotalAmount = 10,
            ReturnDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        ctx.SalesReturns.Add(ret);
        await ctx.SaveChangesAsync();
        ctx.SalesReturnItems.Add(new SalesReturnItem
        {
            SalesReturnId = ret.Id,
            ProductId = product.Id,
            Quantity = 1,
            Price = 10,
            Total = 10
        });
        await ctx.SaveChangesAsync();

        var after = await dashboard.GetSummaryAsync();
        // return effect = 10 - 1*5 = 5 reduces actual profit
        Assert.Equal(-5m, after.ActualProfit.Value - before.ActualProfit.Value);
    }

    [Fact]
    public async Task ReturnOnVoidedInvoice_NoEffect()
    {
        await using var scope = _fixture.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dashboard = scope.ServiceProvider.GetRequiredService<IDashboardService>();

        var product = await CreateProductAsync(ctx, 5, 10);
        var regional = scope.ServiceProvider.GetRequiredService<RegionalContext>();
        var localNow = regional.ToLocal(DateTime.UtcNow);
        var prevMonth = regional.ToUtcSafe(new DateTime(localNow.Year, localNow.Month, 1).AddMonths(-1).AddDays(14));
        var inv = await CreateManualInvoiceAsync(ctx, product.Id, 2, 10, 5, 0, 0, false, prevMonth, voided: true);

        var before = await dashboard.GetSummaryAsync();

        var ret = new SalesReturn
        {
            ReturnNumber = "RET-" + Guid.NewGuid().ToString("N").Substring(0, 6),
            SalesInvoiceId = inv.Id,
            CustomerId = Guid.Empty,
            TotalAmount = 10,
            ReturnDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        ctx.SalesReturns.Add(ret);
        await ctx.SaveChangesAsync();
        ctx.SalesReturnItems.Add(new SalesReturnItem
        {
            SalesReturnId = ret.Id,
            ProductId = product.Id,
            Quantity = 1,
            Price = 10,
            Total = 10
        });
        await ctx.SaveChangesAsync();

        var after = await dashboard.GetSummaryAsync();
        Assert.Equal(0m, after.ActualProfit.Value - before.ActualProfit.Value);
    }

    [Fact]
    public async Task ReturnWithDiscount_SubtractsProportionalDiscount()
    {
        await using var scope = _fixture.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dashboard = scope.ServiceProvider.GetRequiredService<IDashboardService>();

        var product = await CreateProductAsync(ctx, 5, 10);
        var regional = scope.ServiceProvider.GetRequiredService<RegionalContext>();
        var localNow = regional.ToLocal(DateTime.UtcNow);
        var prevMonth = regional.ToUtcSafe(new DateTime(localNow.Year, localNow.Month, 1).AddMonths(-1).AddDays(14));
        var inv = await CreateManualInvoiceAsync(ctx, product.Id, 2, 10, 5, 4, 0, false, prevMonth);

        var before = await dashboard.GetSummaryAsync();

        var ret = new SalesReturn
        {
            ReturnNumber = "RET-" + Guid.NewGuid().ToString("N").Substring(0, 6),
            SalesInvoiceId = inv.Id,
            CustomerId = Guid.Empty,
            TotalAmount = 10,
            ReturnDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        ctx.SalesReturns.Add(ret);
        await ctx.SaveChangesAsync();
        ctx.SalesReturnItems.Add(new SalesReturnItem
        {
            SalesReturnId = ret.Id,
            ProductId = product.Id,
            Quantity = 1,
            Price = 10,
            Total = 10
        });
        await ctx.SaveChangesAsync();

        var after = await dashboard.GetSummaryAsync();
        // discount share = 4*1/2=2; return net = 10-2=8; cost=5; effect=3
        Assert.Equal(-3m, after.ActualProfit.Value - before.ActualProfit.Value);
    }

    [Fact]
    public async Task NetProfit_ActualMinusExpenses_AndFeesSubtracted()
    {
        await using var scope = _fixture.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dashboard = scope.ServiceProvider.GetRequiredService<IDashboardService>();

        var product = await CreateProductAsync(ctx, 600, 1000);
        var regional = scope.ServiceProvider.GetRequiredService<RegionalContext>();
        var localNow = regional.ToLocal(DateTime.UtcNow);
        var now = regional.ToUtcSafe(new DateTime(localNow.Year, localNow.Month, 15, 12, 0, 0));

        var before = await dashboard.GetSummaryAsync();

        var inv = await CreateManualInvoiceAsync(ctx, product.Id, 1, 1000, 600, 0, 50, false, now);

        var expense = new DzentoPOS2.Core.Entities.Accounting.Expense
        {
            Title = "Test expense",
            BranchId = _fixture.Data.BranchId,
            Amount = 100,
            ExpenseDate = now
        };
        ctx.Expenses.Add(expense);
        await ctx.SaveChangesAsync();

        var after = await dashboard.GetSummaryAsync();
        // actual = 1000-600-50 = 350; net = 350-100 = 250
        Assert.Equal(350m, after.ActualProfit.Value - before.ActualProfit.Value);
        Assert.Equal(250m, after.NetProfit.Value - before.NetProfit.Value);
        Assert.Equal(50m, after.MonthlyFeesAmount - before.MonthlyFeesAmount);
    }

    [Fact]
    public async Task EstimatedCost_RaisesFlag()
    {
        await using var scope = _fixture.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dashboard = scope.ServiceProvider.GetRequiredService<IDashboardService>();

        var regional = scope.ServiceProvider.GetRequiredService<RegionalContext>();
        var localNow = regional.ToLocal(DateTime.UtcNow);
        var now = regional.ToUtcSafe(new DateTime(localNow.Year, localNow.Month, 15, 12, 0, 0));
        var product = await CreateProductAsync(ctx, 5, 10);
        await CreateManualInvoiceAsync(ctx, product.Id, 1, 10, 5, 0, 0, true, now);

        var summary = await dashboard.GetSummaryAsync();
        Assert.True(summary.ExpectedProfit.HasEstimatedCost);
        Assert.True(summary.ActualProfit.HasEstimatedCost);
        Assert.True(summary.NetProfit.HasEstimatedCost);
    }

    [Fact]
    public async Task ExpectedStockProfit_AfterBuy6Sell1()
    {
        await using var scope = _fixture.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();
        var dashboard = scope.ServiceProvider.GetRequiredService<IDashboardService>();

        var product = await CreateProductAsync(ctx, 5, 10);
        var v = await ctx.ProductVariants.FirstAsync(x => x.ProductId == product.Id && x.IsDefault);
        ctx.ProductStocks.Add(new ProductStock
        {
            ProductId = product.Id,
            VariantId = v.Id,
            WarehouseId = _fixture.Data.WarehouseId,
            Quantity = new Quantity(6)
        });
        await ctx.SaveChangesAsync();

        var before = await dashboard.GetSummaryAsync();

        await sales.CreateAsync(new CreateSaleDto
        {
            CustomerId = Guid.Empty,
            DiscountMode = DzentoPOS2.Core.Services.Sales.DiscountMode.Percent,
            DiscountValue = 0,
            TaxRate = 0,
            PaidAmount = 10,
            Items = { new CreateSaleItemDto { ProductId = product.Id, Quantity = 1, Price = 10 } }
        });

        var after = await dashboard.GetSummaryAsync();

        Assert.Equal(5m, after.ExpectedSoldProfit.Value - before.ExpectedSoldProfit.Value);
        Assert.Equal(-5m, after.ExpectedStockProfit.Value - before.ExpectedStockProfit.Value);
        Assert.Equal(5m, after.ExpectedProfit.Value -
            before.ExpectedProfit.Value);
    }

    [Fact]
    public async Task AcceptanceExample_ViaDashboard()
    {
        await using var scope = _fixture.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();
        var dashboard = scope.ServiceProvider.GetRequiredService<IDashboardService>();

        var product = await CreateProductAsync(ctx, 600, 1000);

        var before = await dashboard.GetSummaryAsync();

        var sale = await sales.CreateAsync(new CreateSaleDto
        {
            CustomerId = Guid.Empty,
            DiscountMode = DzentoPOS2.Core.Services.Sales.DiscountMode.Percent,
            DiscountValue = 10,
            TaxRate = 19,
            Fees = 50,
            PaidAmount = 2142,
            Items =
            {
                new CreateSaleItemDto { ProductId = product.Id, Quantity = 2, Price = 1000 }
            }
        });

        var after = await dashboard.GetSummaryAsync();

        Assert.Equal(2142m, sale.Total);
        Assert.Equal(800m, after.ExpectedProfit.Value - before.ExpectedProfit.Value);
        Assert.Equal(550m, after.ActualProfit.Value - before.ActualProfit.Value);
        Assert.Equal(550m, after.NetProfit.Value - before.NetProfit.Value);
        Assert.Equal(50m, after.MonthlyFeesAmount - before.MonthlyFeesAmount);
        Assert.Equal(200m, after.MonthlyDiscountAmount - before.MonthlyDiscountAmount);
    }
}
