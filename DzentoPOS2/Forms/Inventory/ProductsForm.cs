using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;
using DzentoPOS2.Shared.Helpers;
using DzentoPOS2.WinForms.Configurations;
using DzentoPOS2.WinForms.Controls;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;
using EZControls.Controls.Inputs;
using FontAwesome.Sharp;

namespace DzentoPOS2.WinForms.Forms.Inventory;

public partial class ProductsForm : LocalizableFormBase
{
    private readonly IProductService _productService;
    private readonly ICurrentSettingsService _currentSettings;

    // Top bar
    private readonly Panel _topBar;
    private readonly EZTextBox _searchBox;
    private readonly ComboBox _cmbWarehouse;
    private readonly ComboBox _cmbCategoryFilter;
    private readonly ComboBox _cmbStatusFilter;
    private readonly ThemedIconButton _btnAdd;
    private readonly Label _lblWarehouseFilter;
    private readonly Label _lblCategoryFilter;
    private readonly Label _lblStatusFilter;

    // KPI Cards
    private readonly TableLayoutPanel _kpiPanel;
    private readonly KpiCard[] _kpiCards = new KpiCard[4];

    // Main content split
    private readonly SplitContainer _splitContainer;
    private readonly DataGridView _grid;
    private readonly TabControl _detailTabs;

    // Detail tab pages
    private readonly TabPage _tabInfo;
    private readonly TabPage _tabPrices;
    private readonly TabPage _tabStock;
    private readonly Label _lblDetailTitle;

    private List<ProductDto> _products = new();
    private List<ProductCategoryDto> _categories = new();
    private List<WarehouseDto> _warehouses = new();

    /// <summary>How long the search box must be idle before it queries.</summary>
    private const int SearchDebounceMs = 300;

    private readonly System.Windows.Forms.Timer _searchDebounce;
    private string _lastLoadedTerm = string.Empty;

    private static readonly Color CardColor1 = AppColors.Primary;
    private static readonly Color CardColor2 = AppColors.Info;
    private static readonly Color CardColor3 = AppColors.Warning;
    private static readonly Color CardColor4 = AppColors.Danger;

    public ProductsForm(
        IProductService productService,
        ICurrentSettingsService currentSettings,
        ILanguageService languageService)
        : base(languageService)
    {
        _productService = productService;
        _currentSettings = currentSettings;
        Size = new Size(1400, 800);
        MinimumSize = new Size(1000, 600);
        BackColor = AppColors.Background;
        StartPosition = FormStartPosition.CenterParent;

        _topBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = AppColors.Surface,
            Padding = new Padding(20, 12, 20, 12)
        };

        _searchBox = new EZTextBox
        {
            Width = 280,
            Location = new Point(20, 10),
            BorderRadius = 12,
            BorderSize = 1,
            BorderColor = AppColors.Border,
            BorderFocusColor = AppColors.Primary,
            ForeColor = AppColors.Ink,
            PlaceholderColor = Color.FromArgb(150, 160, 166),
            Font = new Font("Segoe UI", 10F)
        };
        _searchDebounce = new System.Windows.Forms.Timer { Interval = SearchDebounceMs };
        _searchDebounce.Tick += async (_, _) =>
        {
            _searchDebounce.Stop();
            if (IsDisposedOrDisposing) return;

            // Editing back to the term already on screen must not re-query.
            var term = _searchBox.Text?.Trim() ?? string.Empty;
            if (string.Equals(term, _lastLoadedTerm, StringComparison.Ordinal)) return;

            try
            {
                await LoadGridAsync();
            }
            catch (Exception ex) when (!IsDisposedOrDisposing)
            {
                AppMessageBox.Error(ex, AppStrings.SearchProducts);
            }
        };
        Disposed += (_, _) => _searchDebounce.Dispose();

        _searchBox.TextChanged += (_, _) =>
        {
            if (IsDisposedOrDisposing) return;

            // Coalesce keystrokes into one query per pause instead of running a
            // full load (SQL query + column rebuild + rebind) on every key.
            _searchDebounce.Stop();
            _searchDebounce.Start();
        };

