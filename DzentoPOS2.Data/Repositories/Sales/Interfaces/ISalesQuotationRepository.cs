using DzentoPOS2.Core.Entities.Sales;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Sales.Interfaces
{
    public interface ISalesQuotationRepository : IRepository<SalesQuotation>
    {
        Task<IEnumerable<SalesQuotation>> GetByCustomerAsync(Guid customerId);

        Task<SalesQuotation?> GetByNumberAsync(string quotationNumber);
    }
}