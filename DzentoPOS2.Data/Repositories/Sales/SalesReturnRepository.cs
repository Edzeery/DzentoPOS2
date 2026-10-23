using DzentoPOS2.Core.Entities.Sales;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Sales.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Sales
{
    public class SalesReturnRepository
        : Repository<SalesReturn>,
          ISalesReturnRepository
    {
        public SalesReturnRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<SalesReturn?> GetWithItemsAsync(Guid id)
        {
            return await _dbSet
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);
        }

        public async Task<IEnumerable<SalesReturn>> GetByCustomerAsync(Guid customerId)
        {
            return await _dbSet
                .Where(x =>
                    x.CustomerId == customerId &&
                    !x.IsDeleted)
                .ToListAsync();
        }
    }
}