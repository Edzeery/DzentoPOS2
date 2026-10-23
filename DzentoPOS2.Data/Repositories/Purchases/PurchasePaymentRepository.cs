using DzentoPOS2.Core.Entities.Purchases;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Purchases.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Purchases
{
    public class PurchasePaymentRepository
        : Repository<PurchasePayment>,
          IPurchasePaymentRepository
    {
        public PurchasePaymentRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<PurchasePayment>> GetByInvoiceAsync(Guid purchaseInvoiceId)
        {
            return await _dbSet
                .Where(x =>
                    x.PurchaseInvoiceId == purchaseInvoiceId &&
                    !x.IsDeleted)
                .ToListAsync();
        }
    }
}