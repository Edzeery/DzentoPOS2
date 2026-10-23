using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Inventory.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Inventory
{
    public class ProductStockRepository
        : Repository<ProductStock>,
        IProductStockRepository
    {
        public ProductStockRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<ProductStock?> GetStockAsync(
            Guid productId,
            Guid warehouseId)
        {
            return await _dbSet
                .Include(x => x.Product)
                .Include(x => x.Warehouse)
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.ProductId == productId &&
                    x.WarehouseId == warehouseId &&
                    !x.IsDeleted);
        }

        public async Task<IEnumerable<ProductStock>> GetByWarehouseAsync(
            Guid warehouseId)
        {
            return await _dbSet
                .Include(x => x.Product)
                 .AsNoTracking()
                .Where(x =>
                    x.WarehouseId == warehouseId &&
                    !x.IsDeleted)
                .ToListAsync();
        }
    }
}