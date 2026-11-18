using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;
using DzentoPOS2.WinForms.Themes;
using DzentoPOS2.WinForms.Services.Barcodes;
using DzentoPOS2.WinForms.Controls;
using DzentoPOS2.WinForms.Helpers;

namespace DzentoPOS2.WinForms.Forms.Inventory;

public class ProductEditDialog : ThemedDialog
{
    private readonly IProductService _productService;
    private readonly ILanguageService _languageService;
    private bool _languageSubscribed;
    private readonly ProductDto? _existing;
    private readonly Label _lblName;
    private readonly Label _lblSKU;
    private readonly Label _lblBarcode;
    private readonly Label _lblBrand;
    private readonly Label _lblProductKind;
    private readonly Label _lblCategory;
    private readonly Label _lblUnit;
    private readonly Label _lblCost;
    private readonly Label _lblSale;
    private readonly Label _lblMinStock;
    private readonly Label _lblOpeningStock;
    private readonly TextBox _txtName;
    private readonly TextBox _txtSKU;
    private readonly TextBox _txtBarcode;
    private readonly TextBox _txtBrand;
    private readonly ComboBox _cmbProductKind;
    private readonly ComboBox _cmbCategory;
    private readonly ComboBox _cmbUnit;
    private readonly NumericUpDown _numCost;
    private readonly NumericUpDown _numSale;
    private readonly NumericUpDown _numMinStock;
    private readonly NumericUpDown _numOpeningStock;
    private readonly CheckBox _chkActive;
    private readonly Button _btnSave;
    private readonly Button _btnCancel;
    private readonly Button _btnImage = new() { Text = AppStrings.ChooseImage, Size = new Size(120, 32) };
    private readonly Button _btnBarcodes = new() { Text = AppStrings.Barcodes, Size = new Size(120, 32) };
    private readonly Button _btnVariants = new() { Text = AppStrings.Variants, Size = new Size(120, 32) };

    private readonly IBarcodeScanService? _scanService;
    private readonly DzentoPOS2.WinForms.Services.Images.IImageService? _imageService;
    private readonly IBarcodeService? _barcodeService;
    private readonly Microsoft.EntityFrameworkCore.IDbContextFactory<DzentoPOS2.Data.Context.AppDbContext>? _contextFactory;
    private readonly DzentoPOS2.App.Services.Systems.ICapabilityService? _capabilityService;
    private string? _imagePath;
    private string? _thumbnailPath;
    private readonly PictureBox _picImage = new() { Size = new Size(96, 96), SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle };

