using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;
using DzentoPOS2.Shared.Helpers;
using DzentoPOS2.WinForms.Controls;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Modules.Dashboard;
using DzentoPOS2.WinForms.Themes;
using EZControls.Controls.Inputs;
using FontAwesome.Sharp;

namespace DzentoPOS2.WinForms.Forms.Inventory;

public partial class InventoryForm : LocalizableFormBase
{
    private readonly IProductService _productService;

    private readonly TableLayoutPanel _kpiPanel;
    private readonly Panel _filterPanel;
    private readonly KpiCard[] _kpiCards = new KpiCard[4];
    private readonly EZTextBox _searchBox;
    private readonly ComboBox _cmbCategoryFilter;
    private readonly ComboBox _cmbStockFilter;
    private readonly Label _lblCategoryFilter;
    private readonly Label _lblStockFilter;
    private readonly DataGridView _grid;
    private readonly Panel _detailPanel;
    private readonly Label _lblDetailTitle;
    private readonly DataGridView _detailGrid;

    private static readonly (string Key, IconChar Icon)[] KpiMeta =
    {
        ("TotalProducts", IconChar.BoxesStacked),
        ("StockValue", IconChar.SackDollar),
        ("LowStock", IconChar.ExclamationTriangle),
        ("ActiveProducts", IconChar.CheckCircle),
    };

    private List<ProductDto> _products = new();
    private List<ProductCategoryDto> _categories = new();

    public InventoryForm(IProductService productService, ILanguageService languageService)
        : base(languageService)
    {
        _productService = productService;
        Size = new Size(1200, 700);
        MinimumSize = new Size(1000, 620);
        BackColor = AppColors.Background;

        _kpiPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 160,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = AppColors.Background,
            Padding = new Padding(16, 12, 16, 12)
        };
        for (int i = 0; i < 4; i++)
            _kpiPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

