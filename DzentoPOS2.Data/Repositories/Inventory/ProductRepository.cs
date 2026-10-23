using System.Linq.Expressions;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Inventory.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Inventory
{
    public class ProductRepository
        : Repository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext context)
            : base(context)
        {
        }

        // The product grid and the search show the category and the unit names,
        // so every list read has to load those navigations.
        public override async Task<IReadOnlyList<Product>> GetAllAsync()
        {
            return await _dbSet
                .Include(x => x.Category)
                .Include(x => x.Unit)
                .AsNoTracking()
                .ToListAsync();
        }

        public override async Task<IReadOnlyList<Product>> FindAsync(
            Expression<Func<Product, bool>> predicate)
        {
            return await _dbSet
                .Include(x => x.Category)
                .Include(x => x.Unit)
                .AsNoTracking()
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<Product?> GetBySkuAsync(string sku)
        {
            return await _dbSet
                .Include(x => x.Category)
                .Include(x => x.Unit)
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.SKU == sku &&
                    !x.IsDeleted);
        }

        public async Task<Product?> GetWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(x => x.Category)
                .AsNoTracking()
                .Include(x => x.Unit)
                .Include(x => x.Barcodes)
                .Include(x => x.Prices)
                .Include(x => x.Variants)
                .Include(x => x.Stocks)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);
        }

        public async Task<IEnumerable<Product>> GetActiveAsync()
        {
            return await _dbSet
                .Where(x =>
                    x.IsActive &&
                    !x.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x =>
                    x.CategoryId == categoryId &&
                    !x.IsDeleted)
                .ToListAsync();
        }
    }
}