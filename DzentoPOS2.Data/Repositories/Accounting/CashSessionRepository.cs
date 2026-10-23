using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Accounting.Interfaces;
using DzentoPOS2.Data.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Accounting
{
    public class CashSessionRepository
        : Repository<CashSession>,
        ICashSessionRepository
    {
        public CashSessionRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<CashSession?> GetOpenByCashboxAsync(Guid cashboxId)
        {
            return await _dbSet
                .Where(x =>
                    x.CashboxId == cashboxId &&
                    x.Status == CashSessionStatus.Open &&
                    !x.IsDeleted)
                .OrderByDescending(x => x.OpenedAtUtc)
                .FirstOrDefaultAsync();
        }

        public async Task<CashSession?> GetOpenAsync()
        {
            return await _dbSet
                .Where(x =>
                    x.Status == CashSessionStatus.Open &&
                    !x.IsDeleted)
                .OrderByDescending(x => x.OpenedAtUtc)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<CashSession>> GetRecentAsync(Guid cashboxId, int take)
        {
            return await _dbSet
                .Where(x =>
                    x.CashboxId == cashboxId &&
                    !x.IsDeleted)
                .OrderByDescending(x => x.OpenedAtUtc)
                .Take(take)
                .ToListAsync();
        }
    }
}
