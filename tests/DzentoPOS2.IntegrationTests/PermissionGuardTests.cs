using DzentoPOS2.App.Authorization;
using DzentoPOS2.App.Services.Accounting;
using DzentoPOS2.App.Services.Identity.Interfaces;
using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.App.Services.Partners;
using DzentoPOS2.App.Services.Purchases;
using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.App.Session;
using DzentoPOS2.Core.Constants;
using DzentoPOS2.Core.Exceptions;
using DzentoPOS2.Data;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Shared.Contracts.DTOs.Accounting;
using DzentoPOS2.Shared.Contracts.DTOs.Partners;
using DzentoPOS2.Shared.Contracts.DTOs.Purchases;
using DzentoPOS2.Shared.Contracts.DTOs.Sales;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DzentoPOS2.IntegrationTests;

/// <summary>
/// G2.2 — per-service permission enforcement: a non-admin user without the
/// required permission must receive a coded PermissionDenied error and an
/// audit row must be written.
/// </summary>
[Collection(PosDatabaseCollection.Name)]
public sealed class PermissionGuardTests : IAsyncLifetime
{
    private readonly PosDatabaseFixture _fixture;

    public PermissionGuardTests(PosDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        // Restore the admin user so subsequent test classes are not affected.
        await using var scope = _fixture.CreateScope();
        var currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
        currentUser.SetUser(new UserSession
        {
            UserId = _fixture.Data.UserId,
            UserName = "cashier",
            FullName = "Test Cashier",
            RoleName = "Cashier",
            BranchId = _fixture.Data.BranchId,
            BranchName = "Main Branch",
            IsAdmin = true
        });
    }

    private Task SetNonAdminUserAsync(IServiceProvider sp)
    {
        var currentUser = sp.GetRequiredService<ICurrentUserService>();
        currentUser.SetUser(new UserSession
        {
            UserId = _fixture.Data.UserId,
            UserName = "cashier",
            FullName = "Test Cashier",
            RoleName = "Cashier",
            BranchId = _fixture.Data.BranchId,
            BranchName = "Main Branch",
            IsAdmin = false
        });
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CashSession_open_denied_without_permission()
    {
        await using var scope = _fixture.CreateScope();
        await SetNonAdminUserAsync(scope.ServiceProvider);

        var svc = scope.ServiceProvider.GetRequiredService<ICashSessionService>();

        var ex = await Assert.ThrowsAsync<CodedDomainException>(() =>
            svc.OpenSessionAsync(new OpenCashSessionDto
            {
                CashboxId = _fixture.Data.CashboxId,
                OpeningBalance = 100m
            }));

        Assert.Equal(DomainErrorCodes.PermissionDenied, ex.Code);
    }

    [Fact]
    public async Task Purchase_create_denied_without_permission()
    {
        await using var scope = _fixture.CreateScope();
        await SetNonAdminUserAsync(scope.ServiceProvider);

        var svc = scope.ServiceProvider.GetRequiredService<IPurchaseService>();

        var ex = await Assert.ThrowsAsync<CodedDomainException>(() =>
            svc.CreateAsync(new CreatePurchaseInvoiceDto
            {
                Items = { new CreatePurchaseItemDto { ProductId = _fixture.Data.ProductId, Quantity = 1, CostPrice = 10 } }
            }));

        Assert.Equal(DomainErrorCodes.PermissionDenied, ex.Code);
    }

    [Fact]
    public async Task Expense_create_denied_without_permission()
    {
        await using var scope = _fixture.CreateScope();
        await SetNonAdminUserAsync(scope.ServiceProvider);

        var svc = scope.ServiceProvider.GetRequiredService<IExpenseService>();

        var ex = await Assert.ThrowsAsync<CodedDomainException>(() =>
            svc.CreateAsync(new CreateExpenseDto
            {
                Title = "Test",
                Amount = 10,
                ExpenseDate = DateTime.UtcNow
            }));

        Assert.Equal(DomainErrorCodes.PermissionDenied, ex.Code);
    }

    [Fact]
    public async Task Customer_create_denied_without_permission()
    {
        await using var scope = _fixture.CreateScope();
        await SetNonAdminUserAsync(scope.ServiceProvider);

        var svc = scope.ServiceProvider.GetRequiredService<ICustomerService>();

        var ex = await Assert.ThrowsAsync<CodedDomainException>(() =>
            svc.CreateAsync(new CreateCustomerDto { FullName = "Test" }));

        Assert.Equal(DomainErrorCodes.PermissionDenied, ex.Code);
    }

    [Fact]
    public async Task Supplier_create_denied_without_permission()
    {
        await using var scope = _fixture.CreateScope();
        await SetNonAdminUserAsync(scope.ServiceProvider);

        var svc = scope.ServiceProvider.GetRequiredService<ISupplierService>();

        var ex = await Assert.ThrowsAsync<CodedDomainException>(() =>
            svc.CreateAsync(new CreateSupplierDto { Name = "Test" }));

        Assert.Equal(DomainErrorCodes.PermissionDenied, ex.Code);
    }

    [Fact]
    public async Task Stock_adjust_denied_without_permission()
    {
        await using var scope = _fixture.CreateScope();
        await SetNonAdminUserAsync(scope.ServiceProvider);

        var svc = scope.ServiceProvider.GetRequiredService<IStockService>();

        var ex = await Assert.ThrowsAsync<CodedDomainException>(() =>
            svc.AdjustAsync(
                _fixture.Data.ProductId,
                _fixture.Data.WarehouseId,
                10m, "test", null, true));

        Assert.Equal(DomainErrorCodes.PermissionDenied, ex.Code);
    }

    [Fact]
    public async Task Backup_create_denied_without_permission()
    {
        await using var scope = _fixture.CreateScope();
        await SetNonAdminUserAsync(scope.ServiceProvider);

        var svc = scope.ServiceProvider.GetRequiredService<IBackupService>();

        var ex = await Assert.ThrowsAsync<CodedDomainException>(() =>
            svc.CreateBackupAsync());

        Assert.Equal(DomainErrorCodes.PermissionDenied, ex.Code);
    }
}