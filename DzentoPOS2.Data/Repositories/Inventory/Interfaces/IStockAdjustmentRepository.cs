using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Inventory.Interfaces
{
    public interface IStockAdjustmentRepository
        : IRepository<StockAdjustment>
    {
        Task<StockAdjustment?> GetWithTransactionsAsync(Guid id);

        Task<IEnumerable<StockAdjustment>> GetByWarehouseAsync(Guid warehouseId);
    }
}