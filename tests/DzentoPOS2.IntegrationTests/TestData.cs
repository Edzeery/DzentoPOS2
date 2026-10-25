using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Core.Entities.Identity;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Core.ValueObjects.Common;
using DzentoPOS2.Core.ValueObjects.Inventory;
using DzentoPOS2.Data.Context;

namespace DzentoPOS2.IntegrationTests;

public sealed class TestData
{
    public Guid BranchId { get; private init; }

    public Guid WarehouseId { get; private init; }

    public Guid CashboxId { get; private init; }

    public Guid UserId { get; private init; }

    public Guid ProductId { get; private init; }

    public Guid TrackedProductId { get; private init; }

    public static async Task<TestData> SeedAsync(AppDbContext context)
    {
        var branch = new Branch
        {
            Name = "Main Branch",
            IsMainBranch = true,
            IsActive = true
        };

        var warehouse = new Warehouse
        {
            Name = "Main Warehouse",
            BranchId = branch.Id,
            IsActive = true
        };

        var cashbox = new Cashbox
        {
            Name = "Main Cashbox",
            BranchId = branch.Id,
            IsActive = true
        };

        var user = new User
        {
            FullName = "Test Cashier",
            UserName = "cashier",
            PasswordHash = "not-used-in-tests",
            BranchId = branch.Id,
            IsActive = true
        };

        var category = new ProductCategory
        {
            Name = "Test Category",
            IsActive = true
        };

        var unit = new ProductUnit
        {
            Name = "Piece",
            Symbol = "pc",
            IsBaseUnit = true
        };

        var product = new Product
        {
            Name = "Test Product",
            SKU = "TEST-1",
            CategoryId = category.Id,
            UnitId = unit.Id,
            CostPrice = new Money(5, "DZD"),
            SalePrice = new Money(10, "DZD"),
            MinimumStock = new Quantity(0),
            TrackStock = false,
            IsActive = true
        };

        var trackedProduct = new Product
        {
            Name = "Tracked Product",
            SKU = "TEST-2",
            CategoryId = category.Id,
            UnitId = unit.Id,
            CostPrice = new Money(5, "DZD"),
            SalePrice = new Money(10, "DZD"),
            MinimumStock = new Quantity(0),
            TrackStock = true,
            IsActive = true
        };

        var stock = new ProductStock
        {
            ProductId = trackedProduct.Id,
            WarehouseId = warehouse.Id,
            Quantity = new Quantity(100)
        };

        context.Branches.Add(branch);
        context.Warehouses.Add(warehouse);
        context.Cashboxes.Add(cashbox);
        context.Users.Add(user);
        context.ProductCategories.Add(category);
        context.ProductUnits.Add(unit);
        context.Products.AddRange(product, trackedProduct);
        context.ProductStocks.Add(stock);

        await context.SaveChangesAsync();

        context.CashSessions.Add(new CashSession
        {
            CashboxId = cashbox.Id,
            OpenedAtUtc = DateTime.UtcNow,
            OpenedByUserId = user.Id,
            OpeningBalance = 1000m,
            Status = DzentoPOS2.Core.Enums.CashSessionStatus.Open
        });

        await context.SaveChangesAsync();

        return new TestData
        {
            BranchId = branch.Id,
            WarehouseId = warehouse.Id,
            CashboxId = cashbox.Id,
            UserId = user.Id,
            ProductId = product.Id,
            TrackedProductId = trackedProduct.Id
        };
    }
}