        _cmbWarehouse = CreateFilterCombo(320, 14, 140);
        _cmbCategoryFilter = CreateFilterCombo(480, 14, 140);
        _cmbStatusFilter = CreateFilterCombo(640, 14, 130);

        _lblWarehouseFilter = new Label { Location = new Point(320, 2), Size = new Size(60, 15), Font = new Font("Segoe UI", 8F), ForeColor = AppColors.InkMuted };
        _lblCategoryFilter = new Label { Location = new Point(480, 2), Size = new Size(60, 15), Font = new Font("Segoe UI", 8F), ForeColor = AppColors.InkMuted };
        _lblStatusFilter = new Label { Location = new Point(640, 2), Size = new Size(60, 15), Font = new Font("Segoe UI", 8F), ForeColor = AppColors.InkMuted };

        _btnAdd = ButtonFactory.ToolSolid(IconChar.Plus, AppStrings.AddProduct, ButtonTone.Primary, ButtonFactory.ButtonSize.Medium);
        _btnAdd.Size = new Size(130, 36);
        _btnAdd.Location = new Point(800, 12);
        _btnAdd.Click += (_, _) => RunSafe(AddProduct);

        _topBar.Controls.AddRange(new Control[]
        {
            new Label { Text = "??", Location = new Point(2, 18), Size = new Size(20, 20), Font = new Font("Segoe UI", 12F) },
            _searchBox,
            _lblWarehouseFilter,
            _cmbWarehouse,
            _lblCategoryFilter,
            _cmbCategoryFilter,
            _lblStatusFilter,
            _cmbStatusFilter,
            _btnAdd
        });

