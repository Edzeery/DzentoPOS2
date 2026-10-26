namespace DzentoPOS2.Shared.Contracts.DTOs.Reports
{
    using DzentoPOS2.Shared.Helpers;

    public class DashboardSummaryDto
    {
        public DashboardCardData SalesToday { get; set; } = new();
        public DashboardCardData PurchasesToday { get; set; } = new();
        public DashboardCardData SalesCount { get; set; } = new();
        public DashboardCardData PurchasesCount { get; set; } = new();
        public DashboardCardData MonthlySales { get; set; } = new();
        public DashboardCardData MonthlyPurchases { get; set; } = new();
        public DashboardCardData ExpectedProfit { get; set; } = new();
        public DashboardCardData ActualProfit { get; set; } = new();
        public DashboardCardData ActiveProducts { get; set; } = new();
        public DashboardCardData LowStock { get; set; } = new();
        public DashboardCardData ActiveCustomers { get; set; } = new();
        public DashboardCardData ActiveSuppliers { get; set; } = new();
        public List<LineChartPoint> SalesTrend { get; set; } = [];
        public List<ComparisonPoint> MonthlyComparison { get; set; } = [];
        public List<PieSliceData> CategoryDistribution { get; set; } = [];
        public List<TransactionRow> RecentTransactions { get; set; } = [];
        public List<InsightRow> BestSellingProducts { get; set; } = [];
        public List<InsightRow> StockAlerts { get; set; } = [];
        public List<InsightRow> TopCustomers { get; set; } = [];
        public List<InsightRow> TopSuppliers { get; set; } = [];
    }

    public class DashboardCardData
    {
        /// <summary>The raw numeric value (never a pre-formatted string).</summary>
        public decimal Value { get; set; }

        /// <summary>How <see cref="Value"/> must be formatted.</summary>
        public KpiValueKind Kind { get; set; } = KpiValueKind.Count;

        /// <summary>What the small chip under the value shows.</summary>
        public KpiTrendKind TrendKind { get; set; } = KpiTrendKind.None;

        public decimal TrendPercent { get; set; }

        public bool IsUp => TrendPercent >= 0;
    }

    /// <summary>Content of the small trend chip of a KPI card.</summary>
    public enum KpiTrendKind
    {
        /// <summary>No comparable history: the chip is hidden (never a fake +0%).</summary>
        None,

        /// <summary>Change versus the previous period, e.g. "▲ +12%".</summary>
        Change,

        /// <summary>Profit margin, e.g. "Margin 12.5%".</summary>
        Margin
    }

    public class LineChartPoint
    {
        public string Label { get; set; } = "";
        public decimal Value { get; set; }
    }

    public class ComparisonPoint
    {
        public string Label { get; set; } = "";
        public decimal Sales { get; set; }
        public decimal Purchases { get; set; }
    }

    public class PieSliceData
    {
        public string Category { get; set; } = "";
        public decimal Value { get; set; }
    }

    public class TransactionRow
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = "";
        public string Number { get; set; } = "";
        public string Party { get; set; } = "";
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = "";
    }

    public class InsightRow
    {
        public string Rank { get; set; } = "";
        public string Name { get; set; } = "";
        public decimal Value { get; set; }
        public string DisplayValue { get; set; } = "0";
    }
}
