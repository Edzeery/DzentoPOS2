using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Forms.Inventory;

public class ProductUnitEditDialog : Form
{
    private readonly IProductUnitService _unitService;
    private readonly ILanguageService _languageService;
    private bool _languageSubscribed;
    private readonly ProductUnitDto? _existing;
        private readonly Label _lblName;
        private readonly Label _lblSymbol;
        private readonly TextBox _txtName;
        private readonly TextBox _txtSymbol;
        private readonly CheckBox _chkBaseUnit;
        private readonly Button _btnSave;
        private readonly Button _btnCancel;

    public ProductUnitEditDialog(
        IProductUnitService unitService,
        ILanguageService languageService,
        ProductUnitDto? existing = null)
    {
        _unitService = unitService;
        _languageService = languageService;
        _existing = existing;

        Size = new Size(520, 380);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = AppColors.Surface;

        var lbl = (string text, int y) => new Label
        {
            Text = text,
            Location = new Point(20, y),
            Size = new Size(110, 28),
            Font = new Font("Segoe UI", 10F),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _lblName = lbl(AppStrings.UnitName, 20);
        _lblSymbol = lbl(AppStrings.Symbol, 58);

        _txtName = new TextBox { Location = new Point(140, 20), Size = new Size(340, 28), Font = new Font("Segoe UI", 10F) };
        _txtSymbol = new TextBox { Location = new Point(140, 58), Size = new Size(340, 28), Font = new Font("Segoe UI", 10F) };

        _chkBaseUnit = new CheckBox { Text = AppStrings.BaseUnit, Location = new Point(140, 96), Size = new Size(200, 28), Font = new Font("Segoe UI", 10F), Checked = true };

        _btnSave = ButtonFactory.Primary(AppStrings.Save);
        _btnSave.Location = new Point(140, 136);
        _btnSave.Size = new Size(120, 40);
        _btnSave.Click += async (_, _) => await SaveAsync();

        _btnCancel = ButtonFactory.Neutral(AppStrings.Cancel);
        _btnCancel.Location = new Point(280, 136);
        _btnCancel.Size = new Size(120, 40);
        _btnCancel.DialogResult = DialogResult.Cancel;

        Controls.AddRange(new Control[]
        {
            _lblName, _txtName,
            _lblSymbol, _txtSymbol,
            _chkBaseUnit,
            _btnSave, _btnCancel
        });

        if (existing != null)
        {
            _txtName.Text = existing.Name;
            _txtSymbol.Text = existing.Symbol;
            _chkBaseUnit.Checked = existing.IsBaseUnit;
        }

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
        Text = _existing == null ? AppStrings.AddUnit : AppStrings.EditUnit;
        _lblName.Text = AppStrings.UnitName;
        _lblSymbol.Text = AppStrings.Symbol;
        _chkBaseUnit.Text = AppStrings.BaseUnit;
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

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_txtName.Text))
        {
            MessageBox.Show(AppStrings.ErrorMsg, AppStrings.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            if (_existing == null)
            {
                await _unitService.CreateAsync(new CreateProductUnitDto
                {
                    Name = _txtName.Text.Trim(),
                    Symbol = _txtSymbol.Text.Trim(),
                    IsBaseUnit = _chkBaseUnit.Checked
                });
            }
            else
            {
            await _unitService.UpdateAsync(new UpdateProductUnitDto
            {
                Id = _existing.Id,
                Name = _txtName.Text.Trim(),
                Symbol = _txtSymbol.Text.Trim(),
                IsBaseUnit = _chkBaseUnit.Checked
            });
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, AppStrings.ErrorMsg, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
