namespace DzentoPOS2.Shared.Contracts.DTOs.Partners
{
    public class UpdateCustomerDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Street { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