        _kpiPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 160,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = AppColors.Background,
            Padding = new Padding(20, 12, 20, 0)
        };
        for (int i = 0; i < 4; i++)
            _kpiPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        BuildKpiCards();

        _splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 780,
            SplitterWidth = 2,
            BackColor = AppColors.Border,
            Padding = new Padding(0)
        };
        _grid = new DataGridView { Dock = DockStyle.Fill };
        DataGridViewStyleHelper.ApplyModernStyle(_grid, withHover: true);

        _grid.SelectionChanged += (_, _) => ShowProductDetails();
        _grid.CellDoubleClick += (_, _) => RunSafe(EditProduct);

        var gridContainer = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AppColors.Surface,
            Padding = new Padding(1)
        };
        gridContainer.Paint += (s, e) =>
        {
            if (s is not Panel p) return;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var path = new System.Drawing.Drawing2D.GraphicsPath();
            var r = p.ClientRectangle;
            int radius = 10;
            path.AddArc(r.X, r.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(r.Right - radius * 2 - 1, r.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(r.Right - radius * 2 - 1, r.Bottom - radius * 2 - 1, radius * 2, radius * 2, 0, 90);
            path.AddArc(r.X, r.Bottom - radius * 2 - 1, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            using var pen = new Pen(Color.FromArgb(224, 231, 235), 1);
            e.Graphics.DrawPath(pen, path);
        };
        gridContainer.Controls.Add(_grid);

        _splitContainer.Panel1.Controls.Add(gridContainer);

        _detailTabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9.5F),
            Padding = new Point(12, 6),
            BackColor = AppColors.Surface
        };
        DzentoPOS2.WinForms.Themes.TabsStyler.Apply(_detailTabs);

        _tabInfo = new TabPage();
        _tabPrices = new TabPage();
        _tabStock = new TabPage();

        _lblDetailTitle = new Label
        {
            Dock = DockStyle.Top,
            Height = 40,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = CardColor1,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 0, 0),
            BackColor = AppColors.Surface
        };

        var infoPanel = new Panel { Dock = DockStyle.Fill, BackColor = AppColors.Surface, Padding = new Padding(12) };
        _tabInfo.Controls.Add(infoPanel);

        var pricesPanel = new Panel { Dock = DockStyle.Fill, BackColor = AppColors.Surface, Padding = new Padding(12) };
        _tabPrices.Controls.Add(pricesPanel);

        var stockPanel = new Panel { Dock = DockStyle.Fill, BackColor = AppColors.Surface, Padding = new Padding(12) };
        _tabStock.Controls.Add(stockPanel);

        _detailTabs.Controls.Add(_tabInfo);
        _detailTabs.Controls.Add(_tabPrices);
        _detailTabs.Controls.Add(_tabStock);

        var detailContainer = new Panel { Dock = DockStyle.Fill, BackColor = AppColors.Surface };
        detailContainer.Controls.Add(_detailTabs);
        detailContainer.Controls.Add(_lblDetailTitle);

        _splitContainer.Panel2.Controls.Add(detailContainer);

        Controls.Add(_splitContainer);
        Controls.Add(_kpiPanel);
        Controls.Add(_topBar);
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        try
        {
            // Navigation can dispose this page while these are still running, so
            // stop between steps instead of touching a disposed control tree.
            await LoadLookupsAsync();
            if (IsDisposedOrDisposing) return;
            await LoadStatsAsync();
            if (IsDisposedOrDisposing) return;
            await LoadGridAsync();
        }
        catch (Exception ex) when (!IsDisposedOrDisposing)
        {
            MessageBox.Show($"{AppStrings.ErrorMsg}: {ex.Message}", AppStrings.ErrorMsg, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    protected override void LoadText()
    {
        Text = AppStrings.InventoryManagement;
        _searchBox.PlaceholderText = AppStrings.SearchProducts;
        _btnAdd.Text = "  " + AppStrings.AddProduct;
        _lblWarehouseFilter.Text = AppStrings.WarehouseFilter + ":";
        _lblCategoryFilter.Text = AppStrings.CategoryFilter + ":";
        _lblStatusFilter.Text = AppStrings.StatusFilter + ":";
        _tabInfo.Text = AppStrings.BasicInformation;
        _tabPrices.Text = AppStrings.PricesTab;
        _tabStock.Text = AppStrings.StockTab;
        _lblDetailTitle.Text = AppStrings.SelectProductDetail;

        var kpiTitles = new[] { AppStrings.TotalProducts, AppStrings.ActiveProducts, AppStrings.LowStock, AppStrings.StockValue };
        for (int i = 0; i < 4 && i < _kpiCards.Length; i++)
            _kpiCards[i].CardTitle = kpiTitles[i];
    }

    private async Task LoadLookupsAsync()
    {
        _categories = await _productService.GetCategoriesAsync();
        if (IsDisposedOrDisposing) return;
        _cmbCategoryFilter.Items.Clear();
        _cmbCategoryFilter.Items.Add(AppStrings.All);
        foreach (var c in _categories)
            _cmbCategoryFilter.Items.Add(c.Name);
        _cmbCategoryFilter.SelectedIndex = 0;

        _warehouses = await _productService.GetWarehousesAsync();
        if (IsDisposedOrDisposing) return;
        _cmbWarehouse.Items.Clear();
        _cmbWarehouse.Items.Add(AppStrings.All);
        foreach (var w in _warehouses)
            _cmbWarehouse.Items.Add(w.Name);
        _cmbWarehouse.SelectedIndex = 0;

        _cmbStatusFilter.Items.AddRange(new[] { AppStrings.All, "Active", "Inactive", AppStrings.LowStock });
        _cmbStatusFilter.SelectedIndex = 0;
    }

    private async Task LoadStatsAsync()
    {
        var stats = await _productService.GetInventoryStatsAsync();
        if (IsDisposedOrDisposing) return;

        _kpiCards[0].SetValue(stats.TotalProducts, KpiValueKind.Count);
        _kpiCards[1].SetValue(stats.ActiveProducts, KpiValueKind.Count);
        _kpiCards[2].SetValue(stats.LowStockProducts, KpiValueKind.Count);
        _kpiCards[3].SetValue(stats.TotalStockValue, KpiValueKind.Money);
    }

    private void BuildKpiCards()
    {
        var icons = new[] { IconChar.BoxesStacked, IconChar.CheckCircle, IconChar.ExclamationTriangle, IconChar.SackDollar };
        var keys = new[] { "TotalProducts", "ActiveProducts", "LowStock", "StockValue" };

        for (int i = 0; i < 4; i++)
        {
            _kpiCards[i] = new KpiCard(keys[i], string.Empty, icons[i], AppColors.KpiPalette[i % AppColors.KpiPalette.Length])
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(6)
            };
            _kpiPanel.Controls.Add(_kpiCards[i], i, 0);
        }
    }


    private async Task LoadGridAsync()
    {
        if (IsDisposedOrDisposing) return;

        var term = _searchBox.Text?.Trim();
        _products = string.IsNullOrWhiteSpace(term)
            ? await _productService.GetAllAsync()
            : await _productService.SearchAsync(term);

        if (IsDisposedOrDisposing) return;

        // Remember what the grid is showing so the debounced search can skip a
        // repeat query for the same term.
        _lastLoadedTerm = term ?? string.Empty;

        var catFilter = _cmbCategoryFilter.SelectedIndex;
        if (catFilter > 0 && catFilter <= _categories.Count)
        {
            var catId = _categories[catFilter - 1].Id;
            _products = _products.Where(p => p.CategoryId == catId).ToList();
        }

        var statusFilter = _cmbStatusFilter.SelectedIndex;
        if (statusFilter == 1) _products = _products.Where(p => p.IsActive).ToList();
        else if (statusFilter == 2) _products = _products.Where(p => !p.IsActive).ToList();

        _grid.DataSource = null;
        _grid.Columns.Clear();
        _grid.AutoGenerateColumns = false;

        _grid.Columns.AddRange(new DataGridViewColumn[]
        {
            new DataGridViewTextBoxColumn { HeaderText = "#", DataPropertyName = "Id", Width = 40 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.ProductLabel, DataPropertyName = "Name", Width = 180 },
            new DataGridViewTextBoxColumn { HeaderText = "SKU", DataPropertyName = "SKU", Width = 100 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.CategoryFilter, DataPropertyName = "CategoryName", Width = 110 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Unit, DataPropertyName = "UnitName", Width = 70 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.CostPrice + " (" + CurrencyCode + ")", DataPropertyName = "CostPrice", Width = 100, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.SalePrice + " (" + CurrencyCode + ")", DataPropertyName = "SalePrice", Width = 100, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.MinimumStock, DataPropertyName = "MinimumStock", Width = 80, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } },
        });

        _grid.DataSource = _products;

        if (_grid.Columns.Count > 0 && _products.Count > 0)
            _grid.Rows[0].Selected = true;
    }

    private async void ShowProductDetails()
    {
        if (IsDisposedOrDisposing) return;

        if (_grid.CurrentRow?.DataBoundItem is not ProductDto product)
        {
            _lblDetailTitle.Text = AppStrings.SelectProductDetail;
            return;
        }

        _lblDetailTitle.Text = $"{product.Name} ({product.SKU ?? AppStrings.NoSKU})";

        var infoPanel = _tabInfo.Controls[0] as Panel;
        if (infoPanel != null)
        {
            infoPanel.Controls.Clear();
            infoPanel.Controls.Add(new Label
            {
                Text = $"{AppStrings.ProductLabel}: {product.Name}\n" +
                       $"SKU: {product.SKU ?? "—"}\n" +
                       $"{AppStrings.CategoryFilter}: {product.CategoryName}\n" +
                       $"{AppStrings.Unit}: {product.UnitName}",
                Font = new Font("Segoe UI", 10F),
                AutoSize = true,
                Location = new Point(10, 10)
            });
        }

        var pricesPanel = _tabPrices.Controls[0] as Panel;
        if (pricesPanel != null)
        {
            pricesPanel.Controls.Clear();
            pricesPanel.Controls.Add(new Label
            {
                Text = $"{AppStrings.CostPrice}: {FormatMoney(product.CostPrice)}\n" +
                       $"{AppStrings.SalePrice}: {FormatMoney(product.SalePrice)}\n" +
                       $"{AppStrings.Margin}: {FormatMoney(product.SalePrice - product.CostPrice)}\n" +
                       $"{AppStrings.ProfitPercent}: {(product.SalePrice > 0 ? ((product.SalePrice - product.CostPrice) / product.SalePrice * 100).ToString("N1") : "0")}%",
                Font = new Font("Segoe UI", 10F),
                AutoSize = true,
                Location = new Point(10, 10)
            });
        }

        var stocks = await _productService.GetProductStocksAsync(product.Id);
        if (IsDisposedOrDisposing) return;

        var stockPanel = _tabStock.Controls[0] as Panel;
        if (stockPanel != null)
        {
            stockPanel.Controls.Clear();
            var y = 10;
            stockPanel.Controls.Add(new Label
            {
                Text = AppStrings.Warehouses + ":", Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(10, y), AutoSize = true
            });
            y += 25;
            if (stocks.Count == 0)
            {
                stockPanel.Controls.Add(new Label
                {
                    Text = AppStrings.NoStockRecorded, Font = new Font("Segoe UI", 10F),
                    Location = new Point(10, y), AutoSize = true, ForeColor = Color.Gray
                });
            }
            else
            {
                foreach (var s in stocks)
                {
                    stockPanel.Controls.Add(new Label
                    {
                        Text = $"{s.WarehouseName}: {s.Quantity:N0}",
                        Font = new Font("Segoe UI", 10F),
                        Location = new Point(10, y), AutoSize = true
                    });
                    y += 22;
                }
            }

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 56,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = AppColors.Surface
            };

            var btnAdjust = ButtonFactory.Primary(AppStrings.AdjustStock);
            btnAdjust.Size = new Size(160, 44);
            btnAdjust.Margin = new Padding(10, 6, 0, 6);
            btnAdjust.Click += (_, _) =>
            {
                // The page can be navigated away from (and disposed) while this
                // toolbar is still on screen; ShowDialog(this) reads this form's
                // window handle and throws ObjectDisposedException when it is gone.
                if (IsDisposedOrDisposing) return;

                using var dialog = new StockAdjustDialog(
                    _productService,
                    product.Id,
                    stocks.Sum(s => s.Quantity));

                if (dialog.ShowDialog(this) == DialogResult.OK)
                    ShowProductDetails();
            };

            actions.Controls.Add(btnAdjust);
            stockPanel.Controls.Add(actions);
        }
    }

    private string CurrencyCode => _currentSettings.Regional.CurrencyCode;

    private string FormatMoney(decimal amount) => _currentSettings.Regional.FormatMoney(amount);

    private static ComboBox CreateFilterCombo(int x, int y, int width)
    {
        return new ComboBox
        {
            Location = new Point(x, y + 14),
            Size = new Size(width, 28),
            Font = new Font("Segoe UI", 9.5F),
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat
        };
    }

    private async Task AddProduct()
    {
        if (IsDisposedOrDisposing) return;

        using var dialog = new ProductEditDialog(_productService, _languageService);
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            if (IsDisposedOrDisposing) return;
            await LoadGridAsync();
            if (IsDisposedOrDisposing) return;
            await LoadStatsAsync();
        }
    }

    private async Task EditProduct()
    {
        if (IsDisposedOrDisposing) return;
        if (_grid.CurrentRow?.DataBoundItem is not ProductDto product) return;
        using var dialog = new ProductEditDialog(_productService, _languageService, product);
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            if (IsDisposedOrDisposing) return;
            await LoadGridAsync();
            if (IsDisposedOrDisposing) return;
            await LoadStatsAsync();
        }
    }
}
