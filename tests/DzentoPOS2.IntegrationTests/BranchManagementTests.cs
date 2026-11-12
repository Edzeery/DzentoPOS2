using DzentoPOS2.App;
using DzentoPOS2.App.Authorization;
using DzentoPOS2.App.Services.Businesses;
using DzentoPOS2.App.Session;
using DzentoPOS2.Core.Constants;
using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Core.Entities.Identity;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Core.Exceptions;
using DzentoPOS2.Data;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Businesses.Interfaces;
using DzentoPOS2.Data.Repositories.Common.Interfaces;
using DzentoPOS2.Data.Seed;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Shared.Contracts.DTOs.Businesses;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DzentoPOS2.IntegrationTests;

/// <summary>
/// Phase 1 — branch management core. A branch is created together with its
/// default warehouse and cashbox in one transaction; a failure rolls all three
/// back. Deactivation never deletes rows and is rejected for the main branch,
/// the last active branch, or a branch whose cashbox has an open session.
/// </summary>
public sealed class BranchManagementTests
{
    [Fact]
    public async Task Create_InsertsBranchWarehouseAndCashboxAtomically()
    {
        await WithStoreAsync(async provider =>
        {
            await using var scope = provider.CreateAsyncScope();
            await SignInAsAdminAsync(scope.ServiceProvider);

            var service = scope.ServiceProvider.GetRequiredService<IBranchService>();
            var created = await service.CreateAsync(new CreateBranchDto
            {
                Name = "Downtown",
                City = "Algiers",
                PhoneNumber = "0550000000",
                IsActive = true
            });

            Assert.Equal("Downtown", created.Name);
            Assert.False(created.IsMainBranch); // a main branch already exists

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var warehouse = await context.Warehouses
                .AsNoTracking()
                .SingleAsync(w => w.BranchId == created.Id && !w.IsDeleted);
            var cashbox = await context.Cashboxes
                .AsNoTracking()
                .SingleAsync(c => c.BranchId == created.Id && !c.IsDeleted);

            Assert.Contains("Downtown", warehouse.Name);
            Assert.Contains(AppStrings.MainWarehouse, warehouse.Name);
            Assert.Contains("Downtown", cashbox.Name);
            Assert.Contains(AppStrings.MainCashbox, cashbox.Name);
        });
    }

    [Fact]
    public async Task Create_WithDuplicateName_IsRejectedAndWritesNothing()
    {
        await WithStoreAsync(async provider =>
        {
            await using var scope = provider.CreateAsyncScope();
            await SignInAsAdminAsync(scope.ServiceProvider);

            var service = scope.ServiceProvider.GetRequiredService<IBranchService>();
            await service.CreateAsync(new CreateBranchDto { Name = "Same" });

            var ex = await Assert.ThrowsAsync<CodedDomainException>(() =>
                service.CreateAsync(new CreateBranchDto { Name = "Same" }));

            Assert.Equal(DomainErrorCodes.BranchDuplicateName, ex.Code);

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Equal(1, await context.Branches.CountAsync(b => b.Name == "Same" && !b.IsDeleted));
        });
    }

