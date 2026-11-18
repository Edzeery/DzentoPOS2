using DzentoPOS2.App.Services.Accounting;
using DzentoPOS2.App.Authorization;
using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.Core.Constants;
using DzentoPOS2.App.Services.Partners;
using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.Core.Services.Sales;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Color = System.Drawing.Color;
using Size = System.Drawing.Size;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Accounting;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;
using DzentoPOS2.Shared.Contracts.DTOs.Partners;
using DzentoPOS2.Shared.Contracts.DTOs.Sales;
using DzentoPOS2.WinForms.Configurations;
using DzentoPOS2.WinForms.Controls;
using DzentoPOS2.WinForms.Controls.Pos;
using DzentoPOS2.WinForms.Themes;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Forms.Accounting;
using DzentoPOS2.WinForms.Forms.Customers;
using EZControls.Controls.Inputs;
using FontAwesome.Sharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DzentoPOS2.WinForms.Forms.Sales;

/// <summary>
/// Point of sale screen. The layout lives in the designer file; this file owns
/// the data flow: catalog, cart, totals, payment and printing.
/// </summary>
public partial class SalesForm : LocalizableFormBase, IBranchAware
{
    // The design-time constructor leaves these unset on purpose; the runtime
    // constructor (the one DI uses) always assigns them before InitializeComponent.
    private readonly ISaleService _saleService = null!;
    private readonly IProductService _productService = null!;
    private readonly IPosProductStore _posProductStore = null!;
    private readonly IPosHoldService _holdService = null!;
    private readonly ICustomerService _customerService = null!;
    private readonly ICurrentSettingsService _currentSettings = null!;
    private readonly ICashSessionService _cashSessionService = null!;
    private readonly IBarcodeService _barcodeService = null!;
    private readonly IPosBarcodeResolver _barcodeResolver = null!;
    private readonly ICatalogChangeNotifier _catalogNotifier = null!;
    private readonly IBranchSwitchGuard _branchGuard = null!;
    private readonly IBranchSwitchGuardRegistry _branchGuardRegistry = null!;
    private readonly IServiceProvider _services = null!;

    private List<CustomerDto> _customers = new();
    private List<CreateSaleItemDto> _cart = new();
    private List<SaleDto> _invoices = new();

    /// <summary>Full product rows, loaded only when the calculator asks for them.</summary>
    private List<ProductDto> _products = new();

    private readonly Dictionary<Guid, ProductDto> _productCache = new();
    private readonly Dictionary<Guid, PosProductItem> _catalogByVariant = new();
    private readonly PosCartRowTracker _cartRowTracker = new();
    private readonly List<CartLineRow> _cartRowViews = new();

    private IReadOnlyList<PosProductItem> _catalog = Array.Empty<PosProductItem>();
    private IReadOnlyList<PosProductItem> _filteredItems = Array.Empty<PosProductItem>();
    private Guid? _selectedCategoryId;
    private bool _sessionOpen;

    private PaidMode _paidMode = PaidMode.FullPayment;
    private bool _updatingTotals;
    private bool _canOverridePrice;
    private bool _saleStacked;
    private bool _saleCompact;
    private int _saleCartSize;
    private int _saleCartHeight;

    /// <summary>Card sales settle the exact total; cash sales keep the typed paid amount.</summary>
    private PosPaymentMethod _paymentMethod = PosPaymentMethod.Cash;

    private bool _layoutPending;
    private bool _resizingCartRows;
    private SaleDto? _lastInvoice;
    private bool _catalogReloadPending;
    private int _productFilterVersion;
    private bool _themeSubscribed;

    /// <summary>
    /// Design-time constructor. It only builds the layout and never touches
    /// services, the database or the file system, so Visual Studio can open the
    /// form on its design surface.
    /// </summary>
    public SalesForm()
    {
        InitializeComponent();
    }

    [ActivatorUtilitiesConstructor]
    public SalesForm(
        ISaleService saleService,
        IProductService productService,
        IPosProductStore posProductStore,
        IPosHoldService holdService,
        ICustomerService customerService,
        ICurrentSettingsService currentSettings,
        ICashSessionService cashSessionService,
        IBarcodeService barcodeService,
        IPosBarcodeResolver barcodeResolver,
        ICatalogChangeNotifier catalogNotifier,
        IBranchSwitchGuard branchGuard,
        IBranchSwitchGuardRegistry branchGuardRegistry,
        IServiceProvider services,
        ILanguageService languageService)
        : base(languageService)
    {
        _saleService = saleService;
        _productService = productService;
        _posProductStore = posProductStore;
        _holdService = holdService;
        _customerService = customerService;
        _currentSettings = currentSettings;
        _cashSessionService = cashSessionService;
        _barcodeService = barcodeService;
        _barcodeResolver = barcodeResolver;
        _catalogNotifier = catalogNotifier;
        _branchGuard = branchGuard;
        _branchGuardRegistry = branchGuardRegistry;
        _services = services;

        InitializeComponent();

        _branchGuardRegistry.Register(_branchGuard);
        _catalogNotifier.CatalogChanged += OnCatalogChanged;

        KeyPreview = true;

        Disposed += (_, _) =>
        {
            _branchGuardRegistry.Unregister(_branchGuard);
            _catalogNotifier.CatalogChanged -= OnCatalogChanged;

            if (_themeSubscribed)
            {
                ThemeManager.ThemeChanged -= OnThemeChanged;
                _themeSubscribed = false;
            }
        };
    }

    /// <summary>
    /// F2 focuses the barcode/search box from anywhere, F12 sends the sale.
    /// ProcessCmdKey runs before the focused control, so a text box or the
    /// product list cannot swallow either key.  Both keys are gated by the
    /// selected tab: on the invoices tab F2 focuses the invoice search and
    /// F12 never completes a sale.
    /// </summary>
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        bool posTab = _tabs.SelectedIndex == 0;

        if (keyData == Keys.F2)
        {
            if (IsDisposedOrDisposing) return true;

            if (!posTab)
            {
                _invoiceSearch.Focus();
                _invoiceSearch.SelectAllText();
                return true;
            }

            _productSearch.Focus();
            _productSearch.SelectAllText();
            return true;
        }

