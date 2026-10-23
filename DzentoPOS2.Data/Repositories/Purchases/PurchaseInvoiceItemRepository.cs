using DzentoPOS2.Core.Entities.Purchases;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Purchases.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Purchases
{
    public class PurchaseInvoiceItemRepository
        : Repository<PurchaseInvoiceItem>,
          IPurchaseInvoiceItemRepository
    {
        public PurchaseInvoiceItemRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<PurchaseInvoiceItem>> GetByInvoiceAsync(Guid invoiceId)
        {
            return await _dbSet
                .Where(x =>
                    x.PurchaseInvoiceId == invoiceId &&
                    !x.IsDeleted)
                .ToListAsync();
        }
    }
}