    public ProductEditDialog(IProductService productService, ILanguageService languageService, ProductDto? existing = null, IBarcodeScanService? scanService = null, DzentoPOS2.WinForms.Services.Images.IImageService? imageService = null, IBarcodeService? barcodeService = null, Microsoft.EntityFrameworkCore.IDbContextFactory<DzentoPOS2.Data.Context.AppDbContext>? contextFactory = null, DzentoPOS2.App.Services.Systems.ICapabilityService? capabilityService = null)
    {
        _productService = productService;
        _languageService = languageService;
        _existing = existing;
        _scanService = scanService;
        _imageService = imageService;
        _barcodeService = barcodeService;
        _contextFactory = contextFactory;
        _capabilityService = capabilityService;
        _imagePath = existing?.ImagePath;
        _thumbnailPath = existing?.ThumbnailPath;

        Size = new Size(520, 700);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = AppColors.Surface;

        var lbl = (string text, int y) => new Label
        {
            Text = text,
            Location = new Point(20, y),
            Size = new Size(120, 28),
            Font = new Font("Segoe UI", 10F),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _lblName = lbl(AppStrings.ProductLabel + ":", 20);
        _lblSKU = lbl(AppStrings.SKU, 58);
        _lblBarcode = lbl(AppStrings.BarcodeField + ":", 96);
        _lblCategory = lbl(AppStrings.CategoryFilter + ":", 134);
        _lblUnit = lbl(AppStrings.Unit + ":", 172);
        _lblCost = lbl(AppStrings.CostPrice + ":", 210);
        _lblSale = lbl(AppStrings.SalePrice + ":", 248);
        _lblMinStock = lbl(AppStrings.MinimumStock + ":", 286);
        _lblOpeningStock = lbl(AppStrings.OpeningStock + ":", 324);
        _lblBrand = lbl(AppStrings.BrandLabel, 362);
        _lblProductKind = lbl(AppStrings.ProductKindLabel, 400);

        _txtName = new TextBox { Location = new Point(150, 20), Size = new Size(330, 28), Font = new Font("Segoe UI", 10F) };
        _txtSKU = new TextBox { Location = new Point(150, 58), Size = new Size(330, 28), Font = new Font("Segoe UI", 10F) };
        _txtBarcode = new TextBox { Location = new Point(150, 96), Size = new Size(330, 28), Font = new Font("Segoe UI", 10F) };
        var _btnScanBarcode = new BarcodeScanButton { Location = new Point(485, 96), Size = new Size(28, 28), TargetTextBox = _txtBarcode, ScanService = _scanService };

        _cmbCategory = new ComboBox { Location = new Point(150, 134), Size = new Size(330, 28), Font = new Font("Segoe UI", 10F), DropDownStyle = ComboBoxStyle.DropDownList };
        _cmbUnit = new ComboBox { Location = new Point(150, 172), Size = new Size(330, 28), Font = new Font("Segoe UI", 10F), DropDownStyle = ComboBoxStyle.DropDownList };

        _numCost = new NumericUpDown { Location = new Point(150, 210), Size = new Size(330, 28), Font = new Font("Segoe UI", 10F), DecimalPlaces = 2, Maximum = 999999 };
        _numSale = new NumericUpDown { Location = new Point(150, 248), Size = new Size(330, 28), Font = new Font("Segoe UI", 10F), DecimalPlaces = 2, Maximum = 999999 };
        _numMinStock = new NumericUpDown { Location = new Point(150, 286), Size = new Size(330, 28), Font = new Font("Segoe UI", 10F), DecimalPlaces = 0, Maximum = 999999 };
        _numOpeningStock = new NumericUpDown { Location = new Point(150, 324), Size = new Size(330, 28), Font = new Font("Segoe UI", 10F), DecimalPlaces = 0, Maximum = 999999 };
        _txtBrand = new TextBox { Location = new Point(150, 362), Size = new Size(330, 28), Font = new Font("Segoe UI", 10F) };
        _cmbProductKind = new ComboBox { Location = new Point(150, 400), Size = new Size(330, 28), Font = new Font("Segoe UI", 10F), DropDownStyle = ComboBoxStyle.DropDownList };

        _chkActive = new CheckBox { Text = AppStrings.ProductActive, Location = new Point(150, 440), Size = new Size(200, 28), Font = new Font("Segoe UI", 10F), Checked = true };

        _picImage.Location = new Point(20, 480);
        _btnImage.Location = new Point(130, 510);
        _btnImage.Click += async (_, _) => await PickImageAsync();
        _btnBarcodes.Location = new Point(260, 510);
        _btnBarcodes.Click += async (_, _) => await ManageBarcodesAsync();
        _btnVariants.Location = new Point(390, 510);
        _btnVariants.Click += async (_, _) => await ManageVariantsAsync();

        _btnSave = ButtonFactory.Primary(AppStrings.Save);
        _btnSave.Location = new Point(150, 560);
        _btnSave.Size = new Size(120, 40);
        _btnSave.Click += async (_, _) => await SaveAsync();

        _btnCancel = ButtonFactory.Neutral(AppStrings.Cancel);
        _btnCancel.Location = new Point(290, 560);
        _btnCancel.Size = new Size(120, 40);
        _btnCancel.DialogResult = DialogResult.Cancel;

        if (_existing == null)
        {
            // New product: cannot manage variants/barcodes until it is saved.
            _btnBarcodes.Visible = false;
            _btnVariants.Visible = false;
        }
        else
        {
            _lblOpeningStock.Visible = false;
            _numOpeningStock.Visible = false;
        }

        Controls.AddRange(new Control[]
        {
            _lblName, _txtName,
            _lblSKU, _txtSKU,
            _lblBarcode, _txtBarcode, _btnScanBarcode,
            _lblCategory, _cmbCategory,
            _lblUnit, _cmbUnit,
            _lblCost, _numCost,
            _lblSale, _numSale,
            _lblMinStock, _numMinStock,
            _lblOpeningStock, _numOpeningStock,
            _lblBrand, _txtBrand,
            _lblProductKind, _cmbProductKind,
            _chkActive,
            _picImage, _btnImage, _btnBarcodes, _btnVariants,
            _btnSave, _btnCancel
        });

        LoadText();
        _languageService.LanguageChanged += OnLanguageChanged;
        _languageSubscribed = true;
    }

    private void OnLanguageChanged(object? sender, string culture)
    {
        if (IsDisposed) return;
        if (InvokeRequired) BeginInvoke(LoadText);
        else LoadText();
    }

    private void LoadText()
    {
        Text = _existing == null ? AppStrings.NewProduct : AppStrings.EditProductTitle;
        _lblName.Text = AppStrings.ProductLabel + ":";
        _lblBarcode.Text = AppStrings.BarcodeField + ":";
        _lblCategory.Text = AppStrings.CategoryFilter + ":";
        _lblUnit.Text = AppStrings.Unit + ":";
        _lblCost.Text = AppStrings.CostPrice + ":";
        _lblSale.Text = AppStrings.SalePrice + ":";
        _lblMinStock.Text = AppStrings.MinimumStock + ":";
        _lblOpeningStock.Text = AppStrings.OpeningStock + ":";
        _chkActive.Text = AppStrings.ProductActive;
        _btnSave.Text = AppStrings.Save;
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

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        try
        {
            await LoadLookupsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"{AppStrings.ErrorMsg}: {ex.Message}", AppStrings.ErrorMsg, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        if (_existing != null)
        {
            _txtName.Text = _existing.Name;
            _txtSKU.Text = _existing.SKU;
            _txtBarcode.Text = _existing.Barcode;
            _txtBrand.Text = _existing.Brand ?? "";
            _cmbProductKind.SelectedItem = _existing.ProductKind;
            _numCost.Value = _existing.CostPrice;
            if (!string.IsNullOrWhiteSpace(_existing.ThumbnailPath) && _imageService != null)
            {
                var img = await _imageService.LoadAsync(_existing.ImagePath, true);
                if (img != null) _picImage.Image = img;
            }
            _numSale.Value = _existing.SalePrice;
            _numMinStock.Value = _existing.MinimumStock;
            _chkActive.Checked = _existing.IsActive;
            SelectCombo(_cmbCategory, _existing.CategoryId);
            SelectCombo(_cmbUnit, _existing.UnitId);
        }
    }

    private async Task LoadLookupsAsync()
    {
        var categories = await _productService.GetCategoriesAsync();
        var categoryList = new List<ProductCategoryDto> { new() { Id = Guid.Empty, Name = AppStrings.SelectCategory } };
        categoryList.AddRange(categories);
        _cmbCategory.DataSource = categoryList;
        _cmbCategory.DisplayMember = "Name";
        _cmbCategory.ValueMember = "Id";
        _cmbCategory.SelectedIndex = 0;

        var units = await _productService.GetUnitsAsync();
        var unitList = new List<ProductUnitDto> { new() { Id = Guid.Empty, Name = AppStrings.SelectUnit } };
        unitList.AddRange(units);
        _cmbUnit.DataSource = unitList;
        _cmbUnit.DisplayMember = "Name";
        _cmbUnit.ValueMember = "Id";
        _cmbUnit.SelectedIndex = 0;

        _cmbProductKind.Items.Clear();
        // Filter product kinds based on branch capabilities.
        var caps = _capabilityService != null
            ? await _capabilityService.GetCapabilitiesAsync()
            : (DzentoPOS2.Core.Enums.BranchCapability)unchecked((int)0xFFFFFFFF); // all if no service

        foreach (DzentoPOS2.Core.Enums.ProductKind k in Enum.GetValues(typeof(DzentoPOS2.Core.Enums.ProductKind)))
        {
            bool allowed = k switch
            {
                DzentoPOS2.Core.Enums.ProductKind.Standard => true,
                DzentoPOS2.Core.Enums.ProductKind.Weighed => (caps & DzentoPOS2.Core.Enums.BranchCapability.WeightUnit) != 0,
                DzentoPOS2.Core.Enums.ProductKind.Serialized => (caps & DzentoPOS2.Core.Enums.BranchCapability.SerialNumber) != 0,
                DzentoPOS2.Core.Enums.ProductKind.Service => (caps & DzentoPOS2.Core.Enums.BranchCapability.NoStock) != 0,
                DzentoPOS2.Core.Enums.ProductKind.DigitalCode => (caps & DzentoPOS2.Core.Enums.BranchCapability.DigitalCode) != 0,
                _ => true
            };
            if (allowed)
                _cmbProductKind.Items.Add(k);
        }
        if (_cmbProductKind.Items.Count > 0)
            _cmbProductKind.SelectedIndex = 0;
    }

    private static void SelectCombo(ComboBox cmb, Guid id)
    {
        for (int i = 0; i < cmb.Items.Count; i++)
        {
            var item = cmb.Items[i];
            if (item is ProductCategoryDto cat && cat.Id == id)
            {
                cmb.SelectedIndex = i;
                return;
            }
            if (item is ProductUnitDto unit && unit.Id == id)
            {
                cmb.SelectedIndex = i;
                return;
            }
        }
    }

    private Task ManageVariantsAsync()
    {
        if (_existing == null || _contextFactory == null) return Task.CompletedTask;
        using var ctx = _contextFactory.CreateDbContext();
        using var dlg = new ProductVariantsDialog(ctx, _existing.Id);
        dlg.ShowDialog(this);
        return Task.CompletedTask;
    }

    private async Task ManageBarcodesAsync()
    {
        if (_existing == null || _barcodeService == null) return;
        var variantId = await _barcodeService.GetDefaultVariantIdAsync(_existing.Id);
        if (variantId == Guid.Empty) return;
        using var dlg = new ProductBarcodesDialog(_barcodeService, variantId);
        dlg.ShowDialog(this);
    }

    private async Task PickImageAsync()
    {
        if (_imageService == null) return;
        using var ofd = new OpenFileDialog { Filter = "Images|*.jpg;*.jpeg;*.png;*.bmp" };
        if (ofd.ShowDialog() != DialogResult.OK) return;
        try
        {
            var (img, thumb) = await _imageService.SaveAsync(ofd.FileName, "products");
            _imagePath = img;
            _thumbnailPath = thumb;
            var preview = await _imageService.LoadAsync(img, true);
            if (preview != null) _picImage.Image = preview;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, AppStrings.ErrorMsg, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_txtName.Text))
        {
            MessageBox.Show(AppStrings.EnterProductName, AppStrings.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var categoryId = _cmbCategory.SelectedValue as Guid? ?? Guid.Empty;
        var unitId = _cmbUnit.SelectedValue as Guid? ?? Guid.Empty;

        if (categoryId == Guid.Empty)
        {
            MessageBox.Show(AppStrings.SelectCategoryMsg, AppStrings.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (unitId == Guid.Empty)
        {
            MessageBox.Show(AppStrings.SelectUnitMsg, AppStrings.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_existing == null)
        {
            await _productService.CreateAsync(new CreateProductDto
            {
                Name = _txtName.Text.Trim(),
                SKU = _txtSKU.Text.Trim(),
                Barcode = _txtBarcode.Text.Trim(),
                Brand = _txtBrand.Text.Trim(),
                ProductKind = (DzentoPOS2.Core.Enums.ProductKind)(_cmbProductKind.SelectedItem ?? DzentoPOS2.Core.Enums.ProductKind.Standard),
                ImagePath = _imagePath,
                ThumbnailPath = _thumbnailPath,
                CategoryId = categoryId,
                UnitId = unitId,
                CostPrice = _numCost.Value,
                SalePrice = _numSale.Value,
                MinimumStock = _numMinStock.Value,
                OpeningStock = _numOpeningStock.Value,
                IsActive = _chkActive.Checked
            });
        }
        else
        {
            await _productService.UpdateAsync(new UpdateProductDto
            {
                Id = _existing.Id,
                Name = _txtName.Text.Trim(),
                SKU = _txtSKU.Text.Trim(),
                Barcode = _txtBarcode.Text.Trim(),
                Brand = _txtBrand.Text.Trim(),
                ProductKind = (DzentoPOS2.Core.Enums.ProductKind)(_cmbProductKind.SelectedItem ?? DzentoPOS2.Core.Enums.ProductKind.Standard),
                ImagePath = _imagePath,
                ThumbnailPath = _thumbnailPath,
                CategoryId = categoryId,
                UnitId = unitId,
                CostPrice = _numCost.Value,
                SalePrice = _numSale.Value,
                MinimumStock = _numMinStock.Value,
                IsActive = _chkActive.Checked
            });
        }

        DialogResult = DialogResult.OK;
        Close();
    }
}
