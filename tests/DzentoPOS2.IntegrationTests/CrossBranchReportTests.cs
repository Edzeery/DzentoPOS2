using DzentoPOS2.App;
using DzentoPOS2.App.Services.Businesses;
using DzentoPOS2.App.Services.Reports.Interfaces;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.App.Session;
using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Core.Entities.Identity;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Core.Entities.Partners;
using DzentoPOS2.Core.Entities.Purchases;
using DzentoPOS2.Core.Entities.Sales;
using DzentoPOS2.Core.Services.Sales;
using DzentoPOS2.Core.ValueObjects.Common;
using DzentoPOS2.Core.ValueObjects.Identity;
using DzentoPOS2.Core.ValueObjects.Inventory;
using DzentoPOS2.Data;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Seed;
using DzentoPOS2.Shared.Contracts.DTOs.Businesses;
using DzentoPOS2.Shared.Contracts.DTOs.Reports;
using DzentoPOS2.Shared.Helpers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DzentoPOS2.IntegrationTests;

/// <summary>
/// Phase 4 — unified cross-branch report. One row per branch plus a bold grand
/// total for a period. The report is read-only and only exposes every branch to
/// a caller that actually holds <c>BranchesViewAll</c>; anyone else silently
/// falls back to the active branch. The profit columns mirror the dashboard so
/// the two screens can never disagree.
/// </summary>
public sealed class CrossBranchReportTests
{
    [Fact]
    public async Task Report_WithPermission_ComparesEveryBranchAndTotals()
    {
        await WithStoreAsync(async provider =>
        {
            await using var scope = provider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var mainBranchId = await SignInAsAdminAsync(scope.ServiceProvider);

            var branchService = scope.ServiceProvider.GetRequiredService<IBranchService>();
            var reportService = scope.ServiceProvider.GetRequiredService<ICrossBranchReportService>();

            var branchB = await branchService.CreateAsync(new CreateBranchDto { Name = "Branch B" });

            var productA = await CreateProductAsync(context, cost: 5, sale: 10);
            var productB = await CreateProductAsync(context, cost: 1, sale: 4);

            // Branch A (main): one sale with a partial return, one expense and
            // one purchase.
            var invoiceA = await CreateSaleAsync(context, mainBranchId, productA, qty: 2, price: 10, unitCost: 5);
            await AddReturnAsync(context, invoiceA, productA, qty: 1, price: 10, total: 10);
            await AddExpenseAsync(context, mainBranchId, amount: 3);
            await AddPurchaseAsync(context, mainBranchId, total: 50);

            // Branch B: one sale and one purchase, no returns or expenses.
            await CreateSaleAsync(context, branchB.Id, productB, qty: 3, price: 4, unitCost: 1);
            await AddPurchaseAsync(context, branchB.Id, total: 7);

            var report = await reportService.GetAsync(new CrossBranchReportFilterDto
            {
                Preset = DashboardDatePreset.Today,
                ViewAllBranches = true
            });

            Assert.True(report.ViewAllBranches);
            Assert.Equal(2, report.Rows.Count);

            var rowA = Assert.Single(report.Rows, r => r.BranchId == mainBranchId);
            Assert.True(rowA.IsMainBranch);
            Assert.Equal(1, rowA.SalesCount);
            Assert.Equal(20m, rowA.SalesTotal);
            Assert.Equal(10m, rowA.ReturnsTotal);
            Assert.Equal(10m, rowA.NetSales);
            Assert.Equal(1, rowA.PurchasesCount);
            Assert.Equal(50m, rowA.PurchasesTotal);
            Assert.Equal(3m, rowA.ExpensesTotal);
            // gross = revenue 20 - cost 10 - return effect 5 = 5
            Assert.Equal(5m, rowA.GrossProfit);
            // net = gross 5 - fees 0 - expenses 3 = 2
            Assert.Equal(2m, rowA.NetProfit);

            var rowB = Assert.Single(report.Rows, r => r.BranchId == branchB.Id);
            Assert.False(rowB.IsMainBranch);
            Assert.Equal(1, rowB.SalesCount);
            Assert.Equal(12m, rowB.SalesTotal);
            Assert.Equal(0m, rowB.ReturnsTotal);
            Assert.Equal(12m, rowB.NetSales);
            Assert.Equal(1, rowB.PurchasesCount);
            Assert.Equal(7m, rowB.PurchasesTotal);
            Assert.Equal(0m, rowB.ExpensesTotal);
            Assert.Equal(9m, rowB.GrossProfit); // 12 - 3
            Assert.Equal(9m, rowB.NetProfit);

            // The grand total is exactly the sum of the branch rows.
            var total = report.Total;
            Assert.Equal(2, total.SalesCount);
            Assert.Equal(32m, total.SalesTotal);
            Assert.Equal(10m, total.ReturnsTotal);
            Assert.Equal(22m, total.NetSales);
            Assert.Equal(2, total.PurchasesCount);
            Assert.Equal(57m, total.PurchasesTotal);
            Assert.Equal(3m, total.ExpensesTotal);
            Assert.Equal(14m, total.GrossProfit);
            Assert.Equal(11m, total.NetProfit);
        });
    }

