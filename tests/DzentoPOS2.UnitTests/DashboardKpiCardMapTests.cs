using DzentoPOS2.Shared.Contracts.DTOs.Reports;
using DzentoPOS2.Shared.Helpers;
using Xunit;

namespace DzentoPOS2.UnitTests;

public sealed class DashboardKpiCardMapTests
{
    [Fact]
    public void Build_ReturnsTwelveEntries()
    {
        var data = new DashboardSummaryDto();
        var map = DashboardKpiCardMap.Build(data);
        Assert.Equal(13, map.Count);
        Assert.DoesNotContain(map.Keys, k => k == "NetProfit");
    }

    [Fact]
    public void Build_ContainsExpectedKeys()
    {
        var data = new DashboardSummaryDto();
        var map = DashboardKpiCardMap.Build(data);
        Assert.True(map.ContainsKey("ExpectedProfit"));
        Assert.True(map.ContainsKey("ActualProfit"));
    }
}