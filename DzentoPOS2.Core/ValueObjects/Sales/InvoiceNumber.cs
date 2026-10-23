using DzentoPOS2.Core.ValueObjects.Common;

namespace DzentoPOS2.Core.ValueObjects.Sales
{
    public class InvoiceNumber : ValueObject
    {
        public string Value { get; private set; } = default!;

        private InvoiceNumber() { }

        public InvoiceNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("رقم الفاتورة مطلوب");

            if (value.Length < 5)
                throw new ArgumentException("رقم الفاتورة قصير جدًا");

            Value = value;
        }

        public override string ToString() => Value;

     
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}