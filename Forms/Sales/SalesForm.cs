using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.App.Services.Partners;
using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;
using DzentoPOS2.Shared.Contracts.DTOs.Partners;
using DzentoPOS2.Shared.Contracts.DTOs.Sales;
using DzentoPOS2.WinForms.Configurations;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;
using EZControls.Controls.Inputs;
using FontAwesome.Sharp;

namespace DzentoPOS2.WinForms.Forms.Sales;

public partial class SalesForm : LocalizableFormBase
{
    private readonly ISaleService _saleService;
    private readonly IProductService _productService;
    private readonly ICustomerService _customerService;
    private readonly ICurrentSettingsService _currentSettings;

    private readonly Panel _leftPanel;
    private readonly Panel _rightPanel;
    private readonly EZTextBox _productSearch;
    private readonly IconButton _btnScanBarcode;
    private readonly ComboBox _cmbCustomer;
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
    private readonly IconButton _btnCompleteSale;
    private readonly IconButton _btnRemoveFromCart;
    private readonly IconButton _btnNewSale;
    private readonly IconButton _btnDeleteInvoice;
    private readonly TabControl _tabs;
    private readonly TabPage _tabNewSale;
    private readonly TabPage _tabInvoices;

    private List<ProductDto> _products = new();
    private List<CustomerDto> _customers = new();
    private List<CreateSaleItemDto> _cart = new();
    private List<SaleDto> _invoices = new();

