using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Inventory.Interfaces
{
    public interface IProductCategoryRepository
        : IRepository<ProductCategory>
    {
        Task<IEnumerable<ProductCategory>> GetRootCategoriesAsync();

        Task<ProductCategory?> GetWithChildrenAsync(Guid id);
    }
}