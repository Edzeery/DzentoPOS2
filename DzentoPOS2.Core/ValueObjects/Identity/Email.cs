using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Core.ValueObjects.Common;

namespace DzentoPOS2.Core.ValueObjects.Identity
{
    public class Email : ValueObject
    {
        public string Value { get; private set; } = default!;

        public static implicit operator Email(string value) => new(value);
        public static implicit operator string(Email email) => email.Value;
        private Email() { }

        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(AppStrings.EmailRequired);

            if (!value.Contains('@'))
                throw new ArgumentException(AppStrings.InvalidEmailFormat);

            Value = value.ToLower();
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}