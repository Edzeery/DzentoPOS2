using DzentoPOS2.App.Services.Accounting;
using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.App.Services.Partners;
using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.Core.Services.Sales;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Color = System.Drawing.Color;
using Size = System.Drawing.Size;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;
using DzentoPOS2.Shared.Contracts.DTOs.Partners;
using DzentoPOS2.Shared.Contracts.DTOs.Sales;
using DzentoPOS2.WinForms.Configurations;
using DzentoPOS2.WinForms.Controls;
using DzentoPOS2.WinForms.Themes;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Forms.Customers;
using EZControls.Controls.Inputs;
using FontAwesome.Sharp;

namespace DzentoPOS2.WinForms.Forms.Sales;

public partial class SalesForm : LocalizableFormBase
{
    /// <summary>Top gap kept above the totals block of the basket.</summary>
    private const int TotalsPadding = 6;

    private readonly ISaleService _saleService;
    private readonly IProductService _productService;
    private readonly ICustomerService _customerService;
    private readonly ICurrentSettingsService _currentSettings;
    private readonly ICashSessionService _cashSessionService;

    private readonly Panel _leftPanel;
    private readonly Panel _rightPanel;
    private readonly TableLayoutPanel _saleLayout;
    private readonly TableLayoutPanel _cartLayout;
    private readonly TableLayoutPanel _totalsLayout;
    private readonly EZTextBox _productSearch;
    private readonly EZTextBox _invoiceSearch;
    private readonly ThemedIconButton _btnScanBarcode;
    private readonly ThemedIconButton _btnCalculator;
    private readonly ThemedIconButton _btnAddCustomer;
    private readonly ComboBox _cmbCustomer;
    private readonly Label _lblCustomer;
    private readonly Label _lblCart;
    private readonly DataGridView _productGrid;
    private readonly DataGridView _cartGrid;
    private readonly DataGridView _invoiceGrid;
    private readonly Label _lblSubtotal;
    private readonly Label _lblSubtotalValue;
    private readonly Label _lblDiscount;
    private readonly NumericUpDown _numDiscount;
    private readonly Label _lblTax;
    private readonly NumericUpDown _numTax;
    private readonly Label _lblTotal;
    private readonly Label _lblTotalValue;
    private readonly Label _lblPaid;
    private readonly NumericUpDown _numPaid;
    private readonly ThemedIconButton _btnCompleteSale;
    private readonly ThemedIconButton _btnRemoveFromCart;
    private readonly ThemedIconButton _btnPrintLast;
    private readonly ThemedIconButton _btnNewSale;
    private readonly ThemedIconButton _btnDeleteInvoice;
    private readonly TabControl _tabs;
    private readonly TabPage _tabNewSale;
    private readonly TabPage _tabInvoices;

    private List<ProductDto> _products = new();
    private List<CustomerDto> _customers = new();
    private List<CreateSaleItemDto> _cart = new();
    private List<SaleDto> _invoices = new();
    private bool _paidTouched;
    private bool _updatingTotals;
    private bool _saleStacked;
    private int _saleCartSize;
    private int _saleCartHeight;
    private bool _layoutPending;
    private SaleDto? _lastInvoice;

