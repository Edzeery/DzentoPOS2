using DzentoPOS2.Shared.Contracts.DTOs.Purchases;

namespace DzentoPOS2.App.Services.Purchases
{
    public interface IPurchaseService
    {
        Task<List<PurchaseInvoiceDto>> GetAllAsync();
        Task<PurchaseInvoiceDto?> GetByIdAsync(Guid id);
        Task<List<PurchaseInvoiceDto>> GetBySupplierAsync(Guid supplierId);
        Task<PurchaseInvoiceDto> CreateAsync(CreatePurchaseInvoiceDto dto);
        Task DeleteAsync(Guid id);
        Task<decimal> GetTodayTotalAsync();
        Task<int> GetTodayCountAsync();
    }
}
