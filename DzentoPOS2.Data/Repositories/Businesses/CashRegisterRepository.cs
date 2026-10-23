using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Businesses.Interfaces;
using DzentoPOS2.Data.Repositories.Common;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Businesses
{
    public class CashRegisterRepository
        : Repository<CashRegister>,
          ICashRegisterRepository
    {
        public CashRegisterRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<CashRegister?> GetWithBranchAsync(Guid id)
        {
            return await _dbSet
                .Include(x => x.Branch)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);
        }

        public async Task<IEnumerable<CashRegister>> GetByBranchAsync(
            Guid branchId)
        {
            return await _dbSet
                .Where(x =>
                    x.BranchId == branchId &&
                    !x.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> ExistsByCodeAsync(string code)
        {
            return await _dbSet
                .AnyAsync(x =>
                    x.Code == code &&
                    !x.IsDeleted);
        }
    }
}