using DzentoPOS2.Shared.Contracts.DTOs.Inventory;

namespace DzentoPOS2.App.Services.Inventory;

public interface IProductService
{
    Task<List<ProductDto>> GetAllAsync();
    Task<ProductDto?> GetByIdAsync(Guid id);
    Task<List<ProductDto>> SearchAsync(string? term);
    Task<ProductDto> CreateAsync(CreateProductDto dto);
    Task<ProductDto> UpdateAsync(UpdateProductDto dto);
    Task DeleteAsync(Guid id);
    Task<List<ProductCategoryDto>> GetCategoriesAsync();
    Task<List<ProductUnitDto>> GetUnitsAsync();
    Task<List<WarehouseDto>> GetWarehousesAsync();
    Task<ProductInventoryStatsDto> GetInventoryStatsAsync();
    Task<List<ProductStockDto>> GetProductStocksAsync(Guid productId);
    Task<ProductDto?> GetBySkuAsync(string sku);
}
