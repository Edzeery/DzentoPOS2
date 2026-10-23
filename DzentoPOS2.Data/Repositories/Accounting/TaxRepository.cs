using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Accounting.Interfaces;
using DzentoPOS2.Data.Repositories.Common;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Accounting
{
    public class TaxRepository
        : Repository<Tax>,
        ITaxRepository
    {
        public TaxRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<Tax>> GetActiveAsync()
        {
            return await _dbSet
                .Where(x =>
                    x.IsActive &&
                    !x.IsDeleted)
                .ToListAsync();
        }
    }
}