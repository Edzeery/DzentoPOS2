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
        private DashboardCardData? _actualProfit;
        private decimal _monthlyDiscounts;
        private decimal _monthlyFees;
        private decimal _netProfit;

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
                var card = new KpiCard(Keys[i], titles[i], Icons[i], Colors[i % Colors.Length]);
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
            _actualProfit = data.ActualProfit;
            _monthlyDiscounts = data.MonthlyDiscountAmount;
            _monthlyFees = data.MonthlyFeesAmount;
            _netProfit = data.NetProfit.Value;

            var map = DashboardKpiCardMap.Build(data);

            var digits = KpiCard.CurrencyDigits;
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            var detail = string.Join(Environment.NewLine,
                $"{AppStrings.Discount}: {KpiValueFormatter.Format(_monthlyDiscounts, KpiValueKind.Money, culture, digits)}",
                $"{AppStrings.Fees}: {KpiValueFormatter.Format(_monthlyFees, KpiValueKind.Money, culture, digits)}",
                $"{AppStrings.NetProfit}: {KpiValueFormatter.Format(_netProfit, KpiValueKind.Money, culture, digits)}");

            foreach (var card in _cards)
            {
                if (map.TryGetValue(card.CardKey, out var d))
                {
                    card.SetValue(d.Value, d.Kind, d.TrendKind, d.TrendPercent);
                    card.SetEstimated(d.HasEstimatedCost);
                }

                if (card.CardKey == "ActualProfit")
                    card.SetDetail(detail);
            }
        }
    }
}
