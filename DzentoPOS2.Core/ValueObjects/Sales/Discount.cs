using DzentoPOS2.Core.ValueObjects.Common;

namespace DzentoPOS2.Core.ValueObjects.Sales
{
    public class Discount : ValueObject
    {
        public decimal Value { get; private set; }

        private Discount() { }

        public Discount(decimal value)
        {
            if (value < 0 || value > 100)
                throw new ArgumentException("Discount must be between 0 and 100");

            Value = value;
        }

        public decimal Apply(decimal amount)
        {
            return amount - (amount * Value / 100);
        }
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}