    public SalesForm(
        ISaleService saleService,
        IProductService productService,
        ICustomerService customerService,
        ICurrentSettingsService currentSettings,
        ILanguageService languageService)
        : base(languageService)
    {
        _saleService = saleService;
        _productService = productService;
        _customerService = customerService;
        _currentSettings = currentSettings;

        Size = new Size(1200, 700);
        BackColor = AppColors.Background;

        _tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F) };
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

        _btnScanBarcode = new IconButton
        {
            IconChar = IconChar.Barcode,
            Text = "  " + AppStrings.ScanBarcode,
            IconColor = AppColors.Primary,
            ForeColor = AppColors.Primary,
            Font = AppStyle.ButtonFont,
            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 1, BorderColor = AppColors.Primary },
            BackColor = AppColors.Surface,
            Dock = DockStyle.Right,
            Width = 150,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Cursor = Cursors.Hand
        };
        _btnScanBarcode.Click += (_, _) => ScanBarcode();

        var searchBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 44,
            Padding = new Padding(0, 0, 0, 8),
            BackColor = Color.Transparent
        };
        _productSearch.Dock = DockStyle.Fill;
        _productSearch.Margin = new Padding(0, 0, 8, 0);
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
        _rightPanel = new Panel { Dock = DockStyle.Right, Width = 420, Padding = new Padding(8) };

        var lblCustomer = new Label
        {
            Text = AppStrings.CustomerName + ":",
            Location = new Point(8, 8),
            Size = new Size(100, 22),
            Font = new Font("Segoe UI", 9F)
        };

        _cmbCustomer = new ComboBox
        {
            Location = new Point(110, 6),
            Size = new Size(290, 28),
            Font = new Font("Segoe UI", 10F),
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        _cartGrid = new DataGridView
        {
            Location = new Point(8, 42),
            Size = new Size(400, 250),
            ReadOnly = true,
            MultiSelect = false
        };
        DataGridViewStyleHelper.ApplyModernStyle(_cartGrid);

        _btnRemoveFromCart = new IconButton
        {
            IconChar = IconChar.Minus,
            IconColor = AppColors.Danger,
            ForeColor = AppColors.Danger,
            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 1, BorderColor = AppColors.Danger },
            BackColor = AppColors.Surface,
            Size = new Size(36, 30),
            Location = new Point(372, 42),
            Cursor = Cursors.Hand
        };
        _btnRemoveFromCart.Click += (_, _) => RemoveFromCart();

        int y = 305;
        _lblSubtotal = MakeLabel(AppStrings.Subtotal + ":", 8, y);
        _lblSubtotalValue = MakeValueLabel("0.00", 310, y);
        y += 30;
        _lblDiscount = MakeLabel(AppStrings.Discount + " (%):", 8, y);
        _numDiscount = MakeNumeric(310, y - 2, 0, 100);
        _numDiscount.ValueChanged += (_, _) => UpdateTotals();
        y += 30;
        _lblTax = MakeLabel(AppStrings.TaxRate + " (%):", 8, y);
        _numTax = MakeNumeric(310, y - 2, 0, 100);
        _numTax.ValueChanged += (_, _) => UpdateTotals();
        y += 30;
        _lblTotal = MakeLabel(AppStrings.GrandTotal + ":", 8, y);
        _lblTotalValue = MakeValueLabel("0.00", 310, y);
        _lblTotalValue.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        _lblTotalValue.ForeColor = AppColors.Primary;
        y += 35;
        _lblPaid = MakeLabel(AppStrings.PaidAmount + ":", 8, y);
        _numPaid = MakeNumeric(310, y - 2, 0, 99999999);
        _numPaid.ValueChanged += (_, _) => UpdateTotals();
        y += 40;

        _btnCompleteSale = new IconButton
        {
            IconChar = IconChar.CheckCircle,
            Text = "  " + AppStrings.CompleteSale,
            IconColor = Color.White,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 0, MouseOverBackColor = AppColors.PrimaryHover, MouseDownBackColor = AppColors.PrimaryPressed },
            BackColor = AppColors.Primary,
            Size = new Size(400, 45),
            Location = new Point(8, y),
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Cursor = Cursors.Hand
        };
        _btnCompleteSale.Click += async (_, _) => await CompleteSaleAsync();

        _rightPanel.Controls.AddRange(new Control[]
        {
            lblCustomer, _cmbCustomer,
            _cartGrid, _btnRemoveFromCart,
            _lblSubtotal, _lblSubtotalValue,
            _lblDiscount, _numDiscount,
            _lblTax, _numTax,
            _lblTotal, _lblTotalValue,
            _lblPaid, _numPaid,
            _btnCompleteSale
        });

        _tabNewSale.Controls.Add(_rightPanel);
        _tabNewSale.Controls.Add(_leftPanel);

        // INVOICES TAB
        _invoiceGrid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, MultiSelect = false };
        DataGridViewStyleHelper.ApplyModernStyle(_invoiceGrid);

        var invoiceTopPanel = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = AppColors.Surface, Padding = new Padding(12, 8, 12, 8) };

        _btnNewSale = new IconButton
        {
            IconChar = IconChar.Plus,
            Text = "  " + AppStrings.NewSale,
            IconColor = AppColors.Primary,
            ForeColor = AppColors.Primary,
            Font = AppStyle.ButtonFont,
            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 1, BorderColor = AppColors.Primary },
            BackColor = AppColors.Surface,
            Size = new Size(130, 34),
            Location = new Point(12, 8),
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Cursor = Cursors.Hand
        };
        _btnNewSale.Click += (_, _) => _tabs.SelectedIndex = 0;

        _btnDeleteInvoice = new IconButton
        {
            IconChar = IconChar.Trash,
            Text = "  " + AppStrings.Delete,
            IconColor = AppColors.Danger,
            ForeColor = AppColors.Danger,
            Font = AppStyle.ButtonFont,
            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 1, BorderColor = AppColors.Danger },
            BackColor = AppColors.Surface,
            Size = new Size(100, 34),
            Location = new Point(154, 8),
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Cursor = Cursors.Hand
        };
        _btnDeleteInvoice.Click += async (_, _) => await DeleteInvoiceAsync();

        invoiceTopPanel.Controls.AddRange(new Control[] { _btnNewSale, _btnDeleteInvoice });

        _tabInvoices.Controls.Add(_invoiceGrid);
        _tabInvoices.Controls.Add(invoiceTopPanel);

        _tabs.TabPages.AddRange(new[] { _tabNewSale, _tabInvoices });
        Controls.Add(_tabs);
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await LoadProductsAsync();
        await LoadCustomersAsync();
        await LoadInvoicesAsync();
    }

    protected override void LoadText()
    {
        Text = AppStrings.Sales;
        _tabNewSale.Text = AppStrings.NewSale;
        _tabInvoices.Text = AppStrings.Sales;
        _productSearch.PlaceholderText = AppStrings.SearchProducts;
        _btnScanBarcode.Text = "  " + AppStrings.ScanBarcode;
        _lblSubtotal.Text = AppStrings.Subtotal + ":";
        _lblDiscount.Text = AppStrings.Discount + " (%):";
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
        _cmbCustomer.Items.Add(new { Id = Guid.Empty, FullName = AppStrings.SelectCustomer });
        foreach (var c in _customers)
            _cmbCustomer.Items.Add(c);
        _cmbCustomer.DisplayMember = "FullName";
        _cmbCustomer.ValueMember = "Id";
        _cmbCustomer.SelectedIndex = 0;
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
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.ProductLabel, DataPropertyName = "Name", Width = 200 },
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

    private void RefreshCart()
    {
        var display = _cart.Select(i =>
        {
            var product = _products.FirstOrDefault(p => p.Id == i.ProductId);
            return new
            {
                ProductName = product?.Name ?? "",
                i.Quantity,
                i.Price,
                Total = i.Quantity * i.Price
            };
        }).ToList();

        _cartGrid.DataSource = null;
        _cartGrid.Columns.Clear();
        _cartGrid.AutoGenerateColumns = false;
        _cartGrid.Columns.AddRange(new[]
        {
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.ProductLabel, DataPropertyName = "ProductName", Width = 150 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Quantity, DataPropertyName = "Quantity", Width = 60 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Price + " (" + CurrencyCode + ")", DataPropertyName = "Price", Width = 80, DefaultCellStyle = { Format = "N2" } },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Total + " (" + CurrencyCode + ")", DataPropertyName = "Total", Width = 90, DefaultCellStyle = { Format = "N2" } }
        });
        _cartGrid.DataSource = display;

        UpdateTotals();
    }

    private void UpdateTotals()
    {
        var subtotal = _cart.Sum(i => i.Quantity * i.Price);
        var discountAmount = subtotal * (decimal)_numDiscount.Value / 100;
        var afterDiscount = subtotal - discountAmount;
        var taxAmount = afterDiscount * (decimal)_numTax.Value / 100;
        var total = afterDiscount + taxAmount;

        _lblSubtotalValue.Text = FormatMoney(subtotal);
        _lblTotalValue.Text = FormatMoney(total);

        if (_numPaid.Value == 0)
            _numPaid.Value = (decimal)Math.Round(total, 2);
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

        var customerId = _cmbCustomer.SelectedValue as Guid? ?? Guid.Empty;

        try
        {
            await _saleService.CreateAsync(new CreateSaleDto
            {
                CustomerId = customerId,
                Discount = (decimal)_numDiscount.Value,
                TaxRate = (decimal)_numTax.Value,
                PaidAmount = (decimal)_numPaid.Value,
                Items = _cart.ToList()
            });

            AppMessageBox.Success(AppStrings.DataSaved);

            _cart.Clear();
            _numDiscount.Value = 0;
            _numTax.Value = 0;
            _numPaid.Value = 0;
            RefreshCart();
            await LoadInvoicesAsync();
            _tabs.SelectedIndex = 1;
        }
        catch (Exception ex)
        {
            AppMessageBox.Error(ex.Message);
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

    private static Label MakeLabel(string text, int x, int y) => new()
    {
        Text = text,
        Location = new Point(x, y),
        Size = new Size(150, 22),
        Font = new Font("Segoe UI", 9F),
        TextAlign = ContentAlignment.MiddleLeft
    };

    private static Label MakeValueLabel(string text, int x, int y) => new()
    {
        Text = text,
        Location = new Point(x, y),
        Size = new Size(90, 22),
        Font = new Font("Segoe UI", 10F, FontStyle.Bold),
        TextAlign = ContentAlignment.MiddleRight
    };

    private static NumericUpDown MakeNumeric(int x, int y, decimal min, decimal max) => new()
    {
        Location = new Point(x, y),
        Size = new Size(90, 28),
        Font = new Font("Segoe UI", 10F),
        DecimalPlaces = 2,
        Minimum = min,
        Maximum = max,
        Value = 0
    };
}
