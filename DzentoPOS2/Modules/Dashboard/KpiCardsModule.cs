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

        // Single source of truth for the KPI cards: key + icon. Keeping these in
        // one list (instead of parallel arrays) makes it impossible for the icon
        // mapping to fall out of sync with the card count, which previously
        // caused an IndexOutOfRangeException in Build().
        private static readonly (string Key, IconChar Icon)[] Definitions =
        [
            ("SalesToday", IconChar.ShoppingCart),
            ("PurchasesToday", IconChar.Truck),
            ("SalesCount", IconChar.Receipt),
            ("PurchasesCount", IconChar.ClipboardList),
            ("MonthlySales", IconChar.ChartLine),
            ("MonthlyPurchases", IconChar.BoxOpen),
            ("ExpectedProfit", IconChar.ChartPie),
            ("ExpectedStockProfit", IconChar.MoneyBillWave),
            ("ActualProfit", IconChar.Boxes),
            ("ActiveProducts", IconChar.BoxesStacked),
            ("LowStock", IconChar.ExclamationTriangle),
            ("ActiveCustomers", IconChar.Users),
            ("ActiveSuppliers", IconChar.Handshake),
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
            System.Diagnostics.Debug.Assert(
                titles.Length == Definitions.Length,
                "KPI card titles and definitions must have the same length.");

            for (int i = 0; i < Definitions.Length; i++)
            {
                var (key, icon) = Definitions[i];
                // Fall back to the key if a localized title is ever missing so a
                // mapping mistake degrades gracefully instead of crashing.
                var title = i < titles.Length ? titles[i] : key;
                var card = new KpiCard(key, title, icon, Colors[i % Colors.Length]);
                _cards.Add(card);
            }

            Reflow(grid);

            grid.Resize += (_, _) => Reflow(grid);

            return grid;
        }

        private static string[] GetTitles() =>
        [
            AppStrings.TodaySales,AppStrings.TodayPurchases,AppStrings.SalesInvoicesCount,AppStrings.PurchaseInvoicesCount,
            AppStrings.MonthlySales,AppStrings.MonthlyPurchases,AppStrings.ExpectedProfit,AppStrings.ExpectedStock,AppStrings.ActualProfit,
            AppStrings.ActiveProductsKPI,AppStrings.LowStockKPI,AppStrings.ActiveCustomers,AppStrings.ActiveSuppliers
        ];

        private void Reflow(TableLayoutPanel grid)
        {
            if (_cards.Count == 0) return;
            int cols = grid.Width < 480 ? 1 : grid.Width < 768 ? 2 : grid.Width < 1200 ? 3 : 4;
            if (cols == grid.ColumnCount && grid.Controls.Count == _cards.Count) return;

            grid.SuspendLayout();
            grid.Controls.Clear();
            grid.ColumnCount = cols;
            grid.RowStyles.Clear();
            int rows = (int)Math.Ceiling(_cards.Count / (double)cols);
            for (int r = 0; r < rows; r++)
                grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            for (int i = 0; i < _cards.Count; i++)
                grid.Controls.Add(_cards[i], i % cols, i / cols);
            grid.ResumeLayout();
        }

        public void Update(DashboardSummaryDto data)
        {
            _actualProfit = data.ActualProfit;
            _monthlyDiscounts = data.MonthlyDiscountAmount;
            _monthlyFees = data.MonthlyFeesAmount;
            _netProfit = data.NetProfit.Value;

            var map = DashboardKpiCardMap.Build(data);

            var digits = KpiCard.CurrencyDigits;
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            var expectedFormula = $"{AppStrings.ExpectedProfit} ({AppStrings.ExpectedStock}, NRV) \u2264 {AppStrings.PotentialProfit}";

            foreach (var card in _cards)
            {
                if (map.TryGetValue(card.CardKey, out var d))
                {
                    card.SetValue(d.Value, d.Kind, d.TrendKind, d.TrendPercent);
                    card.SetEstimated(d.HasEstimatedCost);
                }

                if (card.CardKey == "ExpectedProfit")
                    card.SetTooltip(expectedFormula);
            }
        }
    }
}
