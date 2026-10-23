using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Accounting.Interfaces
{
    public interface ICashTransactionRepository
        : IRepository<CashTransaction>
    {
        Task<IEnumerable<CashTransaction>> GetByCashboxAsync(Guid cashboxId);

        Task<IEnumerable<CashTransaction>> GetByDateRangeAsync(
            DateTime startDate,
            DateTime endDate);
    }
}