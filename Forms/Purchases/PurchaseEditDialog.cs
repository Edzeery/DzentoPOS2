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
using System.Reflection;

namespace DzentoPOS2.WinForms.Forms.Purchases;

public class PurchaseEditDialog : Form
{
    private readonly IPurchaseService _purchaseService;
    private readonly IProductService _productService;
    private readonly ISupplierService _supplierService;
    private readonly ILanguageService _languageService;
    private bool _languageSubscribed;

    private readonly Label _lblSupplier;
    private readonly ComboBox _cmbSupplier;
    private readonly Label _lblPaid;
    private readonly NumericUpDown _numPaid;
    private readonly Label _lblProduct;
    private readonly ComboBox _cmbProduct;
    private readonly Label _lblQty;
    private readonly NumericUpDown _numQty;
    private readonly Label _lblCost;
    private readonly NumericUpDown _numCost;
    private readonly Button _btnAddItem;
    private readonly DataGridView _itemsGrid;
    private readonly Label _lblTotal;
    private readonly Label _lblTotalValue;
    private readonly Button _btnSave;
    private readonly Button _btnCancel;

    private readonly List<PurchaseLineItem> _items = new();
    private List<ProductDto> _products = new();
    private List<SupplierDto> _suppliers = new();

    private class PurchaseLineItem
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public decimal Quantity { get; set; }
        public decimal CostPrice { get; set; }
        public decimal Total => Quantity * CostPrice;
    }

    public PurchaseEditDialog(
        IPurchaseService purchaseService,
        IProductService productService,
        ISupplierService supplierService,
        ILanguageService languageService)
    {
        _purchaseService = purchaseService;
        _productService = productService;
        _supplierService = supplierService;
        _languageService = languageService;

        Size = new Size(600, 520);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = AppColors.Surface;

        _lblSupplier = MakeLabel(AppStrings.SelectSupplier, 20, 15);
        _cmbSupplier = new ComboBox { Location = new Point(140, 13), Size = new Size(420, 28), Font = new Font("Segoe UI", 10F), DropDownStyle = ComboBoxStyle.DropDownList };

        _lblPaid = MakeLabel(AppStrings.PaidAmount, 20, 53);
        _numPaid = new NumericUpDown { Location = new Point(140, 51), Size = new Size(200, 28), Font = new Font("Segoe UI", 10F), DecimalPlaces = 2, Maximum = 99999999 };

        _lblProduct = MakeLabel(AppStrings.ProductLabel, 20, 93);
        _cmbProduct = new ComboBox { Location = new Point(140, 91), Size = new Size(420, 28), Font = new Font("Segoe UI", 10F), DropDownStyle = ComboBoxStyle.DropDownList };

        _lblQty = MakeLabel(AppStrings.Quantity, 20, 131);
        _numQty = new NumericUpDown { Location = new Point(140, 129), Size = new Size(100, 28), Font = new Font("Segoe UI", 10F), DecimalPlaces = 2, Minimum = 0.01m, Maximum = 99999, Value = 1 };

        _lblCost = MakeLabel(AppStrings.CostPrice, 250, 131);
        _numCost = new NumericUpDown { Location = new Point(370, 129), Size = new Size(100, 28), Font = new Font("Segoe UI", 10F), DecimalPlaces = 2, Maximum = 999999 };

        _btnAddItem = ButtonFactory.Primary(AppStrings.Add, ButtonFactory.ButtonSize.Small);
        _btnAddItem.Location = new Point(480, 129);
        _btnAddItem.Size = new Size(80, 28);
        _btnAddItem.Font = new Font("Segoe UI", 9F);
        _btnAddItem.Click += (_, _) => AddItem();

        _itemsGrid = new DataGridView
        {
            Location = new Point(20, 170),
            Size = new Size(540, 180),
            ReadOnly = true,
            MultiSelect = false,
            AllowUserToDeleteRows = false
        };

        _lblTotal = MakeLabel(AppStrings.GrandTotal + ":", 20, 360);
        _lblTotalValue = new Label
        {
            Text = "0.00",
            Location = new Point(460, 360),
            Size = new Size(100, 25),
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = AppColors.Primary,
            TextAlign = ContentAlignment.MiddleRight
        };

        _btnSave = ButtonFactory.Primary(AppStrings.CompletePurchase);
        _btnSave.Location = new Point(360, 400);
        _btnSave.Size = new Size(200, 40);
        _btnSave.Click += async (_, _) => await SaveAsync();

        _btnCancel = ButtonFactory.Neutral(AppStrings.Cancel);
        _btnCancel.Location = new Point(20, 400);
        _btnCancel.Size = new Size(120, 40);
        _btnCancel.DialogResult = DialogResult.Cancel;

        Controls.AddRange(new Control[]
        {
            _lblSupplier, _cmbSupplier,
            _lblPaid, _numPaid,
            _lblProduct, _cmbProduct,
            _lblQty, _numQty,
            _lblCost, _numCost,
            _btnAddItem,
            _itemsGrid,
            _lblTotal, _lblTotalValue,
            _btnSave, _btnCancel
        });

        _cmbProduct.SelectedIndexChanged += (_, _) =>
        {
            if (_cmbProduct.SelectedItem is ProductDto p)
                _numCost.Value = p.CostPrice;
        };

        LoadText();
        _languageService.LanguageChanged += OnLanguageChanged;
        _languageSubscribed = true;
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        _products = await _productService.GetAllAsync();
        _suppliers = await _supplierService.GetAllAsync();

        _cmbSupplier.Items.Clear();
        _cmbSupplier.Items.Add(new { Id = Guid.Empty, Name = AppStrings.SelectSupplier });
        foreach (var s in _suppliers)
            _cmbSupplier.Items.Add(s);
        _cmbSupplier.DisplayMember = "Name";
        _cmbSupplier.ValueMember = "Id";
        _cmbSupplier.SelectedIndex = 0;

        _cmbProduct.Items.Clear();
        _cmbProduct.Items.Add(new { Id = Guid.Empty, Name = AppStrings.SearchProducts });
        foreach (var p in _products)
            _cmbProduct.Items.Add(p);
        _cmbProduct.DisplayMember = "Name";
        _cmbProduct.ValueMember = "Id";
        _cmbProduct.SelectedIndex = 0;

        RefreshItemsGrid();
    }

    private void OnLanguageChanged(object? sender, string culture)
    {
        if (IsDisposed) return;
        if (InvokeRequired) BeginInvoke(LoadText);
        else LoadText();
    }

    private void LoadText()
    {
        Text = AppStrings.NewPurchase;
        _lblSupplier.Text = AppStrings.SelectSupplier;
        _lblPaid.Text = AppStrings.PaidAmount;
        _lblProduct.Text = AppStrings.ProductLabel;
        _lblQty.Text = AppStrings.Quantity;
        _lblCost.Text = AppStrings.CostPrice;
        _lblTotal.Text = AppStrings.GrandTotal + ":";
        _btnSave.Text = AppStrings.CompletePurchase;
        _btnCancel.Text = AppStrings.Cancel;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _languageSubscribed)
        {
            _languageService.LanguageChanged -= OnLanguageChanged;
            _languageSubscribed = false;
        }
        base.Dispose(disposing);
    }

    private void AddItem()
    {
        if (_cmbProduct.SelectedItem is not ProductDto product || product.Id == Guid.Empty)
            return;

        var existing = _items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existing != null)
        {
            existing.Quantity += _numQty.Value;
        }
        else
        {
            _items.Add(new PurchaseLineItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = _numQty.Value,
                CostPrice = _numCost.Value
            });
        }

        _numQty.Value = 1;
        RefreshItemsGrid();
    }

    private void RefreshItemsGrid()
    {
        _itemsGrid.DataSource = null;
        _itemsGrid.Columns.Clear();
        _itemsGrid.AutoGenerateColumns = false;
        _itemsGrid.Columns.AddRange(new[]
        {
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.ProductLabel, DataPropertyName = "ProductName", Width = 200 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Quantity, DataPropertyName = "Quantity", Width = 80 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.CostPrice, DataPropertyName = "CostPrice", Width = 100, DefaultCellStyle = { Format = "N2" } },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Total, DataPropertyName = "Total", Width = 100, DefaultCellStyle = { Format = "N2" } }
        });
        _itemsGrid.DataSource = _items.ToList();

        _lblTotalValue.Text = _items.Sum(i => i.Total).ToString("N2");
    }

    private async Task SaveAsync()
    {
        // The drop-down is filled by hand, so SelectedValue is not bound to
        // the entry and stays null even when an item is selected.
        var supplierId = SelectedId(_cmbSupplier);

        if (supplierId == Guid.Empty)
        {
            MessageBox.Show(AppStrings.SelectSupplier, AppStrings.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_items.Count == 0)
        {
            AppMessageBox.Warning(AppStrings.EmptyCart);
            return;
        }

        try
        {
            await _purchaseService.CreateAsync(new CreatePurchaseInvoiceDto
            {
                SupplierId = supplierId,
                PaidAmount = (decimal)_numPaid.Value,
                Items = _items.Select(i => new CreatePurchaseItemDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    CostPrice = i.CostPrice
                }).ToList()
            });

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            AppMessageBox.Error(ex.Message);
        }
    }

    /// <summary>
    /// Identifier of the selected entry of a drop-down that is filled by hand.
    /// Such a combo has no data source, so the value is read from the item.
    /// </summary>
    private static Guid SelectedId(ComboBox combo)
    {
        object? item = combo.SelectedItem;
        if (item == null) return Guid.Empty;

        PropertyInfo? property = item.GetType().GetProperty("Id");
        return property?.GetValue(item) is Guid id ? id : Guid.Empty;
    }

    private static Label MakeLabel(string text, int x, int y) => new()
    {
        Text = text,
        Location = new Point(x, y),
        Size = new Size(120, 22),
        Font = new Font("Segoe UI", 9F),
        TextAlign = ContentAlignment.MiddleLeft
    };
}
