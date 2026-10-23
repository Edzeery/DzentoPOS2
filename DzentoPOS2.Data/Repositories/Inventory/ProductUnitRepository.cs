using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Inventory.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Inventory
{
    public class ProductUnitRepository
        : Repository<ProductUnit>,
        IProductUnitRepository
    {
        public ProductUnitRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<ProductUnit>> GetBaseUnitsAsync()
        {
            return await _dbSet
                .Where(x =>
                    x.IsBaseUnit &&
                    !x.IsDeleted)
                .ToListAsync();
        }
    }
}