        if (keyData == Keys.F12)
        {
            // The button is the single source of truth: it already reflects the
            // cart and the cash session, so F12 cannot bypass either rule.  It
            // is also inert outside the POS tab.
            if (posTab && !IsDisposedOrDisposing && _btnCompleteSale.Enabled)
                RunSafe(CompleteSaleAsync);

            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    /// <summary>
    /// Keeps the two pages strictly isolated.  The POS tree only exists on the
    /// "New Sale" tab; the invoice toolbar, grid and empty-state only exist on
    /// the "Sales" tab.  Forcing the flags on every switch also clears any
    /// stale paint left behind by a previous layout pass.
    /// </summary>
    private void OnTabSelected(object? sender, EventArgs e)
    {
        if (DesignTime.IsActive(this)) return;

        bool posTab = _tabs.SelectedIndex == 0;

        _tabNewSale.SuspendLayout();
        try
        {
            _pageLayout.Visible = posTab;
            _invoiceToolbar.Visible = !posTab;
            _invoiceGrid.Visible = !posTab;
        }
        finally
        {
            _tabNewSale.ResumeLayout(true);
        }

        if (posTab)
        {
            ApplySaleLayout();
        }
        else
        {
            FilterInvoices();
            _invoiceGrid.Invalidate();
            _tabInvoices.Invalidate(true);
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        // The design surface must never start initialization or theme work.
        if (DesignTime.IsActive(this))
            return;

        base.OnLoad(e);

        if (!_themeSubscribed)
        {
            ThemeManager.ThemeChanged += OnThemeChanged;
            _themeSubscribed = true;
        }

        ApplyTheme();

        _ = ApplyPermissionsAsync();
        RunSafe(InitializeAsync);
    }

    public Task OnBranchChangedAsync(Guid branchId)
    {
        RunSafe(InitializeAsync);
        return Task.CompletedTask;
    }

    private async Task ApplyPermissionsAsync()
    {
        try
        {
            var guard = _services?.GetService<IAuthorizationGuard>();
            if (guard is null) return;

            bool canDelete = await guard.IsAllowedAsync(Permissions.SalesDelete);
            bool canOverridePrice = await guard.IsAllowedAsync(Permissions.SalesOverridePrice);
            if (IsDisposedOrDisposing) return;

            _btnDeleteInvoice.Enabled = canDelete;
            _canOverridePrice = canOverridePrice;
            if (_cart.Count > 0)
                RefreshCart();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SalesForm permission check failed: {ex.Message}");
        }
    }

    private async Task InitializeAsync()
    {
        ApplySaleLayout();
        await LoadProductsAsync();
        if (IsDisposedOrDisposing) return;
        await LoadCustomersAsync();
        if (IsDisposedOrDisposing) return;
        await LoadInvoicesAsync();
        if (IsDisposedOrDisposing) return;
        await RefreshSessionStatusAsync();
        if (IsDisposedOrDisposing) return;
        RefreshCart();
    }

    /// <summary>
    /// Updates the session chip and the checkout gate. A sale needs an open
    /// cash session and at least one cart line; both F12 and the button use the
    /// enabled state of the button.
    /// </summary>
    private async Task RefreshSessionStatusAsync()
    {
        CashSessionDto? session = null;
        try
        {
            session = await _cashSessionService.GetOpenSessionAsync();
        }
        catch
        {
            session = null;
        }

        _sessionOpen = session != null;

        if (IsDisposedOrDisposing) return;

        ApplySessionChipText();
        UpdateCheckoutState();
    }

    private void ApplySessionChipText()
    {
        _sessionChip.SetStatus(
            _sessionOpen ? PosSessionState.Open : PosSessionState.Closed,
            _sessionOpen ? AppStrings.SessionOpen : AppStrings.DrawerClosed,
            _sessionOpen ? null : AppStrings.OpenSession);
    }

    private async Task OpenDrawerAsync()
    {
        if (OfferOpenDrawer())
            await RefreshSessionStatusAsync();
    }

    private bool OfferOpenDrawer()
    {
        if (IsDisposedOrDisposing) return false;

        using var dialog = _services.GetRequiredService<CashSessionOpenDialog>();
        dialog.ShowDialog(this);
        return dialog.OpenedSession != null;
    }

    private void OnCardPaymentClick(object? sender, EventArgs e)
    {
        _paymentMethod = PosPaymentMethod.Card;
        _numPaid.Enabled = false;
        _btnPayInFull.Enabled = false;
        UpdatePaymentButtons();
        UpdateTotals();
    }

    /// <summary>
    /// Credit keeps the sale on cash but leaves the paid amount at zero so the
    /// remaining balance is stored against the customer.
    /// </summary>
    private void OnCreditClick(object? sender, EventArgs e)
    {
        _paymentMethod = PosPaymentMethod.Cash;
        _numPaid.Enabled = true;
        _btnPayInFull.Enabled = true;
        _paidMode = PaidMode.Custom;
        _updatingTotals = true;
        try { _numPaid.Value = 0; }
        finally { _updatingTotals = false; }
        UpdatePaymentButtons();
        UpdateTotals();
    }

    private void UpdatePaymentButtons()
    {
        bool card = _paymentMethod == PosPaymentMethod.Card;
        _btnCard.Tone = card ? ButtonTone.Primary : ButtonTone.Neutral;
        _btnCard.Solid = card;
        _btnCard.ApplyTheme();
        _btnPayInFull.Enabled = !card;
    }

    private void OnHoldClick(object? sender, EventArgs e)
    {
        if (_cart.Count == 0) return;
        var selectedCustomer = _cmbCustomer.SelectedItem as CustomerItem;
        _holdService.Hold(_cart, selectedCustomer?.Id ?? Guid.Empty);
        _cart.Clear();
        RefreshCart();
        UpdateTotals();
        AppMessageBox.Success("Held");
    }

    private void OnCancelSaleClick(object? sender, EventArgs e)
    {
        if (_cart.Count == 0) return;
        if (!AppMessageBox.Confirm(AppStrings.Delete, AppStrings.CompleteSale)) return;
        _cart.Clear();
        _numDiscount.Value = 0;
        _numTax.Value = 0;
        _numFees.Value = 0;
        _numPaid.Value = 0;
        _cmbDiscountMode.SelectedIndex = 0;
        _paymentMethod = PosPaymentMethod.Cash;
        _paidMode = PaidMode.FullPayment;
        UpdatePaymentButtons();
        RefreshCart();
        UpdateTotals();
    }

    /// <summary>
    /// Places the product column and the basket next to each other, and stacks
    /// them when the page gets too narrow. The card width itself is decided by
    /// <see cref="PosCardLayoutCalculator"/>, not by this breakpoint.
    /// </summary>
    private void ApplySaleLayout()
    {
        int available = _tabNewSale.ClientSize.Width;
        int height = _tabNewSale.ClientSize.Height;
        if (available <= 0) return;

        bool stacked = available < 900;
        bool compact = available < 640;
        int cartSize = Math.Clamp((int)(available * 0.38), 340, 480);
        int cartHeight = stacked
            ? compact
                // On a phone the catalog must keep at least three rows of
                // chrome plus one card row. The basket gives up height instead
                // and scrolls its own lines and totals above the pinned actions.
                ? Math.Min(420, Math.Max(280, height - 180 - 300))
                : Math.Min(
                    Math.Clamp((int)(height * 0.62), 340, 560),
                    Math.Max(260, height - 160))
            : 0;

        if (stacked == _saleStacked
            && compact == _saleCompact
            && cartSize == _saleCartSize
            && cartHeight == _saleCartHeight)
        {
            UpdateCartRowWidths();
            return;
        }

        _saleStacked = stacked;
        _saleCompact = compact;
        _saleCartSize = cartSize;
        _saleCartHeight = cartHeight;

        ApplyHeroLayout(compact);
        ApplyProductChrome(compact);
        ApplyCartLayout(compact);

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
        UpdateCartRowWidths();
    }

    /// <summary>
    /// The hero strip sits above both columns. On wide screens the total keeps
    /// the full height at the start, with the discount and the cart count
    /// stacked beside it; on phones the three cards stack so the 38 pt total
    /// keeps its line height on a 375 px viewport.
    /// </summary>
    private void ApplyHeroLayout(bool compact)
    {
        _pageLayout.RowStyles[0] = new RowStyle(SizeType.Absolute, compact ? 180F : 110F);

        _heroStrip.SuspendLayout();
        _heroStrip.ColumnStyles.Clear();
        _heroStrip.RowStyles.Clear();

        if (compact)
        {
            _heroStrip.ColumnCount = 1;
            _heroStrip.RowCount = 3;
            _heroStrip.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _heroStrip.RowStyles.Add(new RowStyle(SizeType.Absolute, 86F));
            _heroStrip.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            _heroStrip.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));

            _heroStrip.SetCellPosition(_heroTotalCard, new TableLayoutPanelCellPosition(0, 0));
            _heroStrip.SetRowSpan(_heroTotalCard, 1);
            _heroStrip.SetCellPosition(_discountCard, new TableLayoutPanelCellPosition(0, 1));
            _heroStrip.SetCellPosition(_statsCard, new TableLayoutPanelCellPosition(0, 2));

            _heroTotalCard.Margin = new Padding(0, 0, 0, 4);
            _discountCard.Margin = new Padding(0, 0, 0, 4);
            _statsCard.Margin = new Padding(0);
            _lblHeroStats.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        }
        else
        {
            _heroStrip.ColumnCount = 2;
            _heroStrip.RowCount = 2;
            _heroStrip.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            _heroStrip.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            _heroStrip.RowStyles.Add(new RowStyle(SizeType.Absolute, 66F));
            _heroStrip.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));

