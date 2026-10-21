using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Shared.Contracts.DTOs.Reports;
using DzentoPOS2.WinForms.Themes;
using System.Windows.Forms.DataVisualization.Charting;

namespace DzentoPOS2.WinForms.Modules.Dashboard
{
    internal class ChartsModule
    {
        private readonly RecentTransactionsModule _recentTransactions;
        private Chart _lineChart = null!;
        private Chart _columnChart = null!;
        private Chart _pieChart = null!;

        private static Color TrendSeriesColor => AppColors.KpiPalette[3];
        private static Color SalesSeriesColor => AppColors.KpiPalette[1];
        private static Color PurchasesSeriesColor => AppColors.KpiPalette[4];

        public ChartsModule(RecentTransactionsModule recentTransactions)
        {
            _recentTransactions = recentTransactions;
            ThemeManager.ThemeChanged += OnThemeChanged;
        }

        private void OnThemeChanged(object? sender, EventArgs e)
        {
            if (_lineChart == null) return;
            if (_lineChart.InvokeRequired) { _lineChart.BeginInvoke(ApplyChartsTheme); return; }
            ApplyChartsTheme();
        }

        public void Dispose()
        {
            ThemeManager.ThemeChanged -= OnThemeChanged;
        }

        public Panel Build()
        {
            var panel = new Panel { Dock = DockStyle.Top, Height = 520, BackColor = Color.Transparent };

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2,
                BackColor = Color.Transparent
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            _lineChart = CreateLineChart();
            _columnChart = CreateColumnChart();
            _pieChart = CreatePieChart();

            grid.Controls.Add(UIHelper.WrapInCard(_lineChart, AppStrings.SalesTrend), 0, 0);
            grid.Controls.Add(UIHelper.WrapInCard(_columnChart, AppStrings.MonthlyComparison), 1, 0);
            grid.Controls.Add(UIHelper.WrapInCard(_pieChart, AppStrings.CategoryDistribution), 0, 1);

            var recentGrid = _recentTransactions.Build();
            grid.Controls.Add(UIHelper.WrapInCard(recentGrid, AppStrings.LastTransactions, 42, 16, 58), 1, 1);
            _recentTransactions.SetGrid(recentGrid);

            panel.Controls.Add(grid);
            return panel;
        }

        private Chart CreateLineChart()
        {
            var chart = new Chart();
            chart.ChartAreas.Add(new ChartArea());
            var area = chart.ChartAreas[0];
            area.BackColor = AppColors.Surface;
            area.AxisX.MajorGrid.LineColor = AppColors.GridLine;
            area.AxisY.MajorGrid.LineColor = AppColors.GridLine;
            area.AxisX.LabelStyle.Font = new Font("Segoe UI", 8F);
            area.AxisY.LabelStyle.Font = new Font("Segoe UI", 8F);
            area.AxisX.LabelStyle.ForeColor = AppColors.InkMuted;
            area.AxisY.LabelStyle.ForeColor = AppColors.InkMuted;

            var series = new Series
            {
                Name = AppStrings.SalesChart,
                ChartType = SeriesChartType.SplineArea,
                Color = Color.FromArgb(80, TrendSeriesColor),
                BorderColor = TrendSeriesColor,
                BorderWidth = 3,
                LabelForeColor = AppColors.Ink
            };
            chart.Series.Add(series);

            chart.Legends.Add(new Legend
            {
                Docking = Docking.Top, Font = new Font("Segoe UI", 9F), BackColor = AppColors.Surface, ForeColor = AppColors.Ink
            });
            return chart;
        }

        private void ApplyChartsTheme()
        {
            try
            {
                ApplyChartTheme(_lineChart);
                ApplyChartTheme(_columnChart);
                ApplyChartTheme(_pieChart);
            }
            catch { }
        }

        private static void ApplyChartTheme(Chart chart)
        {
            if (chart == null || chart.IsDisposed) return;
            chart.BackColor = AppColors.Surface;
            foreach (ChartArea area in chart.ChartAreas)
            {
                area.BackColor = AppColors.Surface;
                area.AxisX.LineColor = AppColors.Border;
                area.AxisY.LineColor = AppColors.Border;
                area.AxisX.MajorGrid.LineColor = AppColors.GridLine;
                area.AxisY.MajorGrid.LineColor = AppColors.GridLine;
                area.AxisX.LabelStyle.ForeColor = AppColors.InkMuted;
                area.AxisY.LabelStyle.ForeColor = AppColors.InkMuted;
                area.AxisX.TitleForeColor = AppColors.Ink;
                area.AxisY.TitleForeColor = AppColors.Ink;
            }
            foreach (Legend legend in chart.Legends)
            {
                legend.BackColor = AppColors.Surface;
                legend.ForeColor = AppColors.Ink;
            }
            foreach (Series series in chart.Series)
            {
                series.LabelForeColor = AppColors.Ink;
            }

            if (chart.Series.Count > 0 && chart.Series[0].ChartType == SeriesChartType.SplineArea)
            {
                chart.Series[0].Color = Color.FromArgb(80, TrendSeriesColor);
                chart.Series[0].BorderColor = TrendSeriesColor;
            }
            else if (chart.Series.Count > 1 && chart.Series[0].ChartType == SeriesChartType.Column)
            {
                chart.Series[0].Color = SalesSeriesColor;
                chart.Series[1].Color = PurchasesSeriesColor;
            }
        }

