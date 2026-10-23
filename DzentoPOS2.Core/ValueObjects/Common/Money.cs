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
                throw new ArgumentException("لا يمكن أن يكون المبلغ سالباً");

            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("العملة مطلوبة");

            Amount = amount;
            Currency = currency;
        }

        public Money Add(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException("عدم تطابق العملة");

            return new Money(Amount + other.Amount, Currency);
        }

        public Money Subtract(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException("عدم تطابق العملة");

            return new Money(Amount - other.Amount, Currency);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Amount;
        }
    }
}