namespace DzentoPOS2.Shared.Contracts.DTOs.Reports
{
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
        public decimal Value { get; set; }
        public decimal TrendPercent { get; set; }
        public bool IsUp => TrendPercent >= 0;
        public string DisplayValue { get; set; } = "0";
        public string TrendDisplay { get; set; } = "0%";
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
