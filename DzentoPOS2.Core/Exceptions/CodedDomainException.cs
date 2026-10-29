namespace DzentoPOS2.Core.Exceptions;

/// <summary>
/// Stable error codes for domain/validation failures. They let the UI layer map
/// a failure to a localized <c>AppStrings</c> entry without matching on the
/// (untranslated) exception message.
/// </summary>
public static class DomainErrorCodes
{
    public const string SaleRequiresItem = "sale.requires_item";
    public const string InvalidQuantity = "sale.invalid_quantity";
    public const string InvalidPrice = "sale.invalid_price";
    public const string DiscountNegative = "sale.discount_negative";
    public const string DiscountExceedsSubtotal = "sale.discount_exceeds_subtotal";
    public const string DiscountExceedsLimit = "sale.discount_exceeds_limit";
    public const string InvalidTaxRate = "sale.invalid_tax_rate";
    public const string InvalidPayment = "sale.invalid_payment";
    public const string InvalidCurrency = "sale.invalid_currency";
    public const string InvalidDiscountMode = "sale.invalid_discount_mode";
    public const string CreditRequiresCustomer = "sale.credit_requires_customer";
    public const string InsufficientStock = "inventory.insufficient_stock";
    public const string NoOpenSession = "cash.no_open_session";
    public const string SessionAlreadyOpen = "cash.session_already_open";
    public const string SessionAlreadyClosed = "cash.session_already_closed";
    public const string SessionNotFound = "cash.session_not_found";
    public const string CashboxNotFound = "cash.cashbox_not_found";
}

/// <summary>
/// A <see cref="DomainException"/> that carries a stable <see cref="Code"/>, so
/// the presentation layer can localize it.
/// </summary>
public sealed class CodedDomainException : DomainException
{
    public string Code { get; }

    public CodedDomainException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    public CodedDomainException(string code, string message, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }
}