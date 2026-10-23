using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Inventory.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Inventory
{
    public class StockTransferRepository
        : Repository<StockTransfer>,
        IStockTransferRepository
    {
        public StockTransferRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<StockTransfer?> GetWithItemsAsync(Guid id)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(x => x.FromWarehouse)
                .Include(x => x.ToWarehouse)
                .Include(x => x.CreatedByUser)
                .Include(x => x.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);
        }

        public async Task<IEnumerable<StockTransfer>> GetByWarehouseAsync(Guid warehouseId)
        {
            return await _dbSet
                .Where(x =>
                    (x.FromWarehouseId == warehouseId ||
                     x.ToWarehouseId == warehouseId) &&
                    !x.IsDeleted)
                .OrderByDescending(x => x.TransferDate)
                .ToListAsync();
        }
    }
}