    [Fact]
    public async Task Report_WithoutBranchesViewAll_FallsBackToActiveBranch()
    {
        await WithStoreAsync(async provider =>
        {
            await using var scope = provider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Build the store as the administrator, then hand the session to a
            // Manager: they hold ReportsView but not BranchesViewAll.
            Guid mainBranchId;
            Guid branchBId;
            await using (var setup = provider.CreateAsyncScope())
            {
                mainBranchId = await SignInAsAdminAsync(setup.ServiceProvider);
                var branchService = setup.ServiceProvider.GetRequiredService<IBranchService>();
                branchBId = (await branchService.CreateAsync(new CreateBranchDto { Name = "Branch B" })).Id;
            }

            var managerId = await CreateManagerAsync(context, mainBranchId);

            var currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
            currentUser.SetUser(new UserSession
            {
                UserId = managerId,
                UserName = "manager",
                FullName = "Branch Manager",
                RoleName = "Manager",
                BranchId = mainBranchId,
                BranchName = "Main Branch",
                IsAdmin = false
            });

            var activeBranch = scope.ServiceProvider.GetRequiredService<IActiveBranchContext>();
            await activeBranch.InitializeAsync(currentUser);

            var reportService = scope.ServiceProvider.GetRequiredService<ICrossBranchReportService>();
            var report = await reportService.GetAsync(new CrossBranchReportFilterDto
            {
                Preset = DashboardDatePreset.Today,
                ViewAllBranches = true
            });

            // The request is ignored: only the active branch is reported.
            Assert.False(report.ViewAllBranches);
            var row = Assert.Single(report.Rows);
            Assert.Equal(mainBranchId, row.BranchId);
            Assert.DoesNotContain(report.Rows, r => r.BranchId == branchBId);
        });
    }

    private static async Task<Product> CreateProductAsync(AppDbContext ctx, decimal cost, decimal sale)
    {
        var cat = await ctx.ProductCategories.FirstAsync();
        var unit = await ctx.ProductUnits.FirstAsync();
        var product = new Product
        {
            Name = "CB-" + Guid.NewGuid().ToString("N").Substring(0, 6),
            CategoryId = cat.Id,
            UnitId = unit.Id,
            CostPrice = new Money(cost, "DZD"),
            SalePrice = new Money(sale, "DZD"),
            MinimumStock = new Quantity(0),
            TrackStock = true,
            IsActive = true
        };
        ctx.Products.Add(product);
        await ctx.SaveChangesAsync();

        ctx.ProductVariants.Add(new ProductVariant
        {
            ProductId = product.Id,
            Name = product.Name,
            CostPrice = product.CostPrice,
            SalePrice = product.SalePrice,
            MinimumStock = product.MinimumStock,
            TrackStock = product.TrackStock,
            IsActive = product.IsActive,
            IsDefault = true
        });
        await ctx.SaveChangesAsync();
        return product;
    }

    private static async Task<SalesInvoice> CreateSaleAsync(
        AppDbContext ctx,
        Guid branchId,
        Product product,
        decimal qty,
        decimal price,
        decimal unitCost)
    {
        var subtotal = qty * price;
        var totals = new InvoiceTotals(subtotal, 0m, subtotal, 0m, subtotal, 0m, 0m, subtotal, 0m);
        var invoice = SalesInvoice.Create(
            "CBR-" + Guid.NewGuid().ToString("N").Substring(0, 6),
            Guid.Empty,
            branchId,
            totals,
            DiscountMode.FixedAmount,
            0m,
            0m,
            "DZD",
            0m);

        var variantId = await ctx.ProductVariants
            .Where(v => v.ProductId == product.Id && v.IsDefault)
            .Select(v => v.Id)
            .FirstOrDefaultAsync();

        invoice.Items.Add(new SalesInvoiceItem
        {
            SalesInvoiceId = invoice.Id,
            ProductId = product.Id,
            VariantId = variantId,
            Quantity = qty,
            Price = price,
            Total = subtotal,
            UnitCost = unitCost,
            ListPrice = price,
            DiscountAmount = 0m
        });

        ctx.SalesInvoices.Add(invoice);
        await ctx.SaveChangesAsync();
        return invoice;
    }

