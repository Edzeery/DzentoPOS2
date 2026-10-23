using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Shared.Contracts.DTOs.Reports;
using DzentoPOS2.WinForms.Helpers;

namespace DzentoPOS2.WinForms.Modules.Dashboard
{
    internal class RecentTransactionsModule
    {
        private DataGridView? _grid;

        public DataGridView Build()
        {
            var grid = new DataGridView();
            DataGridViewStyleHelper.ApplyLightStyle(grid);

            grid.Columns.Add("Type", AppStrings.TransactionType);
            grid.Columns.Add("Number", AppStrings.TransactionNumber);
            grid.Columns.Add("Party", AppStrings.Party);
            grid.Columns.Add("Amount", AppStrings.TransactionAmount);
            grid.Columns.Add("Date", AppStrings.TransactionDate);
            grid.Columns.Add("Status", AppStrings.TransactionStatus);

            grid.CellFormatting += (s, e) =>
            {
                var column = grid.Columns[e.ColumnIndex];
                if (column == null) return;

                if (column.Name == "Amount" && e.Value != null)
                    e.Value = $"{e.Value:N2}";

                if (column.Name == "Status" && e.CellStyle != null)
                {
                    var val = e.Value?.ToString();
                    if (val == AppStrings.Paid)
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(76, 175, 80);
                        e.CellStyle.Font = new Font("Segoe UI Semibold", 10F);
                    }
                    else if (val == AppStrings.Pending)
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(255, 152, 0);
                        e.CellStyle.Font = new Font("Segoe UI Semibold", 10F);
                    }
                }
            };

            return grid;
        }

        public void SetGrid(DataGridView grid) => _grid = grid;

        public void Update(DashboardSummaryDto data)
        {
            if (_grid == null) return;
            _grid.Rows.Clear();
            foreach (var t in data.RecentTransactions)
                _grid.Rows.Add(t.Type, t.Number, t.Party, t.Amount, t.Date.ToString("yyyy-MM-dd"), t.Status);
        }
    }
}
