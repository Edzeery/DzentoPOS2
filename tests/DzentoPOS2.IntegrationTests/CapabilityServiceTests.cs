using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DzentoPOS2.IntegrationTests;

[Collection(PosDatabaseCollection.Name)]
public sealed class CapabilityServiceTests
{
    private readonly PosDatabaseFixture _fixture;

    public CapabilityServiceTests(PosDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HasCapability_ReturnsTrue_WhenEnabled()
    {
        await using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var activeBranch = scope.ServiceProvider.GetRequiredService<IActiveBranchContext>();
        var service = new CapabilityService(context, activeBranch);

        var branchId = activeBranch.ActiveBranchId;
        await service.SetCapabilitiesAsync(branchId, BranchCapability.SerialNumber);

        var result = await service.HasCapabilityAsync(BranchCapability.SerialNumber);
        Assert.True(result);
    }

    [Fact]
    public async Task HasCapability_ReturnsFalse_WhenDisabled()
    {
        await using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var activeBranch = scope.ServiceProvider.GetRequiredService<IActiveBranchContext>();
        var service = new CapabilityService(context, activeBranch);

        var result = await service.HasCapabilityAsync(BranchCapability.DigitalCode);
        Assert.False(result);
    }

    [Fact]
    public async Task GetCapabilities_ReturnsAllForMultiService()
    {
        await using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var activeBranch = scope.ServiceProvider.GetRequiredService<IActiveBranchContext>();
        var service = new CapabilityService(context, activeBranch);

        var branchId = activeBranch.ActiveBranchId;
        // Clean up
        var existing = await context.BranchActivities.Where(a => a.BranchId == branchId).ToListAsync();
        if (existing.Count > 0) { context.BranchActivities.RemoveRange(existing); await context.SaveChangesAsync(); }
        context.BranchActivities.Add(new DzentoPOS2.Core.Entities.Businesses.BranchActivity
        {
            BranchId = branchId,
            ActivityType = ActivityType.MultiService,
            IsEnabled = true
        });
        await context.SaveChangesAsync();

        var caps = await service.GetCapabilitiesAsync();
        Assert.True((caps & BranchCapability.SerialNumber) != 0);
        Assert.True((caps & BranchCapability.DigitalCode) != 0);
        Assert.True((caps & BranchCapability.WeightUnit) != 0);
    }

    [Fact]
    public async Task SetCapabilities_PersistsToDatabase()
    {
        await using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var activeBranch = scope.ServiceProvider.GetRequiredService<IActiveBranchContext>();
        var service = new CapabilityService(context, activeBranch);

        var branchId = activeBranch.ActiveBranchId;
        await service.SetCapabilitiesAsync(branchId, BranchCapability.SerialNumber | BranchCapability.Warranty);

        var activities = await context.BranchActivities
            .Where(a => a.BranchId == branchId)
            .ToListAsync();
        Assert.True(activities.Count > 0);
    }
}
