using DzentoPOS2.App;
using DzentoPOS2.App.Services.Accounting;
using DzentoPOS2.App.Services.Identity.Interfaces;
using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.App.Services.Purchases;
using DzentoPOS2.App.Services.Reports.Interfaces;
using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.App.Session;
using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Core.Entities.Identity;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Core.Entities.Purchases;
using DzentoPOS2.Core.Entities.Sales;
using DzentoPOS2.Core.Services.Sales;
using DzentoPOS2.Data;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Seed;
using DzentoPOS2.Shared.Contracts.DTOs.Accounting;
using DzentoPOS2.Shared.Contracts.DTOs.Identity.Auth;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;
using DzentoPOS2.Shared.Contracts.DTOs.Purchases;
using DzentoPOS2.Shared.Contracts.DTOs.Reports;
using DzentoPOS2.Shared.Contracts.DTOs.Sales;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DzentoPOS2.IntegrationTests;

/// <summary>
/// End-to-end startup checks against the real bootstrap path: a fresh database
/// goes through <see cref="DbInitializer"/> (migrations + all seeders), the
/// seeded administrator signs in, and every module writes and reads through
/// the same services the shell uses. The upgrade test replays legacy rows whose
/// BranchId is still empty.
/// </summary>
public sealed class RealPathIntegrationTests
{
    [Fact]
    public async Task FreshDatabase_BootstrapAdminLogin_ThenEveryModuleWritesAndReads()
    {
        var (provider, dbPath) = BuildProvider();

        try
        {
            await DbInitializer.InitializeAsync(provider);

            await using var scope = provider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var mainBranch = await context.Branches
                .AsNoTracking()
                .SingleAsync(b => b.IsMainBranch && b.IsActive && !b.IsDeleted);

            Assert.True(await context.Warehouses
                .AsNoTracking()
                .AnyAsync(w => w.BranchId == mainBranch.Id && w.IsActive));
            Assert.True(await context.Cashboxes
                .AsNoTracking()
                .AnyAsync(c => c.BranchId == mainBranch.Id && c.IsActive));

            var auth = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var login = await auth.LoginAsync(new LoginDto
            {
                UserName = "admin",
                Password = "admin"
            });

            Assert.True(login.Success, login.Message);
            Assert.NotNull(login.Data);
            Assert.True(login.Data!.IsAdmin);
            Assert.Equal(mainBranch.Id, login.Data.BranchId);

            var currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
            var activeBranch = scope.ServiceProvider.GetRequiredService<IActiveBranchContext>();
            await activeBranch.InitializeAsync(currentUser);
            Assert.Equal(mainBranch.Id, activeBranch.ActiveBranchId);

            var cashboxId = await context.Cashboxes
                .AsNoTracking()
                .Where(c => c.BranchId == mainBranch.Id && c.IsActive)
                .OrderBy(c => c.CreatedAt)
                .ThenBy(c => c.Id)
                .Select(c => c.Id)
                .FirstAsync();

            var sessions = scope.ServiceProvider.GetRequiredService<ICashSessionService>();
            var session = await sessions.OpenSessionAsync(new OpenCashSessionDto
            {
                CashboxId = cashboxId,
                OpeningBalance = 1000m,
                Notes = "Real-path integration"
            });
            Assert.Equal(cashboxId, session.CashboxId);

            var categoryId = await context.ProductCategories
                .AsNoTracking()
                .Select(c => c.Id)
                .FirstAsync();
            var unitId = await context.ProductUnits
                .AsNoTracking()
                .Select(u => u.Id)
                .FirstAsync();

            var products = scope.ServiceProvider.GetRequiredService<IProductService>();
            var product = await products.CreateAsync(new CreateProductDto
            {
                Name = "Real Path Product",
                SKU = $"RP-{Guid.NewGuid():N}",
                CategoryId = categoryId,
                UnitId = unitId,
                CostPrice = 5m,
                SalePrice = 10m,
                MinimumStock = 0m,
                OpeningStock = 10m,
                TrackStock = true,
                IsActive = true
            });

            var purchases = scope.ServiceProvider.GetRequiredService<IPurchaseService>();
            var purchase = await purchases.CreateAsync(new CreatePurchaseInvoiceDto
            {
                SupplierId = Guid.NewGuid(),
                PaidAmount = 40m,
                Items =
                {
                    new CreatePurchaseItemDto
                    {
                        ProductId = product.Id,
                        Quantity = 10m,
                        CostPrice = 4m
                    }
                }
            });
            Assert.Equal(40m, purchase.Total);

            var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();
            var sale = await sales.CreateAsync(new CreateSaleDto
            {
                CustomerId = Guid.Empty,
                DiscountMode = DiscountMode.Percent,
                DiscountValue = 0m,
                TaxRate = 0m,
                Fees = 0m,
                PaidAmount = 20m,
                PaymentMethod = "Cash",
                Items =
                {
                    new CreateSaleItemDto
                    {
                        ProductId = product.Id,
                        Quantity = 2m,
                        Price = 10m
                    }
                }
            });
            Assert.Equal(20m, sale.Total);

            var expenseCategoryId = await context.ExpenseCategories
                .AsNoTracking()
                .Select(c => c.Id)
                .FirstAsync();

            var expenses = scope.ServiceProvider.GetRequiredService<IExpenseService>();
            var expense = await expenses.CreateAsync(new CreateExpenseDto
            {
                Title = "Real Path Expense",
                ExpenseCategoryId = expenseCategoryId,
                Amount = 5m,
                ExpenseDate = DateTime.Now
            });
            Assert.Equal(5m, expense.Amount);

            // A read through the dashboard must reflect the writes made above.
            var dashboard = scope.ServiceProvider.GetRequiredService<IDashboardService>();
            var summary = await dashboard.GetSummaryAsync(new DashboardFilterDto
            {
                Preset = DashboardDatePreset.Today
            });
            Assert.Equal(20m, summary.SalesToday.Value);
            Assert.Equal(1m, summary.SalesCount.Value);
            Assert.Equal(40m, summary.PurchasesToday.Value);
            Assert.True(summary.InventoryCostValue > 0m);

            // Branch ownership is persisted on every branch-scoped row.
            Assert.Equal(mainBranch.Id, (await context.SalesInvoices
                .AsNoTracking()
                .SingleAsync(i => i.Id == sale.Id)).BranchId);
            Assert.Equal(mainBranch.Id, (await context.PurchaseInvoices
                .AsNoTracking()
                .SingleAsync(i => i.Id == purchase.Id)).BranchId);
            Assert.Equal(mainBranch.Id, (await context.Expenses
                .AsNoTracking()
                .SingleAsync(e => e.Id == expense.Id)).BranchId);
            Assert.Equal(mainBranch.Id, (await context.Cashboxes
                .AsNoTracking()
                .SingleAsync(c => c.Id == session.CashboxId)).BranchId);
        }
        finally
        {
            await provider.DisposeAsync();
            SqliteConnection.ClearAllPools();
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task Upgrade_BackfillsEmptyBranchIds_AndPreservesExistingBranches()
    {
        var (provider, dbPath) = BuildProvider();

        try
        {
            await using (var scope = provider.CreateAsyncScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await context.Database.MigrateAsync();

                var main = new Branch
                {
                    Name = "Legacy Main",
                    IsMainBranch = true,
                    IsActive = true
                };
                var other = new Branch
                {
                    Name = "Legacy Other",
                    IsMainBranch = false,
                    IsActive = true
                };
                var admin = new User
                {
                    FullName = "Legacy Administrator",
                    UserName = "admin",
                    PasswordHash = "legacy-not-used",
                    IsActive = true,
                    BranchId = null
                };
                var expenseCategory = new ExpenseCategory
                {
                    Name = "Legacy Expense Category"
                };

                context.Branches.AddRange(main, other);
                context.Users.Add(admin);
                context.ExpenseCategories.Add(expenseCategory);

                var emptyBranchSale = SalesInvoice.Create(
                    "LEGACY-SALE-EMPTY",
                    Guid.Empty,
                    Guid.Empty,
                    new[] { new InvoiceLine(1m, 10m) },
                    0m,
                    0m,
                    10m,
                    "DZD",
                    2);

                var otherBranchSale = SalesInvoice.Create(
                    "LEGACY-SALE-OTHER",
                    Guid.Empty,
                    other.Id,
                    new[] { new InvoiceLine(1m, 10m) },
                    0m,
                    0m,
                    10m,
                    "DZD",
                    2);

                context.SalesInvoices.AddRange(emptyBranchSale, otherBranchSale);
                context.PurchaseInvoices.Add(new PurchaseInvoice
                {
                    InvoiceNumber = "LEGACY-PUR-EMPTY",
                    SupplierId = Guid.NewGuid(),
                    BranchId = Guid.Empty,
                    Total = 10m,
                    PaidAmount = 10m
                });
                context.Expenses.Add(new Expense
                {
                    Title = "Legacy Expense",
                    ExpenseCategoryId = expenseCategory.Id,
                    BranchId = Guid.Empty,
                    Amount = 5m,
                    ExpenseDate = DateTime.Now
                });

                await context.SaveChangesAsync();
            }

            await DbInitializer.InitializeAsync(provider);

            await using (var scope = provider.CreateAsyncScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var main = await context.Branches
                    .AsNoTracking()
                    .SingleAsync(b => b.IsMainBranch && b.IsActive && !b.IsDeleted);
                var other = await context.Branches
                    .AsNoTracking()
                    .SingleAsync(b => !b.IsMainBranch && b.Name == "Legacy Other");

                Assert.Equal(main.Id, (await context.Users
                    .AsNoTracking()
                    .SingleAsync(u => u.UserName == "admin")).BranchId);
                Assert.Equal(main.Id, (await context.SalesInvoices
                    .AsNoTracking()
                    .SingleAsync(i => i.InvoiceNumber.Value == "LEGACY-SALE-EMPTY")).BranchId);
                Assert.Equal(other.Id, (await context.SalesInvoices
                    .AsNoTracking()
                    .SingleAsync(i => i.InvoiceNumber.Value == "LEGACY-SALE-OTHER")).BranchId);
                Assert.Equal(main.Id, (await context.PurchaseInvoices
                    .AsNoTracking()
                    .SingleAsync(i => i.InvoiceNumber == "LEGACY-PUR-EMPTY")).BranchId);
                Assert.Equal(main.Id, (await context.Expenses
                    .AsNoTracking()
                    .SingleAsync(e => e.Title == "Legacy Expense")).BranchId);
            }

            // Running the initializer again must not move any row (idempotent).
            await DbInitializer.InitializeAsync(provider);

            await using (var scope = provider.CreateAsyncScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                Assert.Equal(2, await context.SalesInvoices.CountAsync());
                Assert.Equal(1, await context.PurchaseInvoices.CountAsync());
                Assert.Equal(1, await context.Expenses.CountAsync());
                Assert.Equal(0, await context.SalesInvoices
                    .CountAsync(i => i.BranchId == Guid.Empty));
                Assert.Equal(0, await context.PurchaseInvoices
                    .CountAsync(i => i.BranchId == Guid.Empty));
                Assert.Equal(0, await context.Expenses
                    .CountAsync(e => e.BranchId == Guid.Empty));
            }
        }
        finally
        {
            await provider.DisposeAsync();
            SqliteConnection.ClearAllPools();
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task Upgrade_SnapshotBackfill_DoesNotCollideWithAnExistingBranchRow()
    {
        var (provider, dbPath) = BuildProvider();
        var collidingDate = new DateTime(2026, 1, 10);
        var freeDate = new DateTime(2026, 1, 11);

        try
        {
            await using (var scope = provider.CreateAsyncScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await context.Database.MigrateAsync();

                var main = new Branch
                {
                    Name = "Legacy Main",
                    IsMainBranch = true,
                    IsActive = true
                };
                context.Branches.Add(main);

                // Historical aggregate (NULL branch) and a branch-scoped row for
                // the same day: moving the legacy row would break the unique
                // (BranchId, SnapshotDate) index.
                context.InventorySnapshots.AddRange(
                    new InventorySnapshot
                    {
                        BranchId = null,
                        SnapshotDate = collidingDate,
                        TotalQuantity = 1m,
                        CostValue = 10m
                    },
                    new InventorySnapshot
                    {
                        BranchId = main.Id,
                        SnapshotDate = collidingDate,
                        TotalQuantity = 2m,
                        CostValue = 20m
                    },
                    new InventorySnapshot
                    {
                        BranchId = null,
                        SnapshotDate = freeDate,
                        TotalQuantity = 3m,
                        CostValue = 30m
                    });

                await context.SaveChangesAsync();
            }

            await DbInitializer.InitializeAsync(provider);

            await using (var scope = provider.CreateAsyncScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var main = await context.Branches
                    .AsNoTracking()
                    .SingleAsync(b => b.IsMainBranch && b.IsActive && !b.IsDeleted);

                var colliding = await context.InventorySnapshots
                    .AsNoTracking()
                    .Where(s => s.SnapshotDate == collidingDate)
                    .ToListAsync();
                Assert.Equal(2, colliding.Count);
                Assert.Single(colliding, s => s.BranchId == main.Id && s.TotalQuantity == 2m);
                Assert.Single(colliding, s => s.BranchId == null && s.TotalQuantity == 1m);

                var free = await context.InventorySnapshots
                    .AsNoTracking()
                    .SingleAsync(s => s.SnapshotDate == freeDate);
                Assert.Equal(main.Id, free.BranchId);
            }

            // Idempotent: a second run leaves the same rows untouched.
            await DbInitializer.InitializeAsync(provider);

            await using (var scope = provider.CreateAsyncScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                Assert.Equal(3, await context.InventorySnapshots.CountAsync());
                Assert.Equal(1, await context.InventorySnapshots
                    .CountAsync(s => s.BranchId == null));
            }
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
        var directory = Path.Combine(Path.GetTempPath(), "DzentoPOS2.RealPath");
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
