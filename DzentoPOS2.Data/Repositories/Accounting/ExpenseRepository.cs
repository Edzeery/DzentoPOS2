using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Accounting.Interfaces;
using DzentoPOS2.Data.Repositories.Common;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Accounting
{
    public class ExpenseRepository
        : Repository<Expense>,
        IExpenseRepository
    {
        public ExpenseRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<Expense>> GetByBranchAsync(
            Guid branchId)
        {
            return await _dbSet
                .Where(x =>
                    x.BranchId == branchId &&
                    !x.IsDeleted)
                .OrderByDescending(x => x.ExpenseDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Expense>> GetByDateRangeAsync(
            DateTime startDate,
            DateTime endDate)
        {
            return await _dbSet
                .Where(x =>
                    x.ExpenseDate >= startDate &&
                    x.ExpenseDate <= endDate &&
                    !x.IsDeleted)
                .OrderByDescending(x => x.ExpenseDate)
                .ToListAsync();
        }
    }
}