    [Fact]
    public async Task Create_WhenCommitFails_RollsBackEveryRow()
    {
        await WithStoreAsync(async provider =>
        {
            string name;
            await using (var scope = provider.CreateAsyncScope())
            {
                await SignInAsAdminAsync(scope.ServiceProvider);

                var service = new BranchService(
                    scope.ServiceProvider.GetRequiredService<IBranchRepository>(),
                    new FailingCommitUnitOfWork(
                        scope.ServiceProvider.GetRequiredService<IUnitOfWork>()),
                    scope.ServiceProvider.GetRequiredService<AppDbContext>(),
                    scope.ServiceProvider.GetRequiredService<IAuthorizationGuard>(),
                    scope.ServiceProvider.GetRequiredService<IAuditService>());

                name = $"Rollback-{Guid.NewGuid():N}";

                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    service.CreateAsync(new CreateBranchDto { Name = name }));
            }

            // Read the committed database through a fresh scope: the rolled-back
            // branch, warehouse and cashbox must not be there.
            await using var verify = provider.CreateAsyncScope();
            var context = verify.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.False(await context.Branches.AnyAsync(b => b.Name == name && !b.IsDeleted));
            Assert.Equal(0, await context.Warehouses.CountAsync(w => w.Name.StartsWith(name)));
            Assert.Equal(0, await context.Cashboxes.CountAsync(c => c.Name.StartsWith(name)));
        });
    }

    [Fact]
    public async Task Manager_HasBranchesManage_ButCashierDoesNot()
    {
        await WithStoreAsync(async provider =>
        {
            await using var scope = provider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var managerPermissions = await PermissionsForRoleAsync(context, "Manager");
            var cashierPermissions = await PermissionsForRoleAsync(context, "Cashier");

            Assert.Contains(Permissions.BranchesManage, managerPermissions);
            Assert.DoesNotContain(Permissions.BranchesManage, cashierPermissions);
            Assert.DoesNotContain(Permissions.BranchesView, cashierPermissions);
        });
    }

    [Fact]
    public async Task ManagerSession_CanListBranches_WhileCashierIsDenied()
    {
        await WithStoreAsync(async provider =>
        {
            var managerUserId = await CreateManagerUserAsync(provider);

            await using (var managerScope = provider.CreateAsyncScope())
            {
                SetSession(
                    managerScope.ServiceProvider,
                    managerUserId,
                    "manager",
                    "Manager",
                    isAdmin: false);

                var service = managerScope.ServiceProvider.GetRequiredService<IBranchService>();
                var branches = await service.GetAllAsync();
                Assert.NotEmpty(branches);
            }

            await using (var cashierScope = provider.CreateAsyncScope())
            {
                var context = cashierScope.ServiceProvider.GetRequiredService<AppDbContext>();
                var cashierId = await context.Users
                    .AsNoTracking()
                    .Where(u => u.UserName == "cashier")
                    .Select(u => u.Id)
                    .SingleAsync();

                SetSession(
                    cashierScope.ServiceProvider,
                    cashierId,
                    "cashier",
                    "Cashier",
                    isAdmin: false);

                var service = cashierScope.ServiceProvider.GetRequiredService<IBranchService>();
                var ex = await Assert.ThrowsAsync<CodedDomainException>(() => service.GetAllAsync());
                Assert.Equal(DomainErrorCodes.PermissionDenied, ex.Code);
            }
        });
    }

    [Fact]
    public async Task Deactivate_MainBranch_IsRejected()
    {
        await WithStoreAsync(async provider =>
        {
            await using var scope = provider.CreateAsyncScope();
            await SignInAsAdminAsync(scope.ServiceProvider);
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var service = scope.ServiceProvider.GetRequiredService<IBranchService>();

            var mainId = await context.Branches
                .AsNoTracking()
                .Where(b => b.IsMainBranch && !b.IsDeleted)
                .Select(b => b.Id)
                .SingleAsync();

            var ex = await Assert.ThrowsAsync<CodedDomainException>(() =>
                service.SetActiveAsync(mainId, false));
            Assert.Equal(DomainErrorCodes.BranchMainCannotDeactivate, ex.Code);
        });
    }

    [Fact]
    public async Task Deactivate_LastActiveBranch_IsRejected()
    {
        await WithStoreAsync(async provider =>
        {
            await using var scope = provider.CreateAsyncScope();
            await SignInAsAdminAsync(scope.ServiceProvider);
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var service = scope.ServiceProvider.GetRequiredService<IBranchService>();

            // Force a state where the only active branch is a non-main one.
            var main = await context.Branches.SingleAsync(b => b.IsMainBranch && !b.IsDeleted);
            main.IsActive = false;
            var onlyActive = new Branch { Name = "OnlyActive", IsActive = true };
            context.Branches.Add(onlyActive);
            await context.SaveChangesAsync();

            var ex = await Assert.ThrowsAsync<CodedDomainException>(() =>
                service.SetActiveAsync(onlyActive.Id, false));
            Assert.Equal(DomainErrorCodes.BranchLastActive, ex.Code);
        });
    }

    [Fact]
    public async Task Deactivate_WithOpenCashSession_IsRejected()
    {
        await WithStoreAsync(async provider =>
        {
            await using var scope = provider.CreateAsyncScope();
            await SignInAsAdminAsync(scope.ServiceProvider);
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var service = scope.ServiceProvider.GetRequiredService<IBranchService>();

            var branch = await service.CreateAsync(new CreateBranchDto { Name = "Busy" });
            var cashboxId = await context.Cashboxes
                .AsNoTracking()
                .Where(c => c.BranchId == branch.Id && !c.IsDeleted)
                .Select(c => c.Id)
                .SingleAsync();

            context.CashSessions.Add(new CashSession
            {
                CashboxId = cashboxId,
                OpenedAtUtc = DateTime.UtcNow,
                OpenedByUserId = Guid.NewGuid(),
                OpeningBalance = 0m,
                Status = CashSessionStatus.Open
            });
            await context.SaveChangesAsync();

            var ex = await Assert.ThrowsAsync<CodedDomainException>(() =>
                service.SetActiveAsync(branch.Id, false));
            Assert.Equal(DomainErrorCodes.BranchHasOpenSession, ex.Code);
        });
    }

    [Fact]
    public async Task Deactivate_ThenReactivate_RoundTrips()
    {
        await WithStoreAsync(async provider =>
        {
            await using var scope = provider.CreateAsyncScope();
            await SignInAsAdminAsync(scope.ServiceProvider);
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var service = scope.ServiceProvider.GetRequiredService<IBranchService>();

            var branch = await service.CreateAsync(new CreateBranchDto { Name = "Seasonal" });

            var deactivated = await service.SetActiveAsync(branch.Id, false);
            Assert.False(deactivated.IsActive);

            var stillThere = await context.Branches
                .AsNoTracking()
                .SingleAsync(b => b.Id == branch.Id);
            Assert.False(stillThere.IsDeleted); // deactivation never deletes

            var reactivated = await service.SetActiveAsync(branch.Id, true);
            Assert.True(reactivated.IsActive);
        });
    }

    private static async Task<List<string>> PermissionsForRoleAsync(AppDbContext context, string roleName)
    {
        return await (
            from rolePermission in context.RolePermissions
            join role in context.Roles on rolePermission.RoleId equals role.Id
            join permission in context.Permissions on rolePermission.PermissionId equals permission.Id
            where role.Name == roleName
            select permission.Code)
            .ToListAsync();
    }

    private static async Task<Guid> CreateManagerUserAsync(ServiceProvider provider)
    {
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var role = await context.Roles.SingleAsync(r => r.Name == "Manager");
        var user = new User
        {
            FullName = "Branch Manager",
            UserName = "manager",
            PasswordHash = "not-used-in-tests",
            IsActive = true
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        await context.SaveChangesAsync();

        return user.Id;
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
        var directory = Path.Combine(Path.GetTempPath(), "DzentoPOS2.BranchManagement");
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

    /// <summary>
    /// Wraps the real unit of work but fails at commit time, so the transaction
    /// is rolled back after the rows were written within it.
    /// </summary>
    private sealed class FailingCommitUnitOfWork : IUnitOfWork
    {
        private readonly IUnitOfWork _inner;

        public FailingCommitUnitOfWork(IUnitOfWork inner) => _inner = inner;

        public Task<int> SaveChangesAsync() => _inner.SaveChangesAsync();

        public Task BeginTransactionAsync() => _inner.BeginTransactionAsync();

        public Task CommitTransactionAsync()
            => throw new InvalidOperationException("Simulated commit failure.");

        public Task RollbackTransactionAsync() => _inner.RollbackTransactionAsync();

        public void Dispose() => _inner.Dispose();
    }
}
