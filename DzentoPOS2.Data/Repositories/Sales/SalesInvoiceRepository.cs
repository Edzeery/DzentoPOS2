using DzentoPOS2.Core.Entities.Sales;
using DzentoPOS2.Core.ValueObjects.Sales;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Sales.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Sales
{
    public class SalesInvoiceRepository
        : Repository<SalesInvoice>,
          ISalesInvoiceRepository
    {
        public SalesInvoiceRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<SalesInvoice?> GetWithItemsAsync(Guid id)
        {
            return await _dbSet
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);
        }

        public async Task<SalesInvoice?> GetByNumberAsync(InvoiceNumber invoiceNumber)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x =>
                    x.InvoiceNumber == invoiceNumber &&
                    !x.IsDeleted);
        }

        public async Task<IEnumerable<SalesInvoice>> GetByCustomerAsync(Guid customerId)
        {
            return await _dbSet
                .Where(x =>
                    x.CustomerId == customerId &&
                    !x.IsDeleted)
                .ToListAsync();
        }
         
    }
}