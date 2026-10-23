using DzentoPOS2.Core.ValueObjects.Common;

namespace DzentoPOS2.Core.ValueObjects.Inventory
{
    public class Quantity : ValueObject
    {
        public decimal Value { get; private set; }

        private Quantity() { }

        public Quantity(decimal value)
        {
            if (value < 0)
                throw new ArgumentException("Quantity cannot be negative");

            Value = value;
        }

        public Quantity Add(Quantity q)
            => new Quantity(Value + q.Value);

        public Quantity Subtract(Quantity q)
        {
            if (Value - q.Value < 0)
                throw new InvalidOperationException("Insufficient stock");

            return new Quantity(Value - q.Value);
        }

        public static implicit operator Quantity(decimal value)
            => new Quantity(value);

        public static implicit operator decimal(Quantity q)
            => q.Value;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}