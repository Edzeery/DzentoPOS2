using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DzentoPOS2.IntegrationTests;

[Collection(PosDatabaseCollection.Name)]
public sealed class StockReloadRaceTests
{
    private readonly PosDatabaseFixture _fixture;

    public StockReloadRaceTests(PosDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task RapidSwitch_DoesNotCorruptStock()
    {
        await using var scope = _fixture.CreateScope();
        var activeBranch = scope.ServiceProvider.GetRequiredService<IActiveBranchContext>();
        var posStore = scope.ServiceProvider.GetRequiredService<IPosProductStore>();
        var products = scope.ServiceProvider.GetRequiredService<IProductService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var categoryId = await context.ProductCategories.Select(c => c.Id).FirstAsync();
        var unitId = await context.ProductUnits.Select(u => u.Id).FirstAsync();

        // Dedicated product so the assertion is independent of other tests
        // that share this database.
        var product = await products.CreateAsync(new CreateProductDto
        {
            Name = "Race Product",
            SKU = $"RACE-{Guid.NewGuid():N}",
            CategoryId = categoryId,
            UnitId = unitId,
            CostPrice = 5,
            SalePrice = 10,
            MinimumStock = 0,
            OpeningStock = 25,
            TrackStock = true,
            IsActive = true
        });

        posStore.Invalidate();
        var before = await posStore.GetAllAsync();
        Assert.Equal(25m, before.Single(x => x.ProductId == product.Id).Stock);

        // Create a second branch without a warehouse.
        var branchB = new DzentoPOS2.Core.Entities.Businesses.Branch
        {
            Name = "Branch B Race",
            IsMainBranch = false,
            IsActive = true
        };
        context.Branches.Add(branchB);
        await context.SaveChangesAsync();

        // Rapidly switch A -> B -> A.
        await activeBranch.TrySwitchBranchAsync(branchB.Id);
        await activeBranch.TrySwitchBranchAsync(_fixture.Data.BranchId);

        // The final state must be branch A and the dedicated product's stock
        // must be the branch-A quantity (not branch B's empty stock).
        var after = await posStore.GetAllAsync();
        Assert.Equal(_fixture.Data.BranchId, activeBranch.ActiveBranchId);
        Assert.Equal(25m, after.Single(x => x.ProductId == product.Id).Stock);
    }
}