        private static Chart CreateColumnChart()
        {
            var chart = new Chart();
            chart.ChartAreas.Add(new ChartArea());
            var area = chart.ChartAreas[0];
            area.BackColor = AppColors.Surface;
            area.AxisX.MajorGrid.LineColor = AppColors.GridLine;
            area.AxisY.MajorGrid.LineColor = AppColors.GridLine;
            area.AxisX.LabelStyle.Font = new Font("Segoe UI", 8F);
            area.AxisY.LabelStyle.Font = new Font("Segoe UI", 8F);

            chart.Series.Add(new Series { Name = AppStrings.SalesChart, ChartType = SeriesChartType.Column, Color = SalesSeriesColor, BorderWidth = 2 });
            chart.Series.Add(new Series { Name = AppStrings.PurchasesChart, ChartType = SeriesChartType.Column, Color = PurchasesSeriesColor, BorderWidth = 2 });

            chart.Legends.Add(new Legend
            {
                Docking = Docking.Top, Font = new Font("Segoe UI", 9F), BackColor = AppColors.Surface, ForeColor = AppColors.Ink
            });
            return chart;
        }

        private static Chart CreatePieChart()
        {
            var chart = new Chart();
            chart.ChartAreas.Add(new ChartArea());
            var area = chart.ChartAreas[0];
            area.BackColor = AppColors.Surface;
            area.Area3DStyle.Enable3D = true;
            area.Area3DStyle.Inclination = 30;

            var series = new Series
            {
                Name = AppStrings.CategoriesChart,
                ChartType = SeriesChartType.Pie,
                IsValueShownAsLabel = true,
                LabelFormat = "{0}",
                Font = new Font("Segoe UI", 9F),
                LabelForeColor = AppColors.Ink
            };
            series["PieLabelStyle"] = "Outside";
            series["PieLineColor"] = "White";
            series["PieDrawingStyle"] = "SoftEdge";
            chart.Series.Add(series);

            chart.Legends.Add(new Legend
            {
                Docking = Docking.Bottom, Font = new Font("Segoe UI", 9F), BackColor = AppColors.Surface, ForeColor = AppColors.Ink
            });
            return chart;
        }

        public void Update(DashboardSummaryDto data)
        {
            UpdateLineChart(data.SalesTrend);
            UpdateColumnChart(data.MonthlyComparison);
            UpdatePieChart(data.CategoryDistribution);
        }

        private void UpdateLineChart(List<LineChartPoint> points)
        {
            _lineChart.Series[0].Points.Clear();
            foreach (var pt in points)
                _lineChart.Series[0].Points.AddXY(pt.Label, pt.Value);
        }

        private void UpdateColumnChart(List<ComparisonPoint> points)
        {
            _columnChart.Series[0].Points.Clear();
            _columnChart.Series[1].Points.Clear();
            foreach (var pt in points)
            {
                _columnChart.Series[0].Points.AddXY(pt.Label, pt.Sales);
                _columnChart.Series[1].Points.AddXY(pt.Label, pt.Purchases);
            }
        }

        private void UpdatePieChart(List<PieSliceData> slices)
        {
            _pieChart.Series[0].Points.Clear();
            var pieColors = AppColors.KpiPalette;
            int ci = 0;
            foreach (var slice in slices)
            {
                var dp = new DataPoint();
                dp.SetValueXY(slice.Category, slice.Value);
                if (ci < pieColors.Length)
                    dp.Color = pieColors[ci++];
                dp.Label = $"{slice.Category}\n{slice.Value:N0}";
                dp.ToolTip = $"{slice.Category}: {slice.Value:N2}";
                _pieChart.Series[0].Points.Add(dp);
            }

            if (slices.Count == 0)
            {
                _pieChart.Series[0].Points.Add(new DataPoint { Color = AppColors.Border });
                _pieChart.Series[0].Points[0].SetValueXY(AppStrings.NoDataAvailable, 1);
            }
        }
    }
}
