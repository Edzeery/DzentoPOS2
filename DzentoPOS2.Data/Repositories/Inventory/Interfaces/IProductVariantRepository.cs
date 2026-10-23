using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Inventory.Interfaces
{
    public interface IProductVariantRepository
        : IRepository<ProductVariant>
    {
        Task<IEnumerable<ProductVariant>> GetByProductAsync(Guid productId);
    }
}