    public SalesForm(
        ISaleService saleService,
        IProductService productService,
        ICustomerService customerService,
        ICurrentSettingsService currentSettings,
        ICashSessionService cashSessionService,
        ILanguageService languageService)
        : base(languageService)
    {
        _saleService = saleService;
        _productService = productService;
        _customerService = customerService;
        _currentSettings = currentSettings;
        _cashSessionService = cashSessionService;

        Size = new Size(1200, 700);
        BackColor = AppColors.Background;

        _tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5F), Padding = new Point(12, 6), SizeMode = TabSizeMode.Fixed, ItemSize = new Size(140, 36) };
        DzentoPOS2.WinForms.Themes.TabsStyler.Apply(_tabs);
        _tabNewSale = new TabPage { Text = AppStrings.NewSale, BackColor = AppColors.Background };
        _tabInvoices = new TabPage { Text = AppStrings.Sales, BackColor = AppColors.Background };

        // LEFT PANEL - Product search + grid
        _leftPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };

        _productSearch = new EZTextBox
        {
            Dock = DockStyle.Top,

            Height = 36,
            Font = new Font("Segoe UI", 11F),
            PlaceholderText = AppStrings.SearchProducts
        };
        _productSearch.TextChanged += (_, _) => FilterProducts();
        _productSearch.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                AddFirstProductToCart();
            }
        };

        _btnScanBarcode = ButtonFactory.Tool(IconChar.Barcode, AppStrings.ScanBarcode, ButtonTone.Primary, ButtonFactory.ButtonSize.Medium);
        _btnScanBarcode.Size = new Size(150, 38);
        _btnScanBarcode.Dock = DockStyle.Right;
        _btnScanBarcode.Margin = new Padding(0, 0, 0, 4);
        _btnScanBarcode.Click += (_, _) => ScanBarcode();

        var searchBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 48,
            Padding = new Padding(0, 0, 0, 8),
            BackColor = Color.Transparent
        };
        _productSearch.Dock = DockStyle.Fill;
        _productSearch.Margin = new Padding(0, 0, 8, 0);
        _productSearch.Height = 38;
        searchBar.Controls.Add(_productSearch);
        searchBar.Controls.Add(_btnScanBarcode);

        _productGrid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, MultiSelect = false };
        DataGridViewStyleHelper.ApplyModernStyle(_productGrid);
        _productGrid.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex >= 0 && _productGrid.Rows[e.RowIndex].DataBoundItem is ProductDto p)
                AddToCart(p);
        };

        _leftPanel.Controls.Add(_productGrid);
        _leftPanel.Controls.Add(searchBar);

        // RIGHT PANEL - Cart + totals
        _rightPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };

        _lblCustomer = new Label
        {
            Text = AppStrings.CustomerName + ":",
            Location = new Point(8, 8),
            Size = new Size(100, 22),
            Font = new Font("Segoe UI", 9F)
        };

        _cmbCustomer = new ComboBox
        {
            Location = new Point(110, 4),
            Size = new Size(246, 28),
            Font = new Font("Segoe UI", 10F),
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        _btnAddCustomer = ButtonFactory.Tool(IconChar.UserPlus, AppStrings.AddCustomer, ButtonTone.Primary, ButtonFactory.ButtonSize.Small);
        _btnAddCustomer.Dock = DockStyle.Fill;
        _btnAddCustomer.Margin = Padding.Empty;
        _btnAddCustomer.Click += (_, _) => AddCustomer();

        var customerRow = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 34,
            ColumnCount = 3,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        customerRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));
        customerRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62F));
        customerRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38F));
        customerRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        _lblCustomer.Dock = DockStyle.Fill;
        _lblCustomer.Location = Point.Empty;
        _lblCustomer.Size = new Size(100, 22);
        _lblCustomer.TextAlign = ContentAlignment.MiddleLeft;
        _lblCustomer.Margin = Padding.Empty;

        _cmbCustomer.Dock = DockStyle.Fill;
        _cmbCustomer.Location = Point.Empty;
        _cmbCustomer.Size = new Size(246, 28);
        _cmbCustomer.Margin = Padding.Empty;

        _btnAddCustomer.Dock = DockStyle.Fill;
        _btnAddCustomer.Location = Point.Empty;
        _btnAddCustomer.Size = new Size(36, 30);
        _btnAddCustomer.Margin = Padding.Empty;

        customerRow.Controls.Add(_lblCustomer, 0, 0);
        customerRow.Controls.Add(_cmbCustomer, 1, 0);
        customerRow.Controls.Add(_btnAddCustomer, 2, 0);

        _cartGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = false,
            MultiSelect = false,
            AllowUserToAddRows = false,
            EditMode = DataGridViewEditMode.EditOnEnter
        };
        DataGridViewStyleHelper.ApplyModernStyle(_cartGrid);
        _cartGrid.CellEndEdit += (_, _) => OnCartCellEdited();
        _cartGrid.CellContentClick += (_, e) => OnCartCellContentClick(e);
        _cartGrid.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Delete)
            {
                e.Handled = true;
                RemoveFromCart();
            }
        };

        _btnRemoveFromCart = ButtonFactory.Tool(IconChar.Trash, AppStrings.Delete, ButtonTone.Neutral, ButtonFactory.ButtonSize.Small);
        _btnRemoveFromCart.Dock = DockStyle.Right;
        _btnRemoveFromCart.Size = new Size(90, 28);
        _btnRemoveFromCart.Margin = Padding.Empty;
        _btnRemoveFromCart.Click += (_, _) => ClearCart();

        // Cart header: title on the leading edge, clear button on the trailing one.
        var cartHeader = new Panel { Dock = DockStyle.Top, Height = 34, Padding = new Padding(0, 0, 0, 6) };
        _lblCart = new Label
        {
            Text = AppStrings.Cart,
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
            ForeColor = AppColors.Ink,
            TextAlign = ContentAlignment.MiddleLeft
        };
        cartHeader.Controls.Add(_lblCart);
        cartHeader.Controls.Add(_btnRemoveFromCart);

        // The grid is added first so the header keeps the top edge.
        var cartArea = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 4, 0, 0) };
        cartArea.Controls.Add(_cartGrid);
        cartArea.Controls.Add(cartHeader);

        // Totals: label column grows, value column keeps a fixed width.
        var totals = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Margin = Padding.Empty,
            Padding = new Padding(0, TotalsPadding, 0, 0)
        };
        _totalsLayout = totals;
        totals.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        totals.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
        for (int i = 0; i < 5; i++)
            totals.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));

        _lblSubtotal = MakeLabel(AppStrings.Subtotal + ":");
        _lblSubtotalValue = MakeValueLabel("0.00");
        _lblDiscount = MakeLabel(AppStrings.Discount + ":");
        _numDiscount = MakeNumeric(0, 99999999);
        _numDiscount.ValueChanged += (_, _) => UpdateTotals();
        _lblTax = MakeLabel(AppStrings.TaxRate + " (%):");
        _numTax = MakeNumeric(0, 100);
        _numTax.ValueChanged += (_, _) => UpdateTotals();
        _lblTotal = MakeLabel(AppStrings.GrandTotal + ":");
        _lblTotalValue = MakeValueLabel("0.00");
        _lblTotalValue.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        _lblTotalValue.ForeColor = AppColors.Primary;
        _lblPaid = MakeLabel(AppStrings.PaidAmount + ":");
        _numPaid = MakeNumeric(0, 99999999);
        _numPaid.ValueChanged += (_, _) => OnPaidChanged();

        totals.Controls.Add(_lblSubtotal, 0, 0);
        totals.Controls.Add(_lblSubtotalValue, 1, 0);
        totals.Controls.Add(_lblDiscount, 0, 1);
        totals.Controls.Add(_numDiscount, 1, 1);
        totals.Controls.Add(_lblTax, 0, 2);
        totals.Controls.Add(_numTax, 1, 2);
        totals.Controls.Add(_lblTotal, 0, 3);
        totals.Controls.Add(_lblTotalValue, 1, 3);
        totals.Controls.Add(_lblPaid, 0, 4);
        totals.Controls.Add(_numPaid, 1, 4);

        _btnCalculator = ButtonFactory.Tool(IconChar.Calculator, AppStrings.Calculator, ButtonTone.Primary, ButtonFactory.ButtonSize.Medium);
        _btnCalculator.Dock = DockStyle.Fill;
        _btnCalculator.Margin = new Padding(0, 0, 6, 0);
        _btnCalculator.Click += (_, _) => OpenCalculator();

        _btnCompleteSale = ButtonFactory.ToolSolid(IconChar.CheckCircle, AppStrings.CompleteSale, ButtonTone.Primary, ButtonFactory.ButtonSize.Large);
        _btnCompleteSale.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        _btnCompleteSale.Dock = DockStyle.Fill;
        _btnCompleteSale.Margin = Padding.Empty;
        _btnCompleteSale.Click += async (_, _) => await CompleteSaleAsync();

        _btnPrintLast = ButtonFactory.Tool(IconChar.Print, AppStrings.Print, ButtonTone.Neutral, ButtonFactory.ButtonSize.Medium);
        _btnPrintLast.Dock = DockStyle.Fill;
        _btnPrintLast.Margin = new Padding(0, 0, 6, 0);
        _btnPrintLast.Enabled = false;
        _btnPrintLast.Click += (_, _) => PrintLastInvoice();

        var actions = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = new Padding(0, 6, 0, 0)
        };
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
        actions.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        actions.Controls.Add(_btnCalculator, 0, 0);
        actions.Controls.Add(_btnPrintLast, 1, 0);
        actions.Controls.Add(_btnCompleteSale, 2, 0);

        // Basket layout: customer row, cart grid, totals, actions.
        _cartLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        _cartLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _cartLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        _cartLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _cartLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 156F));
        _cartLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));

        customerRow.Dock = DockStyle.Fill;
        _cartLayout.Controls.Add(customerRow, 0, 0);
        _cartLayout.Controls.Add(cartArea, 0, 1);
        _cartLayout.Controls.Add(totals, 0, 2);
        _cartLayout.Controls.Add(actions, 0, 3);
        _rightPanel.Controls.Add(_cartLayout);

        // Sale layout: products and basket side by side, stacked when narrow.
        _saleLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        _tabNewSale.Controls.Add(_saleLayout);
        ApplySaleLayout();
        _tabNewSale.Resize += OnSaleTabResized;

        // INVOICES TAB
        _invoiceGrid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, MultiSelect = false };
        DataGridViewStyleHelper.ApplyModernStyle(_invoiceGrid);

        _invoiceSearch = new EZTextBox
        {
            Dock = DockStyle.Fill,
            Height = 36,
            Font = new Font("Segoe UI", 11F),
            PlaceholderText = AppStrings.SearchInvoices
        };
        _invoiceSearch.TextChanged += (_, _) => FilterInvoices();

        _btnNewSale = ButtonFactory.Tool(IconChar.Plus, AppStrings.NewSale, ButtonTone.Primary, ButtonFactory.ButtonSize.Medium);
        _btnNewSale.Dock = DockStyle.Fill;
        _btnNewSale.Margin = new Padding(0, 0, 8, 0);
        _btnNewSale.Click += (_, _) => _tabs.SelectedIndex = 0;

        _btnDeleteInvoice = ButtonFactory.ToolDelete(AppStrings.Delete, ButtonFactory.ButtonSize.Medium);
        _btnDeleteInvoice.Dock = DockStyle.Fill;
        _btnDeleteInvoice.Margin = Padding.Empty;
        _btnDeleteInvoice.Click += async (_, _) => await DeleteInvoiceAsync();

        // Toolbar: the search field grows, the actions keep a fixed width.
        // A table layout mirrors itself in RTL, so the actions stay on the
        // trailing edge in both languages.
        var invoiceToolbar = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 58,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = AppColors.Surface,
            Padding = new Padding(12, 10, 12, 10),
            Margin = Padding.Empty
        };
        invoiceToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        invoiceToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
        invoiceToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
        invoiceToolbar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        _invoiceSearch.Margin = new Padding(0, 0, 8, 0);
        invoiceToolbar.Controls.Add(_invoiceSearch, 0, 0);
        invoiceToolbar.Controls.Add(_btnNewSale, 1, 0);
        invoiceToolbar.Controls.Add(_btnDeleteInvoice, 2, 0);

        _tabInvoices.Controls.Add(_invoiceGrid);
        _tabInvoices.Controls.Add(invoiceToolbar);

        _tabs.TabPages.AddRange(new[] { _tabNewSale, _tabInvoices });
        Controls.Add(_tabs);
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        ApplySaleLayout();
        await LoadProductsAsync();
        await LoadCustomersAsync();
        await LoadInvoicesAsync();
    }

    /// <summary>
    /// Places the product list and the basket next to each other, and stacks them
    /// when the page gets too narrow. The grid never slides under the basket
    /// panel any more, which used to hide the product search field.
    /// </summary>
    private void ApplySaleLayout()
    {
        int available = _tabNewSale.ClientSize.Width;
        if (available <= 0) return;

        bool stacked = available < 780;
        int cartSize = Math.Clamp((int)(available * 0.38), 300, 420);
        int cartHeight = stacked
            ? Math.Clamp((int)(_tabNewSale.ClientSize.Height * 0.55), 300, 380)
            : 0;

        if (stacked == _saleStacked && cartSize == _saleCartSize && cartHeight == _saleCartHeight) return;

        _saleStacked = stacked;
        _saleCartSize = cartSize;
        _saleCartHeight = cartHeight;

        ApplyCartLayout(stacked);

        _saleLayout.SuspendLayout();
        _saleLayout.Controls.Clear();
        _saleLayout.ColumnStyles.Clear();
        _saleLayout.RowStyles.Clear();

        if (stacked)
        {
            _saleLayout.ColumnCount = 1;
            _saleLayout.RowCount = 2;
            _saleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _saleLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _saleLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, cartHeight));
            _saleLayout.Controls.Add(_leftPanel, 0, 0);
            _saleLayout.Controls.Add(_rightPanel, 0, 1);
        }
        else
        {
            _saleLayout.ColumnCount = 2;
            _saleLayout.RowCount = 1;
            _saleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _saleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, cartSize));
            _saleLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _saleLayout.Controls.Add(_leftPanel, 0, 0);
            _saleLayout.Controls.Add(_rightPanel, 1, 0);
        }

        _saleLayout.ResumeLayout(true);
    }

    /// <summary>
    /// Keeps the basket usable when it is stacked under the product list: the
    /// totals rows shrink so the cart grid always keeps a visible height.
    /// </summary>
    private void ApplyCartLayout(bool compact)
    {
        int totalsBlock = compact ? 136 : 156;
        int totalsRow = (totalsBlock - TotalsPadding) / 5;
        int actionsBlock = compact ? 56 : 62;

        for (int i = 0; i < _totalsLayout.RowStyles.Count; i++)
            _totalsLayout.RowStyles[i] = new RowStyle(SizeType.Absolute, totalsRow);

        _cartLayout.RowStyles[2] = new RowStyle(SizeType.Absolute, totalsBlock);
        _cartLayout.RowStyles[3] = new RowStyle(SizeType.Absolute, actionsBlock);
    }

    /// <summary>
    /// The resize event fires before the new size is applied, so the layout is
    /// recomputed on the next message loop pass with the settled measurements.
    /// </summary>
    private void OnSaleTabResized(object? sender, EventArgs e)
    {
        if (_layoutPending) return;

        _layoutPending = true;
        BeginInvoke(new Action(() =>
        {
            _layoutPending = false;
            ApplySaleLayout();
        }));
    }

    protected override void LoadText()
    {
        Text = AppStrings.PointOfSale;
        _tabNewSale.Text = AppStrings.NewSale;
        _tabInvoices.Text = AppStrings.Sales;
        _productSearch.PlaceholderText = AppStrings.SearchProducts;
        _invoiceSearch.PlaceholderText = AppStrings.SearchInvoices;
        _btnScanBarcode.Text = "  " + AppStrings.ScanBarcode;
        _lblCustomer.Text = AppStrings.CustomerName + ":";
        _lblCart.Text = AppStrings.Cart;
        _btnAddCustomer.Text = "  " + AppStrings.AddCustomer;
        _btnAddCustomer.AccessibleDescription = AppStrings.AddCustomer;
        _btnCalculator.Text = "  " + AppStrings.Calculator;
        _lblSubtotal.Text = AppStrings.Subtotal + ":";
        _lblDiscount.Text = AppStrings.Discount + ":";
        _lblTax.Text = AppStrings.TaxRate + " (%):";
        _lblTotal.Text = AppStrings.GrandTotal + ":";
        _lblPaid.Text = AppStrings.PaidAmount + ":";
        _btnCompleteSale.Text = "  " + AppStrings.CompleteSale;
        _btnNewSale.Text = "  " + AppStrings.NewSale;
        _btnDeleteInvoice.Text = "  " + AppStrings.Delete;
    }

    private async Task LoadProductsAsync()
    {
        _products = await _productService.GetAllAsync();
        BindProductGrid(_products);
    }

    private async Task LoadCustomersAsync()
    {
        _customers = await _customerService.GetAllAsync();
        _cmbCustomer.Items.Clear();
        _cmbCustomer.Items.Add(new CustomerItem(Guid.Empty, AppStrings.SelectCustomer));
        foreach (var c in _customers)
            _cmbCustomer.Items.Add(new CustomerItem(c.Id, c.FullName));
        _cmbCustomer.DisplayMember = nameof(CustomerItem.FullName);
        _cmbCustomer.ValueMember = nameof(CustomerItem.Id);
        _cmbCustomer.SelectedIndex = 0;
    }

    /// <summary>Creates a customer from the sales screen and selects it right away.</summary>
    private void AddCustomer()
    {
        using var dialog = new CustomerEditDialog(_customerService, _languageService);

        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        var created = dialog.CreatedCustomer;
        if (created is null) return;

        _customers.Add(created);

        var item = new CustomerItem(created.Id, created.FullName);
        _cmbCustomer.Items.Add(item);
        _cmbCustomer.SelectedItem = item;

        AppMessageBox.Success(AppStrings.CustomerAdded);
    }

    private async Task LoadInvoicesAsync()
    {
        _invoices = await _saleService.GetAllAsync();
        _invoiceGrid.DataSource = null;
        _invoiceGrid.Columns.Clear();
        _invoiceGrid.AutoGenerateColumns = false;
        _invoiceGrid.Columns.AddRange(new[]
        {
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.InvoiceNumber, DataPropertyName = "InvoiceNumber", Width = 160 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.CustomerName, DataPropertyName = "CustomerName", Width = 150 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Total + " (" + CurrencyCode + ")", DataPropertyName = "Total", Width = 100, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.PaidAmount + " (" + CurrencyCode + ")", DataPropertyName = "PaidAmount", Width = 100, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.RemainingAmount + " (" + CurrencyCode + ")", DataPropertyName = "RemainingAmount", Width = 100, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Date, DataPropertyName = "CreatedAt", Width = 120, DefaultCellStyle = { Format = "yyyy/MM/dd HH:mm" } }
        });
        _invoiceGrid.DataSource = _invoices;
        FilterInvoices();
    }

    /// <summary>
    /// Filters the invoices list by invoice number, customer name or date.
    /// The filter runs on the loaded rows, so it reacts immediately.
    /// </summary>
    private void FilterInvoices()
    {
        var term = _invoiceSearch.Text?.Trim();
        var filtered = string.IsNullOrWhiteSpace(term)
            ? _invoices
            : _invoices.Where(i =>
                (i.InvoiceNumber?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
                || (i.CustomerName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
                || i.CreatedAt.ToString("yyyy/MM/dd").Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();

        _invoiceGrid.DataSource = filtered;
    }

    private void FilterProducts()
    {
        var term = _productSearch.Text?.Trim();
        var filtered = string.IsNullOrWhiteSpace(term)
            ? _products
            : _products.Where(p => p.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                || (p.SKU?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
        BindProductGrid(filtered);
    }

    private void BindProductGrid(List<ProductDto> products)
    {
        _productGrid.DataSource = null;
        _productGrid.Columns.Clear();
        _productGrid.AutoGenerateColumns = false;
        _productGrid.Columns.AddRange(new[]
        {
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.ProductLabel, DataPropertyName = "Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = 120 },
            new DataGridViewTextBoxColumn { HeaderText = "SKU", DataPropertyName = "SKU", Width = 100 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.SalePrice + " (" + CurrencyCode + ")", DataPropertyName = "SalePrice", Width = 100, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } }
        });
        _productGrid.DataSource = products;
    }

    private void ScanBarcode()
    {
        var settings = _currentSettings.Current;

        if (!settings.CameraBarcodeEnabled)
        {
            AppMessageBox.Warning(AppStrings.EnableCameraScanning);
            return;
        }

        string? scanned = null;

        using (var overlay = new BarcodeScannerOverlay(settings.BarcodeCameraDevice))
        {
            overlay.BarcodeDetected += code => scanned = code;
            overlay.ShowDialog(this);
        }

        if (string.IsNullOrWhiteSpace(scanned)) return;

        var product = _products.FirstOrDefault(p =>
            string.Equals(p.Barcode, scanned, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(p.SKU, scanned, StringComparison.OrdinalIgnoreCase));

        if (product is null)
        {
            AppMessageBox.Warning($"{AppStrings.BarcodeNotFound}: {scanned}");
            _productSearch.Text = scanned;
            return;
        }

        AddToCart(product);
    }

    private void AddFirstProductToCart()
    {
        if (_productGrid.CurrentRow?.DataBoundItem is ProductDto p)
            AddToCart(p);
        else if (_products.Count > 0)
            AddToCart(_products[0]);
    }

    private void AddToCart(ProductDto product)
    {
        if (!product.IsActive)
        {
            AppMessageBox.Warning(AppStrings.OperationFailed);
            return;
        }

        var existing = _cart.FirstOrDefault(i => i.ProductId == product.Id);
        if (existing != null)
            existing.Quantity += 1;
        else
            _cart.Add(new CreateSaleItemDto { ProductId = product.Id, Quantity = 1, Price = product.SalePrice });

        RefreshCart();
    }

    private void RemoveFromCart()
    {
        if (_cartGrid.CurrentRow?.Index is int idx && idx >= 0 && idx < _cart.Count)
        {
            _cart.RemoveAt(idx);
            RefreshCart();
        }
    }

    private void ClearCart()
    {
        if (_cart.Count == 0) return;
        var result = MessageBox.Show(AppStrings.DeleteSaleConfirm, AppStrings.DeleteConfirmation, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result == DialogResult.Yes)
        {
            _cart.Clear();
            RefreshCart();
        }
    }

    private void RefreshCart()
    {
        var display = _cart.Select(i =>
        {
            var product = _products.FirstOrDefault(p => p.Id == i.ProductId);
            return new
            {
                ProductName = product?.Name ?? (string.IsNullOrWhiteSpace(i.Description) ? "-" : i.Description),
                i.Quantity,
                i.Price,
                Total = i.Quantity * i.Price
            };
        }).ToList();

        _cartGrid.DataSource = null;
        _cartGrid.Columns.Clear();
        _cartGrid.AutoGenerateColumns = false;
        _cartGrid.Columns.AddRange(new DataGridViewColumn[]
        {
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.ProductLabel, DataPropertyName = "ProductName", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = 90, ReadOnly = true },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Quantity, DataPropertyName = "Quantity", Width = 70 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Price + " (" + CurrencyCode + ")", DataPropertyName = "Price", Width = 80, DefaultCellStyle = { Format = "N2" } },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Total + " (" + CurrencyCode + ")", DataPropertyName = "Total", Width = 90, DefaultCellStyle = { Format = "N2" }, ReadOnly = true },
            new DataGridViewButtonColumn { HeaderText = AppStrings.Delete, Text = "X", UseColumnTextForButtonValue = true, Width = 40 }
        });
        _cartGrid.DataSource = display;

        UpdateTotals();
    }

    private void OnCartCellEdited()
    {
        if (_cartGrid.CurrentRow?.Index is int idx && idx >= 0 && idx < _cart.Count)
        {
            try
            {
                var qtyVal = _cartGrid.Rows[idx].Cells["Quantity"].Value;
                if (qtyVal is decimal qty)
                {
                    if (qty <= 0) qty = 1;
                    _cart[idx].Quantity = qty;
                }
                else if (qtyVal != null && decimal.TryParse(qtyVal.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var qv) && qv > 0)
                    _cart[idx].Quantity = qv;
                else if (qtyVal != null && decimal.TryParse(qtyVal.ToString(), out var qv2) && qv2 > 0)
                    _cart[idx].Quantity = qv2;

                var priceVal = _cartGrid.Rows[idx].Cells["Price"].Value;
                if (priceVal is decimal pr)
                {
                    if (pr < 0) pr = 0;
                    _cart[idx].Price = pr;
                }
                else if (priceVal != null && decimal.TryParse(priceVal.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var pv) && pv >= 0)
                    _cart[idx].Price = pv;
                else if (priceVal != null && decimal.TryParse(priceVal.ToString(), out var pv2) && pv2 >= 0)
                    _cart[idx].Price = pv2;

                RefreshCart();
            }
            catch
            {
                RefreshCart();
            }
        }
    }

    private void OnCartCellContentClick(DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
        if (_cartGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.ColumnIndex == _cartGrid.Columns.Count - 1)
        {
            if (e.RowIndex >= 0 && e.RowIndex < _cart.Count)
            {
                _cart.RemoveAt(e.RowIndex);
                RefreshCart();
            }
        }
    }

    private void UpdateTotals()
    {
        var digits = _currentSettings.Regional.CurrencyDecimalDigits;
        var subtotal = 0m;
        var total = 0m;

        if (_cart.Count > 0)
        {
            var totals = InvoiceCalculator.Calculate(
                _cart.Select(i => new InvoiceLine(i.Quantity, i.Price)).ToList(),
                (decimal)_numDiscount.Value,
                (decimal)_numTax.Value,
                (decimal)_numPaid.Value,
                digits);

            subtotal = totals.Subtotal;
            total = totals.Total;
        }

        _lblSubtotalValue.Text = FormatMoney(subtotal);
        _lblTotalValue.Text = FormatMoney(total);

        // the paid amount follows the total until the cashier types his own value
        if (!_paidTouched)
        {
            _updatingTotals = true;
            try
            {
                _numPaid.Value = decimal.Round(
                    total,
                    digits,
                    MidpointRounding.AwayFromZero);
            }
            finally
            {
                _updatingTotals = false;
            }
        }
    }

    private void OnPaidChanged()
    {
        if (_updatingTotals) return;

        _paidTouched = true;
        UpdateTotals();
    }

    /// <summary>Opens the calculator and routes its result to the sales screen.</summary>
    private void OpenCalculator()
    {
        var cartTotal = _cart.Sum(i => i.Quantity * i.Price);

        using var dialog = new SalesCalculatorDialog(
            _languageService,
            _products,
            cartTotal,
            _currentSettings.Regional.CurrencySymbol);

        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        var outcome = dialog.Outcome;
        if (outcome is null) return;

        ApplyCalculatorOutcome(outcome);
    }

    private void ApplyCalculatorOutcome(CalculatorOutcome outcome)
    {
        switch (outcome.Target)
        {
            case CalculatorTarget.ProductLine:
                if (outcome.ProductId is Guid productId)
                {
                    var product = _products.FirstOrDefault(p => p.Id == productId);
                    if (product is not null)
                    {
                        decimal price = outcome.UnitPrice > 0 ? outcome.UnitPrice : product.SalePrice;
                        decimal quantity = price > 0 ? Math.Round(outcome.Value / price, 3) : 1m;
                        if (quantity <= 0) quantity = 1m;

                        var existing = _cart.FirstOrDefault(i => i.ProductId == productId);
                        if (existing is not null)
                        {
                            existing.Quantity += quantity;
                            existing.Price = price;
                        }
                        else
                        {
                            _cart.Add(new CreateSaleItemDto
                            {
                                ProductId = productId,
                                Quantity = quantity,
                                Price = price
                            });
                        }
                    }
                }
                break;

            case CalculatorTarget.CustomLine:
                _cart.Add(new CreateSaleItemDto
                {
                    ProductId = Guid.Empty,
                    Quantity = 1,
                    Price = outcome.Value,
                    Description = outcome.Description
                });
                break;

            case CalculatorTarget.SetPaid:
                _numPaid.Value = Math.Min(outcome.Value, _numPaid.Maximum);
                _paidTouched = true;
                break;

            case CalculatorTarget.SetDiscount:
                _numDiscount.Value = outcome.Value < 0 ? 0 : Math.Min(outcome.Value, _numDiscount.Maximum);
                break;

            case CalculatorTarget.SetTax:
                _numTax.Value = Math.Clamp(outcome.Value, _numTax.Minimum, _numTax.Maximum);
                break;
        }

        RefreshCart();
    }

    private string CurrencyCode => _currentSettings.Regional.CurrencyCode;

    private string FormatMoney(decimal amount) => _currentSettings.Regional.FormatMoney(amount);

    private async Task CompleteSaleAsync()
    {
        if (_cart.Count == 0)
        {
            AppMessageBox.Warning(AppStrings.EmptyCart);
            return;
        }

        // The combo is filled by hand, so SelectedValue is not bound to the
        // item (it stays null even with a selected entry). The identifier is
        // therefore read from the selected item itself.
        var customerId = _cmbCustomer.SelectedItem is CustomerItem selectedCustomer
            ? selectedCustomer.Id
            : Guid.Empty;

        try
        {
            if (customerId == Guid.Empty)
            {
                AppMessageBox.Warning(AppStrings.SelectCustomer);
                return;
            }

            var openSession = await _cashSessionService.GetOpenSessionAsync();
            if (openSession == null)
            {
                AppMessageBox.Warning(AppStrings.NoOpenSession);
                return;
            }

            var sale = await _saleService.CreateAsync(new CreateSaleDto
            {
                CustomerId = customerId,
                Discount = (decimal)_numDiscount.Value,
                TaxRate = (decimal)_numTax.Value,
                PaidAmount = (decimal)_numPaid.Value,
                Items = _cart.Select(c => new CreateSaleItemDto
                {
                    ProductId = c.ProductId,
                    Quantity = c.Quantity,
                    Price = c.Price,
                    Description = c.Description
                }).ToList()
            });

            _lastInvoice = sale;
            if (_btnPrintLast != null)
                _btnPrintLast.Enabled = true;

            AppMessageBox.Success(AppStrings.DataSaved);

            _cart.Clear();
            _numDiscount.Value = 0;
            _numTax.Value = 0;
            _numPaid.Value = 0;
            _paidTouched = false;
            RefreshCart();
            await LoadInvoicesAsync();
            _tabs.SelectedIndex = 1;
        }
        catch (Exception ex)
        {
            AppMessageBox.Error(ex, "CompleteSale");
        }
    }

    private async Task DeleteInvoiceAsync()
    {
        if (_invoiceGrid.CurrentRow?.DataBoundItem is not SaleDto invoice) return;
        var result = MessageBox.Show(
            $"{AppStrings.DeleteSaleConfirm}\n{invoice.InvoiceNumber}",
            AppStrings.DeleteConfirmation,
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (result == DialogResult.Yes)
        {
            await _saleService.DeleteAsync(invoice.Id);
            await LoadInvoicesAsync();
        }
    }

    private void PrintLastInvoice()
    {
        if (_lastInvoice == null) return;
        try
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var path = Path.Combine(Path.GetTempPath(), $"Invoice_{_lastInvoice.InvoiceNumber}_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));
                    page.Header().Text($"Invoice { _lastInvoice.InvoiceNumber }").SemiBold().FontSize(16);
                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Date: {_lastInvoice.CreatedAt:yyyy-MM-dd HH:mm}");
                        col.Item().Text($"Total: {_lastInvoice.Total:N2} {CurrencyCode}");
                        col.Item().Text($"Paid: {_lastInvoice.PaidAmount:N2} {CurrencyCode}");
                        col.Item().Text($"Remaining: {_lastInvoice.RemainingAmount:N2} {CurrencyCode}");
                    });
                    page.Footer().AlignCenter().Text("DzentoPOS2");
                });
            }).GeneratePdf(path);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = path, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            AppMessageBox.Error(ex, "PrintLastInvoice");
        }
    }

    private static Label MakeLabel(string text) => new()
    {
        Text = text,
        Dock = DockStyle.Fill,
        Margin = Padding.Empty,
        Font = new Font("Segoe UI", 9F),
        TextAlign = ContentAlignment.MiddleLeft
    };

    private static Label MakeValueLabel(string text) => new()
    {
        Text = text,
        Dock = DockStyle.Fill,
        Margin = Padding.Empty,
        Font = new Font("Segoe UI", 10F, FontStyle.Bold),
        TextAlign = ContentAlignment.MiddleRight
    };

    private static NumericUpDown MakeNumeric(decimal min, decimal max) => new()
    {
        Dock = DockStyle.Fill,
        Margin = Padding.Empty,
        Font = new Font("Segoe UI", 10F),
        DecimalPlaces = 2,
        Minimum = min,
        Maximum = max,
        Value = 0
    };

    /// <summary>Uniform customer entry so the combo always exposes a Guid value.</summary>
    private sealed record CustomerItem(Guid Id, string FullName);
}
