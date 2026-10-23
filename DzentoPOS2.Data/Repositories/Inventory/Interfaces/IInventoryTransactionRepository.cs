using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Inventory.Interfaces
{
    public interface IInventoryTransactionRepository
        : IRepository<InventoryTransaction>
    {
        Task<IEnumerable<InventoryTransaction>> GetByProductAsync(Guid productId);

        Task<IEnumerable<InventoryTransaction>> GetByWarehouseAsync(Guid warehouseId);
    }
}