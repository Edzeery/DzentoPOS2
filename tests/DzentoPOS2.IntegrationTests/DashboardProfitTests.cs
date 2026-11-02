using DzentoPOS2.App.Services.Reports.Interfaces;
using DzentoPOS2.Core.Entities.Sales;
using DzentoPOS2.Core.ValueObjects.Common;
using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DzentoPOS2.IntegrationTests;

[Collection(PosDatabaseCollection.Name)]
public sealed class DashboardProfitTests
{
    private readonly PosDatabaseFixture _fixture;

    public DashboardProfitTests(PosDatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task GetSummary_DoesNotThrow()
    {
        await using var scope = _fixture.CreateScope();
        var dashboard = scope.ServiceProvider.GetRequiredService<IDashboardService>();
        var summary = await dashboard.GetSummaryAsync();
        Assert.NotNull(summary);
        Assert.NotNull(summary.ExpectedProfit);
        Assert.NotNull(summary.ActualProfit);
        Assert.NotNull(summary.NetProfit);
    }
}