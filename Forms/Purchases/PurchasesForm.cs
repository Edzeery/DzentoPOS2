using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.App.Services.Partners;
using DzentoPOS2.App.Services.Purchases;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;
using DzentoPOS2.Shared.Contracts.DTOs.Partners;
using DzentoPOS2.Shared.Contracts.DTOs.Purchases;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;
using FontAwesome.Sharp;

namespace DzentoPOS2.WinForms.Forms.Purchases;

public partial class PurchasesForm : LocalizableFormBase
{
    private readonly IPurchaseService _purchaseService;
    private readonly IProductService _productService;
    private readonly ISupplierService _supplierService;

    private readonly Panel _topPanel;
    private readonly IconButton _btnNewPurchase;
    private readonly IconButton _btnDelete;
    private readonly DataGridView _grid;
    private readonly DataGridView _detailGrid;
    private readonly SplitContainer _splitContainer;

    private List<PurchaseInvoiceDto> _invoices = new();
    private List<ProductDto> _products = new();
    private List<SupplierDto> _suppliers = new();

    public PurchasesForm(
        IPurchaseService purchaseService,
        IProductService productService,
        ISupplierService supplierService,
        ILanguageService languageService)
        : base(languageService)
    {
        _purchaseService = purchaseService;
        _productService = productService;
        _supplierService = supplierService;

        Size = new Size(1100, 650);
        BackColor = AppColors.Background;

        _topPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = AppColors.Surface,
            Padding = new Padding(16, 10, 16, 10)
        };

        _btnNewPurchase = ButtonFactory.Tool(IconChar.Plus, AppStrings.NewPurchase, AppColors.Primary, ButtonFactory.ButtonSize.Medium);
        _btnNewPurchase.Location = new Point(16, 12);
        _btnNewPurchase.Click += async (_, _) => await AddPurchase();

        _btnDelete = ButtonFactory.ToolDelete(AppStrings.Delete, ButtonFactory.ButtonSize.Medium);
        _btnDelete.Location = new Point(188, 12);
        _btnDelete.Click += async (_, _) => await DeletePurchase();

        _topPanel.Controls.AddRange(new Control[] { _btnDelete, _btnNewPurchase });

        _splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 300
        };

        _grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, MultiSelect = false };
        DataGridViewStyleHelper.ApplyModernStyle(_grid);
        _grid.SelectionChanged += (_, _) => ShowDetails();

        _detailGrid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, MultiSelect = false };
        DataGridViewStyleHelper.ApplyModernStyle(_detailGrid);

        _splitContainer.Panel1.Controls.Add(_grid);
        _splitContainer.Panel2.Controls.Add(_detailGrid);

        Controls.Add(_splitContainer);
        Controls.Add(_topPanel);
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        _products = await _productService.GetAllAsync();
        _suppliers = await _supplierService.GetAllAsync();
        await LoadDataAsync();
    }

    protected override void LoadText()
    {
        Text = AppStrings.Purchases;
        _btnNewPurchase.Text = "  " + AppStrings.NewPurchase;
        _btnDelete.Text = "  " + AppStrings.Delete;
    }

    private async Task LoadDataAsync()
    {
        _invoices = await _purchaseService.GetAllAsync();

        _grid.DataSource = null;
        _grid.Columns.Clear();
        _grid.AutoGenerateColumns = false;
        _grid.Columns.AddRange(new[]
        {
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.InvoiceNumber, DataPropertyName = "InvoiceNumber", Width = 160 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.SupplierName, DataPropertyName = "SupplierName", Width = 160 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Total, DataPropertyName = "Total", Width = 110, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.PaidAmount, DataPropertyName = "PaidAmount", Width = 110, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.RemainingAmount, DataPropertyName = "RemainingAmount", Width = 110, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Date, DataPropertyName = "CreatedAt", Width = 130, DefaultCellStyle = { Format = "yyyy/MM/dd HH:mm" } }
        });
        _grid.DataSource = _invoices;
    }

    private void ShowDetails()
    {
        if (_grid.CurrentRow?.DataBoundItem is not PurchaseInvoiceDto invoice)
        {
            _detailGrid.DataSource = null;
            return;
        }

        var details = invoice.Items.Select(i =>
        {
            var product = _products.FirstOrDefault(p => p.Id == i.ProductId);
            return new
            {
                ProductName = product?.Name ?? "",
                SKU = product?.SKU ?? "",
                i.Quantity,
                i.CostPrice,
                i.Total
            };
        }).ToList();

        _detailGrid.DataSource = null;
        _detailGrid.Columns.Clear();
        _detailGrid.AutoGenerateColumns = false;
        _detailGrid.Columns.AddRange(new[]
        {
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.ProductLabel, DataPropertyName = "ProductName", Width = 200 },
            new DataGridViewTextBoxColumn { HeaderText = "SKU", DataPropertyName = "SKU", Width = 100 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Quantity, DataPropertyName = "Quantity", Width = 80 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.CostPrice, DataPropertyName = "CostPrice", Width = 100, DefaultCellStyle = { Format = "N2" } },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Total, DataPropertyName = "Total", Width = 100, DefaultCellStyle = { Format = "N2" } }
        });
        _detailGrid.DataSource = details;
    }

    private async Task AddPurchase()
    {
        using var dialog = new PurchaseEditDialog(_purchaseService, _productService, _supplierService, _languageService);
        if (dialog.ShowDialog() == DialogResult.OK)
            await LoadDataAsync();
    }

    private async Task DeletePurchase()
    {
        if (_grid.CurrentRow?.DataBoundItem is not PurchaseInvoiceDto invoice) return;
        var result = MessageBox.Show(
            $"{AppStrings.DeletePurchaseConfirm}\n{invoice.InvoiceNumber}",
            AppStrings.DeleteConfirmation,
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (result == DialogResult.Yes)
        {
            await _purchaseService.DeleteAsync(invoice.Id);
            await LoadDataAsync();
        }
    }
}
