using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Accounting.Interfaces;
using DzentoPOS2.Data.Repositories.Common;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Accounting
{
    public class CashTransactionRepository
        : Repository<CashTransaction>,
        ICashTransactionRepository
    {
        public CashTransactionRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<CashTransaction>> GetByCashboxAsync(
            Guid cashboxId)
        {
            return await _dbSet
                .Where(x =>
                    x.CashboxId == cashboxId &&
                    !x.IsDeleted)
                .OrderByDescending(x => x.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CashTransaction>> GetByDateRangeAsync(
            DateTime startDate,
            DateTime endDate)
        {
            return await _dbSet
                .Where(x =>
                    x.TransactionDate >= startDate &&
                    x.TransactionDate <= endDate &&
                    !x.IsDeleted)
                .OrderByDescending(x => x.TransactionDate)
                .ToListAsync();
        }
    }
}