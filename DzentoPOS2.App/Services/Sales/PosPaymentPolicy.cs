namespace DzentoPOS2.App.Services.Sales;

/// <summary>Payment methods offered by the POS screen.</summary>
public enum PosPaymentMethod
{
    Cash,
    Card
}

/// <summary>
/// Pure rules for the payment section of the POS screen. Card payments are
/// captured for the exact total, so they never leave a credit balance and the
/// cash drawer must not be credited for them.
/// </summary>
public static class PosPaymentPolicy
{
    public const string Cash = "Cash";
    public const string Card = "Card";

    /// <summary>Value persisted in <c>CreateSaleDto.PaymentMethod</c>.</summary>
    public static string ToApiValue(PosPaymentMethod method) =>
        method == PosPaymentMethod.Card ? Card : Cash;

    /// <summary>
    /// Card terminals settle the exact amount, so the paid value equals the
    /// total regardless of what the cashier typed. Cash keeps the typed value
    /// and may therefore leave a remaining balance (credit sale).
    /// </summary>
    public static decimal NormalizePaid(
        PosPaymentMethod method,
        decimal total,
        decimal typedPaid) =>
        method == PosPaymentMethod.Card ? total : typedPaid;

    /// <summary>
    /// Only an underpaid cash sale needs a customer to carry the balance.
    /// A card sale is always settled when it is completed.
    /// </summary>
    public static bool RequiresCustomer(
        PosPaymentMethod method,
        decimal total,
        decimal paid) =>
        method != PosPaymentMethod.Card && paid < total;
}
