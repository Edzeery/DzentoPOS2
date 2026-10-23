using DzentoPOS2.Core.Entities.Sales;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Sales.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Sales
{
    public class SalesPaymentRepository
        : Repository<SalesPayment>,
          ISalesPaymentRepository
    {
        public SalesPaymentRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<SalesPayment>> GetByInvoiceAsync(Guid invoiceId)
        {
            return await _dbSet
                .Where(x =>
                    x.SalesInvoiceId == invoiceId &&
                    !x.IsDeleted)
                .ToListAsync();
        }
    }
}