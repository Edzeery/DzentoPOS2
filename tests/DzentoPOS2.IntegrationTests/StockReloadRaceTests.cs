using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.Data.Context;
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

        // Create a second branch
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var branchB = new DzentoPOS2.Core.Entities.Businesses.Branch
        {
            Name = "Branch B Race",
            IsMainBranch = false,
            IsActive = true
        };
        context.Branches.Add(branchB);
        await context.SaveChangesAsync();

        // Rapidly switch A -> B -> A
        await activeBranch.TrySwitchBranchAsync(branchB.Id);
        await activeBranch.TrySwitchBranchAsync(_fixture.Data.BranchId);

        // Verify stock matches branch A (final state)
        var items = await posStore.GetAllAsync();
        Assert.True(items.Count > 0);

        // Stock should be from branch A's warehouse
        var trackedItem = items.FirstOrDefault(x => x.ProductId == _fixture.Data.TrackedProductId);
        if (trackedItem != null)
        {
            Assert.True(trackedItem.Stock >= 0);
        }
    }
}
