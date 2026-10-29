using DzentoPOS2.Core.Exceptions;

namespace DzentoPOS2.Core.Services.Sales;

public readonly record struct InvoiceLine(decimal Quantity, decimal Price);

/// <summary>
/// How the invoice-level discount is interpreted:
/// <see cref="Percent"/> = percentage of the subtotal,
/// <see cref="FixedAmount"/> = absolute money amount.
/// </summary>
public enum DiscountMode
{
    Percent = 0,
    FixedAmount = 1
}

public sealed record InvoiceTotals(
    decimal Subtotal,
    decimal DiscountAmount,
    decimal AfterDiscount,
    decimal TaxAmount,
    decimal Total,
    decimal TenderedAmount,
    decimal AppliedPayment,
    decimal Remaining,
    decimal Change);

/// <summary>
/// Pure invoice math. This is the single source of truth for every amount the
/// screen previews, the invoice stores, the receipt prints and the cash drawer
/// records. It performs no rounding-side effects and never touches the database.
/// </summary>
public static class InvoiceCalculator
{
    public static InvoiceTotals Calculate(
        IReadOnlyCollection<InvoiceLine> lines,
        DiscountMode discountMode,
        decimal discountValue,
        decimal taxRate,
        decimal tenderedAmount,
        int currencyDecimalDigits)
    {
        if (lines is null || lines.Count == 0)
            throw new CodedDomainException(DomainErrorCodes.SaleRequiresItem, "Sale must have at least one item");

        if (!Enum.IsDefined(typeof(DiscountMode), discountMode))
            throw new CodedDomainException(DomainErrorCodes.InvalidDiscountMode, "Discount mode is invalid");

        if (discountValue < 0)
            throw new CodedDomainException(DomainErrorCodes.DiscountNegative, "Discount cannot be negative");

        if (taxRate < 0 || taxRate > 100)
            throw new CodedDomainException(DomainErrorCodes.InvalidTaxRate, "Tax must be between 0 and 100");

        if (tenderedAmount < 0)
            throw new CodedDomainException(DomainErrorCodes.InvalidPayment, "Paid amount cannot be negative");

        if (currencyDecimalDigits < 0 || currencyDecimalDigits > 4)
            throw new CodedDomainException(DomainErrorCodes.InvalidCurrency, "Currency decimal digits are invalid");

        // Each line is rounded first so the printed lines always add up to the
        // subtotal (no drift between the lines and their sum).
        decimal subtotal = 0;
        foreach (var line in lines)
        {
            if (line.Quantity <= 0)
                throw new CodedDomainException(DomainErrorCodes.InvalidQuantity, "Item quantity must be greater than zero");

            if (line.Price < 0)
                throw new CodedDomainException(DomainErrorCodes.InvalidPrice, "Item price cannot be negative");

            subtotal += Round(line.Quantity * line.Price, currencyDecimalDigits);
        }

        subtotal = Round(subtotal, currencyDecimalDigits);

        decimal discountAmount;
        if (discountMode == DiscountMode.Percent)
        {
            if (discountValue > 100)
                throw new CodedDomainException(DomainErrorCodes.DiscountExceedsSubtotal, "Discount percentage must be between 0 and 100");

            discountAmount = Round(subtotal * discountValue / 100m, currencyDecimalDigits);
            if (discountAmount > subtotal)
                discountAmount = subtotal;
        }
        else
        {
            if (discountValue > subtotal)
                throw new CodedDomainException(DomainErrorCodes.DiscountExceedsSubtotal, "Discount cannot exceed subtotal");

            discountAmount = Round(discountValue, currencyDecimalDigits);
        }

        var afterDiscount = subtotal - discountAmount;
        var taxAmount = Round(afterDiscount * taxRate / 100m, currencyDecimalDigits);
        var total = afterDiscount + taxAmount;

        var tendered = Round(tenderedAmount, currencyDecimalDigits);
        var appliedPayment = Math.Min(tendered, total);
        var remaining = Math.Max(0, total - tendered);
        var change = Math.Max(0, tendered - total);

        return new InvoiceTotals(
            subtotal,
            discountAmount,
            afterDiscount,
            taxAmount,
            total,
            tendered,
            appliedPayment,
            remaining,
            change);
    }

    private static decimal Round(decimal value, int digits)
    {
        return decimal.Round(value, digits, MidpointRounding.AwayFromZero);
    }
}