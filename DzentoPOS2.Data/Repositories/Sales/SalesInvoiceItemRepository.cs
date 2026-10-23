using DzentoPOS2.Core.Entities.Sales;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Sales.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Sales
{
    public class SalesInvoiceItemRepository
        : Repository<SalesInvoiceItem>,
          ISalesInvoiceItemRepository
    {
        public SalesInvoiceItemRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<SalesInvoiceItem>> GetByInvoiceAsync(Guid invoiceId)
        {
            return await _dbSet
                .Where(x =>
                    x.SalesInvoiceId == invoiceId &&
                    !x.IsDeleted)
                .ToListAsync();
        }
    }
}