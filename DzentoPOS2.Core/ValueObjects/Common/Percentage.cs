namespace DzentoPOS2.Core.ValueObjects.Common
{
    public class Percentage 
    {
        public decimal Value { get; private set; }

        private Percentage() { }

        public Percentage(decimal value)
        {
            if (value < 0 || value > 100)
                throw new ArgumentException("Percentage must be between 0 and 100");

            Value = value;
        }

        public decimal Apply(decimal amount)
        {
            return amount * Value / 100;
        }
    }
}