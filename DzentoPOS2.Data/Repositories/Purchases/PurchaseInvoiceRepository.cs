using DzentoPOS2.Core.Entities.Purchases;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Purchases.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Purchases
{
    public class PurchaseInvoiceRepository
        : Repository<PurchaseInvoice>,
          IPurchaseInvoiceRepository
    {
        public PurchaseInvoiceRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<PurchaseInvoice?> GetWithItemsAsync(Guid id)
        {
            return await _dbSet
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);
        }

        public async Task<PurchaseInvoice?> GetByNumberAsync(string invoiceNumber)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x =>
                    x.InvoiceNumber == invoiceNumber &&
                    !x.IsDeleted);
        }

        public async Task<IEnumerable<PurchaseInvoice>> GetBySupplierAsync(Guid supplierId)
        {
            return await _dbSet
                .Where(x =>
                    x.SupplierId == supplierId &&
                    !x.IsDeleted)
                .ToListAsync();
        }
    }
}