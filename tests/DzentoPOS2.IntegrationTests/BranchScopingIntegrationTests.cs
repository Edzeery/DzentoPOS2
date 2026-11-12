using DzentoPOS2.App;
using DzentoPOS2.App.Services.Accounting;
using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.App.Services.Reports.Interfaces;
using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.App.Session;
using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Core.Exceptions;
using DzentoPOS2.Core.Services.Sales;
using DzentoPOS2.Core.ValueObjects.Inventory;
using DzentoPOS2.Data;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Shared.Contracts.DTOs.Accounting;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;
using DzentoPOS2.Shared.Contracts.DTOs.Reports;
using DzentoPOS2.Shared.Contracts.DTOs.Sales;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DzentoPOS2.IntegrationTests;

/// <summary>
/// Phase 6.0д-2: every branch-scoped read and write is isolated to the active
/// branch (cashboxes, sessions, valuation, snapshots, dashboard). Each test
/// owns its database so branch switches never leak into other tests.
/// </summary>
public sealed class BranchScopingIntegrationTests : IAsyncLifetime
{
    private ServiceProvider _provider = null!;
    private string _dbPath = string.Empty;
    private TestData _data = null!;

    public async Task InitializeAsync()
    {
        (_provider, _dbPath) = BuildProvider();

        await using var scope = _provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();

        _data = await TestData.SeedAsync(context);

        var currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
        currentUser.SetUser(new UserSession
        {
            UserId = _data.UserId,
            UserName = "cashier",
            FullName = "Test Cashier",
            RoleName = "SuperAdmin",
            BranchId = _data.BranchId,
            BranchName = "Main Branch",
            IsAdmin = true
        });

        var activeBranch = scope.ServiceProvider.GetRequiredService<IActiveBranchContext>();
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

    private static async Task<(Guid BranchId, Guid WarehouseId, Guid CashboxId)> CreateBranchBAsync(
        AppDbContext context)
    {
        var branch = new Branch
        {
            Name = "Branch B",
            IsMainBranch = false,
            IsActive = true
        };
        var warehouse = new Warehouse
        {
            Name = "Warehouse B",
            BranchId = branch.Id,
            IsActive = true
        };
        var cashbox = new Cashbox
        {
            Name = "Cashbox B",
            BranchId = branch.Id,
            IsActive = true
        };

        context.Branches.Add(branch);
        context.Warehouses.Add(warehouse);
        context.Cashboxes.Add(cashbox);
        await context.SaveChangesAsync();

        return (branch.Id, warehouse.Id, cashbox.Id);
    }

    private static CreateSaleDto Sale(Guid productId, decimal quantity, decimal price)
    {
        return new CreateSaleDto
        {
            CustomerId = Guid.Empty,
            DiscountMode = DiscountMode.Percent,
            DiscountValue = 0,
            TaxRate = 0,
            Fees = 0,
            PaidAmount = quantity * price,
            PaymentMethod = "Cash",
            Items =
            {
                new CreateSaleItemDto
                {
                    ProductId = productId,
                    Quantity = quantity,
                    Price = price
                }
            }
        };
    }

    [Fact]
    public async Task Cashboxes_ListOnlyTheActiveBranch()
    {
        await using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sessions = scope.ServiceProvider.GetRequiredService<ICashSessionService>();
        var activeBranch = scope.ServiceProvider.GetRequiredService<IActiveBranchContext>();

        var (branchB, _, cashboxB) = await CreateBranchBAsync(context);

        var boxesA = await sessions.GetCashboxesAsync();
        Assert.Contains(boxesA, b => b.Id == _data.CashboxId);
        Assert.DoesNotContain(boxesA, b => b.Id == cashboxB);

        Assert.True(await activeBranch.TrySwitchBranchAsync(branchB));

        var boxesB = await sessions.GetCashboxesAsync();
        Assert.Contains(boxesB, b => b.Id == cashboxB);
        Assert.DoesNotContain(boxesB, b => b.Id == _data.CashboxId);
    }

    [Fact]
    public async Task OpenSession_RejectsACashboxFromAnotherBranch()
    {
        await using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sessions = scope.ServiceProvider.GetRequiredService<ICashSessionService>();
        var activeBranch = scope.ServiceProvider.GetRequiredService<IActiveBranchContext>();

        var (branchB, _, cashboxB) = await CreateBranchBAsync(context);
        Assert.NotEqual(_data.BranchId, branchB);
        Assert.Equal(_data.BranchId, activeBranch.ActiveBranchId);

        var error = await Assert.ThrowsAsync<CodedDomainException>(
            () => sessions.OpenSessionAsync(new OpenCashSessionDto
            {
                CashboxId = cashboxB,
                OpeningBalance = 100m
            }));

        Assert.Equal(DomainErrorCodes.CashboxNotFound, error.Code);
        Assert.False(await context.CashSessions
            .AsNoTracking()
            .AnyAsync(s => s.CashboxId == cashboxB));
    }

    [Fact]
    public async Task OpenSession_IsIndependentPerBranch()
    {
        await using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sessions = scope.ServiceProvider.GetRequiredService<ICashSessionService>();
        var activeBranch = scope.ServiceProvider.GetRequiredService<IActiveBranchContext>();

        var (branchB, _, cashboxB) = await CreateBranchBAsync(context);

        // The fixture already has an open session on branch A.
        var sessionA = await sessions.GetOpenSessionAsync();
        Assert.NotNull(sessionA);
        Assert.Equal(_data.CashboxId, sessionA!.CashboxId);

        Assert.True(await activeBranch.TrySwitchBranchAsync(branchB));
        Assert.Null(await sessions.GetOpenSessionAsync());

        var sessionB = await sessions.OpenSessionAsync(new OpenCashSessionDto
        {
            CashboxId = cashboxB,
            OpeningBalance = 250m
        });
        Assert.Equal(cashboxB, sessionB.CashboxId);

        var openB = await sessions.GetOpenSessionAsync();
        Assert.NotNull(openB);
        Assert.Equal(sessionB.Id, openB!.Id);

        Assert.True(await activeBranch.TrySwitchBranchAsync(_data.BranchId));
        var openA = await sessions.GetOpenSessionAsync();
        Assert.NotNull(openA);
        Assert.Equal(sessionA.Id, openA!.Id);
    }

    [Fact]
    public async Task InventoryValuation_IsComputedPerBranch_AndCacheIsInvalidatedOnSwitch()
    {
        await using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var valuation = scope.ServiceProvider.GetRequiredService<IInventoryValuationService>();
        var activeBranch = scope.ServiceProvider.GetRequiredService<IActiveBranchContext>();

        var valuationA = await valuation.GetValuationAsync();
        Assert.True(valuationA.TotalQuantity >= 100m);

        var (branchB, _, _) = await CreateBranchBAsync(context);
        Assert.True(await activeBranch.TrySwitchBranchAsync(branchB));

        var valuationB = await valuation.GetValuationAsync();
        Assert.Equal(0m, valuationB.TotalQuantity);
        Assert.Equal(0m, valuationB.CostValue);

        Assert.True(await activeBranch.TrySwitchBranchAsync(_data.BranchId));
        var valuationAAgain = await valuation.GetValuationAsync();
        Assert.Equal(valuationA.TotalQuantity, valuationAAgain.TotalQuantity);
        Assert.Equal(valuationA.CostValue, valuationAAgain.CostValue);
    }

    [Fact]
    public async Task InventorySnapshot_IsWrittenForEachBranchSeparately()
    {
        await using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var snapshots = scope.ServiceProvider.GetRequiredService<IInventorySnapshotService>();
        var activeBranch = scope.ServiceProvider.GetRequiredService<IActiveBranchContext>();

        await snapshots.UpsertTodayAsync(new InventoryValuationDto
        {
            TotalQuantity = 11m,
            CostValue = 110m
        });

        var (branchB, _, _) = await CreateBranchBAsync(context);
        Assert.True(await activeBranch.TrySwitchBranchAsync(branchB));

        await snapshots.UpsertTodayAsync(new InventoryValuationDto
        {
            TotalQuantity = 22m,
            CostValue = 220m
        });

        var rows = await context.InventorySnapshots
            .AsNoTracking()
            .Where(s => s.BranchId == _data.BranchId || s.BranchId == branchB)
            .ToListAsync();

        Assert.Equal(2, rows.Count);

        var rowA = Assert.Single(rows, s => s.BranchId == _data.BranchId);
        Assert.Equal(11m, rowA.TotalQuantity);
        Assert.Equal(110m, rowA.CostValue);

        var rowB = Assert.Single(rows, s => s.BranchId == branchB);
        Assert.Equal(22m, rowB.TotalQuantity);
        Assert.Equal(220m, rowB.CostValue);
    }

    [Fact]
    public async Task Dashboard_ViewAllWithoutPermission_FallsBackToActiveBranch()
    {
        await using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var activeBranch = scope.ServiceProvider.GetRequiredService<IActiveBranchContext>();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();
        var dashboard = scope.ServiceProvider.GetRequiredService<IDashboardService>();

        var baseline = await dashboard.GetSummaryAsync(
            new DashboardFilterDto { Preset = DashboardDatePreset.Today });

        var (branchB, _, cashboxB) = await CreateBranchBAsync(context);
        context.CashSessions.Add(new CashSession
        {
            CashboxId = cashboxB,
            OpenedAtUtc = DateTime.UtcNow,
            OpenedByUserId = _data.UserId,
            OpeningBalance = 100m,
            Status = CashSessionStatus.Open
        });
        await context.SaveChangesAsync();

        Assert.True(await activeBranch.TrySwitchBranchAsync(branchB));
        var saleB = await sales.CreateAsync(Sale(_data.TrackedProductId, 1m, 10m));
        Assert.NotEqual(Guid.Empty, saleB.Id);

        Assert.True(await activeBranch.TrySwitchBranchAsync(_data.BranchId));

        // A cashier has no BranchesViewAll permission: the requested scope is
        // ignored and only the active branch is reported.
        var currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
        currentUser.SetUser(new UserSession
        {
            UserId = _data.UserId,
            UserName = "cashier",
            FullName = "Test Cashier",
            RoleName = "Cashier",
            BranchId = _data.BranchId,
            BranchName = "Main Branch",
            IsAdmin = false
        });

        var withoutPermission = await dashboard.GetSummaryAsync(new DashboardFilterDto
        {
            Preset = DashboardDatePreset.Today,
            ViewAllBranches = true
        });
        Assert.Equal(baseline.SalesCount.Value, withoutPermission.SalesCount.Value);
        Assert.Equal(baseline.SalesToday.Value, withoutPermission.SalesToday.Value);

        // With the permission (SuperAdmin bypass) the same flag includes the
        // invoice created in branch B.
        currentUser.SetUser(new UserSession
        {
            UserId = _data.UserId,
            UserName = "cashier",
            FullName = "Test Cashier",
            RoleName = "SuperAdmin",
            BranchId = _data.BranchId,
            BranchName = "Main Branch",
            IsAdmin = true
        });

        var withPermission = await dashboard.GetSummaryAsync(new DashboardFilterDto
        {
            Preset = DashboardDatePreset.Today,
            ViewAllBranches = true
        });
        Assert.Equal(1m, withPermission.SalesCount.Value);
        Assert.Equal(10m, withPermission.SalesToday.Value);
    }

    private static (ServiceProvider Provider, string DbPath) BuildProvider()
    {
        var directory = Path.Combine(Path.GetTempPath(), "DzentoPOS2.BranchScoping");
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
