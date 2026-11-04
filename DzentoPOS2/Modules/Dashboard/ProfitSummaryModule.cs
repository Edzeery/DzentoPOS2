using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Shared.Contracts.DTOs.Reports;
using DzentoPOS2.Shared.Helpers;
using DzentoPOS2.WinForms.Themes;
using System.Globalization;
using System.Windows.Forms;

namespace DzentoPOS2.WinForms.Modules.Dashboard
{
    internal class ProfitSummaryModule
    {
        private TableLayoutPanel _grid = null!;
        private Label _title = null!;
        private readonly Dictionary<string, Label> _values = new();

        public ProfitSummaryModule()
        {
        }

        private void EnsureBuilt()
        {
            if (_title != null && !_title.IsDisposed)
                return;

            _values.Clear();

            _title = new Label
            {
                Text = AppStrings.ProfitSummaryTitle,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = AppColors.Ink,
                Dock = DockStyle.Top,
                Height = 28,
                TextAlign = ContentAlignment.MiddleLeft,
                RightToLeft = RightToLeft.Inherit
            };

            _grid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                MaximumSize = new Size(560, 0),
                ColumnCount = 2,
                RowCount = 0,
                Margin = new Padding(0, 8, 0, 0),
                Padding = new Padding(12),
                BackColor = AppColors.Surface
            };
            _grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));

            BuildRows();
        }

        private void BuildRows()
        {
            AddRow("Revenue", AppStrings.NetRevenue);
            AddRow("Cost", AppStrings.CostOfGoods);
            AddRow("Returns", AppStrings.Returns);
            AddRow("Gross", AppStrings.GrossProfit);
            AddRow("Discounts", AppStrings.Discount);
            AddRow("Fees", AppStrings.Fees);
            AddRow("Actual", AppStrings.ActualProfit);
            AddRow("Expenses", AppStrings.Expenses);
            AddRow("Net", AppStrings.NetProfit);
            AddRow("Expected", AppStrings.ExpectedProfit);
            AddRow("ExpectedSold", AppStrings.ExpectedSold);
            AddRow("ExpectedStock", AppStrings.ExpectedStock);
        }

        private void AddRow(string key, string label)
        {
            int row = _grid.RowCount++;
            _grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var lbl = new Label
            {
                Text = label,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false,
                Margin = new Padding(4, 2, 4, 2)
            };
            var val = new Label
            {
                Text = "0",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                AutoSize = false,
                Margin = new Padding(4, 2, 4, 2)
            };
            _grid.Controls.Add(lbl, 0, row);
            _grid.Controls.Add(val, 1, row);
            _values[key] = val;
        }

        public Control Build()
        {
            EnsureBuilt();

            var panel = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                BackColor = AppColors.Surface,
                Padding = new Padding(0, 0, 0, 16)
            };
            panel.Controls.Add(_grid);
            panel.Controls.Add(_title);
            ApplyTheme();
            return panel;
        }

        public void ApplyTheme()
        {
            if (_title.IsDisposed) return;
            _title.ForeColor = AppColors.Ink;
            _grid.BackColor = AppColors.Surface;
            foreach (Control c in _grid.Controls)
            {
                if (c is Label l) l.ForeColor = AppColors.Ink;
            }
        }

        public void Update(DashboardSummaryDto data, int currencyDigits)
        {
            var culture = CultureInfo.CurrentCulture;
            Set("Revenue", data.MonthlyRevenue, currencyDigits, culture);
            Set("Cost", data.MonthlyCost, currencyDigits, culture);
            Set("Returns", data.MonthlyReturns, currencyDigits, culture);
            Set("Gross", data.MonthlyGrossProfit, currencyDigits, culture);
            Set("Discounts", data.MonthlyDiscountAmount, currencyDigits, culture);
            Set("Fees", data.MonthlyFeesAmount, currencyDigits, culture);
            Set("Actual", data.ActualProfit.Value, currencyDigits, culture);
            Set("Expenses", data.MonthlyExpenses, currencyDigits, culture);
            Set("Net", data.NetProfit.Value, currencyDigits, culture);
            Set("Expected", data.ExpectedProfit.Value, currencyDigits, culture);
            Set("ExpectedSold", data.ExpectedSoldProfit.Value, currencyDigits, culture);
            Set("ExpectedStock", data.ExpectedStockProfit.Value, currencyDigits, culture);
        }

        private void Set(string key, decimal value, int digits, CultureInfo culture)
        {
            if (_values.TryGetValue(key, out var lbl))
                lbl.Text = KpiValueFormatter.Format(value, KpiValueKind.Money, culture, digits);
        }
    }
}