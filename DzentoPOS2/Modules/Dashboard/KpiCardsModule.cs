using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Shared.Contracts.DTOs.Reports;
using DzentoPOS2.Shared.Helpers;
using DzentoPOS2.WinForms.Controls;
using DzentoPOS2.WinForms.Themes;
using FontAwesome.Sharp;

namespace DzentoPOS2.WinForms.Modules.Dashboard
{
    internal class KpiCardsModule
    {
        private readonly List<KpiCard> _cards = new();

        private static Color[] Colors => AppColors.KpiPalette;

        private static readonly IconChar[] Icons =
        [
            IconChar.ShoppingCart, IconChar.Truck, IconChar.Receipt, IconChar.ClipboardList,
            IconChar.ChartLine, IconChar.BoxOpen, IconChar.ChartPie, IconChar.MoneyBillWave,
            IconChar.Boxes, IconChar.ExclamationTriangle, IconChar.Users,
            IconChar.Handshake,
        ];

        private static readonly string[] Keys =
        [
            "SalesToday","PurchasesToday","SalesCount","PurchasesCount",
            "MonthlySales","MonthlyPurchases","ExpectedProfit","ActualProfit",
            "ActiveProducts","LowStock","ActiveCustomers","ActiveSuppliers"
        ];

        public TableLayoutPanel Build()
        {
            _cards.Clear();
            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 4,
                RowCount = 3,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 24)
            };

            for (int i = 0; i < 4; i++)
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            for (int i = 0; i < 3; i++)
                grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var titles = GetTitles();
            for (int i = 0; i < Keys.Length; i++)
            {
                var card = new KpiCard(Keys[i], titles[i], Icons[i], Colors[i]);
                _cards.Add(card);
                grid.Controls.Add(card, i % 4, i / 4);
            }

            for (int i = Keys.Length; i < 12; i++)
                grid.Controls.Add(new Panel(), i % 4, i / 4);

            return grid;
        }

        private static string[] GetTitles() =>
        [
            AppStrings.TodaySales,AppStrings.TodayPurchases,AppStrings.SalesInvoicesCount,AppStrings.PurchaseInvoicesCount,
            AppStrings.MonthlySales,AppStrings.MonthlyPurchases,AppStrings.ExpectedProfit,AppStrings.ActualProfit,
            AppStrings.ActiveProductsKPI,AppStrings.LowStockKPI,AppStrings.ActiveCustomers,AppStrings.ActiveSuppliers
        ];

        public void Update(DashboardSummaryDto data)
        {
            var map = new Dictionary<string, DashboardCardData>
            {
                [Keys[0]] = data.SalesToday,
                [Keys[1]] = data.PurchasesToday,
                [Keys[2]] = data.SalesCount,
                [Keys[3]] = data.PurchasesCount,
                [Keys[4]] = data.MonthlySales,
                [Keys[5]] = data.MonthlyPurchases,
                [Keys[6]] = data.ExpectedProfit,
                [Keys[7]] = data.ActualProfit,
                [Keys[8]] = data.ActiveProducts,
                [Keys[9]] = data.LowStock,
                [Keys[10]] = data.ActiveCustomers,
                [Keys[11]] = data.ActiveSuppliers,
            };

            foreach (var card in _cards)
            {
                if (map.TryGetValue(card.CardKey, out var d))
                    card.SetValue(d.Value, d.Kind, d.TrendKind, d.TrendPercent);
            }
        }
    }
}
