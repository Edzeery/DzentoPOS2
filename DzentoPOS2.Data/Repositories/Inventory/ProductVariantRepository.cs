using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Inventory.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Inventory
{
    public class ProductVariantRepository
        : Repository<ProductVariant>,
        IProductVariantRepository
    {
        public ProductVariantRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<ProductVariant>> GetByProductAsync(Guid productId)
        {
            return await _dbSet
                .Where(x =>
                    x.ProductId == productId &&
                    !x.IsDeleted)
                .ToListAsync();
        }
    }
}