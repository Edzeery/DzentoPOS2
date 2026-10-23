using DzentoPOS2.Shared.Contracts.DTOs.Reports;

namespace DzentoPOS2.App.Services.Reports.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetSummaryAsync();
    }
}
