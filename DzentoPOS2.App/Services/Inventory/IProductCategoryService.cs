using DzentoPOS2.Shared.Contracts.DTOs.Inventory;

namespace DzentoPOS2.App.Services.Inventory;

public interface IProductCategoryService
{
    Task<List<ProductCategoryDto>> GetAllAsync();
    Task<ProductCategoryDto?> GetByIdAsync(Guid id);
    Task<List<ProductCategoryDto>> SearchAsync(string? term);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null);
    Task<ProductCategoryDto> CreateAsync(CreateProductCategoryDto dto);
    Task<ProductCategoryDto> UpdateAsync(UpdateProductCategoryDto dto);
    Task DeleteAsync(Guid id);
}
