using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Inventory.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Inventory
{
    public class StockAdjustmentRepository
        : Repository<StockAdjustment>,
        IStockAdjustmentRepository
    {
        public StockAdjustmentRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<StockAdjustment?> GetWithTransactionsAsync(Guid id)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(x => x.Warehouse)
                .Include(x => x.CreatedByUser)
                .Include(x => x.Transactions)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);
        }

        public async Task<IEnumerable<StockAdjustment>> GetByWarehouseAsync(Guid warehouseId)
        {
            return await _dbSet
                .Where(x =>
                    x.WarehouseId == warehouseId &&
                    !x.IsDeleted)
                .OrderByDescending(x => x.AdjustmentDate)
                .ToListAsync();
        }
    }
}