using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Inventory.Interfaces
{
    public interface IStockTransferRepository
        : IRepository<StockTransfer>
    {
        Task<StockTransfer?> GetWithItemsAsync(Guid id);

        Task<IEnumerable<StockTransfer>> GetByWarehouseAsync(Guid warehouseId);
    }
}