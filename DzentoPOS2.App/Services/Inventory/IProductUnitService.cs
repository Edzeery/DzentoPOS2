using DzentoPOS2.Shared.Contracts.DTOs.Inventory;

namespace DzentoPOS2.App.Services.Inventory;

public interface IProductUnitService
{
    Task<List<ProductUnitDto>> GetAllAsync();
    Task<ProductUnitDto?> GetByIdAsync(Guid id);
    Task<List<ProductUnitDto>> SearchAsync(string? term);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null);
    Task<bool> ExistsBySymbolAsync(string symbol, Guid? excludeId = null);
    Task<List<ProductUnitDto>> GetBaseUnitsAsync();
    Task<ProductUnitDto> CreateAsync(CreateProductUnitDto dto);
    Task<ProductUnitDto> UpdateAsync(UpdateProductUnitDto dto);
    Task DeleteAsync(Guid id);
}
