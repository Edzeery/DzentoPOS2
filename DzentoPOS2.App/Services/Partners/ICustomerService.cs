using DzentoPOS2.Shared.Contracts.DTOs.Partners;

namespace DzentoPOS2.App.Services.Partners
{
    public interface ICustomerService
    {
        Task<List<CustomerDto>> GetAllAsync();
        Task<CustomerDto?> GetByIdAsync(Guid id);
        Task<List<CustomerDto>> SearchAsync(string? term);
        Task<CustomerDto> CreateAsync(CreateCustomerDto dto);
        Task<CustomerDto> UpdateAsync(UpdateCustomerDto dto);
        Task DeleteAsync(Guid id);
    }
}
