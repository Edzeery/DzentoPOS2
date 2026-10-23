using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Businesses.Interfaces;
using DzentoPOS2.Data.Repositories.Common;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Businesses
{
    public class WarehouseRepository
        : Repository<Warehouse>,
          IWarehouseRepository
    {
        public WarehouseRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<Warehouse?> GetWithBranchAsync(Guid id)
        {
            return await _dbSet
                .Include(x => x.Branch)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);
        }

        public async Task<IEnumerable<Warehouse>> GetByBranchAsync(
            Guid branchId)
        {
            return await _dbSet
                .Where(x =>
                    x.BranchId == branchId &&
                    !x.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _dbSet
                .AnyAsync(x =>
                    x.Name == name &&
                    !x.IsDeleted);
        }
    }
}