            _heroStrip.SetCellPosition(_heroTotalCard, new TableLayoutPanelCellPosition(0, 0));
            _heroStrip.SetRowSpan(_heroTotalCard, 2);
            _heroStrip.SetCellPosition(_discountCard, new TableLayoutPanelCellPosition(1, 0));
            _heroStrip.SetCellPosition(_statsCard, new TableLayoutPanelCellPosition(1, 1));

            _heroTotalCard.Margin = new Padding(0, 0, 8, 0);
            _discountCard.Margin = new Padding(8, 0, 0, 0);
            _statsCard.Margin = new Padding(8, 0, 0, 0);
            _lblHeroStats.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        }

        _heroStrip.ResumeLayout(true);
        _heroStrip.Invalidate(true);
    }

    /// <summary>
    /// Phone-width chrome. The scan button loses its label so the search field
    /// keeps a usable width and the category strip gets room for its horizontal
    /// scrollbar.
    /// </summary>
    private void ApplyProductChrome(bool compact)
    {
        int scanWidth = compact ? 52 : 150;

        if ((int)_searchBar.ColumnStyles[1].Width != scanWidth)
            _searchBar.ColumnStyles[1] = new ColumnStyle(SizeType.Absolute, scanWidth);

        _btnScanBarcode.Text = compact ? string.Empty : "  " + AppStrings.ScanBarcode;
        _toolTip.SetToolTip(_btnScanBarcode, AppStrings.ScanBarcode);

        // Row 2 of the product column is the category strip; row 1 is the
        // search bar. Keep the phone strip taller for its scrollbar.
        float categoryHeight = compact ? 56F : 44F;
        if (_productColumn.RowStyles[2].Height != categoryHeight)
            _productColumn.RowStyles[2] = new RowStyle(SizeType.Absolute, categoryHeight);
    }

    /// <summary>
    /// At tablet and phone widths the calculator and print buttons become
    /// icon-only; the four payment actions and the big checkout button keep
    /// their labels.
    /// </summary>
    private void ApplyCartLayout(bool compact)
    {
        // The hero already shows the item count on phones, so the cart header
        // is dropped there to keep one full cart line visible above the pinned
        // actions; desktop and tablet keep it for the clear-cart action.
        _cartHeader.Visible = !compact;
        _basketLayout.RowStyles[0] = new RowStyle(SizeType.Absolute, compact ? 40F : 44F);
        _basketLayout.RowStyles[1] = new RowStyle(SizeType.Absolute, compact ? 0F : 36F);

        // The "Customer name" caption wraps and clips in Arabic at phone width;
        // the combo already says "Select customer", so the caption is dropped.
        _lblCustomer.Visible = !compact;
        _customerRow.ColumnStyles[0] = new ColumnStyle(
            SizeType.Percent, compact ? 0F : 26F);
        _customerRow.ColumnStyles[1] = new ColumnStyle(
            SizeType.Percent, compact ? 100F : 74F);

        _btnCalculator.Text = compact ? string.Empty : "  " + AppStrings.Calculator;
        _btnPrintLast.Text = compact ? string.Empty : "  " + AppStrings.Print;

        _toolTip.SetToolTip(_btnCalculator, AppStrings.Calculator);
        _toolTip.SetToolTip(_btnPrintLast, AppStrings.Print);
    }

    /// <summary>
    /// The resize event fires before the new size is applied, so the layout is
    /// recomputed on the next message loop pass with the settled measurements.
    /// </summary>
    private void OnSaleTabResized(object? sender, EventArgs e)
    {
        if (_layoutPending || DesignTime.IsActive(this)) return;

        _layoutPending = true;
        BeginInvoke(new Action(() =>
        {
            _layoutPending = false;
            ApplySaleLayout();
        }));
    }

    private void OnBasketScrollSizeChanged(object? sender, EventArgs e) => UpdateCartRowWidths();

    private void OnCartLinesSizeChanged(object? sender, EventArgs e) => UpdateCartRowWidths();

    /// <summary>
    /// The cart rows are hosted by an auto-sized flow panel inside a scrolling
    /// panel, so their width has to follow the visible basket width. The guard
    /// stops the width change from re-entering the layout.
    /// </summary>
    private void UpdateCartRowWidths()
    {
        if (_resizingCartRows || IsDisposedOrDisposing) return;

        _resizingCartRows = true;
        try
        {
            int width = _basketScroll.ClientSize.Width;
            if (_basketScroll.VerticalScroll.Visible)
                width -= SystemInformation.VerticalScrollBarWidth;

            if (width <= 0)
                width = _basketLayout.Width;

            width = Math.Max(220, width);

            foreach (CartLineRow row in _cartRowViews)
                row.Width = width;

            _lblEmptyCart.Width = width;
        }
        finally
        {
            _resizingCartRows = false;
        }
    }

    private void ApplyTheme()
    {
        if (IsDisposedOrDisposing) return;

        SuspendLayout();
        try
        {
            ThemeManager.ApplyToControl(this);

            BackColor = AppColors.Background;
            _tabNewSale.BackColor = AppColors.Background;
            _tabInvoices.BackColor = AppColors.Background;

            _leftPanel.BackColor = AppColors.Background;
            _rightPanel.BackColor = AppColors.Surface;
            _basketScroll.BackColor = AppColors.Surface;
            _basketLayout.BackColor = AppColors.Surface;
            _cartLines.BackColor = AppColors.Surface;
            _cartHeader.BackColor = AppColors.Surface;
            _customerRow.BackColor = AppColors.Surface;
            _totalsLayout.BackColor = AppColors.Surface;
            _discountEditorPanel.BackColor = AppColors.Surface;
            _paidEditorPanel.BackColor = AppColors.Surface;
            _productHeader.BackColor = AppColors.Background;
            _heroStrip.BackColor = AppColors.Background;
            _heroTotalCard.BackColor = AppColors.Surface;
            _discountCard.BackColor = AppColors.Surface;
            _statsCard.BackColor = AppColors.Surface;

            _lblHeroTotal.ForeColor = AppColors.Primary;
            _lblHeroDiscount.ForeColor = AppColors.Success;
            _lblHeroStats.ForeColor = AppColors.Ink;
            _lblHeroTotalCaption.ForeColor = AppColors.InkMuted;
            _lblHeroDiscountCaption.ForeColor = AppColors.InkMuted;
            _lblProductCount.ForeColor = AppColors.InkMuted;
            _lblTotalValue.ForeColor = AppColors.Primary;
            _lblDiscountValue.ForeColor = AppColors.InkMuted;
            _lblEmptyCart.ForeColor = AppColors.InkMuted;
            _lblNoInvoices.ForeColor = AppColors.InkMuted;
            _lblNoInvoices.BackColor = AppColors.Background;

            ApplyInputTheme(_productSearch);
            ApplyInputTheme(_invoiceSearch);

            _lblEmptyCart.Text = AppStrings.EmptyCart;
            UpdatePaymentButtons();
        }
        finally
        {
            ResumeLayout(true);
        }

        UpdateTotals();
    }

    private static void ApplyInputTheme(EZTextBox input)
    {
        input.BackColor = AppColors.InputBackground;
        input.ForeColor = AppColors.Ink;
        input.BorderColor = AppColors.InputBorder;
        input.BorderFocusColor = AppColors.InputBorderFocus;
        input.PlaceholderColor = AppColors.InkMuted;
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        if (IsDisposedOrDisposing) return;

        if (InvokeRequired)
        {
            if (IsHandleCreated)
                BeginInvoke(new Action(ApplyTheme));
            return;
        }

        ApplyTheme();
    }

    protected override void LoadText()
    {
        Text = AppStrings.PointOfSale;
        _tabNewSale.Text = AppStrings.NewSale;
        _tabInvoices.Text = AppStrings.Sales;
        _productSearch.PlaceholderText = AppStrings.SearchProducts + " (F2)";
        _invoiceSearch.PlaceholderText = AppStrings.SearchInvoices;
        _btnScanBarcode.Text = "  " + AppStrings.ScanBarcode;
        _btnScanBarcode.AccessibleDescription = AppStrings.ScanBarcode;
        _lblCustomer.Text = AppStrings.CustomerName + ":";
        _lblCart.Text = AppStrings.Cart + (_cart.Count > 0 ? $" ({_cart.Count})" : string.Empty);
        _btnAddCustomer.Text = string.Empty;
        _btnAddCustomer.AccessibleDescription = AppStrings.AddCustomer;
        _btnClearCart.Text = "  " + AppStrings.Delete;
        _btnClearCart.AccessibleDescription = AppStrings.Delete;
        _lblSubtotal.Text = AppStrings.Subtotal + ":";
        _lblDiscount.Text = AppStrings.Discount + ":";
        _lblAfterDiscount.Text = AppStrings.AfterDiscount + ":";
        _lblTax.Text = AppStrings.TaxRate + " (%):";
        _lblTaxAmount.Text = AppStrings.TaxAmount + ":";
        _lblFees.Text = AppStrings.Fees + ":";
        _lblTotal.Text = AppStrings.GrandTotal + ":";
        _lblPaid.Text = AppStrings.PaidAmount + ":";
        _btnPayInFull.Text = string.Empty;
        _btnPayInFull.AccessibleDescription = AppStrings.PayInFull;
        _btnCompleteSale.Text = "  " + AppStrings.CompleteSale;
        _btnCompleteSale.AccessibleDescription = AppStrings.CompleteSale;
        _btnPrintLast.Text = "  " + AppStrings.Print;
        _btnCalculator.Text = "  " + AppStrings.Calculator;
        _btnNewSale.Text = "  " + AppStrings.NewSale;
        _btnDeleteInvoice.Text = "  " + AppStrings.Delete;
        _lblEmptyCart.Text = AppStrings.EmptyCart;
        _lblNoInvoices.Text = AppStrings.NoInvoices;

        _lblHeroTotalCaption.Text = AppStrings.GrandTotal;
        _lblHeroDiscountCaption.Text = AppStrings.Discount;
        _lblBalance.Text = AppStrings.RemainingAmount + ":";
        _btnCard.Text = "  " + AppStrings.Card;
        _btnCredit.Text = "  " + AppStrings.Credit;
        _btnHold.Text = "  " + AppStrings.Hold;
        _btnCancelSale.Text = "  " + AppStrings.Cancel;
        _btnViewToggle.AccessibleDescription = AppStrings.Products;

        _toolTip.SetToolTip(_btnAddCustomer, AppStrings.AddCustomer);
        _toolTip.SetToolTip(_btnPayInFull, AppStrings.PayInFull);
        _toolTip.SetToolTip(_btnClearCart, AppStrings.Delete);
        _toolTip.SetToolTip(_btnScanBarcode, AppStrings.ScanBarcode);
        _toolTip.SetToolTip(_btnCalculator, AppStrings.Calculator);
        _toolTip.SetToolTip(_btnPrintLast, AppStrings.Print);
        _toolTip.SetToolTip(_btnCompleteSale, AppStrings.CompleteSale);
        _toolTip.SetToolTip(_productSearch, AppStrings.SearchProducts + " (F2)");
        _toolTip.SetToolTip(_btnNewSale, AppStrings.NewSale + " (F12 = " + AppStrings.CompleteSale + ")");

        int discountModeIndex = _cmbDiscountMode.SelectedIndex;
        _cmbDiscountMode.Items.Clear();
        _cmbDiscountMode.Items.AddRange(new object[] { AppStrings.DiscountModePercent, AppStrings.DiscountModeAmount });
        _cmbDiscountMode.SelectedIndex = discountModeIndex < 0 ? 0 : discountModeIndex;

        ApplySessionChipText();
        ApplyProductChrome(_saleCompact);
        ApplyCartLayout(_saleCompact);
        RefreshCartRowTexts();

        // FormLocalizationHelper forces every FlowLayoutPanel to RightToLeft in
        // Arabic; the cart is a vertical stack and must stay TopDown.
        _cartLines.FlowDirection = FlowDirection.TopDown;
    }

    // =========================
    // Catalog
    // =========================

    private async Task LoadProductsAsync()
    {
        _catalog = await _posProductStore.GetAllAsync();
        if (IsDisposedOrDisposing) return;

        _catalogByVariant.Clear();
        foreach (PosProductItem item in _catalog)
            _catalogByVariant[item.VariantId] = item;

        RebuildCategoryBar();
        await FilterProductsAsync();
    }

    /// <summary>
    /// Builds the chips from the catalog itself. Duplicate ids are ignored and
    /// blank category names never create an unreadable chip.
    /// </summary>
    private void RebuildCategoryBar()
    {
        var options = _catalog
            .Where(i => i.CategoryId != Guid.Empty && !string.IsNullOrWhiteSpace(i.CategoryName))
            .GroupBy(i => i.CategoryId)
            .Select(g => new PosCategoryOption(g.Key, g.First().CategoryName!))
            .OrderBy(o => o.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        _categoryBar.SetCategories(options);
    }

    private void OnCategorySelected(object? sender, PosCategoryFilterChangedEventArgs e)
    {
        _selectedCategoryId = e.CategoryId;
        RunSafe(FilterProductsAsync);
    }

    /// <summary>
    /// Filters the in-memory catalog by name, SKU, barcode and category. The
    /// barcode lookup uses the same resolver as the scanner; a version guard
    /// drops the result of a superseded keystroke.
    /// </summary>
    private async Task FilterProductsAsync()
    {
        int version = ++_productFilterVersion;
        string? term = PosProductFilter.Normalize(_productSearch.Text);

        HashSet<Guid>? barcodeProductIds = null;
        if (term is not null)
        {
            var matches = await _barcodeResolver.ResolveAsync(term);
            if (version != _productFilterVersion || IsDisposedOrDisposing) return;

            if (matches.Count > 0)
                barcodeProductIds = matches.Select(m => m.ProductId).ToHashSet();
        }

        IReadOnlyList<PosProductItem> filtered = PosProductFilter.Apply(
            _catalog,
            term,
            _selectedCategoryId,
            barcodeProductIds);

        if (version != _productFilterVersion || IsDisposedOrDisposing) return;

        _filteredItems = filtered;
        _productList.Items = filtered;

        if (_lblProductCount is not null)
            _lblProductCount.Text = $"{filtered.Count} {AppStrings.Products}";
    }

    /// <summary>Reloads the catalog when products, variants or barcodes change.</summary>
    private void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
    {
        if (IsDisposedOrDisposing) return;

        // Stock-only notifications already update the cached item; the screen
        // shows stock in the cards, so a full reload is only needed for the
        // catalog itself.
        if (!e.FullInvalidation) return;

        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => OnCatalogChanged(sender, e)));
            return;
        }

        if (_catalogReloadPending) return;
        _catalogReloadPending = true;
        RunSafe(ReloadCatalogAsync);
    }

    private async Task ReloadCatalogAsync()
    {
        try
        {
            _products.Clear();
            await LoadProductsAsync();
        }
        finally
        {
            _catalogReloadPending = false;
        }
    }

    // =========================
    // Adding products
    // =========================

    private void OnProductItemActivated(object? sender, PosProductItem item) =>
        RunSafe(() => AddVariantToCartAsync(item));

    private void OnProductSearchTextChanged(object? sender, EventArgs e) =>
        RunSafe(FilterProductsAsync);

    private void OnProductSearchKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter) return;

        e.SuppressKeyPress = true;
        RunSafe(AddFirstProductToCartAsync);
    }

    private void ScanBarcode()
    {
        if (IsDisposedOrDisposing) return;

        var settings = _currentSettings.Current;

        if (!settings.CameraBarcodeEnabled)
        {
            AppMessageBox.Warning(AppStrings.EnableCameraScanning);
            return;
        }

        using var overlay = new BarcodeScannerOverlay(settings.BarcodeCameraDevice);
        overlay.ContinuousMode = true;
        overlay.BarcodeDetected += code =>
            RunSafe(async () => { await AddByBarcodeAsync(code, showWarning: true); });
        overlay.ShowDialog(this);
    }

    private enum BarcodeAddResult
    {
        NotFound,
        Added,
        Cancelled
    }

    /// <summary>
    /// The single barcode entry point. The camera, a keyboard-wedge reader and
    /// manual typing all resolve through <see cref="IPosBarcodeResolver"/>.
    /// </summary>
    private async Task<BarcodeAddResult> AddByBarcodeAsync(string? code, bool showWarning)
    {
        var text = code?.Trim();
        if (string.IsNullOrEmpty(text))
            return BarcodeAddResult.NotFound;

        var matches = await _barcodeResolver.ResolveAsync(text);
        if (matches.Count == 0)
        {
            if (showWarning)
            {
                AppMessageBox.Warning($"{AppStrings.BarcodeNotFound}: {text}");
                _productSearch.Text = text;
            }
            return BarcodeAddResult.NotFound;
        }

        if (matches.Count == 1)
            return await AddResolvedItemAsync(matches[0], showWarning)
                ? BarcodeAddResult.Added
                : BarcodeAddResult.NotFound;

        var variantIds = matches.Select(m => m.VariantId).Distinct().ToList();
        using var picker = new SharedBarcodePickerDialog(
            _services.GetRequiredService<DzentoPOS2.Data.Context.AppDbContext>(),
            variantIds.AsReadOnly());
        if (picker.ShowDialog(this) != DialogResult.OK)
            return BarcodeAddResult.Cancelled;

        var selected = matches.FirstOrDefault(m => m.VariantId == picker.SelectedVariantId);
        if (selected is null)
            return BarcodeAddResult.NotFound;

        return await AddResolvedItemAsync(selected, showWarning)
            ? BarcodeAddResult.Added
            : BarcodeAddResult.NotFound;
    }

    private async Task<bool> AddResolvedItemAsync(PosProductItem item, bool showWarning)
    {
        var product = await GetProductAsync(item.ProductId);

        if (product is null)
        {
            if (showWarning)
                AppMessageBox.Warning($"{AppStrings.BarcodeNotFound}: {item.PrimaryBarcode}");
            return false;
        }

        bool added = await AddToCartAsync(product, item.VariantId, item.Name, item.SalePrice);
        if (added)
            ShowScanConfirmation(item);

        return added;
    }

    /// <summary>Adds the variant behind a product card.</summary>
    private async Task AddVariantToCartAsync(PosProductItem item)
    {
        var product = await GetProductAsync(item.ProductId);
        if (product is null)
        {
            AppMessageBox.Warning(AppStrings.OperationFailed);
            return;
        }

        if (await AddToCartAsync(product, item.VariantId, item.Name, item.SalePrice))
            ShowScanConfirmation(item);
    }

    private async Task<ProductDto?> GetProductAsync(Guid productId)
    {
        if (_productCache.TryGetValue(productId, out ProductDto? cached))
            return cached;

        ProductDto? product = _products.FirstOrDefault(p => p.Id == productId)
            ?? await _productService.GetByIdAsync(productId);

        if (product is not null)
            _productCache[productId] = product;

        return product;
    }

    private void ShowScanConfirmation(PosProductItem item)
    {
        string detail = FormatMoney(item.SalePrice);
        if (!string.IsNullOrWhiteSpace(item.PrimaryBarcode))
            detail += "   " + item.PrimaryBarcode;

        _scanStrip.ShowConfirmation(
            string.IsNullOrWhiteSpace(item.Name) ? detail : item.Name,
            detail,
            durationMs: 2500);
    }

    private async Task AddFirstProductToCartAsync()
    {
        var term = _productSearch.Text?.Trim();

        // An exact barcode always wins over the highlighted card.
        if (!string.IsNullOrWhiteSpace(term))
        {
            var result = await AddByBarcodeAsync(term, showWarning: false);
            if (result != BarcodeAddResult.NotFound)
                return;
        }

        PosProductItem? item = _productList.SelectedItem;
        if (item is null && _filteredItems.Count == 1)
            item = _filteredItems[0];

        // Never fall back to an arbitrary product: an unmatched search must not
        // add the first item in the catalog.
        if (item is not null)
            await AddVariantToCartAsync(item);
    }

    /// <summary>
    /// Adds one line to the cart. Returns false when the cashier cancelled a
    /// weight or batch prompt, or when the product is no longer sellable.
    /// </summary>
    private async Task<bool> AddToCartAsync(
        ProductDto product,
        Guid variantId,
        string variantName,
        decimal price)
    {
        if (!product.IsActive)
        {
            AppMessageBox.Warning(AppStrings.OperationFailed);
            return false;
        }

        decimal quantity = 1m;

        // Weighed products: the cashier enters the actual weight; the price
        // stays per unit and the line quantity becomes the weight.
        if (product.IsWeighed)
        {
            using var weightDialog = new WeightEntryDialog(price, product.Name);
            if (weightDialog.ShowDialog(this) != DialogResult.OK || weightDialog.Weight <= 0)
                return false;
            quantity = weightDialog.Weight;
        }

        // Every cart line carries a concrete VariantId. Without this, a
        // double-click (empty id) and a scan (real id) would not merge.
        if (variantId == Guid.Empty)
            variantId = await _barcodeService.GetDefaultVariantIdAsync(product.Id);

        if (variantId == Guid.Empty)
        {
            AppMessageBox.Warning(AppStrings.OperationFailed);
            return false;
        }

        // Batch-tracked products: let the cashier pick the batch to consume.
        // An explicit batch is honoured by StockService; the rest of the
        // quantity (if any) falls back to FEFO.
        Guid batchId = Guid.Empty;
        if (product.TrackBatch)
        {
            var warehouseId = await GetActiveWarehouseIdAsync();
            if (warehouseId is null)
            {
                AppMessageBox.Warning(AppStrings.OperationFailed);
                return false;
            }

            using var batchDialog = new BatchSelectionDialog(
                _services.GetRequiredService<DzentoPOS2.Data.Context.AppDbContext>(),
                variantId,
                warehouseId.Value,
                quantity);

            if (batchDialog.ShowDialog(this) != DialogResult.OK)
                return false;

            batchId = batchDialog.SelectedBatchId;
        }

        PosCartMerger.AddOrMerge(_cart, new CreateSaleItemDto
        {
            ProductId = product.Id,
            VariantId = variantId,
            VariantName = variantName,
            Quantity = quantity,
            Price = price,
            OriginalPrice = price,
            BatchId = batchId
        });

        RefreshCart();
        return true;
    }

    private async Task<Guid?> GetActiveWarehouseIdAsync()
    {
        var context = _services.GetRequiredService<DzentoPOS2.Data.Context.AppDbContext>();
        var branchId = _services
            .GetRequiredService<IActiveBranchContext>()
            .ActiveBranchId;

        return await context.Warehouses
            .AsNoTracking()
            .Where(w => w.BranchId == branchId && w.IsActive)
            .OrderBy(w => w.CreatedAt)
            .ThenBy(w => w.Id)
            .Select(w => (Guid?)w.Id)
            .FirstOrDefaultAsync();
    }

    // =========================
    // Cart rows
    // =========================

    private void ClearCart()
    {
        if (_cart.Count == 0) return;

        var result = MessageBox.Show(
            AppStrings.DeleteSaleConfirm,
            AppStrings.DeleteConfirmation,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            _cart.Clear();
            RefreshCart();
        }
    }

    /// <summary>
    /// Applies the cart diff to the row list. Only added, removed and changed
    /// rows are touched, so a quantity step does not rebuild the basket.
    /// </summary>
    private void RefreshCart()
    {
        PosCartRowDiff diff = _cartRowTracker.Apply(_cart);

        if (diff.FullReset || diff.OrderChanged)
        {
            RebuildCartRows();
        }
        else
        {
            for (int i = diff.RemovedRows.Count - 1; i >= 0; i--)
                RemoveCartRowAt(diff.RemovedRows[i]);

            foreach (int index in diff.AddedRows)
                InsertCartRow(index);

            foreach (int index in diff.ChangedRows)
                UpdateCartRow(index);
        }

        _lblEmptyCart.Visible = _cart.Count == 0;
        _lblEmptyCart.Text = AppStrings.EmptyCart;
        _btnClearCart.Enabled = _cart.Count > 0;
        _lblCart.Text = AppStrings.Cart + (_cart.Count > 0 ? $" ({_cart.Count})" : string.Empty);

        // Update branch switch guard based on cart state.
        _branchGuard.CanSwitchBranch = _cart.Count == 0;

        UpdateCartRowWidths();
        UpdateTotals();
    }

    private void RebuildCartRows()
    {
        foreach (CartLineRow row in _cartRowViews)
        {
            _cartLines.Controls.Remove(row);
            row.Dispose();
        }

        _cartRowViews.Clear();

        for (int i = 0; i < _cart.Count; i++)
            InsertCartRow(i);

        UpdateCartRowWidths();
    }

    private void InsertCartRow(int index)
    {
        if (index < 0 || index >= _cart.Count) return;

        CreateSaleItemDto line = _cart[index];
        var row = new CartLineRow
        {
            Height = 56,
            Margin = new Padding(0, 0, 0, 6),
            Tag = line
        };

        row.QuantityChanged += (_, e) => OnCartQuantityChanged(row, e.Quantity);
        row.RemoveRequested += (_, _) => OnCartRemoveRequested(row);
        row.PriceEditRequested += (_, _) => EditCartPrice(row);

        FillCartRow(row, line);
        _cartRowViews.Insert(index, row);
        _cartLines.Controls.Add(row);

        if (index < _cartLines.Controls.Count - 1)
            _cartLines.Controls.SetChildIndex(row, index);
    }

    private void RemoveCartRowAt(int index)
    {
        if (index < 0 || index >= _cartRowViews.Count) return;

        CartLineRow row = _cartRowViews[index];
        _cartRowViews.RemoveAt(index);
        _cartLines.Controls.Remove(row);
        row.Dispose();
    }

    private void UpdateCartRow(int index)
    {
        if (index < 0 || index >= _cart.Count || index >= _cartRowViews.Count) return;

        CartLineRow row = _cartRowViews[index];
        row.Tag = _cart[index];
        FillCartRow(row, _cart[index]);
    }

    private void FillCartRow(CartLineRow row, CreateSaleItemDto line)
    {
        // Weighed lines carry a fractional quantity (e.g. 0.750 kg); the
        // stepper must not clamp it up to a whole unit.
        row.MinimumQuantity = line.Quantity > 0 && line.Quantity < 1 ? line.Quantity : 1;

        row.SetLine(
            DisplayNameFor(line),
            BuildCartSubtitle(line),
            line.Quantity,
            line.Price,
            CurrencyCode,
            canRemove: true);
    }

    private string DisplayNameFor(CreateSaleItemDto line)
    {
        if (!string.IsNullOrWhiteSpace(line.VariantName))
            return line.VariantName;

        if (_catalogByVariant.TryGetValue(line.VariantId, out PosProductItem? item)
            && !string.IsNullOrWhiteSpace(item.Name))
        {
            return item.Name;
        }

        if (_productCache.TryGetValue(line.ProductId, out ProductDto? product))
            return product.Name;

        return string.IsNullOrWhiteSpace(line.Description) ? "-" : line.Description;
    }

    private string? BuildCartSubtitle(CreateSaleItemDto line)
    {
        if (_catalogByVariant.TryGetValue(line.VariantId, out PosProductItem? item))
            return string.IsNullOrWhiteSpace(item.SKU) ? item.PrimaryBarcode : item.SKU;

        return null;
    }

    private void OnCartQuantityChanged(CartLineRow row, decimal quantity)
    {
        if (row.Tag is not CreateSaleItemDto line) return;

        int index = _cart.IndexOf(line);
        if (index < 0) return;

        line.Quantity = quantity;
        RefreshCart();
    }

    private void OnCartRemoveRequested(CartLineRow row)
    {
        if (row.Tag is not CreateSaleItemDto line) return;

        int index = _cart.IndexOf(line);
        if (index < 0) return;

        _cart.RemoveAt(index);
        RefreshCart();
    }

    /// <summary>Price override is permission-gated, as it was on the grid.</summary>
    private void EditCartPrice(CartLineRow row)
    {
        if (!_canOverridePrice) return;
        if (row.Tag is not CreateSaleItemDto line) return;

        using var dialog = new PriceOverrideDialog(DisplayNameFor(line), line.Price, CurrencyCode);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        line.Price = dialog.Price;
        line.PriceOverridden = line.Price != line.OriginalPrice;
        RefreshCart();
    }

    /// <summary>
    /// Re-applies the localized text to the existing rows after a language
    /// change without rebuilding them.
    /// </summary>
    private void RefreshCartRowTexts()
    {
        for (int i = 0; i < _cartRowViews.Count && i < _cart.Count; i++)
            UpdateCartRow(i);

        _lblEmptyCart.Text = AppStrings.EmptyCart;
        _lblCart.Text = AppStrings.Cart + (_cart.Count > 0 ? $" ({_cart.Count})" : string.Empty);
    }

    // =========================
    // Customers and invoices
    // =========================

    private async Task LoadCustomersAsync()
    {
        _customers = await _customerService.GetAllAsync();
        if (IsDisposedOrDisposing) return;

        _cmbCustomer.Items.Clear();
        _cmbCustomer.Items.Add(new CustomerItem(Guid.Empty, AppStrings.SelectCustomer));
        foreach (var c in _customers)
            _cmbCustomer.Items.Add(new CustomerItem(c.Id, c.FullName));
        _cmbCustomer.DisplayMember = nameof(CustomerItem.FullName);
        _cmbCustomer.ValueMember = nameof(CustomerItem.Id);
        _cmbCustomer.SelectedIndex = 0;
    }

    private void OnAddCustomerClick(object? sender, EventArgs e) => AddCustomer();

    /// <summary>Creates a customer from the sales screen and selects it right away.</summary>
    private void AddCustomer()
    {
        if (IsDisposedOrDisposing) return;

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
        if (IsDisposedOrDisposing) return;

        _invoiceGrid.DataSource = null;
        _invoiceGrid.Columns.Clear();
        _invoiceGrid.AutoGenerateColumns = false;
        _invoiceGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        _invoiceGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        _invoiceGrid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
        _invoiceGrid.Columns.AddRange(new[]
        {
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.InvoiceNumber, DataPropertyName = "InvoiceNumber", Width = 150 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.CustomerName, DataPropertyName = "CustomerName", Width = 150 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Subtotal + " (" + CurrencyCode + ")", DataPropertyName = "Subtotal", Width = 110, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Discount, DataPropertyName = "DiscountDisplay", Width = 120, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.TaxAmount + " (" + CurrencyCode + ")", DataPropertyName = "TaxAmount", Width = 100, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Total + " (" + CurrencyCode + ")", DataPropertyName = "Total", Width = 110, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.PaidAmount + " (" + CurrencyCode + ")", DataPropertyName = "PaidAmount", Width = 100, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.RemainingAmount + " (" + CurrencyCode + ")", DataPropertyName = "RemainingAmount", Width = 110, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Change + " (" + CurrencyCode + ")", DataPropertyName = "ChangeAmount", Width = 100, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Date, DataPropertyName = "CreatedAt", Width = 130, DefaultCellStyle = { Format = "yyyy/MM/dd HH:mm" } }
        });
        _invoiceGrid.DataSource = _invoices;
        FilterInvoices();
    }

    private void OnInvoiceSearchTextChanged(object? sender, EventArgs e) => FilterInvoices();

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
        _lblNoInvoices.Visible = filtered.Count == 0;
    }

    // =========================
    // Totals and payment
    // =========================

    /// <summary>Preview shared with the printed receipt and the saved invoice.</summary>
    private InvoiceTotals? CalculateTotals()
    {
        if (_cart.Count == 0)
            return null;

        var digits = _currentSettings.Regional.CurrencyDecimalDigits;
        var mode = _cmbDiscountMode.SelectedIndex == 1
            ? DiscountMode.FixedAmount
            : DiscountMode.Percent;
        var discountValue = (decimal)_numDiscount.Value;
        var taxRate = (decimal)_numTax.Value;
        var fees = (decimal)_numFees.Value;
        var paid = (decimal)_numPaid.Value;

        if (mode == DiscountMode.FixedAmount)
        {
            var subtotalPreview = _cart.Sum(i =>
                decimal.Round(i.Quantity * i.Price, digits, MidpointRounding.AwayFromZero));
            if (discountValue > subtotalPreview)
                discountValue = subtotalPreview;
        }
        else if (discountValue > 100)
        {
            // Kept at the maximum for the live preview; the service rejects it.
            discountValue = 100;
        }

        return InvoiceCalculator.Calculate(
            _cart.Select(i => new InvoiceLine(i.Quantity, i.Price)).ToList(),
            mode,
            discountValue,
            taxRate,
            paid,
            digits,
            fees);
    }

    private void OnDiscountModeChanged(object? sender, EventArgs e) => UpdateTotals();

    private void OnDiscountValueChanged(object? sender, EventArgs e) => UpdateTotals();

    private void OnTaxValueChanged(object? sender, EventArgs e) => UpdateTotals();

    private void OnFeesValueChanged(object? sender, EventArgs e) => UpdateTotals();

    private void UpdateTotals()
    {
        if (_updatingTotals || _lblTotalValue is null) return;

        var totals = CalculateTotals();

        var subtotal = totals?.Subtotal ?? 0;
        var discountAmount = totals?.DiscountAmount ?? 0;
        var afterDiscount = totals?.AfterDiscount ?? 0;
        var taxAmount = totals?.TaxAmount ?? 0;
        var total = totals?.Total ?? 0;

        _lblSubtotalValue.Text = FormatMoney(subtotal);
        _lblDiscountValue.Text = FormatMoney(discountAmount);
        _lblDiscountValue.ForeColor = discountAmount > 0 ? AppColors.Success : AppColors.InkMuted;
        _lblAfterDiscountValue.Text = FormatMoney(afterDiscount);
        _lblTaxAmountValue.Text = FormatMoney(taxAmount);
        _lblTotalValue.Text = FormatMoney(total);

        if (_lblHeroTotal is not null)
        {
            _lblHeroTotal.Text = FormatMoney(total);
            _lblHeroDiscount.Text = FormatMoney(discountAmount);
            _lblHeroDiscount.ForeColor = discountAmount > 0 ? AppColors.Success : AppColors.InkMuted;
            _lblHeroStats.Text = $"{_cart.Count} {AppStrings.Cart}";
        }

        var percentMode = _cmbDiscountMode.SelectedIndex != 1;
        _lblDiscount.Text = discountAmount > 0 && percentMode && _numDiscount.Value > 0
            ? $"{AppStrings.Discount} ({_numDiscount.Value:0.##}%):"
            : AppStrings.Discount + ":";

        // Full payment follows the total on every change; custom keeps the
        // value the cashier typed.
        if (_paidMode == PaidMode.FullPayment)
        {
            _updatingTotals = true;
            try
            {
                _numPaid.Value = Math.Min(Math.Max(total, _numPaid.Minimum), _numPaid.Maximum);
            }
            finally
            {
                _updatingTotals = false;
            }
        }

        // Card payments always settle the exact total.
        if (SelectedPaymentMethod == PosPaymentMethod.Card)
        {
            _updatingTotals = true;
            try
            {
                _numPaid.Value = Math.Min(Math.Max(total, _numPaid.Minimum), _numPaid.Maximum);
            }
            finally
            {
                _updatingTotals = false;
            }
        }

        var paid = _numPaid.Value;
        var remaining = Math.Max(0, total - paid);
        var change = Math.Max(0, paid - total);

        if (remaining > 0)
        {
            _lblBalance.Text = AppStrings.RemainingAmount + ":";
            _lblBalanceValue.Text = FormatMoney(remaining);
            _lblBalanceValue.ForeColor = AppColors.Warning;
        }
        else
        {
            _lblBalance.Text = AppStrings.Change + ":";
            _lblBalanceValue.Text = FormatMoney(change);
            _lblBalanceValue.ForeColor = change > 0 ? AppColors.Success : AppColors.Ink;
        }

        UpdateCheckoutState();
    }

    private void UpdateCheckoutState()
    {
        if (_btnCompleteSale is null) return;

        // Empty cart and closed session both disable checkout; F12 reads this.
        _btnCompleteSale.Enabled = _sessionOpen && _cart.Count > 0;
    }

    private void OnPaidValueChanged(object? sender, EventArgs e)
    {
        if (_updatingTotals) return;

        var digits = _currentSettings.Regional.CurrencyDecimalDigits;
        var total = CalculateTotals()?.Total ?? 0;

        // Typing the exact total switches back to full payment.
        _paidMode = _numPaid.Value == decimal.Round(total, digits, MidpointRounding.AwayFromZero)
            ? PaidMode.FullPayment
            : PaidMode.Custom;

        UpdateTotals();
    }

    private void OnPayInFullClick(object? sender, EventArgs e) => SetFullPayment();

    private void SetFullPayment()
    {
        _paidMode = PaidMode.FullPayment;
        UpdateTotals();
    }

    private PosPaymentMethod SelectedPaymentMethod => _paymentMethod;

    private void OnCalculatorClick(object? sender, EventArgs e) => RunSafe(OpenCalculatorAsync);

    /// <summary>Opens the calculator and routes its result to the sales screen.</summary>
    private async Task OpenCalculatorAsync()
    {
        if (IsDisposedOrDisposing) return;

        // The calculator is the only consumer of the full ProductDto list, so
        // it is loaded on demand instead of on every screen open.
        if (_products.Count == 0)
        {
            _products = await _productService.GetAllAsync();
            if (IsDisposedOrDisposing) return;
        }

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

    // =========================
    // Checkout and printing
    // =========================

    private void OnCompleteSaleClick(object? sender, EventArgs e) => RunSafe(CompleteSaleAsync);

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
        var selectedCustomer = _cmbCustomer.SelectedItem as CustomerItem;
        var customerId = selectedCustomer?.Id ?? Guid.Empty;
        var customerName = selectedCustomer?.FullName ?? string.Empty;

        var totals = CalculateTotals();
        if (totals is null)
        {
            AppMessageBox.Warning(AppStrings.EmptyCart);
            return;
        }

        var paymentMethod = SelectedPaymentMethod;
        var paidAmount = PosPaymentPolicy.NormalizePaid(paymentMethod, totals.Total, (decimal)_numPaid.Value);

        var discountMode = _cmbDiscountMode.SelectedIndex == 1
            ? DiscountMode.FixedAmount
            : DiscountMode.Percent;

        try
        {
            if (discountMode == DiscountMode.Percent && _numDiscount.Value > 100)
            {
                AppMessageBox.Warning(AppStrings.DiscountExceedsLimit);
                return;
            }

            if (PosPaymentPolicy.RequiresCustomer(paymentMethod, totals.Total, paidAmount))
            {
                if (customerId == Guid.Empty)
                {
                    AppMessageBox.Warning(AppStrings.CreditSaleRequiresCustomer);
                    return;
                }

                var confirmed = AppMessageBox.Confirm(
                    string.Format(
                        AppStrings.CreditSaleConfirm,
                        customerName,
                        FormatMoney(totals.Total - paidAmount)),
                    AppStrings.CompleteSale);

                if (!confirmed)
                    return;
            }

            var openSession = await _cashSessionService.GetOpenSessionAsync();
            if (openSession == null)
            {
                // No drawer: make the reason visible and offer to open one. The
                // service still rejects the sale if the user cancels.
                await RefreshSessionStatusAsync();
                AppMessageBox.Warning(AppStrings.DrawerNotOpen);

                if (!OfferOpenDrawer())
                    return;

                openSession = await _cashSessionService.GetOpenSessionAsync();
                if (openSession == null)
                    return;

                await RefreshSessionStatusAsync();
            }

            var sale = await _saleService.CreateAsync(new CreateSaleDto
            {
                CustomerId = customerId,
                DiscountMode = discountMode,
                DiscountValue = (decimal)_numDiscount.Value,
                TaxRate = (decimal)_numTax.Value,
                Fees = (decimal)_numFees.Value,
                PaidAmount = paidAmount,
                PaymentMethod = PosPaymentPolicy.ToApiValue(paymentMethod),
                Items = PosCheckoutMapper.ToSaleItems(_cart)
            });

            _lastInvoice = sale;
            _btnPrintLast.Enabled = true;

            var deliveredCodes = sale.Items
                .Where(i => !string.IsNullOrWhiteSpace(i.DigitalCode))
                .Select(i => $"{i.VariantName}: {i.DigitalCode}")
                .ToList();

            if (deliveredCodes.Count > 0)
            {
                MessageBox.Show(
                    AppStrings.DigitalCodeDeliveryHint
                        + Environment.NewLine + Environment.NewLine
                        + string.Join(Environment.NewLine, deliveredCodes),
                    AppStrings.DigitalCodeDeliveredTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                AppMessageBox.Success(AppStrings.DataSaved);
            }

            _cart.Clear();
            _numDiscount.Value = 0;
            _numTax.Value = 0;
            _numFees.Value = 0;
            _numPaid.Value = 0;
            _cmbDiscountMode.SelectedIndex = 0;
            _paymentMethod = PosPaymentMethod.Cash;
            _numPaid.Enabled = true;
            _btnPayInFull.Enabled = true;
            _paidMode = PaidMode.FullPayment;
            UpdatePaymentButtons();
            RefreshCart();
            await LoadInvoicesAsync();
            _tabs.SelectedIndex = 1;
        }
        catch (Exception ex)
        {
            AppMessageBox.Error(ex, "CompleteSale");
        }
    }

    private void OnDeleteInvoiceClick(object? sender, EventArgs e) => RunSafe(DeleteInvoiceAsync);

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

    private void OnNewSaleClick(object? sender, EventArgs e) => _tabs.SelectedIndex = 0;

    private void OnClearCartClick(object? sender, EventArgs e) => ClearCart();

    private void OnScanBarcodeClick(object? sender, EventArgs e) => ScanBarcode();

    private void OnViewModeToggleClick(object? sender, EventArgs e)
    {
        _productList.ViewMode = _productList.ViewMode == PosViewMode.Cards
            ? PosViewMode.List
            : PosViewMode.Cards;

        _btnViewToggle.IconChar = _productList.ViewMode == PosViewMode.Cards
            ? FontAwesome.Sharp.IconChar.Th
            : FontAwesome.Sharp.IconChar.List;
    }

    private void OnSessionActionClicked(object? sender, EventArgs e) => RunSafe(OpenDrawerAsync);

    private void OnPrintLastClick(object? sender, EventArgs e) => PrintLastInvoice();

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
                    page.Header().Text($"{AppStrings.ReceiptInvoice} { _lastInvoice.InvoiceNumber }").SemiBold().FontSize(16);
                    page.Content().Column(col =>
                    {
                        col.Item().Text($"{AppStrings.Date}: {_lastInvoice.CreatedAt:yyyy-MM-dd HH:mm}");
                        col.Item().Text($"{AppStrings.Subtotal}: {_lastInvoice.Subtotal:N2} {CurrencyCode}");
                        col.Item().Text($"{AppStrings.Discount}: {_lastInvoice.DiscountDisplay} {CurrencyCode}");
                        col.Item().Text($"{AppStrings.TaxAmount}: {_lastInvoice.TaxAmount:N2} {CurrencyCode}");
                        col.Item().Text($"{AppStrings.Total}: {_lastInvoice.Total:N2} {CurrencyCode}");
                        col.Item().Text($"{AppStrings.PaidAmount}: {_lastInvoice.PaidAmount:N2} {CurrencyCode}");
                        col.Item().Text($"{AppStrings.RemainingAmount}: {_lastInvoice.RemainingAmount:N2} {CurrencyCode}");
                        col.Item().Text($"{AppStrings.Change}: {_lastInvoice.ChangeAmount:N2} {CurrencyCode}");
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

    /// <summary>Uniform customer entry so the combo always exposes a Guid value.</summary>
    private sealed record CustomerItem(Guid Id, string FullName);

    /// <summary>
    /// How the paid amount behaves: either mirror the total automatically, or
    /// use the value the cashier typed (<see cref="_paidMode"/>).
    /// </summary>
    private enum PaidMode
    {
        FullPayment,
        Custom
    }
}
