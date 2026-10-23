using DzentoPOS2.Core.Entities.Sales;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Sales.Interfaces
{
    public interface ISalesReturnRepository : IRepository<SalesReturn>
    {
        Task<SalesReturn?> GetWithItemsAsync(Guid id);

        Task<IEnumerable<SalesReturn>> GetByCustomerAsync(Guid customerId);
    }
}