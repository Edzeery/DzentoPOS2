using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Shared.Contracts.DTOs.Reports;
using DzentoPOS2.WinForms.Helpers;

namespace DzentoPOS2.WinForms.Modules.Dashboard
{
    internal class InsightsModule
    {
        private DataGridView? _bestGrid;
        private DataGridView? _alertsGrid;
        private DataGridView? _customersGrid;
        private DataGridView? _suppliersGrid;

        public Panel Build()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 360,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 24)
            };

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = Color.Transparent
            };

            for (int i = 0; i < 4; i++)
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            var bestGrid = BuildInsightGrid(AppStrings.ProductLabel, "Total");
            var alertsGrid = BuildInsightGrid(AppStrings.ProductLabel, AppStrings.MinimumStockAmount);
            var customersGrid = BuildInsightGrid(AppStrings.CustomerLabel, "Total");
            var suppliersGrid = BuildInsightGrid(AppStrings.SupplierLabel, "Total");

            _bestGrid = bestGrid;
            _alertsGrid = alertsGrid;
            _customersGrid = customersGrid;
            _suppliersGrid = suppliersGrid;

            grid.Controls.Add(UIHelper.WrapInCard(bestGrid, AppStrings.TopSellingProducts, 36, 16, 50), 0, 0);
            grid.Controls.Add(UIHelper.WrapInCard(alertsGrid, AppStrings.StockAlerts, 36, 16, 50), 1, 0);
            grid.Controls.Add(UIHelper.WrapInCard(customersGrid, AppStrings.BestCustomers, 36, 16, 50), 2, 0);
            grid.Controls.Add(UIHelper.WrapInCard(suppliersGrid, AppStrings.MostActiveSuppliers, 36, 16, 50), 3, 0);

            panel.Controls.Add(grid);
            return panel;
        }

        private static DataGridView BuildInsightGrid(string col2Header, string col3Header)
        {
            var dgv = new DataGridView();
            DataGridViewStyleHelper.ApplyLightStyle(dgv);

            dgv.Columns.Add("Rank", "#");
            dgv.Columns.Add("Name", col2Header);
            dgv.Columns.Add("Value", col3Header);

            dgv.Columns[0].FillWeight = 15F;
            dgv.Columns[1].FillWeight = 55F;
            dgv.Columns[2].FillWeight = 30F;

            return dgv;
        }

        public void Update(DashboardSummaryDto data)
        {
            FillGrid(_bestGrid, data.BestSellingProducts);
            FillGrid(_alertsGrid, data.StockAlerts);
            FillGrid(_customersGrid, data.TopCustomers);
            FillGrid(_suppliersGrid, data.TopSuppliers);
        }

        private static void FillGrid(DataGridView? grid, List<InsightRow> rows)
        {
            // Build() may not have run yet (for example a branch change raised
            // before the form loaded). Nothing to fill in that case.
            if (grid is null || grid.IsDisposed) return;

            grid.Rows.Clear();
            foreach (var r in rows)
                grid.Rows.Add(r.Rank, r.Name, r.DisplayValue);
        }
    }
}
