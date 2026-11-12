using DzentoPOS2.Shared.Contracts.DTOs.Reports;
using DzentoPOS2.Shared.Helpers;
using Xunit;

namespace DzentoPOS2.UnitTests;

public sealed class DashboardKpiCardMapTests
{
    [Fact]
    public void Build_ReturnsThirteenEntries()
    {
        var data = new DashboardSummaryDto();
        var map = DashboardKpiCardMap.Build(data);
        Assert.Equal(13, map.Count);
        Assert.DoesNotContain(map.Keys, k => k == "NetProfit");
    }

    /// <summary>
    /// Locks the dashboard card keys to the ones the dashboard modules render.
    /// A card added to (or removed from) one side without the other would leave
    /// a KPI silently missing; the KpiCardsModule definitions must match this.
    /// </summary>
    [Fact]
    public void Build_ContainsExactlyTheRenderedCardKeys()
    {
        var map = DashboardKpiCardMap.Build(new DashboardSummaryDto());
        string[] expected =
        [
            "SalesToday", "PurchasesToday", "SalesCount", "PurchasesCount",
            "MonthlySales", "MonthlyPurchases", "ExpectedProfit", "ExpectedStockProfit",
            "ActualProfit", "ActiveProducts", "LowStock", "ActiveCustomers", "ActiveSuppliers"
        ];
        Assert.Equal(expected.OrderBy(k => k), map.Keys.OrderBy(k => k));
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