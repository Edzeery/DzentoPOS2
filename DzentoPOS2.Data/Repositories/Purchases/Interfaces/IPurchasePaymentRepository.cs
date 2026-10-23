using DzentoPOS2.Core.Entities.Purchases;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Purchases.Interfaces
{
    public interface IPurchasePaymentRepository : IRepository<PurchasePayment>
    {
        Task<IEnumerable<PurchasePayment>> GetByInvoiceAsync(Guid purchaseInvoiceId);
    }
}