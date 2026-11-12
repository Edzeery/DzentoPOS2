using DzentoPOS2.App;
using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Data;
using DzentoPOS2.Data.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DzentoPOS2.IntegrationTests;

/// <summary>
/// Serialized-stock lifecycle (Phase 6.1): lookup by serial/IMEI, availability
/// filtering, sale linkage and warranty dates, and return handling.
/// Each test receives its own migrated database.
/// </summary>
public sealed class SerialUnitTests : IAsyncLifetime
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

        var variant = await context.ProductVariants
            .FirstAsync(v => v.ProductId == _data.TrackedProductId && v.IsDefault);
        variant.WarrantyMonths = 12;
        await context.SaveChangesAsync();
        _variantId = variant.Id;
    }

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();
        SqliteConnection.ClearAllPools();

        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }

    private AsyncServiceScope CreateScope() => _provider.CreateAsyncScope();

    private SerialUnit NewUnit(
        string? serial = null,
        string? imei1 = null,
        string? imei2 = null,
        Guid? warehouseId = null)
    {
        return new SerialUnit
        {
            VariantId = _variantId,
            WarehouseId = warehouseId ?? _data.WarehouseId,
            SerialNumber = serial,
            Imei1 = imei1,
            Imei2 = imei2,
            Status = SerialUnitStatus.Available,
            UnitCost = 5m,
            ReceivedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Create_PersistsAnAvailableUnit()
    {
        await using var scope = CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ISerialUnitService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var unit = NewUnit("SN-CREATE", "IMEI-CREATE-1");
        await service.CreateAsync(unit);

        var saved = await context.SerialUnits.AsNoTracking().SingleAsync(s => s.Id == unit.Id);
        Assert.Equal(SerialUnitStatus.Available, saved.Status);
        Assert.Equal("SN-CREATE", saved.SerialNumber);
        Assert.Equal("IMEI-CREATE-1", saved.Imei1);
        Assert.Equal(_variantId, saved.VariantId);
    }

    [Fact]
    public async Task GetById_ReturnsTheUnit()
    {
        await using var scope = CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ISerialUnitService>();

        var unit = NewUnit("SN-GET");
        await service.CreateAsync(unit);

        var loaded = await service.GetByIdAsync(unit.Id);
        Assert.NotNull(loaded);
        Assert.Equal(unit.Id, loaded!.Id);
        Assert.Equal("SN-GET", loaded.SerialNumber);
    }

    [Fact]
    public async Task GetById_Unknown_ReturnsNull()
    {
        await using var scope = CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ISerialUnitService>();

        Assert.Null(await service.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetBySerialOrImei_FindsSerialAndBothImeis()
    {
        await using var scope = CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ISerialUnitService>();

        var bySerial = NewUnit(serial: "SN-LOOKUP");
        var byImei1 = NewUnit(imei1: "IMEI-LOOKUP-1");
        var byImei2 = NewUnit(imei2: "IMEI-LOOKUP-2");

        await service.CreateAsync(bySerial);
        await service.CreateAsync(byImei1);
        await service.CreateAsync(byImei2);

        Assert.Equal(bySerial.Id, (await service.GetBySerialOrImeiAsync("SN-LOOKUP"))!.Id);
        Assert.Equal(byImei1.Id, (await service.GetBySerialOrImeiAsync("IMEI-LOOKUP-1"))!.Id);
        Assert.Equal(byImei2.Id, (await service.GetBySerialOrImeiAsync("IMEI-LOOKUP-2"))!.Id);

        // Surrounding whitespace is trimmed before lookup.
        Assert.Equal(bySerial.Id, (await service.GetBySerialOrImeiAsync("  SN-LOOKUP  "))!.Id);
    }

    [Fact]
    public async Task GetBySerialOrImei_BlankOrUnknown_ReturnsNull()
    {
        await using var scope = CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ISerialUnitService>();

        await service.CreateAsync(NewUnit(serial: "SN-KNOWN"));

        Assert.Null(await service.GetBySerialOrImeiAsync(null!));
        Assert.Null(await service.GetBySerialOrImeiAsync("   "));
        Assert.Null(await service.GetBySerialOrImeiAsync("SN-UNKNOWN"));
    }

    [Fact]
    public async Task GetAvailableUnits_FiltersByVariantWarehouseAndStatus()
    {
        await using var scope = CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ISerialUnitService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var otherWarehouse = new Warehouse
        {
            Name = "Warehouse B",
            BranchId = _data.BranchId,
            IsActive = true
        };
        context.Warehouses.Add(otherWarehouse);
        await context.SaveChangesAsync();

        var availableHere = NewUnit(serial: "SN-AVAILABLE");
        var soldHere = NewUnit(serial: "SN-SOLD");
        var availableElsewhere = NewUnit(serial: "SN-ELSEWHERE", warehouseId: otherWarehouse.Id);

        await service.CreateAsync(availableHere);
        await service.CreateAsync(soldHere);
        await service.CreateAsync(availableElsewhere);
        await service.MarkSoldAsync(soldHere.Id, Guid.NewGuid(), DateTime.UtcNow);

        var units = await service.GetAvailableUnitsAsync(_variantId, _data.WarehouseId);

        Assert.Single(units);
        Assert.Equal(availableHere.Id, units[0].Id);
    }

    [Fact]
    public async Task MarkSold_SetsStatusInvoiceLinkAndSaleDate()
    {
        await using var scope = CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ISerialUnitService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var unit = NewUnit(serial: "SN-MARK-SOLD");
        await service.CreateAsync(unit);
        var invoiceItemId = Guid.NewGuid();
        var saleDate = DateTime.UtcNow.AddMinutes(-5);

        await service.MarkSoldAsync(unit.Id, invoiceItemId, saleDate);

        var saved = await context.SerialUnits.AsNoTracking().SingleAsync(s => s.Id == unit.Id);
        Assert.Equal(SerialUnitStatus.Sold, saved.Status);
        Assert.Equal(invoiceItemId, saved.SalesInvoiceItemId);
        Assert.Equal(saleDate, saved.SoldAt);
        Assert.Equal(saleDate, saved.WarrantyStartsAt);
    }

    [Fact]
    public async Task MarkSold_ComputesWarrantyExpiryFromTheVariant()
    {
        await using var scope = CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ISerialUnitService>();

        var unit = NewUnit(serial: "SN-WARRANTY");
        await service.CreateAsync(unit);
        var saleDate = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);

        await service.MarkSoldAsync(unit.Id, Guid.NewGuid(), saleDate);

        var saved = await service.GetByIdAsync(unit.Id);
        Assert.NotNull(saved);
        Assert.Equal(saleDate.AddMonths(12), saved!.WarrantyExpiresAt);
    }

    [Fact]
    public async Task MarkReturned_ClearsSaleFieldsAndRestoresReturnedStatus()
    {
        await using var scope = CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ISerialUnitService>();

        var unit = NewUnit(serial: "SN-RETURN");
        await service.CreateAsync(unit);
        await service.MarkSoldAsync(unit.Id, Guid.NewGuid(), DateTime.UtcNow);

        await service.MarkReturnedAsync(unit.Id);

        var saved = await service.GetByIdAsync(unit.Id);
        Assert.NotNull(saved);
        Assert.Equal(SerialUnitStatus.Returned, saved!.Status);
        Assert.Null(saved.SalesInvoiceItemId);
        Assert.Null(saved.SoldAt);
        Assert.Null(saved.WarrantyStartsAt);
        Assert.Null(saved.WarrantyExpiresAt);
    }

    [Fact]
    public async Task GetWarrantyInfo_ReturnsProductNameAndWarrantyState()
    {
        await using var scope = CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ISerialUnitService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var unit = NewUnit(serial: "SN-INFO", imei1: "IMEI-INFO");
        await service.CreateAsync(unit);
        var saleDate = DateTime.UtcNow.AddDays(-1);
        await service.MarkSoldAsync(unit.Id, Guid.NewGuid(), saleDate);

        var info = await service.GetWarrantyInfoAsync("IMEI-INFO");

        Assert.NotNull(info);
        Assert.Equal("SN-INFO", info!.SerialNumber);
        Assert.Equal("IMEI-INFO", info.IMEI);
        Assert.Equal("Tracked Product", info.ProductName);
        Assert.Equal(SerialUnitStatus.Sold, info.Status);
        Assert.Equal(saleDate, info.SaleDate);
        Assert.True(info.IsUnderWarranty);

        var missing = await service.GetWarrantyInfoAsync("SN-NOT-FOUND");
        Assert.Null(missing);

        var productName = await context.Products
            .Where(p => p.Id == _data.TrackedProductId)
            .Select(p => p.Name)
            .SingleAsync();
        Assert.Equal(productName, info.ProductName);
    }

    [Fact]
    public async Task MarkSold_AndMarkReturned_UnknownUnit_AreNoOps()
    {
        await using var scope = CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ISerialUnitService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await service.MarkSoldAsync(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
        await service.MarkReturnedAsync(Guid.NewGuid());

        Assert.Equal(0, await context.SerialUnits.CountAsync());
    }

    private static (ServiceProvider Provider, string DbPath) BuildProvider()
    {
        var directory = Path.Combine(Path.GetTempPath(), "DzentoPOS2.SerialUnits");
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
