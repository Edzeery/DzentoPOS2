using DzentoPOS2.Shared.Contracts.DTOs.Reports;

namespace DzentoPOS2.App.Services.Reports.Interfaces;

/// <summary>
/// Unified report that compares every branch side by side for one period. It is
/// read-only and never changes any data. A user without
/// <c>BranchesViewAll</c> sees only the active branch.
/// </summary>
public interface ICrossBranchReportService
{
    Task<CrossBranchReportDto> GetAsync(CrossBranchReportFilterDto? filter = null);
}
