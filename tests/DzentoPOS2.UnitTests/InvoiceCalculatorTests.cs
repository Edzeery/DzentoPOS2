using DzentoPOS2.Core.Exceptions;
using DzentoPOS2.Core.Services.Sales;
using Xunit;

namespace DzentoPOS2.UnitTests;

public sealed class InvoiceCalculatorTests
{
    private static InvoiceTotals Calc(
        decimal quantity,
        decimal price,
        DiscountMode mode = DiscountMode.Percent,
        decimal discount = 0,
        decimal tax = 0,
        decimal paid = 0,
        int digits = 2)
    {
        return InvoiceCalculator.Calculate(
            new[] { new InvoiceLine(quantity, price) },
            mode,
            discount,
            tax,
            paid,
            digits);
    }

    private static InvoiceTotals CalcLines(
        IReadOnlyCollection<InvoiceLine> lines,
        DiscountMode mode = DiscountMode.Percent,
        decimal discount = 0,
        decimal tax = 0,
        decimal paid = 0,
        int digits = 2)
    {
        return InvoiceCalculator.Calculate(lines, mode, discount, tax, paid, digits);
    }

    // ---- basics -----------------------------------------------------------

    [Fact]
    public void CalculatesSimpleInvoice()
    {
        var totals = Calc(2, 50, paid: 100);

        Assert.Equal(100, totals.Subtotal);
        Assert.Equal(0, totals.DiscountAmount);
        Assert.Equal(100, totals.AfterDiscount);
        Assert.Equal(0, totals.TaxAmount);
        Assert.Equal(100, totals.Total);
        Assert.Equal(100, totals.AppliedPayment);
        Assert.Equal(0, totals.Remaining);
        Assert.Equal(0, totals.Change);
    }

    [Fact]
    public void TaxIsAppliedAfterDiscount()
    {
        var totals = Calc(1, 100, discount: 10, tax: 20, paid: 108);

        Assert.Equal(100, totals.Subtotal);
        Assert.Equal(10, totals.DiscountAmount);
        Assert.Equal(90, totals.AfterDiscount);
        Assert.Equal(18, totals.TaxAmount);
        Assert.Equal(108, totals.Total);
    }

    [Fact]
    public void ZeroTaxProducesNoTaxAmount()
    {
        var totals = Calc(1, 50, discount: 10, tax: 0, paid: 45);

        Assert.Equal(5, totals.DiscountAmount);
        Assert.Equal(0, totals.TaxAmount);
        Assert.Equal(45, totals.Total);
    }

    [Fact]
    public void NineteenPercentTaxIsRoundedOnce()
    {
        var totals = Calc(1, 100, discount: 0, tax: 19, paid: 119);

        Assert.Equal(119, totals.Total);
        Assert.Equal(19, totals.TaxAmount);
    }

    // ---- discount modes ---------------------------------------------------

    [Fact]
    public void ZeroPercentDiscountLeavesSubtotalUntouched()
    {
        var totals = Calc(1, 75, DiscountMode.Percent, 0, 19);

        Assert.Equal(0, totals.DiscountAmount);
        Assert.Equal(75, totals.AfterDiscount);
    }

    [Fact]
    public void HundredPercentDiscountProducesZeroTaxAndTotal()
    {
        var totals = Calc(1, 75, DiscountMode.Percent, 100, 19);

        Assert.Equal(75, totals.DiscountAmount);
        Assert.Equal(0, totals.AfterDiscount);
        Assert.Equal(0, totals.TaxAmount);
        Assert.Equal(0, totals.Total);
    }

    [Fact]
    public void FixedDiscountEqualsSubtotalProducesZeroTotal()
    {
        var totals = Calc(1, 75, DiscountMode.FixedAmount, 75, 19);

        Assert.Equal(75, totals.DiscountAmount);
        Assert.Equal(0, totals.AfterDiscount);
        Assert.Equal(0, totals.Total);
    }

    [Fact]
    public void FixedDiscountPartialIsSubtractedDirectly()
    {
        var totals = Calc(1, 100, DiscountMode.FixedAmount, 30, 20, 84);

        Assert.Equal(100, totals.Subtotal);
        Assert.Equal(30, totals.DiscountAmount);
        Assert.Equal(70, totals.AfterDiscount);
        Assert.Equal(14, totals.TaxAmount);
        Assert.Equal(84, totals.Total);
    }

    [Fact]
    public void FixedDiscountAboveSubtotalIsRejected()
    {
        Assert.ThrowsAny<DomainException>(() =>
            Calc(1, 50, DiscountMode.FixedAmount, 50.01m));
    }

    [Fact]
    public void PercentAboveHundredIsRejected()
    {
        Assert.ThrowsAny<DomainException>(() =>
            Calc(1, 50, DiscountMode.Percent, 100.01m));
    }

    [Fact]
    public void FixedDiscountIsRoundedToCurrencyDigits()
    {
        var totals = Calc(1, 100, DiscountMode.FixedAmount, 12.345m, 0, 0, 2);

        Assert.Equal(12.35m, totals.DiscountAmount);
    }

    [Fact]
    public void InvalidDiscountModeIsRejected()
    {
        Assert.ThrowsAny<DomainException>(() =>
            Calc(1, 10, (DiscountMode)99));
    }

