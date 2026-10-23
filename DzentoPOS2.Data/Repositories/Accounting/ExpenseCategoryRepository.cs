using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Accounting.Interfaces;
using DzentoPOS2.Data.Repositories.Common;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Accounting
{
    public class ExpenseCategoryRepository
        : Repository<ExpenseCategory>,
        IExpenseCategoryRepository
    {
        public ExpenseCategoryRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<ExpenseCategory?> GetByNameAsync(
            string name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x =>
                    x.Name == name &&
                    !x.IsDeleted);
        }
    }
}