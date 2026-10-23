using DzentoPOS2.Core.Exceptions;
using DzentoPOS2.Core.Services.Sales;
using Xunit;

namespace DzentoPOS2.UnitTests;

public sealed class InvoiceCalculatorTests
{
    private static InvoiceTotals Calculate(
        decimal quantity,
        decimal price,
        decimal discount = 0,
        decimal tax = 0,
        decimal paid = 0,
        int digits = 2)
    {
        return InvoiceCalculator.Calculate(
            new[] { new InvoiceLine(quantity, price) },
            discount,
            tax,
            paid,
            digits);
    }

    [Fact]
    public void CalculatesSimpleInvoice()
    {
        var totals = Calculate(2, 50, paid: 100);

        Assert.Equal(100, totals.Subtotal);
        Assert.Equal(0, totals.DiscountAmount);
        Assert.Equal(0, totals.TaxAmount);
        Assert.Equal(100, totals.Total);
        Assert.Equal(0, totals.RemainingAmount);
        Assert.Equal(0, totals.ChangeAmount);
    }

    [Fact]
    public void TaxIsAppliedAfterDiscount()
    {
        var totals = Calculate(1, 100, discount: 10, tax: 20, paid: 108);

        Assert.Equal(100, totals.Subtotal);
        Assert.Equal(10, totals.DiscountAmount);
        Assert.Equal(90, totals.AfterDiscount);
        Assert.Equal(18, totals.TaxAmount);
        Assert.Equal(108, totals.Total);
    }

    [Fact]
    public void HundredPercentDiscountProducesZeroTaxAndTotal()
    {
        var totals = Calculate(1, 75, discount: 100, tax: 19);

        Assert.Equal(75, totals.DiscountAmount);
        Assert.Equal(0, totals.AfterDiscount);
        Assert.Equal(0, totals.TaxAmount);
        Assert.Equal(0, totals.Total);
    }

    [Fact]
    public void PaymentAboveTotalProducesChange()
    {
        var totals = Calculate(1, 20, tax: 10, paid: 50);

        Assert.Equal(22, totals.Total);
        Assert.Equal(0, totals.RemainingAmount);
        Assert.Equal(28, totals.ChangeAmount);
    }

    [Fact]
    public void PartialPaymentProducesRemaining()
    {
        var totals = Calculate(2, 30, tax: 10, paid: 25);

        Assert.Equal(66, totals.Total);
        Assert.Equal(41, totals.RemainingAmount);
        Assert.Equal(0, totals.ChangeAmount);
    }

    [Fact]
    public void RoundsHalfAwayFromZero()
    {
        var totals = Calculate(1, 10.005m, digits: 2);

        Assert.Equal(10.01m, totals.Subtotal);
    }

    [Fact]
    public void RoundsDiscountAndTaxIndependently()
    {
        var totals = Calculate(1, 99.99m, discount: 12.5m, tax: 9m, paid: 0, digits: 2);

        Assert.Equal(99.99m, totals.Subtotal);
        Assert.Equal(12.50m, totals.DiscountAmount);
        Assert.Equal(87.49m, totals.AfterDiscount);
        Assert.Equal(7.87m, totals.TaxAmount);
        Assert.Equal(95.36m, totals.Total);
        Assert.Equal(95.36m, totals.RemainingAmount);
    }

    [Fact]
    public void SumsMultipleLinesBeforeRounding()
    {
        var totals = InvoiceCalculator.Calculate(
            new[]
            {
                new InvoiceLine(3, 0.335m),
                new InvoiceLine(1, 0.005m)
            },
            0,
            0,
            0,
            2);

        Assert.Equal(1.01m, totals.Subtotal);
    }

    [Fact]
    public void AcceptsZeroPriceLine()
    {
        var totals = Calculate(1, 0, tax: 20);

        Assert.Equal(0, totals.Total);
    }

    [Fact]
    public void RejectsEmptyInvoice()
    {
        Assert.Throws<DomainException>(() =>
            InvoiceCalculator.Calculate(Array.Empty<InvoiceLine>(), 0, 0, 0, 2));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RejectsNonPositiveQuantity(decimal quantity)
    {
        Assert.Throws<DomainException>(() => Calculate(quantity, 10));
    }

    [Fact]
    public void RejectsNegativePrice()
    {
        Assert.Throws<DomainException>(() => Calculate(1, -1));
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    public void RejectsInvalidDiscount(decimal discount)
    {
        Assert.Throws<DomainException>(() => Calculate(1, 10, discount: discount));
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    public void RejectsInvalidTax(decimal tax)
    {
        Assert.Throws<DomainException>(() => Calculate(1, 10, tax: tax));
    }

    [Fact]
    public void RejectsNegativePayment()
    {
        Assert.Throws<DomainException>(() => Calculate(1, 10, paid: -1));
    }
}
