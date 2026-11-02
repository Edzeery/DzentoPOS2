using DzentoPOS2.Core.Services.Sales;
using Xunit;

namespace DzentoPOS2.UnitTests;

public sealed class ProfitAcceptanceTests
{
    [Fact]
    public void AcceptanceInvoice()
    {
        // 2 x 1000, cost 600, discount 10%, tax 19%, fees 50
        var lines = new[] { new InvoiceLine(2, 1000m) };
        var totals = InvoiceCalculator.Calculate(lines, DiscountMode.Percent, 10m, 19m, 2142m, 2);

        Assert.Equal(2142m, totals.Total);

        var profitLines = new[]
        {
            new ProfitLine(2, 1000m, 1000m, 600m, totals.DiscountAmount)
        };
        var profit = ProfitCalculator.CalculateInvoice(profitLines, fees: 50m, digits: 2);

        Assert.Equal(800m, profit.ExpectedProfit);
        Assert.Equal(600m, profit.GrossProfit);
        Assert.Equal(550m, profit.ActualProfit);
    }
}