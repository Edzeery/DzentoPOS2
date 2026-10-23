using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Inventory.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Inventory
{
    public class ProductCategoryRepository
        : Repository<ProductCategory>,
        IProductCategoryRepository
    {
        public ProductCategoryRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<ProductCategory>> GetRootCategoriesAsync()
        {
            return await _dbSet
                .Where(x =>
                    x.ParentCategoryId == null &&
                    !x.IsDeleted)

                .ToListAsync();
        }

        public async Task<ProductCategory?> GetWithChildrenAsync(Guid id)
        {
            return await _dbSet
                .Include(x => x.Children)
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);
        }
    }
}