    [Fact]
    public void NegativeDiscountIsRejected()
    {
        Assert.ThrowsAny<DomainException>(() => Calc(1, 10, discount: -0.01m));
    }

    // ---- rounding ---------------------------------------------------------

    [Fact]
    public void RoundsHalfAwayFromZero()
    {
        var totals = Calc(1, 10.005m, digits: 2);

        Assert.Equal(10.01m, totals.Subtotal);
    }

    [Fact]
    public void RoundsEachLineBeforeSumming()
    {
        // 3 * 0.335 = 1.005 -> 1.01 ; 1 * 0.005 = 0.005 -> 0.01
        var totals = CalcLines(new[]
        {
            new InvoiceLine(3, 0.335m),
            new InvoiceLine(1, 0.005m)
        });

        Assert.Equal(1.02m, totals.Subtotal);
    }

    [Fact]
    public void LineRoundingDriftSumsCorrectly()
    {
        // three lines of 0.333 each round to 0.33 -> 0.99 (not 1.00).
        var totals = CalcLines(new[]
        {
            new InvoiceLine(1, 0.333m),
            new InvoiceLine(1, 0.333m),
            new InvoiceLine(1, 0.333m)
        });

        Assert.Equal(0.99m, totals.Subtotal);
    }

    [Fact]
    public void RoundsDiscountAndTaxIndependently()
    {
        var totals = Calc(1, 99.99m, discount: 12.5m, tax: 9m, paid: 0, digits: 2);

        Assert.Equal(99.99m, totals.Subtotal);
        Assert.Equal(12.50m, totals.DiscountAmount);
        Assert.Equal(87.49m, totals.AfterDiscount);
        Assert.Equal(7.87m, totals.TaxAmount);
        Assert.Equal(95.36m, totals.Total);
        Assert.Equal(95.36m, totals.Remaining);
    }

    [Fact]
    public void AcceptsZeroPriceLine()
    {
        var totals = Calc(1, 0, tax: 20);

        Assert.Equal(0, totals.Total);
    }

    // ---- payment / change -------------------------------------------------

    [Fact]
    public void OverpaymentProducesChangeAndCapsAppliedPaymentAtTotal()
    {
        var totals = Calc(1, 20, tax: 10, paid: 50);

        Assert.Equal(22, totals.Total);
        Assert.Equal(50, totals.TenderedAmount);
        Assert.Equal(22, totals.AppliedPayment);
        Assert.Equal(0, totals.Remaining);
        Assert.Equal(28, totals.Change);
    }

    [Fact]
    public void ExactPaymentHasNoChangeOrRemaining()
    {
        var totals = Calc(1, 20, tax: 10, paid: 22);

        Assert.Equal(22, totals.AppliedPayment);
        Assert.Equal(0, totals.Remaining);
        Assert.Equal(0, totals.Change);
    }

    [Fact]
    public void ZeroPaidProducesFullRemaining()
    {
        var totals = Calc(2, 30, tax: 10, paid: 0);

        Assert.Equal(66, totals.Total);
        Assert.Equal(0, totals.AppliedPayment);
        Assert.Equal(66, totals.Remaining);
        Assert.Equal(0, totals.Change);
    }

    [Fact]
    public void PartialPaymentProducesRemaining()
    {
        var totals = Calc(2, 30, tax: 10, paid: 25);

        Assert.Equal(66, totals.Total);
        Assert.Equal(25, totals.AppliedPayment);
        Assert.Equal(41, totals.Remaining);
        Assert.Equal(0, totals.Change);
    }

    [Fact]
    public void AppliedPaymentIsAlwaysMinOfPaidAndTotal()
    {
        var under = Calc(1, 100, paid: 40);
        var over = Calc(1, 100, paid: 140);

        Assert.Equal(40, under.AppliedPayment);
        Assert.Equal(100, over.AppliedPayment);
    }

    // ---- validation -------------------------------------------------------

    [Fact]
    public void RejectsEmptyInvoice()
    {
        Assert.ThrowsAny<DomainException>(() =>
            InvoiceCalculator.Calculate(
                Array.Empty<InvoiceLine>(),
                DiscountMode.Percent,
                0,
                0,
                0,
                2));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RejectsNonPositiveQuantity(decimal quantity)
    {
        Assert.ThrowsAny<DomainException>(() => Calc(quantity, 10));
    }

    [Fact]
    public void RejectsNegativePrice()
    {
        Assert.ThrowsAny<DomainException>(() => Calc(1, -1));
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    public void RejectsInvalidTax(decimal tax)
    {
        Assert.ThrowsAny<DomainException>(() => Calc(1, 10, tax: tax));
    }

    [Fact]
    public void RejectsNegativePayment()
    {
        Assert.ThrowsAny<DomainException>(() => Calc(1, 10, paid: -1));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(5)]
    public void RejectsInvalidCurrencyDigits(int digits)
    {
        Assert.ThrowsAny<DomainException>(() => Calc(1, 10, digits: digits));
    }

    [Fact]
    public void ThreeDecimalCurrencyRoundsToThreeDigits()
    {
        var totals = Calc(1, 10.0005m, digits: 3);

        Assert.Equal(10.001m, totals.Subtotal);
    }
}