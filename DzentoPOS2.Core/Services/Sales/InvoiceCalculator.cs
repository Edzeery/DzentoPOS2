using DzentoPOS2.Core.Exceptions;

namespace DzentoPOS2.Core.Services.Sales;

public readonly record struct InvoiceLine(decimal Quantity, decimal Price);

public sealed record InvoiceTotals(
    decimal Subtotal,
    decimal DiscountAmount,
    decimal AfterDiscount,
    decimal TaxAmount,
    decimal Total,
    decimal PaidAmount,
    decimal RemainingAmount,
    decimal ChangeAmount);

public static class InvoiceCalculator
{
    public static InvoiceTotals Calculate(
        IReadOnlyCollection<InvoiceLine> lines,
        decimal discountPercent,
        decimal taxRate,
        decimal paidAmount,
        int currencyDecimalDigits)
    {
        if (lines is null || lines.Count == 0)
            throw new DomainException("Sale must have at least one item");

        if (discountPercent < 0 || discountPercent > 100)
            throw new DomainException("Discount must be between 0 and 100");

        if (taxRate < 0 || taxRate > 100)
            throw new DomainException("Tax must be between 0 and 100");

        if (paidAmount < 0)
            throw new DomainException("Paid amount cannot be negative");

        if (currencyDecimalDigits < 0 || currencyDecimalDigits > 4)
            throw new DomainException("Currency decimal digits are invalid");

        decimal rawSubtotal = 0;

        foreach (var line in lines)
        {
            if (line.Quantity <= 0)
                throw new DomainException("Item quantity must be greater than zero");

            if (line.Price < 0)
                throw new DomainException("Item price cannot be negative");

            rawSubtotal += line.Quantity * line.Price;
        }

        var subtotal = Round(rawSubtotal, currencyDecimalDigits);
        var discountAmount = Round(
            subtotal * discountPercent / 100m,
            currencyDecimalDigits);
        var afterDiscount = subtotal - discountAmount;
        var taxAmount = Round(
            afterDiscount * taxRate / 100m,
            currencyDecimalDigits);
        var total = afterDiscount + taxAmount;
        var paid = Round(paidAmount, currencyDecimalDigits);
        var remaining = Math.Max(0, total - paid);
        var change = Math.Max(0, paid - total);

        return new InvoiceTotals(
            subtotal,
            discountAmount,
            afterDiscount,
            taxAmount,
            total,
            paid,
            remaining,
            change);
    }

    private static decimal Round(decimal value, int digits)
    {
        return decimal.Round(value, digits, MidpointRounding.AwayFromZero);
    }
}
