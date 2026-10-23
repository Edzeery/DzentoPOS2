using DzentoPOS2.Core.ValueObjects.Common;

namespace DzentoPOS2.Core.ValueObjects.Sales
{
    public class Tax : ValueObject
    {
        public decimal Rate { get; private set; } // %

        private Tax() { }

        public Tax(decimal rate)
        {
            if (rate < 0 || rate > 100)
                throw new ArgumentException("Tax must be between 0 and 100");

            Rate = rate;
        }

        public decimal Apply(decimal amount)
        {
            return amount * Rate / 100;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Rate;
        }
    }
}