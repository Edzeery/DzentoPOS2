using DzentoPOS2.Core.Entities.Sales;
using DzentoPOS2.Core.ValueObjects.Sales;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Sales.Interfaces
{
    public interface ISalesInvoiceRepository : IRepository<SalesInvoice>
    {
        Task<SalesInvoice?> GetWithItemsAsync(Guid id);

        Task<SalesInvoice?> GetByNumberAsync(InvoiceNumber invoiceNumber);

        Task<IEnumerable<SalesInvoice>> GetByCustomerAsync(Guid customerId);
    }
}