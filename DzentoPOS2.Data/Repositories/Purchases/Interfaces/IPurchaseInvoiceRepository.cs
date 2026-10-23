using DzentoPOS2.Core.Entities.Purchases;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Purchases.Interfaces
{
    public interface IPurchaseInvoiceRepository : IRepository<PurchaseInvoice>
    {
        Task<PurchaseInvoice?> GetWithItemsAsync(Guid id);

        Task<PurchaseInvoice?> GetByNumberAsync(string invoiceNumber);

        Task<IEnumerable<PurchaseInvoice>> GetBySupplierAsync(Guid supplierId);
    }
}