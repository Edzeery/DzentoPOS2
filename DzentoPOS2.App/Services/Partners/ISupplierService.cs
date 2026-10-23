using DzentoPOS2.Shared.Contracts.DTOs.Partners;

namespace DzentoPOS2.App.Services.Partners
{
    public interface ISupplierService
    {
        Task<List<SupplierDto>> GetAllAsync();
        Task<SupplierDto?> GetByIdAsync(Guid id);
        Task<List<SupplierDto>> SearchAsync(string? term);
        Task<SupplierDto> CreateAsync(CreateSupplierDto dto);
        Task<SupplierDto> UpdateAsync(UpdateSupplierDto dto);
        Task DeleteAsync(Guid id);
    }
}
