using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.Core.Services.Sales;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Shared.Contracts.DTOs.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DzentoPOS2.IntegrationTests;

[Collection(PosDatabaseCollection.Name)]
public sealed class BranchIsolationTests
{
    private readonly PosDatabaseFixture _fixture;

    public BranchIsolationTests(PosDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SaleInBranchA_RecordsInvoiceInBranchAOnly()
    {
        await using var scope = _fixture.CreateScope();
        var saleService = scope.ServiceProvider.GetRequiredService<ISaleService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var activeBranch = scope.ServiceProvider.GetRequiredService<IActiveBranchContext>();

        // Ensure we're on the default branch (branch A from TestData)
        var branchAId = _fixture.Data.BranchId;

        var dto = new CreateSaleDto
        {
            CustomerId = Guid.Empty,
            DiscountMode = DiscountMode.Percent,
            DiscountValue = 0,
            TaxRate = 0,
            Fees = 0,
            PaidAmount = 10,
            PaymentMethod = "Cash",
            Items = new List<CreateSaleItemDto>
            {
                new() { ProductId = _fixture.Data.TrackedProductId, Quantity = 1, Price = 10 }
            }
        };

        var invoice = await saleService.CreateAsync(dto);
        Assert.NotNull(invoice);

        // Verify invoice is in branch A
        var savedInvoice = await context.SalesInvoices
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == invoice.Id);
        Assert.NotNull(savedInvoice);
        Assert.Equal(branchAId, savedInvoice.BranchId);
    }

    [Fact]
    public async Task SaleInBranchB_RecordsInvoiceInBranchBOnly()
    {
        // Create a second branch
        await using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var activeBranch = scope.ServiceProvider.GetRequiredService<IActiveBranchContext>();

        var branchB = new DzentoPOS2.Core.Entities.Businesses.Branch
        {
            Name = "Branch B",
            IsMainBranch = false,
            IsActive = true
        };
        context.Branches.Add(branchB);
        await context.SaveChangesAsync();

        // Create warehouse and cashbox for branch B
        var warehouseB = new DzentoPOS2.Core.Entities.Businesses.Warehouse
        {
            Name = "WH B",
            BranchId = branchB.Id,
            IsActive = true
        };
        var cashboxB = new DzentoPOS2.Core.Entities.Accounting.Cashbox
        {
            Name = "CB B",
            BranchId = branchB.Id,
            IsActive = true
        };
        context.Warehouses.Add(warehouseB);
        context.Cashboxes.Add(cashboxB);
        await context.SaveChangesAsync();

        // Open a cash session for branch B
        var user = scope.ServiceProvider.GetRequiredService<DzentoPOS2.App.Session.ICurrentUserService>();
        context.CashSessions.Add(new DzentoPOS2.Core.Entities.Accounting.CashSession
        {
            CashboxId = cashboxB.Id,
            OpenedAtUtc = DateTime.UtcNow,
            OpenedByUserId = _fixture.Data.UserId,
            OpeningBalance = 1000m,
            Status = DzentoPOS2.Core.Enums.CashSessionStatus.Open
        });
        await context.SaveChangesAsync();

        // Set user as SuperAdmin for branch switching
        var currentUser = scope.ServiceProvider.GetRequiredService<DzentoPOS2.App.Session.ICurrentUserService>();
        if (currentUser is DzentoPOS2.App.Session.CurrentUserService cus)
        {
            cus.SetUser(new DzentoPOS2.App.Session.UserSession
            {
                UserId = _fixture.Data.UserId,
                UserName = "cashier",
                FullName = "Test Cashier",
                RoleName = "SuperAdmin",
                BranchId = _fixture.Data.BranchId,
                BranchName = "Main Branch",
                IsAdmin = true
            });
        }

        // Verify ActiveBranchId was initialized
        var beforeSwitch = activeBranch.ActiveBranchId;

        // Switch to branch B
        var switchResult = await activeBranch.TrySwitchBranchAsync(branchB.Id);
        var afterSwitch = activeBranch.ActiveBranchId;

        // Debug info
        Console.WriteLine($"DEBUG: beforeSwitch={beforeSwitch}, switchResult={switchResult}, afterSwitch={afterSwitch}, branchB={branchB.Id}");

        Assert.True(switchResult, "Switch to branch B failed");
        Assert.Equal(branchB.Id, afterSwitch);

        var saleService = scope.ServiceProvider.GetRequiredService<ISaleService>();
        var dto = new CreateSaleDto
        {
            CustomerId = Guid.Empty,
            DiscountMode = DiscountMode.Percent,
            DiscountValue = 0,
            TaxRate = 0,
            Fees = 0,
            PaidAmount = 10,
            PaymentMethod = "Cash",
            Items = new List<CreateSaleItemDto>
            {
                new() { ProductId = _fixture.Data.TrackedProductId, Quantity = 1, Price = 10 }
            }
        };

        var invoice = await saleService.CreateAsync(dto);
        Assert.NotNull(invoice);

        var savedInvoice = await context.SalesInvoices
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == invoice.Id);
        Assert.NotNull(savedInvoice);
        Assert.Equal(branchB.Id, savedInvoice.BranchId);

        // Switch back to branch A
        await activeBranch.TrySwitchBranchAsync(_fixture.Data.BranchId);
    }

    [Fact]
    public async Task BranchA_DoesNotSeeBranchBInvoices()
    {
        await using var scope = _fixture.CreateScope();
        var saleService = scope.ServiceProvider.GetRequiredService<ISaleService>();
        var activeBranch = scope.ServiceProvider.GetRequiredService<IActiveBranchContext>();

        // Ensure we're on branch A
        await activeBranch.TrySwitchBranchAsync(_fixture.Data.BranchId);

        var invoices = await saleService.GetAllAsync();
        // All invoices should be from branch A (the default branch)
        foreach (var inv in invoices)
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var saved = await context.SalesInvoices.AsNoTracking().FirstOrDefaultAsync(i => i.Id == inv.Id);
            if (saved != null)
                Assert.Equal(_fixture.Data.BranchId, saved.BranchId);
        }
    }

    [Fact]
    public async Task POSStock_PerBranch()
    {
        await using var scope = _fixture.CreateScope();
        var posStore = scope.ServiceProvider.GetRequiredService<IPosProductStore>();
        var activeBranch = scope.ServiceProvider.GetRequiredService<IActiveBranchContext>();

        // Load products for branch A
        await activeBranch.TrySwitchBranchAsync(_fixture.Data.BranchId);
        var itemsA = await posStore.GetAllAsync();
        Assert.True(itemsA.Count > 0);

        // Stock should be from branch A's warehouse
        var trackedItem = itemsA.FirstOrDefault(x => x.ProductId == _fixture.Data.TrackedProductId);
        if (trackedItem != null)
        {
            Assert.True(trackedItem.Stock >= 0);
        }
    }

    [Fact]
    public async Task CartGuard_BlocksSwitch()
    {
        // This test verifies the guard logic, not the full UI
        var guard = new DzentoPOS2.App.Services.Systems.BranchSwitchGuard();
        guard.CanSwitchBranch = false;
        var registry = new DzentoPOS2.App.Services.Systems.BranchSwitchGuardRegistry();
        registry.Register(guard);

        Assert.False(registry.CanSwitch);
    }
}
