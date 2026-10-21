using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Forms.Inventory;

public class ProductEditDialog : Form
{
    private readonly IProductService _productService;
    private readonly ILanguageService _languageService;
    private bool _languageSubscribed;
    private readonly ProductDto? _existing;
    private readonly Label _lblName;
    private readonly Label _lblSKU;
    private readonly Label _lblCategory;
    private readonly Label _lblUnit;
    private readonly Label _lblCost;
    private readonly Label _lblSale;
    private readonly Label _lblMinStock;
    private readonly TextBox _txtName;
    private readonly TextBox _txtSKU;
    private readonly ComboBox _cmbCategory;
    private readonly ComboBox _cmbUnit;
    private readonly NumericUpDown _numCost;
    private readonly NumericUpDown _numSale;
    private readonly NumericUpDown _numMinStock;
    private readonly CheckBox _chkActive;
    private readonly Button _btnSave;
    private readonly Button _btnCancel;

    public ProductEditDialog(IProductService productService, ILanguageService languageService, ProductDto? existing = null)
    {
        _productService = productService;
        _languageService = languageService;
        _existing = existing;

        Size = new Size(520, 460);
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
        _lblSKU = lbl("SKU:", 58);
        _lblCategory = lbl(AppStrings.CategoryFilter + ":", 96);
        _lblUnit = lbl(AppStrings.Unit + ":", 134);
        _lblCost = lbl(AppStrings.CostPrice + ":", 172);
        _lblSale = lbl(AppStrings.SalePrice + ":", 210);
        _lblMinStock = lbl(AppStrings.MinimumStock + ":", 248);

        _txtName = new TextBox { Location = new Point(150, 20), Size = new Size(330, 28), Font = new Font("Segoe UI", 10F) };
        _txtSKU = new TextBox { Location = new Point(150, 58), Size = new Size(330, 28), Font = new Font("Segoe UI", 10F) };

        _cmbCategory = new ComboBox { Location = new Point(150, 96), Size = new Size(330, 28), Font = new Font("Segoe UI", 10F), DropDownStyle = ComboBoxStyle.DropDownList };
        _cmbUnit = new ComboBox { Location = new Point(150, 134), Size = new Size(330, 28), Font = new Font("Segoe UI", 10F), DropDownStyle = ComboBoxStyle.DropDownList };

        _numCost = new NumericUpDown { Location = new Point(150, 172), Size = new Size(330, 28), Font = new Font("Segoe UI", 10F), DecimalPlaces = 2, Maximum = 999999 };
        _numSale = new NumericUpDown { Location = new Point(150, 210), Size = new Size(330, 28), Font = new Font("Segoe UI", 10F), DecimalPlaces = 2, Maximum = 999999 };
        _numMinStock = new NumericUpDown { Location = new Point(150, 248), Size = new Size(330, 28), Font = new Font("Segoe UI", 10F), DecimalPlaces = 0, Maximum = 999999 };

        _chkActive = new CheckBox { Text = AppStrings.ProductActive, Location = new Point(150, 286), Size = new Size(200, 28), Font = new Font("Segoe UI", 10F), Checked = true };

        _btnSave = ButtonFactory.Primary(AppStrings.Save);
        _btnSave.Location = new Point(150, 330);
        _btnSave.Size = new Size(120, 40);
        _btnSave.Click += async (_, _) => await SaveAsync();

        _btnCancel = ButtonFactory.Neutral(AppStrings.Cancel);
        _btnCancel.Location = new Point(290, 330);
        _btnCancel.Size = new Size(120, 40);
        _btnCancel.DialogResult = DialogResult.Cancel;

        Controls.AddRange(new Control[]
        {
            _lblName, _txtName,
            _lblSKU, _txtSKU,
            _lblCategory, _cmbCategory,
            _lblUnit, _cmbUnit,
            _lblCost, _numCost,
            _lblSale, _numSale,
            _lblMinStock, _numMinStock,
            _chkActive,
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
        _lblCategory.Text = AppStrings.CategoryFilter + ":";
        _lblUnit.Text = AppStrings.Unit + ":";
        _lblCost.Text = AppStrings.CostPrice + ":";
        _lblSale.Text = AppStrings.SalePrice + ":";
        _lblMinStock.Text = AppStrings.MinimumStock + ":";
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
            _numCost.Value = _existing.CostPrice;
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
                CategoryId = categoryId,
                UnitId = unitId,
                CostPrice = _numCost.Value,
                SalePrice = _numSale.Value,
                MinimumStock = _numMinStock.Value,
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
