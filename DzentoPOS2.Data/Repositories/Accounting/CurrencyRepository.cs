using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Accounting.Interfaces;
using DzentoPOS2.Data.Repositories.Common;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Accounting
{
    public class CurrencyRepository
        : Repository<Currency>,
        ICurrencyRepository
    {
        public CurrencyRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<Currency?> GetByCodeAsync(string code)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x =>
                    x.Code == code &&
                    !x.IsDeleted);
        }

        public async Task<Currency?> GetDefaultCurrencyAsync()
        {
            return await _dbSet
                .FirstOrDefaultAsync(x =>
                    x.IsDefault &&
                    !x.IsDeleted);
        }
    }
}