        for (int i = 0; i < 4; i++)
        {
            var (key, icon) = KpiMeta[i];
            _kpiCards[i] = new KpiCard(key, string.Empty, icon, AppColors.KpiPalette[i % AppColors.KpiPalette.Length])
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(6)
            };
            _kpiPanel.Controls.Add(_kpiCards[i], i, 0);
        }

        _filterPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 55,
            BackColor = AppColors.Surface,
            Padding = new Padding(16, 10, 16, 10)
        };

        _searchBox = new EZTextBox
        {
            Location = new Point(16, 12),
            Size = new Size(220, 30),
            Font = new Font("Segoe UI", 10F),
            PlaceholderText = AppStrings.Search
        };
        _searchBox.TextChanged += (_, _) => RunSafe(LoadDataAsync);

        _lblCategoryFilter = new Label
        {
            Location = new Point(260, 18),
            Size = new Size(60, 20),
            Font = new Font("Segoe UI", 9F),
            Text = AppStrings.CategoryFilter + ":"
        };

        _cmbCategoryFilter = new ComboBox
        {
            Location = new Point(325, 14),
            Size = new Size(180, 28),
            Font = new Font("Segoe UI", 10F),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _cmbCategoryFilter.SelectedIndexChanged += (_, _) => RunSafe(LoadDataAsync);

        _lblStockFilter = new Label
        {
            Location = new Point(525, 18),
            Size = new Size(60, 20),
            Font = new Font("Segoe UI", 9F),
            Text = AppStrings.StatusFilter + ":"
        };

        _cmbStockFilter = new ComboBox
        {
            Location = new Point(590, 14),
            Size = new Size(150, 28),
            Font = new Font("Segoe UI", 10F),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _cmbStockFilter.SelectedIndexChanged += (_, _) => RunSafe(LoadDataAsync);

        _filterPanel.Controls.AddRange(new Control[]
        {
            _searchBox, _lblCategoryFilter, _cmbCategoryFilter, _lblStockFilter, _cmbStockFilter
        });

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 380
        };

        _grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, MultiSelect = false };
        DataGridViewStyleHelper.ApplyModernStyle(_grid);
        _grid.SelectionChanged += (_, _) => RunSafe(ShowDetailsAsync);

        _detailPanel = new Panel { Dock = DockStyle.Fill, BackColor = AppColors.Surface, Padding = new Padding(12) };

        _lblDetailTitle = new Label
        {
            Dock = DockStyle.Top,
            Height = 28,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = AppColors.Primary
        };

        _detailGrid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, MultiSelect = false };
        DataGridViewStyleHelper.ApplyModernStyle(_detailGrid);

        _detailPanel.Controls.Add(_detailGrid);
        _detailPanel.Controls.Add(_lblDetailTitle);

        split.Panel1.Controls.Add(_grid);
        split.Panel2.Controls.Add(_detailPanel);

        Controls.Add(split);
        Controls.Add(_filterPanel);
        Controls.Add(_kpiPanel);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        RunSafe(InitializeAsync);
    }

    private async Task InitializeAsync()
    {
        _categories = await _productService.GetCategoriesAsync();
        if (IsDisposedOrDisposing) return;
        _cmbCategoryFilter.Items.Clear();
        _cmbCategoryFilter.Items.Add(new { Id = Guid.Empty, Name = AppStrings.All });
        foreach (var c in _categories)
            _cmbCategoryFilter.Items.Add(c);
        _cmbCategoryFilter.DisplayMember = "Name";
        _cmbCategoryFilter.ValueMember = "Id";
        _cmbCategoryFilter.SelectedIndex = 0;

        _cmbStockFilter.Items.Clear();
        _cmbStockFilter.Items.Add(new { Id = Guid.Empty, Name = AppStrings.All });
        _cmbStockFilter.Items.Add(new { Id = Guid.Empty, Name = AppStrings.Active });
        _cmbStockFilter.Items.Add(new { Id = Guid.Empty, Name = AppStrings.LowStock });
        _cmbStockFilter.DisplayMember = "Name";
        _cmbStockFilter.SelectedIndex = 0;

        await LoadStatsAsync();
        if (IsDisposedOrDisposing) return;
        await LoadDataAsync();
    }

    protected override void LoadText()
    {
        Text = AppStrings.Inventory;
        _searchBox.PlaceholderText = AppStrings.Search;
        _lblCategoryFilter.Text = AppStrings.CategoryFilter + ":";
        _lblStockFilter.Text = AppStrings.StatusFilter + ":";
        _kpiCards[0].CardTitle = AppStrings.TotalProducts;
        _kpiCards[1].CardTitle = AppStrings.StockValue;
        _kpiCards[2].CardTitle = AppStrings.LowStock;
        _kpiCards[3].CardTitle = AppStrings.ActiveProducts;
    }

    private async Task LoadStatsAsync()
    {
        var stats = await _productService.GetInventoryStatsAsync();
        if (IsDisposedOrDisposing) return;
        _kpiCards[0].SetValue(stats.TotalProducts, KpiValueKind.Count);
        _kpiCards[1].SetValue(stats.TotalStockValue, KpiValueKind.Money);
        _kpiCards[2].SetValue(stats.LowStockProducts, KpiValueKind.Count);
        _kpiCards[3].SetValue(stats.ActiveProducts, KpiValueKind.Count);
    }

    private async Task LoadDataAsync()
    {
        var term = _searchBox.Text?.Trim();
        _products = string.IsNullOrWhiteSpace(term)
            ? await _productService.GetAllAsync()
            : await _productService.SearchAsync(term);

        if (IsDisposedOrDisposing) return;

        var categoryId = GetSelectedValue(_cmbCategoryFilter);
        if (categoryId != Guid.Empty)
            _products = _products.Where(p => p.CategoryId == categoryId).ToList();

        var stockFilterIndex = _cmbStockFilter.SelectedIndex;
        if (stockFilterIndex == 1)
            _products = _products.Where(p => p.IsActive).ToList();
        else if (stockFilterIndex == 2)
            _products = _products.Where(p => p.MinimumStock > 0).ToList();

        _grid.DataSource = null;
        _grid.Columns.Clear();
        _grid.AutoGenerateColumns = false;
        _grid.Columns.AddRange(new[]
        {
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.ProductLabel, DataPropertyName = "Name", Width = 220 },
            new DataGridViewTextBoxColumn { HeaderText = "SKU", DataPropertyName = "SKU", Width = 120 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.CategoryFilter, DataPropertyName = "CategoryName", Width = 140 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Unit, DataPropertyName = "UnitName", Width = 100 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.CostPrice, DataPropertyName = "CostPrice", Width = 110, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.SalePrice, DataPropertyName = "SalePrice", Width = 110, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.MinimumStock, DataPropertyName = "MinimumStock", Width = 100, DefaultCellStyle = { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Statut, DataPropertyName = "IsActive", Width = 80 }
        });
        _grid.DataSource = _products;
    }

    private async Task ShowDetailsAsync()
    {
        if (IsDisposedOrDisposing) return;

        if (_grid.CurrentRow?.DataBoundItem is not ProductDto product)
        {
            _detailGrid.DataSource = null;
            _lblDetailTitle.Text = "";
            return;
        }

        _lblDetailTitle.Text = $"{product.Name}  ({product.SKU})";

        var stocks = await _productService.GetProductStocksAsync(product.Id);

        if (IsDisposedOrDisposing) return;

        _detailGrid.DataSource = null;
        _detailGrid.Columns.Clear();
        _detailGrid.AutoGenerateColumns = false;
        _detailGrid.Columns.AddRange(new[]
        {
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Warehouse, DataPropertyName = "WarehouseName", Width = 250 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Quantity, DataPropertyName = "Quantity", Width = 150, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } }
        });
        _detailGrid.DataSource = stocks;
    }

    private static Guid GetSelectedValue(ComboBox cmb)
    {
        if (cmb.SelectedItem == null) return Guid.Empty;
        var prop = cmb.SelectedItem.GetType().GetProperty("Id");
        return prop?.GetValue(cmb.SelectedItem) is Guid id ? id : Guid.Empty;
    }
}
