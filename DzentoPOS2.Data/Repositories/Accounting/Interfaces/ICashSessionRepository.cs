using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Accounting.Interfaces
{
    public interface ICashSessionRepository
        : IRepository<CashSession>
    {
        Task<CashSession?> GetOpenByCashboxAsync(Guid cashboxId);

        Task<CashSession?> GetOpenAsync();

        Task<IEnumerable<CashSession>> GetRecentAsync(Guid cashboxId, int take);
    }
}
