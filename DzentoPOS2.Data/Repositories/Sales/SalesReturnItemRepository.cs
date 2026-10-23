using DzentoPOS2.Core.Entities.Sales;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Sales.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Sales
{
    public class SalesReturnItemRepository
        : Repository<SalesReturnItem>,
          ISalesReturnItemRepository
    {
        public SalesReturnItemRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<SalesReturnItem>> GetByReturnAsync(Guid returnId)
        {
            return await _dbSet
                .Where(x =>
                    x.SalesReturnId == returnId &&
                    !x.IsDeleted)
                .ToListAsync();
        }
    }
}