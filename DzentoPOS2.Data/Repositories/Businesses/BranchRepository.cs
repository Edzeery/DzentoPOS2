using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Businesses.Interfaces;
using DzentoPOS2.Data.Repositories.Common;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Businesses
{
    public class BranchRepository
        : Repository<Branch>,
          IBranchRepository
    {
        public BranchRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<Branch?> GetWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(x => x.Warehouses)
                .Include(x => x.CashRegisters)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);
        }

        public async Task<IEnumerable<Branch>> GetActiveBranchesAsync()
        {
            return await _dbSet
                .Where(x =>
                    x.IsActive &&
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