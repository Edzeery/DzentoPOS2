using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Inventory.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<Product?> GetBySkuAsync(string sku);

        Task<Product?> GetWithDetailsAsync(Guid id);

        Task<IEnumerable<Product>> GetActiveAsync();

        Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId);
    }
}