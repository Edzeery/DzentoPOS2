using DzentoPOS2.App;
using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.App.Session;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Core.ValueObjects.Common;
using DzentoPOS2.Core.ValueObjects.Inventory;
using DzentoPOS2.Data;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Shared.Contracts.DTOs.Sales;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DzentoPOS2.IntegrationTests;

/// <summary>
/// Digital-code lifecycle (Phase 3): bulk import, listing/search, allocation
/// when a digital-kind product is sold and release back to the pool when the
/// sale is voided. Each test receives its own migrated database seeded with a
/// digital-kind product.
/// </summary>
public sealed class DigitalCodeTests : IAsyncLifetime
{
    private ServiceProvider _provider = null!;
    private string _dbPath = string.Empty;
    private TestData _data = null!;
    private Guid _digitalProductId;
    private Guid _digitalVariantId;

    public async Task InitializeAsync()
    {
        (_provider, _dbPath) = BuildProvider();

        await using var scope = _provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();

        _data = await TestData.SeedAsync(context);

        var category = await context.ProductCategories.FirstAsync();
        var unit = await context.ProductUnits.FirstAsync();

        var product = new Product
        {
            Name = "Digital Card",
            SKU = "DIG-1",
            CategoryId = category.Id,
            UnitId = unit.Id,
            ProductKind = ProductKind.DigitalCode,
            CostPrice = new Money(8, "DZD"),
            SalePrice = new Money(10, "DZD"),
            MinimumStock = new Quantity(0),
            TrackStock = false,
            IsActive = true
        };

        var variant = new ProductVariant
        {
            ProductId = product.Id,
            Name = product.Name,
            CostPrice = product.CostPrice,
            SalePrice = product.SalePrice,
            MinimumStock = product.MinimumStock,
            TrackStock = product.TrackStock,
            IsActive = true,
            IsDefault = true
        };

        context.Products.Add(product);
        context.ProductVariants.Add(variant);
        await context.SaveChangesAsync();

        _digitalProductId = product.Id;
        _digitalVariantId = variant.Id;

        var currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
        currentUser.SetUser(new UserSession
        {
            UserId = _data.UserId,
            UserName = "cashier",
            FullName = "Test Cashier",
            RoleName = "Cashier",
            BranchId = _data.BranchId,
            BranchName = "Main Branch",
            IsAdmin = true
        });

        var activeBranch = scope.ServiceProvider
            .GetRequiredService<DzentoPOS2.App.Services.Systems.IActiveBranchContext>();
        await activeBranch.InitializeAsync(currentUser);
    }

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();
        SqliteConnection.ClearAllPools();

        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }

    private AsyncServiceScope CreateScope() => _provider.CreateAsyncScope();

    private static CreateSaleDto DigitalSale(Guid productId, Guid variantId, decimal price = 10) => new()
    {
        PaidAmount = price,
        Items =
        {
            new CreateSaleItemDto
            {
                ProductId = productId,
                VariantId = variantId,
                Quantity = 1,
                Price = price
            }
        }
    };

    [Fact]
    public async Task Import_AddsCodesAndReportsSkippedLines()
    {
        await using var scope = CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IDigitalCodeService>();

        var first = await service.ImportAsync(
            _digitalVariantId,
            new[] { "CARD-1", "CARD-2", "CARD-1", "  ", string.Empty });

        Assert.Equal(2, first.Imported);
        Assert.Equal(3, first.Skipped);

        var second = await service.ImportAsync(
            _digitalVariantId,
            new[] { "CARD-2", "CARD-3" });

        Assert.Equal(1, second.Imported);
        Assert.Equal(1, second.Skipped);

        Assert.Equal(3, await service.GetAvailableCountAsync(_digitalVariantId));
    }

    [Fact]
    public async Task Import_EmptyInput_IsANoOp()
    {
        await using var scope = CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IDigitalCodeService>();

        var result = await service.ImportAsync(_digitalVariantId, Array.Empty<string>());

        Assert.Equal(0, result.Imported);
        Assert.Equal(0, result.Skipped);
        Assert.Equal(0, await service.GetAvailableCountAsync(_digitalVariantId));
    }

    [Fact]
    public async Task GetDigitalVariants_ListsOnlyDigitalKindProducts()
    {
        await using var scope = CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IDigitalCodeService>();

        var variants = await service.GetDigitalVariantsAsync();

        var option = Assert.Single(variants);
        Assert.Equal(_digitalVariantId, option.VariantId);
        Assert.Equal(_digitalProductId, option.ProductId);
        Assert.Equal("Digital Card", option.DisplayName);
    }

    [Fact]
    public async Task Search_FiltersByVariantStatusAndTerm()
    {
        await using var scope = CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IDigitalCodeService>();

        await service.ImportAsync(_digitalVariantId, new[] { "SEARCH-1", "SEARCH-2", "OTHER-1" });

        var all = await service.SearchAsync(_digitalVariantId, null, null);
        Assert.Equal(3, all.Count);
        Assert.All(all, c => Assert.Equal("Digital Card", c.VariantName));

        var byTerm = await service.SearchAsync(_digitalVariantId, null, "SEARCH");
        Assert.Equal(2, byTerm.Count);

        var available = await service.SearchAsync(_digitalVariantId, DigitalCodeStatus.Available, null);
        Assert.Equal(3, available.Count);

        var delivered = await service.SearchAsync(_digitalVariantId, DigitalCodeStatus.Delivered, null);
        Assert.Empty(delivered);
    }

    [Fact]
    public async Task Search_UnknownCode_ReturnsEmpty()
    {
        await using var scope = CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IDigitalCodeService>();

        await service.ImportAsync(_digitalVariantId, new[] { "KNOWN-1" });

        var result = await service.SearchAsync(_digitalVariantId, null, "MISSING");
        Assert.Empty(result);
    }

    [Fact]
    public async Task Allocate_MarksCodesDeliveredAndLinksTheInvoice()
    {
        await using var scope = CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IDigitalCodeService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await service.ImportAsync(_digitalVariantId, new[] { "ALLOC-1", "ALLOC-2", "ALLOC-3" });

        var invoiceId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var deliveredAt = DateTime.UtcNow;

        var allocated = await service.AllocateForSaleAsync(
            _digitalVariantId, invoiceId, itemId, 2, deliveredAt);

        Assert.Equal(2, allocated.Count);
        Assert.All(allocated, c => Assert.Equal(DigitalCodeStatus.Delivered, c.Status));

        var stored = await context.DigitalCodes
            .AsNoTracking()
            .Where(c => c.SalesInvoiceId == invoiceId)
            .ToListAsync();

        Assert.Equal(2, stored.Count);
        Assert.All(stored, c =>
        {
            Assert.Equal(itemId, c.SalesInvoiceItemId);
            Assert.Equal(deliveredAt, c.DeliveredAt);
        });

        Assert.Equal(1, await service.GetAvailableCountAsync(_digitalVariantId));
    }

    [Fact]
    public async Task Allocate_ZeroQuantity_IsANoOp()
    {
        await using var scope = CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IDigitalCodeService>();

        await service.ImportAsync(_digitalVariantId, new[] { "ZERO-1" });

        var allocated = await service.AllocateForSaleAsync(
            _digitalVariantId, Guid.NewGuid(), Guid.NewGuid(), 0, DateTime.UtcNow);

        Assert.Empty(allocated);
        Assert.Equal(1, await service.GetAvailableCountAsync(_digitalVariantId));
    }

    [Fact]
    public async Task ReturnForSale_ReleasesTheCodesBackToThePool()
    {
        await using var scope = CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IDigitalCodeService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await service.ImportAsync(_digitalVariantId, new[] { "RET-1", "RET-2" });
        var invoiceId = Guid.NewGuid();
        await service.AllocateForSaleAsync(
            _digitalVariantId, invoiceId, Guid.NewGuid(), 2, DateTime.UtcNow);

        var released = await service.ReturnForSaleAsync(invoiceId);

        Assert.Equal(2, released);
        Assert.Equal(2, await service.GetAvailableCountAsync(_digitalVariantId));

        var stored = await context.DigitalCodes.AsNoTracking().ToListAsync();
        Assert.All(stored, c =>
        {
            Assert.Equal(DigitalCodeStatus.Available, c.Status);
            Assert.Null(c.SalesInvoiceId);
            Assert.Null(c.SalesInvoiceItemId);
            Assert.Null(c.DeliveredAt);
        });
    }

    [Fact]
    public async Task Cancel_RemovesTheCodeFromThePool()
    {
        await using var scope = CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IDigitalCodeService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await service.ImportAsync(_digitalVariantId, new[] { "CANCEL-1" });
        var code = await context.DigitalCodes.AsNoTracking().SingleAsync();

        await service.CancelAsync(code.Id);

        var stored = await context.DigitalCodes.AsNoTracking().SingleAsync();
        Assert.Equal(DigitalCodeStatus.Cancelled, stored.Status);
        Assert.Equal(0, await service.GetAvailableCountAsync(_digitalVariantId));
    }

    [Fact]
    public async Task Sale_AllocatesAndStampsTheDeliveredCode()
    {
        await using var scope = CreateScope();
        var codes = scope.ServiceProvider.GetRequiredService<IDigitalCodeService>();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await codes.ImportAsync(_digitalVariantId, new[] { "SALE-1", "SALE-2" });

        var sale = await sales.CreateAsync(DigitalSale(_digitalProductId, _digitalVariantId));

        var item = Assert.Single(sale.Items);
        Assert.False(string.IsNullOrWhiteSpace(item.DigitalCode));
        Assert.Contains(item.DigitalCode!, new[] { "SALE-1", "SALE-2" });

        var delivered = await context.DigitalCodes
            .AsNoTracking()
            .Where(c => c.Status == DigitalCodeStatus.Delivered)
            .ToListAsync();

        Assert.Single(delivered);
        Assert.Equal(sale.Id, delivered[0].SalesInvoiceId);
        Assert.Equal(1, await codes.GetAvailableCountAsync(_digitalVariantId));

        var storedItem = await context.SalesInvoiceItems
            .AsNoTracking()
            .SingleAsync(i => i.SalesInvoiceId == sale.Id);
        Assert.Equal(item.DigitalCode, storedItem.DigitalCode);
    }

    [Fact]
    public async Task Sale_ExhaustedPool_DoesNotBlockTheSale()
    {
        await using var scope = CreateScope();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();

        var sale = await sales.CreateAsync(DigitalSale(_digitalProductId, _digitalVariantId));

        var item = Assert.Single(sale.Items);
        Assert.True(string.IsNullOrEmpty(item.DigitalCode));
    }

    [Fact]
    public async Task SaleVoid_ReleasesTheDeliveredCode()
    {
        await using var scope = CreateScope();
        var codes = scope.ServiceProvider.GetRequiredService<IDigitalCodeService>();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await codes.ImportAsync(_digitalVariantId, new[] { "VOID-1" });

        var sale = await sales.CreateAsync(DigitalSale(_digitalProductId, _digitalVariantId));
        Assert.Equal(0, await codes.GetAvailableCountAsync(_digitalVariantId));

        await sales.VoidAsync(sale.Id, "test void");

        Assert.Equal(1, await codes.GetAvailableCountAsync(_digitalVariantId));

        var stored = await context.DigitalCodes.AsNoTracking().SingleAsync();
        Assert.Equal(DigitalCodeStatus.Available, stored.Status);
        Assert.Null(stored.SalesInvoiceId);
        Assert.Null(stored.DeliveredAt);
    }

    private static (ServiceProvider Provider, string DbPath) BuildProvider()
    {
        var directory = Path.Combine(Path.GetTempPath(), "DzentoPOS2.DigitalCodes");
        Directory.CreateDirectory(directory);

        var dbPath = Path.Combine(directory, $"{Guid.NewGuid():N}.db");

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={dbPath}"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddDataServices(configuration);
        services.AddApplicationServices();

        return (services.BuildServiceProvider(), dbPath);
    }
}
