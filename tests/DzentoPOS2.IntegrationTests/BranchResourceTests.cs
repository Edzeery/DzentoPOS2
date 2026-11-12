using DzentoPOS2.App;
using DzentoPOS2.App.Authorization;
using DzentoPOS2.App.Services.Businesses;
using DzentoPOS2.App.Session;
using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Core.Exceptions;
using DzentoPOS2.Data;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Seed;
using DzentoPOS2.Shared.Contracts.DTOs.Businesses;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DzentoPOS2.IntegrationTests;

/// <summary>
/// Phase 3 — branch warehouses and cashboxes. They are added and
/// activated/deactivated, never deleted; the service refuses to leave a branch
/// with no active warehouse or cashbox, and refuses to deactivate a cashbox
/// with an open session.
/// </summary>
public sealed class BranchResourceTests
{
    [Fact]
    public async Task CreateWarehouse_AddsRowToTheBranch()
    {
        await WithStoreAsync(async provider =>
        {
            await using var scope = provider.CreateAsyncScope();
            await SignInAsAdminAsync(scope.ServiceProvider);

            var branchService = scope.ServiceProvider.GetRequiredService<IBranchService>();
            var service = scope.ServiceProvider.GetRequiredService<IBranchResourceService>();

            var branch = await branchService.CreateAsync(new CreateBranchDto { Name = "North" });

            var created = await service.CreateWarehouseAsync(new CreateWarehouseDto
            {
                BranchId = branch.Id,
                Name = "Cold storage",
                Location = "Ground floor"
            });

            Assert.Equal("Cold storage", created.Name);
            Assert.Equal("Ground floor", created.Location);
            Assert.True(created.IsActive);

            var warehouses = await service.GetWarehousesAsync(branch.Id);
            Assert.Contains(warehouses, w => w.Name == "Cold storage");
        });
    }

    [Fact]
    public async Task CreateWarehouse_WithDuplicateName_IsRejected()
    {
        await WithStoreAsync(async provider =>
        {
            await using var scope = provider.CreateAsyncScope();
            await SignInAsAdminAsync(scope.ServiceProvider);

            var branchService = scope.ServiceProvider.GetRequiredService<IBranchService>();
            var service = scope.ServiceProvider.GetRequiredService<IBranchResourceService>();
            var branch = await branchService.CreateAsync(new CreateBranchDto { Name = "North" });

            await service.CreateWarehouseAsync(new CreateWarehouseDto { BranchId = branch.Id, Name = "Same" });

            var ex = await Assert.ThrowsAsync<CodedDomainException>(() =>
                service.CreateWarehouseAsync(new CreateWarehouseDto { BranchId = branch.Id, Name = "Same" }));
            Assert.Equal(DomainErrorCodes.WarehouseDuplicateName, ex.Code);
        });
    }

    [Fact]
    public async Task Deactivate_LastActiveWarehouse_IsRejected()
    {
        await WithStoreAsync(async provider =>
        {
            await using var scope = provider.CreateAsyncScope();
            await SignInAsAdminAsync(scope.ServiceProvider);

            var branchService = scope.ServiceProvider.GetRequiredService<IBranchService>();
            var service = scope.ServiceProvider.GetRequiredService<IBranchResourceService>();
            var branch = await branchService.CreateAsync(new CreateBranchDto { Name = "North" });

            // A new branch has exactly one (default) warehouse.
            var only = (await service.GetWarehousesAsync(branch.Id)).Single();

            var ex = await Assert.ThrowsAsync<CodedDomainException>(() =>
                service.SetWarehouseActiveAsync(only.Id, false));
            Assert.Equal(DomainErrorCodes.WarehouseLastActive, ex.Code);
        });
    }

