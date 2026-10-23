using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Inventory.Interfaces
{
    public interface IProductStockRepository
        : IRepository<ProductStock>
    {
        Task<ProductStock?> GetStockAsync(
            Guid productId,
            Guid warehouseId);

        Task<IEnumerable<ProductStock>> GetByWarehouseAsync(
            Guid warehouseId);
    }
}