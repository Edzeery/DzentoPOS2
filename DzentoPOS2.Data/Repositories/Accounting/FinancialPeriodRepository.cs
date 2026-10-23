using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Accounting.Interfaces;
using DzentoPOS2.Data.Repositories.Common;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Accounting
{
    public class FinancialPeriodRepository
        : Repository<FinancialPeriod>,
        IFinancialPeriodRepository
    {
        public FinancialPeriodRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<FinancialPeriod?> GetOpenPeriodAsync()
        {
            return await _dbSet
                .FirstOrDefaultAsync(x =>
                    !x.IsClosed &&
                    !x.IsDeleted);
        }
    }
}