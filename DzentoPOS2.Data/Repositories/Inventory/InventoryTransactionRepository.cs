using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Inventory.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Inventory
{
    public class InventoryTransactionRepository
        : Repository<InventoryTransaction>,
        IInventoryTransactionRepository
    {
        public InventoryTransactionRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<InventoryTransaction>> GetByProductAsync(Guid productId)
        {
            return await _dbSet
                .Include(x => x.Warehouse)
                .Where(x =>
                    x.ProductId == productId &&
                    !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryTransaction>> GetByWarehouseAsync(Guid warehouseId)
        {
            return await _dbSet
                .Include(x => x.Product)
                .Where(x =>
                    x.WarehouseId == warehouseId &&
                    !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}