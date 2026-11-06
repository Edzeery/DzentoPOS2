using DzentoPOS2.Localization.Resources;
namespace DzentoPOS2.Core.ValueObjects.Common
{
    public class Money : ValueObject
    {
        public decimal Amount { get; private set; }
        public string Currency { get; private set; } = default!;

        private Money() { }

        public Money(decimal amount, string currency)
        {
            if (amount < 0)
                throw new ArgumentException(AppStrings.AmountCannotBeNegative);

            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException(AppStrings.CurrencyRequired);

            Amount = amount;
            Currency = currency;
        }

        public Money Add(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException(AppStrings.CurrencyMismatch);

            return new Money(Amount + other.Amount, Currency);
        }

        public Money Subtract(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException(AppStrings.CurrencyMismatch);

            return new Money(Amount - other.Amount, Currency);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Amount;
        }
    }
}