using DzentoPOS2.Shared.Contracts.DTOs.Sales;

namespace DzentoPOS2.App.Services.Sales
{
    public interface ISaleService
    {
        Task<List<SaleDto>> GetAllAsync();
        Task<SaleDto?> GetByIdAsync(Guid id);
        Task<List<SaleDto>> GetByCustomerAsync(Guid customerId);
        Task<SaleDto> CreateAsync(CreateSaleDto dto);
        Task DeleteAsync(Guid id);
        Task VoidAsync(Guid id, string reason);
        Task<decimal> GetTodayTotalAsync();
        Task<int> GetTodayCountAsync();
    }
}
