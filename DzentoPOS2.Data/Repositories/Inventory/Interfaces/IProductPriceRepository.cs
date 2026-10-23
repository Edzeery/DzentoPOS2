using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Inventory.Interfaces
{
    public interface IProductPriceRepository
        : IRepository<ProductPrice>
    {
        Task<IEnumerable<ProductPrice>> GetByProductAsync(Guid productId);

        Task<ProductPrice?> GetCurrentPriceAsync(Guid productId);
    }
}