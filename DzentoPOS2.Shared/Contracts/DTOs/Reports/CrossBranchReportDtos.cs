namespace DzentoPOS2.Shared.Contracts.DTOs.Reports;

/// <summary>
/// Filter for the cross-branch report. <see cref="ViewAllBranches"/> is only a
/// request: the service honours it only when the signed-in user holds
/// <c>BranchesViewAll</c>; otherwise the report is limited to the active branch.
/// </summary>
public class CrossBranchReportFilterDto
{
    public DashboardDatePreset Preset { get; set; } = DashboardDatePreset.ThisMonth;

    public DateTime? CustomFrom { get; set; }

    public DateTime? CustomTo { get; set; }

    public bool ViewAllBranches { get; set; } = true;
}

/// <summary>
/// One figure per branch for the selected period plus a grand total. Every
/// branch that is in scope appears even when it had no activity, so the report
/// is a true comparison rather than a list of branches that happened to sell.
/// </summary>
public class CrossBranchReportDto
{
    public DateTime RangeStartUtc { get; set; }

    public DateTime RangeEndUtc { get; set; }

    public bool ViewAllBranches { get; set; }

    public List<BranchReportRowDto> Rows { get; set; } = [];

    public BranchReportRowDto Total { get; set; } = new();
}

public class BranchReportRowDto
{
    public Guid BranchId { get; set; }

    public string BranchName { get; set; } = string.Empty;

    public bool IsMainBranch { get; set; }

    public bool IsActive { get; set; }

    public int SalesCount { get; set; }

    public decimal SalesTotal { get; set; }

    public decimal ReturnsTotal { get; set; }

    /// <summary>Sales net of returns (<see cref="SalesTotal"/> − <see cref="ReturnsTotal"/>).</summary>
    public decimal NetSales { get; set; }

    public int PurchasesCount { get; set; }

    public decimal PurchasesTotal { get; set; }

    public decimal ExpensesTotal { get; set; }

    public decimal GrossProfit { get; set; }

    public decimal NetProfit { get; set; }
}
