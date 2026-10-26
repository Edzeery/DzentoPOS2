using DzentoPOS2.App.Services.Reports.Interfaces;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Reports;
using DzentoPOS2.Shared.Helpers;
using DzentoPOS2.WinForms.Controls;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Modules.Dashboard;
using DzentoPOS2.WinForms.Themes;
using FontAwesome.Sharp;

namespace DzentoPOS2.WinForms.Forms.Reports;

public partial class ReportsForm : LocalizableFormBase
{
    private readonly IDashboardService _dashboardService;

    private readonly TableLayoutPanel _kpiPanel;
    private readonly KpiCard[] _kpiCards = new KpiCard[8];
    private readonly TabControl _tabs;
    private readonly TabPage _tabBestSelling;
    private readonly TabPage _tabCustomers;
    private readonly TabPage _tabSuppliers;
    private readonly TabPage _tabStockAlerts;
    private readonly DataGridView _gridBestSelling;
    private readonly DataGridView _gridCustomers;
    private readonly DataGridView _gridSuppliers;
    private readonly DataGridView _gridStockAlerts;

    private static readonly (string Key, IconChar Icon)[] KpiMeta =
    {
        ("SalesToday", IconChar.ShoppingCart),
        ("PurchasesToday", IconChar.Truck),
        ("MonthlySales", IconChar.ChartLine),
        ("MonthlyPurchases", IconChar.Receipt),
        ("ExpectedProfit", IconChar.ChartPie),
        ("ActualProfit", IconChar.MoneyBillWave),
        ("SalesCount", IconChar.FileInvoiceDollar),
        ("PurchasesCount", IconChar.ClipboardList),
    };

