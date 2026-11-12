using DzentoPOS2.App;
using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Data;
using DzentoPOS2.Data.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DzentoPOS2.IntegrationTests;

/// <summary>
/// Batch/expiry lifecycle (Phase 6.1/6.2): persistence, lookup, expiry windows,
/// quantity adjustments and FEFO selection. Each test gets a fresh database.
/// </summary>
public sealed class BatchServiceTests : IAsyncLifetime
{
    private ServiceProvider _provider = null!;
    private string _dbPath = string.Empty;
    private TestData _data = null!;
    private Guid _variantId;

    public async Task InitializeAsync()
    {
        (_provider, _dbPath) = BuildProvider();

        await using var scope = _provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();

        _data = await TestData.SeedAsync(context);
        _variantId = await context.ProductVariants
            .Where(v => v.ProductId == _data.TrackedProductId && v.IsDefault)
            .Select(v => v.Id)
            .FirstAsync();
    }

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();
        SqliteConnection.ClearAllPools();

        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }

    private AsyncServiceScope CreateScope() => _provider.CreateAsyncScope();

    [Fact]
    public async Task Create_PersistsTheBatch()
    {
        await using var scope = CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IBatchService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var expiry = DateTime.UtcNow.Date.AddDays(30);

        var batch = await service.CreateAsync(
            _variantId,
            _data.WarehouseId,
            "B-CREATE",
            expiry,
            quantity: 12m,
            unitCost: 4.5m);

        var saved = await context.Batches.AsNoTracking().SingleAsync(b => b.Id == batch.Id);
        Assert.Equal("B-CREATE", saved.BatchNumber);
        Assert.Equal(12m, saved.Quantity);
        Assert.Equal(4.5m, saved.UnitCost);
        Assert.Equal(expiry, saved.ExpiryDate);
        Assert.Equal(_variantId, saved.VariantId);
        Assert.Equal(_data.WarehouseId, saved.WarehouseId);
    }

    [Fact]
    public async Task GetById_ReturnsTheBatchWithItsVariantAndWarehouse()
    {
        await using var scope = CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IBatchService>();

        var batch = await service.CreateAsync(
            _variantId,
            _data.WarehouseId,
            "B-GET",
            DateTime.UtcNow.AddDays(10),
            quantity: 3m,
            unitCost: 1m);

        var loaded = await service.GetByIdAsync(batch.Id);

        Assert.NotNull(loaded);
        Assert.Equal("B-GET", loaded!.BatchNumber);
        Assert.NotNull(loaded.Variant);
        Assert.Equal(_variantId, loaded.Variant.Id);
        Assert.NotNull(loaded.Warehouse);
        Assert.Equal(_data.WarehouseId, loaded.Warehouse.Id);
    }

    [Fact]
    public async Task GetById_Unknown_ReturnsNull()
    {
        await using var scope = CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IBatchService>();

        Assert.Null(await service.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetByVariant_OrdersByEarliestExpiryWithNoExpiryLast()
    {
        await using var scope = CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IBatchService>();

        var noExpiry = await service.CreateAsync(
            _variantId, _data.WarehouseId, "B-NONE", null, 1m, 1m);
        var later = await service.CreateAsync(
            _variantId, _data.WarehouseId, "B-LATER", DateTime.UtcNow.AddDays(40), 1m, 1m);
        var sooner = await service.CreateAsync(
            _variantId, _data.WarehouseId, "B-SOONER", DateTime.UtcNow.AddDays(5), 1m, 1m);

        var batches = await service.GetByVariantAsync(_variantId);

        Assert.Equal(
            new[] { sooner.Id, later.Id, noExpiry.Id },
            batches.Select(b => b.Id).ToArray());
    }

    [Fact]
    public async Task GetByWarehouse_FiltersByWarehouse()
    {
        await using var scope = CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IBatchService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var otherWarehouse = new Warehouse
        {
            Name = "Warehouse B",
            BranchId = _data.BranchId,
            IsActive = true
        };
        context.Warehouses.Add(otherWarehouse);
        await context.SaveChangesAsync();

        var here = await service.CreateAsync(
            _variantId, _data.WarehouseId, "B-HERE", DateTime.UtcNow.AddDays(10), 2m, 1m);
        await service.CreateAsync(
            _variantId, otherWarehouse.Id, "B-THERE", DateTime.UtcNow.AddDays(10), 2m, 1m);

        var batches = await service.GetByWarehouseAsync(_data.WarehouseId);

        Assert.Single(batches);
        Assert.Equal(here.Id, batches[0].Id);
    }

    [Fact]
    public async Task GetExpiringWithinDays_ReturnsOnlyUpcomingBatches()
    {
        await using var scope = CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IBatchService>();

        var expired = await service.CreateAsync(
            _variantId, _data.WarehouseId, "B-EXPIRED", DateTime.UtcNow.AddDays(-1), 1m, 1m);
        var soon = await service.CreateAsync(
            _variantId, _data.WarehouseId, "B-SOON", DateTime.UtcNow.AddDays(5), 1m, 1m);
        var later = await service.CreateAsync(
            _variantId, _data.WarehouseId, "B-LATER", DateTime.UtcNow.AddDays(40), 1m, 1m);
        var noExpiry = await service.CreateAsync(
            _variantId, _data.WarehouseId, "B-NONE", null, 1m, 1m);

        var expiring = await service.GetExpiringWithinDaysAsync(10);

        Assert.Single(expiring);
        Assert.Equal(soon.Id, expiring[0].Id);
        Assert.DoesNotContain(expiring, b => b.Id == expired.Id);
        Assert.DoesNotContain(expiring, b => b.Id == later.Id);
        Assert.DoesNotContain(expiring, b => b.Id == noExpiry.Id);
    }

    [Fact]
    public async Task AdjustQuantity_AddsAndClampsAtZero()
    {
        await using var scope = CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IBatchService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var batch = await service.CreateAsync(
            _variantId, _data.WarehouseId, "B-ADJUST", DateTime.UtcNow.AddDays(10), 3m, 1m);

        await service.AdjustQuantityAsync(batch.Id, 7m);
        Assert.Equal(10m, (await context.Batches.AsNoTracking()
            .SingleAsync(b => b.Id == batch.Id)).Quantity);

        await service.AdjustQuantityAsync(batch.Id, -50m);
        Assert.Equal(0m, (await context.Batches.AsNoTracking()
            .SingleAsync(b => b.Id == batch.Id)).Quantity);
    }

    [Fact]
    public async Task GetFEFOSelection_ConsumesEarliestExpiryFirst_AndStopsAtRequiredQuantity()
    {
        await using var scope = CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IBatchService>();

        var later = await service.CreateAsync(
            _variantId, _data.WarehouseId, "B-LATER", DateTime.UtcNow.AddDays(20), 5m, 1m);
        var earlier = await service.CreateAsync(
            _variantId, _data.WarehouseId, "B-EARLIER", DateTime.UtcNow.AddDays(5), 4m, 1m);

        var selected = await service.GetFEFOSelectionAsync(_variantId, _data.WarehouseId, 6m);

        Assert.Equal(2, selected.Count);
        Assert.Equal(earlier.Id, selected[0].Id);
        Assert.Equal(4m, selected[0].Quantity);
        Assert.Equal(later.Id, selected[1].Id);
        Assert.Equal(2m, selected[1].Quantity);
        Assert.Equal(6m, selected.Sum(b => b.Quantity));
    }

    [Fact]
    public async Task GetFEFOSelection_SkipsEmptyBatches()
    {
        await using var scope = CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IBatchService>();

        var empty = await service.CreateAsync(
            _variantId, _data.WarehouseId, "B-EMPTY", DateTime.UtcNow.AddDays(1), 0m, 1m);
        var stocked = await service.CreateAsync(
            _variantId, _data.WarehouseId, "B-STOCKED", DateTime.UtcNow.AddDays(10), 8m, 1m);

        var selected = await service.GetFEFOSelectionAsync(_variantId, _data.WarehouseId, 3m);

        Assert.Single(selected);
        Assert.Equal(stocked.Id, selected[0].Id);
        Assert.Equal(3m, selected[0].Quantity);
        Assert.DoesNotContain(selected, b => b.Id == empty.Id);
    }

    private static (ServiceProvider Provider, string DbPath) BuildProvider()
    {
        var directory = Path.Combine(Path.GetTempPath(), "DzentoPOS2.Batches");
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
