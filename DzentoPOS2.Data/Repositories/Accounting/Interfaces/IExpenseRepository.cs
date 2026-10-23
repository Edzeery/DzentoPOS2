using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Accounting.Interfaces
{
    public interface IExpenseRepository
        : IRepository<Expense>
    {
        Task<IEnumerable<Expense>> GetByBranchAsync(Guid branchId);

        Task<IEnumerable<Expense>> GetByDateRangeAsync(
            DateTime startDate,
            DateTime endDate);
    }
}