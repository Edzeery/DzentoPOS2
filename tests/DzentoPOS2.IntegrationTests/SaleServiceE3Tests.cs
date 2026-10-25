using DzentoPOS2.App;
using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.App.Session;
using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Core.Entities.Sales;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Core.Services.Sales;
using DzentoPOS2.Core.ValueObjects.Common;
using DzentoPOS2.Core.ValueObjects.Inventory;
using DzentoPOS2.Data;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common.Interfaces;
using DzentoPOS2.Data.Seed;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;
using DzentoPOS2.Shared.Contracts.DTOs.Sales;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DzentoPOS2.IntegrationTests;

/// <summary>
/// Regression tests for the FK-on-Complete-Sale bug:
/// the store must be created once at startup (bootstrap) and never lazily in
/// the middle of a sale, and a failed save must not poison the shared
/// (root-resolved) <c>AppDbContext</c> for the rest of the session.
/// </summary>
[Collection(PosDatabaseCollection.Name)]
public sealed class SaleServiceE3Tests
{
    private readonly PosDatabaseFixture _fixture;

    public SaleServiceE3Tests(PosDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Consecutive_sales_do_not_create_branches_warehouses_or_cashboxes()
    {
        await using var scope = _fixture.CreateScope();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var branches = await context.Branches.CountAsync();
        var warehouses = await context.Warehouses.CountAsync();
        var cashboxes = await context.Cashboxes.CountAsync();

        for (var i = 0; i < 2; i++)
        {
            await sales.CreateAsync(new CreateSaleDto
            {
                PaidAmount = 10,
                Items =
                {
                    new CreateSaleItemDto
                    {
                        ProductId = _fixture.Data.ProductId,
                        Quantity = 1,
                        Price = 10
                    }
                }
            });
        }

        Assert.Equal(branches, await context.Branches.CountAsync());
        Assert.Equal(warehouses, await context.Warehouses.CountAsync());
        Assert.Equal(cashboxes, await context.Cashboxes.CountAsync());
    }

    [Fact]
    public async Task Failed_save_does_not_poison_the_next_save()
    {
        await using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // A leftover (poisoned) invoice pointing at a CashSession that does not
        // exist. Saving it fails with the reported FK error.
        context.SalesInvoices.Add(SalesInvoice.Create(
            "INV-POISON",
            Guid.Empty,
            _fixture.Data.BranchId,
            new[] { new InvoiceLine(1, 10) },
            0,
            0,
            0,
            "DZD",
            2));
        context.SalesInvoices.Local.First().AssignCashSession(Guid.NewGuid());

        var ex = await Assert.ThrowsAsync<DbUpdateException>(
            () => unitOfWork.SaveChangesAsync());

        Assert.Contains("FOREIGN KEY", ex.ToString(), StringComparison.OrdinalIgnoreCase);

        // UnitOfWork cleared the tracker, so the poisoned invoice is gone and
        // the very next unrelated save must succeed.
        Assert.DoesNotContain(
            context.ChangeTracker.Entries(),
            e => e.Entity is SalesInvoice);

        var branch = new Branch
        {
            Name = $"PoisonCheck-{Guid.NewGuid():N}",
            IsMainBranch = false,
            IsActive = true
        };

        context.Branches.Add(branch);
        await unitOfWork.SaveChangesAsync();

        Assert.True(await context.Branches
            .AsNoTracking()
            .AnyAsync(b => b.Id == branch.Id));
    }

    [Fact]
    public async Task Sale_succeeds_when_main_branch_has_no_warehouse_and_user_has_no_branch()
    {
        var (provider, _) = BuildProvider();

        try
        {
            await using var scope = provider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.MigrateAsync();

            // Reported shape: a non-main branch owns the only warehouse, while
            // the main branch owns the cashbox (with an open session) but no
            // warehouse. The user has no branch.
            var branchB = new Branch { Name = "Branch B", IsMainBranch = false, IsActive = true };
            var branchA = new Branch { Name = "Main A", IsMainBranch = true, IsActive = true };
            context.AddRange(branchB, branchA);
            await context.SaveChangesAsync();

            context.AddRange(
                new Warehouse { Name = "WH B", BranchId = branchB.Id, IsActive = true },
                new Cashbox { Name = "CB A", BranchId = branchA.Id, IsActive = true });
            await context.SaveChangesAsync();

            var category = new ProductCategory { Name = "C", IsActive = true };
            var unit = new ProductUnit { Name = "U", Symbol = "u", IsBaseUnit = true };
            context.AddRange(category, unit);
            await context.SaveChangesAsync();

            var product = new Product
            {
                Name = "P1",
                SKU = "P1",
                CategoryId = category.Id,
                UnitId = unit.Id,
                CostPrice = new Money(1, "DZD"),
                SalePrice = new Money(10, "DZD"),
                MinimumStock = new Quantity(0),
                TrackStock = false,
                IsActive = true
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var cashboxA = await context.Cashboxes.SingleAsync(c => c.BranchId == branchA.Id);
            context.CashSessions.Add(new CashSession
            {
                CashboxId = cashboxA.Id,
                OpenedAtUtc = DateTime.UtcNow,
                OpeningBalance = 1000m,
                Status = CashSessionStatus.Open
            });
            await context.SaveChangesAsync();

            // Startup bootstrap must give the main branch an active warehouse.
            await StoreBootstrapper.EnsureInitializedAsync(context);

            Assert.True(await context.Warehouses
                .AsNoTracking()
                .AnyAsync(w => w.BranchId == branchA.Id && w.IsActive));

            var currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
            currentUser.SetUser(new UserSession
            {
                UserId = Guid.NewGuid(),
                UserName = "cashier",
                BranchId = null
            });

            var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();

            var sale = await sales.CreateAsync(new CreateSaleDto
            {
                PaidAmount = 10,
                Items =
                {
                    new CreateSaleItemDto
                    {
                        ProductId = product.Id,
                        Quantity = 1,
                        Price = 10
                    }
                }
            });

            Assert.StartsWith("INV-", sale.InvoiceNumber);

            var saved = await context.SalesInvoices
                .AsNoTracking()
                .SingleAsync(x => x.Id == sale.Id);

            Assert.Equal(branchA.Id, saved.BranchId);
        }
        finally
        {
            await provider.DisposeAsync();
        }
    }

    [Fact]
    public async Task Legacy_add_cash_sessions_upgrade_completes_a_real_form_sale_without_creating_store_rows()
    {
        var (provider, dbPath) = BuildProvider();

        try
        {
            await using var scope = provider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // 1) A legacy database: only migrated up to AddCashSessions.
            var migrator = context.Database.GetService<IMigrator>();
            await migrator.MigrateAsync("20261023082755_AddCashSessions");

            // 2) Pre-existing rows that must survive the upgrade. The branch has
            //    a cashbox but no warehouse: exactly the shape that used to make
            //    the sale create a warehouse lazily (from here the F1 FK blow-up).
            var branch = new Branch { Name = "Legacy Branch", IsMainBranch = true, IsActive = true };
            var cashbox = new Cashbox { Name = "Legacy Cashbox", BranchId = branch.Id, IsActive = true };
            context.AddRange(branch, cashbox);
            await context.SaveChangesAsync();

            // Legacy invoice + cash rows: these tables only gain their E3 columns
            // later, so write them with the old shape.
            await context.Database.ExecuteSqlRawAsync(
                "INSERT INTO \"SalesInvoices\" (\"Id\",\"InvoiceNumber\",\"CustomerId\",\"BranchId\",\"TotalAmount\",\"TotalCurrency\",\"Discount\",\"TaxRate\",\"PaidAmount\",\"PaidCurrency\",\"RemainingAmount\",\"RemainingCurrency\",\"IsDeleted\",\"CreatedAt\") "
                + "VALUES ({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13});",
                Guid.NewGuid(), "LEGACY-INV-1", Guid.Empty, branch.Id,
                "10", "DZD", "0", "0", "10", "DZD", "0", "DZD", 0, DateTime.UtcNow);

            await context.Database.ExecuteSqlRawAsync(
                "INSERT INTO \"CashTransactions\" (\"Id\",\"CashboxId\",\"Amount\",\"TransactionType\",\"ReferenceNumber\",\"TransactionDate\",\"IsDeleted\",\"CreatedAt\") "
                + "VALUES ({0},{1},{2},{3},{4},{5},{6},{7});",
                Guid.NewGuid(), cashbox.Id, "10", 1, "LEGACY-CT-1", DateTime.UtcNow, 0, DateTime.UtcNow);

            // 3) Upgrade to head exactly like DbInitializer does.
            await LegacySchemaRepair.RepairAsync(context.Database.GetDbConnection());
            await context.Database.MigrateAsync();

            Assert.Equal(1, await context.SalesInvoices.IgnoreQueryFilters().CountAsync());
            Assert.Equal(1, await context.CashTransactions.IgnoreQueryFilters().CountAsync());

            // Startup bootstrap must be a no-op (the store already exists).
            await StoreBootstrapper.EnsureInitializedAsync(context);

            var currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
            currentUser.SetUser(new UserSession
            {
                UserId = Guid.NewGuid(),
                UserName = "cashier",
                BranchId = null
            });

            // 4) A product seeded through the opening-quantity path.
            var category = new ProductCategory { Name = "Cat", IsActive = true };
            var unit = new ProductUnit { Name = "Piece", Symbol = "pc", IsBaseUnit = true };
            context.AddRange(category, unit);
            await context.SaveChangesAsync();

            var products = scope.ServiceProvider.GetRequiredService<IProductService>();
            var product = await products.CreateAsync(new CreateProductDto
            {
                Name = "Legacy Product",
                SKU = "LEGACY-P1",
                CategoryId = category.Id,
                UnitId = unit.Id,
                CostPrice = 5,
                SalePrice = 10,
                MinimumStock = 0,
                OpeningStock = 100,
                TrackStock = true,
                IsActive = true
            });

            context.CashSessions.Add(new CashSession
            {
                CashboxId = cashbox.Id,
                OpenedAtUtc = DateTime.UtcNow,
                OpeningBalance = 1000m,
                Status = CashSessionStatus.Open
            });
            await context.SaveChangesAsync();

            var branches = await context.Branches.CountAsync();
            var warehouses = await context.Warehouses.CountAsync();
            var cashboxes = await context.Cashboxes.CountAsync();

            // 5) The exact payload shape SalesForm sends (default payment method,
            //    no discount/tax, paid == total).
            var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();
            var sale = await sales.CreateAsync(new CreateSaleDto
            {
                CustomerId = Guid.Empty,
                TaxRate = 0,
                PaidAmount = 10,
                Items =
                {
                    new CreateSaleItemDto
                    {
                        ProductId = product.Id,
                        Quantity = 1,
                        Price = 10
                    }
                }
            });

            Assert.StartsWith("INV-", sale.InvoiceNumber);

            // The sale must not create any store rows.
            Assert.Equal(branches, await context.Branches.CountAsync());
            Assert.Equal(warehouses, await context.Warehouses.CountAsync());
            Assert.Equal(cashboxes, await context.Cashboxes.CountAsync());

            // Nothing dangling after the whole upgrade + sale.
            Assert.Empty(await ForeignKeyViolationsAsync(dbPath));

            var saved = await context.SalesInvoices
                .AsNoTracking()
                .SingleAsync(x => x.Id == sale.Id);

            Assert.Equal(branch.Id, saved.BranchId);
        }
        finally
        {
            await provider.DisposeAsync();
        }
    }

    private static (ServiceProvider Provider, string DbPath) BuildProvider()
    {
        var directory = Path.Combine(Path.GetTempPath(), "DzentoPOS2.E3");
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

    private static async Task<List<string>> ForeignKeyViolationsAsync(string dbPath)
    {
        await using var connection = new SqliteConnection($"Data Source={dbPath}");
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_key_check;";

        var violations = new List<string>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            violations.Add(reader.GetString(0));

        return violations;
    }
}
