using System.Collections.Generic;
using DzentoPOS2.Shared.Contracts.DTOs.Reports;

namespace DzentoPOS2.Shared.Helpers
{
    public static class DashboardKpiCardMap
    {
        public static Dictionary<string, DashboardCardData> Build(DashboardSummaryDto data)
        {
            return new Dictionary<string, DashboardCardData>
            {
                ["SalesToday"] = data.SalesToday,
                ["PurchasesToday"] = data.PurchasesToday,
                ["SalesCount"] = data.SalesCount,
                ["PurchasesCount"] = data.PurchasesCount,
                ["MonthlySales"] = data.MonthlySales,
                ["MonthlyPurchases"] = data.MonthlyPurchases,
                ["ExpectedProfit"] = data.ExpectedProfit,
                ["ActualProfit"] = data.ActualProfit,
                ["ActiveProducts"] = data.ActiveProducts,
                ["LowStock"] = data.LowStock,
                ["ActiveCustomers"] = data.ActiveCustomers,
                ["ActiveSuppliers"] = data.ActiveSuppliers,
            };
        }
    }
}