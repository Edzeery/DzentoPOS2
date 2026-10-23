using DzentoPOS2.Core.ValueObjects.Common;

namespace DzentoPOS2.Core.ValueObjects.Identity
{
    public class PhoneNumber : ValueObject
    {
        public string Value { get; private set; } = default!;

        public static implicit operator PhoneNumber(string value)
        {
            return new PhoneNumber(value);
        }

        public static implicit operator string(PhoneNumber phone) => phone.Value;
        private PhoneNumber() { }

        public PhoneNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("رقم الهاتف مطلوب");

            Value = value;
        }
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}