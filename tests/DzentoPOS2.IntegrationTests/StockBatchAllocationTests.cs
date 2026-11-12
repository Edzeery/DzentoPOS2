using DzentoPOS2.App;
using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Core.ValueObjects.Common;
using DzentoPOS2.Core.ValueObjects.Inventory;
using DzentoPOS2.Data;
using DzentoPOS2.Data.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DzentoPOS2.IntegrationTests;

/// <summary>
/// Phase 6.2 batch consumption in <see cref="StockService"/>: the batch picked
/// by the cashier is consumed first, the remainder falls back to FEFO, and the
/// decrements are persisted (regression test for the old AsNoTracking load).
/// </summary>
public sealed class StockBatchAllocationTests : IAsyncLifetime
{
    private ServiceProvider _provider = null!;
    private string _dbPath = string.Empty;
    private TestData _data = null!;
    private Guid _productId;
    private Guid _variantId;
    private Guid _nearBatchId;
    private Guid _farBatchId;

    public async Task InitializeAsync()
    {
        (_provider, _dbPath) = BuildProvider();

        await using var scope = _provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();

        _data = await TestData.SeedAsync(context);

        var categoryId = await context.ProductCategories.Select(c => c.Id).FirstAsync();
        var unitId = await context.ProductUnits.Select(u => u.Id).FirstAsync();

        var product = new Product
        {
            Name = "Batch Sale Product",
            SKU = $"BATCH-SALE-{Guid.NewGuid():N}",
            CategoryId = categoryId,
            UnitId = unitId,
            CostPrice = new Money(5m, "DZD"),
            SalePrice = new Money(10m, "DZD"),
            MinimumStock = new Quantity(0m),
            TrackStock = true,
            IsActive = true
        };
        var variant = new ProductVariant
        {
            ProductId = product.Id,
            Name = product.Name,
            CostPrice = product.CostPrice,
            SalePrice = product.SalePrice,
            MinimumStock = product.MinimumStock,
            TrackStock = true,
            IsActive = true,
            IsDefault = true,
            TrackBatch = true
        };
        var stock = new ProductStock
        {
            ProductId = product.Id,
            VariantId = variant.Id,
            WarehouseId = _data.WarehouseId,
            Quantity = new Quantity(30m)
        };
        var now = DateTime.UtcNow;
        var near = new Batch
        {
            VariantId = variant.Id,
            WarehouseId = _data.WarehouseId,
            BatchNumber = "B-NEAR",
            ExpiryDate = now.AddDays(5),
            Quantity = 10m,
            UnitCost = 5m,
            ReceivedAt = now
        };
        var far = new Batch
        {
            VariantId = variant.Id,
            WarehouseId = _data.WarehouseId,
            BatchNumber = "B-FAR",
            ExpiryDate = now.AddDays(30),
            Quantity = 20m,
            UnitCost = 5m,
            ReceivedAt = now
        };

        context.Products.Add(product);
        context.ProductVariants.Add(variant);
        context.ProductStocks.Add(stock);
        context.Batches.AddRange(near, far);
        await context.SaveChangesAsync();

        _productId = product.Id;
        _variantId = variant.Id;
        _nearBatchId = near.Id;
        _farBatchId = far.Id;
    }

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();
        SqliteConnection.ClearAllPools();

        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }

    private async Task<StockService> CreateStockServiceAsync(AsyncServiceScope scope)
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var clock = scope.ServiceProvider.GetRequiredService<IBusinessClock>();
        var batches = scope.ServiceProvider.GetRequiredService<IBatchService>();
        return new StockService(context, clock, guard: null, logger: null, batchService: batches);
    }

    private Task ApplySaleAsync(StockService stock, decimal quantity, Guid batchId)
    {
        return stock.ApplySaleAsync(
            Guid.NewGuid(),
            $"INV-{Guid.NewGuid():N}",
            _data.WarehouseId,
            new[]
            {
                new StockChange(
                    _productId,
                    _variantId,
                    quantity,
                    BatchId: batchId)
            },
            _data.UserId);
    }

    [Fact]
    public async Task Sale_ConsumesThePreferredBatchFirst()
    {
        await using var scope = _provider.CreateAsyncScope();
        var stock = await CreateStockServiceAsync(scope);
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await ApplySaleAsync(stock, 5m, _farBatchId);

        var batches = await context.Batches.AsNoTracking().ToListAsync();
        Assert.Equal(15m, batches.Single(b => b.Id == _farBatchId).Quantity);
        Assert.Equal(10m, batches.Single(b => b.Id == _nearBatchId).Quantity);

        var remaining = await context.ProductStocks
            .AsNoTracking()
            .SingleAsync(s => s.VariantId == _variantId && s.WarehouseId == _data.WarehouseId);
        Assert.Equal(25m, remaining.Quantity.Value);
    }

    [Fact]
    public async Task Sale_ConsumesThePreferredBatchThenTheRemainderFEFO()
    {
        await using var scope = _provider.CreateAsyncScope();
        var stock = await CreateStockServiceAsync(scope);
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // The pick exceeds the near batch: 10 come from it, 5 from the next
        // FEFO batch.
        await ApplySaleAsync(stock, 15m, _nearBatchId);

        var batches = await context.Batches.AsNoTracking().ToListAsync();
        Assert.Equal(0m, batches.Single(b => b.Id == _nearBatchId).Quantity);
        Assert.Equal(15m, batches.Single(b => b.Id == _farBatchId).Quantity);

        var remaining = await context.ProductStocks
            .AsNoTracking()
            .SingleAsync(s => s.VariantId == _variantId && s.WarehouseId == _data.WarehouseId);
        Assert.Equal(15m, remaining.Quantity.Value);
    }

    [Fact]
    public async Task Sale_WithoutAPick_FallsBackToFEFO()
    {
        await using var scope = _provider.CreateAsyncScope();
        var stock = await CreateStockServiceAsync(scope);
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await ApplySaleAsync(stock, 6m, Guid.Empty);

        var batches = await context.Batches.AsNoTracking().ToListAsync();
        Assert.Equal(4m, batches.Single(b => b.Id == _nearBatchId).Quantity);
        Assert.Equal(20m, batches.Single(b => b.Id == _farBatchId).Quantity);

        var remaining = await context.ProductStocks
            .AsNoTracking()
            .SingleAsync(s => s.VariantId == _variantId && s.WarehouseId == _data.WarehouseId);
        Assert.Equal(24m, remaining.Quantity.Value);
    }

    [Fact]
    public async Task ConsecutiveSales_PersistBatchDecrementsAcrossScopes()
    {
        await using (var firstScope = _provider.CreateAsyncScope())
        {
            var stock = await CreateStockServiceAsync(firstScope);
            await ApplySaleAsync(stock, 2m, _farBatchId);
        }

        await using (var secondScope = _provider.CreateAsyncScope())
        {
            var stock = await CreateStockServiceAsync(secondScope);
            await ApplySaleAsync(stock, 3m, _farBatchId);
        }

        await using var verifyScope = _provider.CreateAsyncScope();
        var context = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var batches = await context.Batches.AsNoTracking().ToListAsync();

        Assert.Equal(15m, batches.Single(b => b.Id == _farBatchId).Quantity);
        Assert.Equal(10m, batches.Single(b => b.Id == _nearBatchId).Quantity);

        var remaining = await context.ProductStocks
            .AsNoTracking()
            .SingleAsync(s => s.VariantId == _variantId && s.WarehouseId == _data.WarehouseId);
        Assert.Equal(25m, remaining.Quantity.Value);
    }

    private static (ServiceProvider Provider, string DbPath) BuildProvider()
    {
        var directory = Path.Combine(Path.GetTempPath(), "DzentoPOS2.StockBatches");
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
