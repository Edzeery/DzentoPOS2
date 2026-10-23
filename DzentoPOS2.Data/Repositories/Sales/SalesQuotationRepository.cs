using DzentoPOS2.Core.Entities.Sales;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Sales.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Sales
{
    public class SalesQuotationRepository
        : Repository<SalesQuotation>,
          ISalesQuotationRepository
    {
        public SalesQuotationRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<SalesQuotation>> GetByCustomerAsync(Guid customerId)
        {
            return await _dbSet
                .Where(x =>
                    x.CustomerId == customerId &&
                    !x.IsDeleted)
                .ToListAsync();
        }

        public async Task<SalesQuotation?> GetByNumberAsync(string quotationNumber)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x =>
                    x.QuotationNumber == quotationNumber &&
                    !x.IsDeleted);
        }
    }
}