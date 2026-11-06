using DzentoPOS2.Core.Entities.Sales;
using DzentoPOS2.Core.Services.Sales;
using Xunit;

namespace DzentoPOS2.UnitTests;

public sealed class SalesInvoiceCreateTests
{
    [Fact]
    public void CreateAppliesDiscountBeforeTaxAndStoresFees()
    {
        var lines = new[] { new InvoiceLine(2, 1000m) };
        var totals = InvoiceCalculator.Calculate(lines, DiscountMode.Percent, 10m, 19m, 2142m, 2);

        var invoice = SalesInvoice.Create("INV-1", Guid.Empty, Guid.Empty, totals, DiscountMode.Percent, 10m, 19m, "DZD", 50m);

        Assert.Equal(2000m, invoice.Subtotal.Amount);
        Assert.Equal(200m, invoice.DiscountAmount.Amount);
        Assert.Equal(342m, invoice.TaxAmount.Amount);
        Assert.Equal(2142m, invoice.Total.Amount);
        Assert.Equal(50m, invoice.FeesAmount.Amount);
    }

    [Fact]
    public void StoredTotalMatchesCalculatorOutput()
    {
        var lines = new[] { new InvoiceLine(2, 1000m) };
        var totals = InvoiceCalculator.Calculate(lines, DiscountMode.Percent, 10m, 19m, 0m, 2);
        var invoice = SalesInvoice.Create("INV-2", Guid.Empty, Guid.Empty, totals, DiscountMode.Percent, 10m, 19m, "DZD");
        Assert.Equal(totals.Total, invoice.Total.Amount);
    }
}