    public ReportsForm(IDashboardService dashboardService, ILanguageService languageService)
        : base(languageService)
    {
        _dashboardService = dashboardService;
        Size = new Size(1200, 720);
        BackColor = AppColors.Background;

        _kpiPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 288,
            ColumnCount = 4,
            RowCount = 2,
            BackColor = AppColors.Background,
            Padding = new Padding(16, 12, 16, 12)
        };
        for (int i = 0; i < 4; i++)
            _kpiPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        for (int i = 0; i < 2; i++)
            _kpiPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 132F));

        for (int i = 0; i < 8; i++)
        {
            var (key, icon) = KpiMeta[i];
            _kpiCards[i] = new KpiCard(key, string.Empty, icon, AppColors.KpiPalette[i % AppColors.KpiPalette.Length])
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(6)
            };
            _kpiPanel.Controls.Add(_kpiCards[i], i % 4, i / 4);
        }

        _tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F) };
        DzentoPOS2.WinForms.Themes.TabsStyler.Apply(_tabs);

        _tabBestSelling = new TabPage();
        _tabCustomers = new TabPage();
        _tabSuppliers = new TabPage();
        _tabStockAlerts = new TabPage();

        _gridBestSelling = CreateGrid(out _gridCustomers, out _gridSuppliers, out _gridStockAlerts);

        _tabBestSelling.Controls.Add(_gridBestSelling);
        _tabCustomers.Controls.Add(_gridCustomers);
        _tabSuppliers.Controls.Add(_gridSuppliers);
        _tabStockAlerts.Controls.Add(_gridStockAlerts);

        _tabs.TabPages.AddRange(new[] { _tabBestSelling, _tabCustomers, _tabSuppliers, _tabStockAlerts });

        Controls.Add(_tabs);
        Controls.Add(_kpiPanel);
    }

    private static DataGridView CreateGrid(out DataGridView customers, out DataGridView suppliers, out DataGridView alerts)
    {
        customers = CreateSingleGrid();
        suppliers = CreateSingleGrid();
        alerts = CreateSingleGrid();
        return CreateSingleGrid();
    }

    private static DataGridView CreateSingleGrid()
    {
        var grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, MultiSelect = false };
        DataGridViewStyleHelper.ApplyModernStyle(grid);
        return grid;
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        try
        {
            var summary = await _dashboardService.GetSummaryAsync();
            ApplySummary(summary);
        }
        catch (Exception ex)
        {
            AppMessageBox.Error(ex.Message);
        }
    }

    protected override void LoadText()
    {
        Text = AppStrings.Reports;
        _kpiCards[0].CardTitle = AppStrings.TodaySales;
        _kpiCards[1].CardTitle = AppStrings.TodayPurchases;
        _kpiCards[2].CardTitle = AppStrings.MonthlySales;
        _kpiCards[3].CardTitle = AppStrings.MonthlyPurchases;
        _kpiCards[4].CardTitle = AppStrings.ExpectedProfit;
        _kpiCards[5].CardTitle = AppStrings.ActualProfit;
        _kpiCards[6].CardTitle = AppStrings.SalesInvoicesCount;
        _kpiCards[7].CardTitle = AppStrings.PurchaseInvoicesCount;

        _tabBestSelling.Text = AppStrings.TopSellingProducts;
        _tabCustomers.Text = AppStrings.BestCustomers;
        _tabSuppliers.Text = AppStrings.MostActiveSuppliers;
        _tabStockAlerts.Text = AppStrings.StockAlerts;

        SetupInsightGrid(_gridBestSelling, AppStrings.ProductLabel);
        SetupInsightGrid(_gridCustomers, AppStrings.CustomerLabel);
        SetupInsightGrid(_gridSuppliers, AppStrings.SupplierLabel);
        SetupInsightGrid(_gridStockAlerts, AppStrings.ProductLabel);
    }

    private void ApplySummary(DashboardSummaryDto s)
    {
        _kpiCards[0].SetValue(s.SalesToday.Value, s.SalesToday.Kind, s.SalesToday.TrendKind, s.SalesToday.TrendPercent);
        _kpiCards[1].SetValue(s.PurchasesToday.Value, s.PurchasesToday.Kind, s.PurchasesToday.TrendKind, s.PurchasesToday.TrendPercent);
        _kpiCards[2].SetValue(s.MonthlySales.Value, s.MonthlySales.Kind, s.MonthlySales.TrendKind, s.MonthlySales.TrendPercent);
        _kpiCards[3].SetValue(s.MonthlyPurchases.Value, s.MonthlyPurchases.Kind, s.MonthlyPurchases.TrendKind, s.MonthlyPurchases.TrendPercent);
        _kpiCards[4].SetValue(s.ExpectedProfit.Value, s.ExpectedProfit.Kind, s.ExpectedProfit.TrendKind, s.ExpectedProfit.TrendPercent);
        _kpiCards[5].SetValue(s.ActualProfit.Value, s.ActualProfit.Kind, s.ActualProfit.TrendKind, s.ActualProfit.TrendPercent);
        _kpiCards[6].SetValue(s.SalesCount.Value, s.SalesCount.Kind, s.SalesCount.TrendKind, s.SalesCount.TrendPercent);
        _kpiCards[7].SetValue(s.PurchasesCount.Value, s.PurchasesCount.Kind, s.PurchasesCount.TrendKind, s.PurchasesCount.TrendPercent);

        _gridBestSelling.DataSource = s.BestSellingProducts;
        _gridCustomers.DataSource = s.TopCustomers;
        _gridSuppliers.DataSource = s.TopSuppliers;
        _gridStockAlerts.DataSource = s.StockAlerts;
    }

    private void SetupInsightGrid(DataGridView grid, string nameHeader)
    {
        grid.Columns.Clear();
        grid.AutoGenerateColumns = false;
        grid.Columns.AddRange(new[]
        {
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Rank, DataPropertyName = "Rank", Width = 70 },
            new DataGridViewTextBoxColumn { HeaderText = nameHeader, DataPropertyName = "Name", Width = 350 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Amount, DataPropertyName = "DisplayValue", Width = 150, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } }
        });
    }
}
