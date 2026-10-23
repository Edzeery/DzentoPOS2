using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Inventory.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Inventory
{
    public class ProductPriceRepository
        : Repository<ProductPrice>,
        IProductPriceRepository
    {
        public ProductPriceRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<ProductPrice>> GetByProductAsync(Guid productId)
        {
            return await _dbSet
                .Where(x =>
                    x.ProductId == productId &&
                    !x.IsDeleted)
                .AsNoTracking()
                .OrderByDescending(x => x.StartDate)
                .ToListAsync();
        }

        public async Task<ProductPrice?> GetCurrentPriceAsync(Guid productId)
        {
            var now = DateTime.UtcNow;

            return await _dbSet
                .FirstOrDefaultAsync(x =>
                    x.ProductId == productId &&
                    x.StartDate <= now &&
                    (x.EndDate == null || x.EndDate >= now) &&
                    !x.IsDeleted);
        }
    }
}