    [Fact]
    public async Task Deactivate_ThenReactivate_Warehouse_RoundTrips()
    {
        await WithStoreAsync(async provider =>
        {
            await using var scope = provider.CreateAsyncScope();
            await SignInAsAdminAsync(scope.ServiceProvider);

            var branchService = scope.ServiceProvider.GetRequiredService<IBranchService>();
            var service = scope.ServiceProvider.GetRequiredService<IBranchResourceService>();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var branch = await branchService.CreateAsync(new CreateBranchDto { Name = "North" });

            var extra = await service.CreateWarehouseAsync(new CreateWarehouseDto
            {
                BranchId = branch.Id,
                Name = "Overflow"
            });

            var deactivated = await service.SetWarehouseActiveAsync(extra.Id, false);
            Assert.False(deactivated.IsActive);

            var row = await context.Warehouses.AsNoTracking().SingleAsync(w => w.Id == extra.Id);
            Assert.False(row.IsDeleted); // deactivation never deletes

            var reactivated = await service.SetWarehouseActiveAsync(extra.Id, true);
            Assert.True(reactivated.IsActive);
        });
    }

    [Fact]
    public async Task CreateCashbox_AddsRowToTheBranch()
    {
        await WithStoreAsync(async provider =>
        {
            await using var scope = provider.CreateAsyncScope();
            await SignInAsAdminAsync(scope.ServiceProvider);

            var branchService = scope.ServiceProvider.GetRequiredService<IBranchService>();
            var service = scope.ServiceProvider.GetRequiredService<IBranchResourceService>();
            var branch = await branchService.CreateAsync(new CreateBranchDto { Name = "North" });

            var created = await service.CreateCashboxAsync(new CreateCashboxDto
            {
                BranchId = branch.Id,
                Name = "Front desk"
            });

            Assert.Equal("Front desk", created.Name);
            Assert.True(created.IsActive);

            var cashboxes = await service.GetCashboxesAsync(branch.Id);
            Assert.Contains(cashboxes, c => c.Name == "Front desk");
        });
    }

    [Fact]
    public async Task CreateCashbox_WithDuplicateName_IsRejected()
    {
        await WithStoreAsync(async provider =>
        {
            await using var scope = provider.CreateAsyncScope();
            await SignInAsAdminAsync(scope.ServiceProvider);

            var branchService = scope.ServiceProvider.GetRequiredService<IBranchService>();
            var service = scope.ServiceProvider.GetRequiredService<IBranchResourceService>();
            var branch = await branchService.CreateAsync(new CreateBranchDto { Name = "North" });

            await service.CreateCashboxAsync(new CreateCashboxDto { BranchId = branch.Id, Name = "Same" });

            var ex = await Assert.ThrowsAsync<CodedDomainException>(() =>
                service.CreateCashboxAsync(new CreateCashboxDto { BranchId = branch.Id, Name = "Same" }));
            Assert.Equal(DomainErrorCodes.CashboxDuplicateName, ex.Code);
        });
    }

    [Fact]
    public async Task Deactivate_LastActiveCashbox_IsRejected()
    {
        await WithStoreAsync(async provider =>
        {
            await using var scope = provider.CreateAsyncScope();
            await SignInAsAdminAsync(scope.ServiceProvider);

            var branchService = scope.ServiceProvider.GetRequiredService<IBranchService>();
            var service = scope.ServiceProvider.GetRequiredService<IBranchResourceService>();
            var branch = await branchService.CreateAsync(new CreateBranchDto { Name = "North" });

            var only = (await service.GetCashboxesAsync(branch.Id)).Single();

            var ex = await Assert.ThrowsAsync<CodedDomainException>(() =>
                service.SetCashboxActiveAsync(only.Id, false));
            Assert.Equal(DomainErrorCodes.CashboxLastActive, ex.Code);
        });
    }

    [Fact]
    public async Task Deactivate_CashboxWithOpenSession_IsRejected()
    {
        await WithStoreAsync(async provider =>
        {
            await using var scope = provider.CreateAsyncScope();
            await SignInAsAdminAsync(scope.ServiceProvider);

            var branchService = scope.ServiceProvider.GetRequiredService<IBranchService>();
            var service = scope.ServiceProvider.GetRequiredService<IBranchResourceService>();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var branch = await branchService.CreateAsync(new CreateBranchDto { Name = "North" });

            // Two active cashboxes so the "last active" rule does not fire first.
            var extra = await service.CreateCashboxAsync(new CreateCashboxDto
            {
                BranchId = branch.Id,
                Name = "Second"
            });

            context.CashSessions.Add(new CashSession
            {
                CashboxId = extra.Id,
                OpenedAtUtc = DateTime.UtcNow,
                OpenedByUserId = Guid.NewGuid(),
                OpeningBalance = 0m,
                Status = CashSessionStatus.Open
            });
            await context.SaveChangesAsync();

            var ex = await Assert.ThrowsAsync<CodedDomainException>(() =>
                service.SetCashboxActiveAsync(extra.Id, false));
            Assert.Equal(DomainErrorCodes.CashboxHasOpenSession, ex.Code);
        });
    }

