using DzentoPOS2.Core.Entities.Sales;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Sales.Interfaces
{
    public interface ISalesInvoiceItemRepository : IRepository<SalesInvoiceItem>
    {
        Task<IEnumerable<SalesInvoiceItem>> GetByInvoiceAsync(Guid invoiceId);
    }
}