using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Accounting.Interfaces;
using DzentoPOS2.Data.Repositories.Common;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Accounting
{
    public class CashboxRepository
        : Repository<Cashbox>,
        ICashboxRepository
    {
        public CashboxRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<Cashbox?> GetByNameAsync(string name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x =>
                    x.Name == name &&
                    !x.IsDeleted);
        }

        public async Task<IEnumerable<Cashbox>> GetActiveAsync()
        {
            return await _dbSet
                .Where(x =>
                    x.IsActive &&
                    !x.IsDeleted)
                .ToListAsync();
        }
    }
}