    [Fact]
    public async Task Deactivate_ThenReactivate_Cashbox_RoundTrips()
    {
        await WithStoreAsync(async provider =>
        {
            await using var scope = provider.CreateAsyncScope();
            await SignInAsAdminAsync(scope.ServiceProvider);

            var branchService = scope.ServiceProvider.GetRequiredService<IBranchService>();
            var service = scope.ServiceProvider.GetRequiredService<IBranchResourceService>();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var branch = await branchService.CreateAsync(new CreateBranchDto { Name = "North" });

            var extra = await service.CreateCashboxAsync(new CreateCashboxDto
            {
                BranchId = branch.Id,
                Name = "Second"
            });

            var deactivated = await service.SetCashboxActiveAsync(extra.Id, false);
            Assert.False(deactivated.IsActive);

            var row = await context.Cashboxes.AsNoTracking().SingleAsync(c => c.Id == extra.Id);
            Assert.False(row.IsDeleted); // deactivation never deletes

            var reactivated = await service.SetCashboxActiveAsync(extra.Id, true);
            Assert.True(reactivated.IsActive);
        });
    }

    [Fact]
    public async Task Cashier_WithoutPermission_IsDenied()
    {
        await WithStoreAsync(async provider =>
        {
            Guid branchId;
            await using (var setup = provider.CreateAsyncScope())
            {
                await SignInAsAdminAsync(setup.ServiceProvider);
                var branchService = setup.ServiceProvider.GetRequiredService<IBranchService>();
                branchId = (await branchService.CreateAsync(new CreateBranchDto { Name = "North" })).Id;
            }

            await using var scope = provider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var cashierId = await context.Users
                .AsNoTracking()
                .Where(u => u.UserName == "cashier")
                .Select(u => u.Id)
                .SingleAsync();

            SetSession(scope.ServiceProvider, cashierId, "cashier", "Cashier", isAdmin: false);

            var service = scope.ServiceProvider.GetRequiredService<IBranchResourceService>();
            var ex = await Assert.ThrowsAsync<CodedDomainException>(() =>
                service.GetWarehousesAsync(branchId));
            Assert.Equal(DomainErrorCodes.PermissionDenied, ex.Code);
        });
    }

    private static async Task SignInAsAdminAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();
        var adminId = await context.Users
            .AsNoTracking()
            .Where(u => u.UserName == "admin")
            .Select(u => u.Id)
            .SingleAsync();

        SetSession(serviceProvider, adminId, "admin", "SuperAdmin", isAdmin: true);
    }

    private static void SetSession(
        IServiceProvider serviceProvider,
        Guid userId,
        string userName,
        string roleName,
        bool isAdmin)
    {
        var currentUser = serviceProvider.GetRequiredService<ICurrentUserService>();
        currentUser.SetUser(new UserSession
        {
            UserId = userId,
            UserName = userName,
            FullName = userName,
            RoleName = roleName,
            IsAdmin = isAdmin
        });
    }

    private static async Task WithStoreAsync(Func<ServiceProvider, Task> body)
    {
        var (provider, dbPath) = BuildProvider();
        try
        {
            await DbInitializer.InitializeAsync(provider);
            await body(provider);
        }
        finally
        {
            await provider.DisposeAsync();
            SqliteConnection.ClearAllPools();
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    private static (ServiceProvider Provider, string DbPath) BuildProvider()
    {
        var directory = Path.Combine(Path.GetTempPath(), "DzentoPOS2.BranchResources");
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
