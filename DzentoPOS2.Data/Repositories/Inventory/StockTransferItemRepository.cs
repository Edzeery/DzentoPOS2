using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Inventory.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Inventory
{
    public class StockTransferItemRepository
        : Repository<StockTransferItem>,
        IStockTransferItemRepository
    {
        public StockTransferItemRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<StockTransferItem>> GetByTransferAsync(Guid transferId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(x => x.Product)
                .Where(x =>
                    x.StockTransferId == transferId &&
                    !x.IsDeleted)
                .ToListAsync();
        }
    }
}