    private static async Task AddReturnAsync(
        AppDbContext ctx,
        SalesInvoice invoice,
        Product product,
        decimal qty,
        decimal price,
        decimal total)
    {
        var ret = new SalesReturn
        {
            ReturnNumber = "RET-" + Guid.NewGuid().ToString("N").Substring(0, 6),
            SalesInvoiceId = invoice.Id,
            CustomerId = Guid.Empty,
            TotalAmount = total,
            ReturnDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        ctx.SalesReturns.Add(ret);
        await ctx.SaveChangesAsync();

        ctx.SalesReturnItems.Add(new SalesReturnItem
        {
            SalesReturnId = ret.Id,
            ProductId = product.Id,
            Quantity = qty,
            Price = price,
            Total = total
        });
        await ctx.SaveChangesAsync();
    }

    private static async Task AddExpenseAsync(AppDbContext ctx, Guid branchId, decimal amount)
    {
        var category = await ctx.ExpenseCategories.FirstAsync();
        ctx.Expenses.Add(new Expense
        {
            Title = "Rent",
            ExpenseCategoryId = category.Id,
            BranchId = branchId,
            Amount = amount,
            ExpenseDate = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();
    }

    private static async Task AddPurchaseAsync(AppDbContext ctx, Guid branchId, decimal total)
    {
        var supplier = await ctx.Suppliers.FirstOrDefaultAsync();
        if (supplier is null)
        {
            supplier = new Supplier { Name = "Supplier" };
            ctx.Suppliers.Add(supplier);
            await ctx.SaveChangesAsync();
        }

        ctx.PurchaseInvoices.Add(new PurchaseInvoice
        {
            InvoiceNumber = "PUR-" + Guid.NewGuid().ToString("N").Substring(0, 6),
            SupplierId = supplier.Id,
            BranchId = branchId,
            Total = total,
            PaidAmount = total,
            RemainingAmount = 0m
        });
        await ctx.SaveChangesAsync();
    }

    private static async Task<Guid> CreateManagerAsync(AppDbContext ctx, Guid branchId)
    {
        var managerRole = await ctx.Roles.FirstAsync(r => r.Name == "Manager");
        var suffix = Guid.NewGuid().ToString("N").Substring(0, 6);

        var user = new User
        {
            FullName = "Branch Manager",
            UserName = "manager-" + suffix,
            Email = new Email($"manager-{suffix}@dzentopos.local"),
            PhoneNumber = new PhoneNumber("0600000009"),
            PasswordHash = PasswordHelper.Hash("manager"),
            BranchId = branchId,
            IsActive = true
        };
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        ctx.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = managerRole.Id });
        await ctx.SaveChangesAsync();
        return user.Id;
    }

    private static async Task<Guid> SignInAsAdminAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();
        var admin = await context.Users
            .AsNoTracking()
            .Where(u => u.UserName == "admin")
            .Select(u => new { u.Id, u.BranchId })
            .SingleAsync();

        var branchId = admin.BranchId ?? throw new InvalidOperationException("Admin has no branch.");

        var currentUser = serviceProvider.GetRequiredService<ICurrentUserService>();
        currentUser.SetUser(new UserSession
        {
            UserId = admin.Id,
            UserName = "admin",
            FullName = "System Administrator",
            RoleName = "SuperAdmin",
            BranchId = branchId,
            BranchName = "Main Branch",
            IsAdmin = true
        });

        if (!currentUser.IsAuthenticated)
            throw new InvalidOperationException("Session was not established.");

        var activeBranch = serviceProvider.GetRequiredService<IActiveBranchContext>();
        await activeBranch.InitializeAsync(currentUser);

        return branchId;
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
        var directory = Path.Combine(Path.GetTempPath(), "DzentoPOS2.CrossBranchReport");
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
