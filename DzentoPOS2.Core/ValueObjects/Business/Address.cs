using DzentoPOS2.Core.ValueObjects.Common;

namespace DzentoPOS2.Core.ValueObjects.Business
{
    public class Address : ValueObject
    {
        public string Country { get; private set; } = default!;
        public string City { get; private set; } = default!;
        public string Street { get; private set; } = default!;

        private Address() { } 

        public Address(string country, string city, string street)
        {
            Country = country;
            City = city;
            Street = street;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Country;
            yield return City;
            yield return Street;
        }
    }
}