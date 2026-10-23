using DzentoPOS2.Core.Entities.Purchases;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Purchases.Interfaces
{
    public interface IPurchaseReturnRepository : IRepository<PurchaseReturn>
    {
        Task<PurchaseReturn?> GetWithItemsAsync(Guid id);

        Task<IEnumerable<PurchaseReturn>> GetBySupplierAsync(Guid supplierId);
    }
}