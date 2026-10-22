using DzentoPOS2.App.Services.Partners;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Partners;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Forms.Suppliers;

public class SupplierEditDialog : Form
{
    private readonly ISupplierService _supplierService;
    private readonly ILanguageService _languageService;
    private bool _languageSubscribed;
    private readonly SupplierDto? _existing;
    private readonly Label _lblName;
    private readonly Label _lblPhone;
    private readonly Label _lblCountry;
    private readonly Label _lblCity;
    private readonly Label _lblStreet;
    private readonly TextBox _txtName;
    private readonly TextBox _txtPhone;
    private readonly TextBox _txtCountry;
    private readonly TextBox _txtCity;
    private readonly TextBox _txtStreet;
    private readonly CheckBox _chkActive;
    private readonly Button _btnSave;
    private readonly Button _btnCancel;

    /// <summary>The supplier created by this dialog (null when cancelled or edited).</summary>
    public SupplierDto? CreatedSupplier { get; private set; }

    public SupplierEditDialog(ISupplierService supplierService, ILanguageService languageService, SupplierDto? existing = null)
    {
        _supplierService = supplierService;
        _languageService = languageService;
        _existing = existing;

        Size = new Size(460, 400);
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

        _lblName = lbl(AppStrings.SupplierName, 20);
        _lblPhone = lbl(AppStrings.Phone, 58);
        _lblCountry = lbl(AppStrings.Country, 96);
        _lblCity = lbl(AppStrings.City, 134);
        _lblStreet = lbl(AppStrings.Address, 172);

        _txtName = new TextBox { Location = new Point(140, 20), Size = new Size(280, 28), Font = new Font("Segoe UI", 10F) };
        _txtPhone = new TextBox { Location = new Point(140, 58), Size = new Size(280, 28), Font = new Font("Segoe UI", 10F) };
        _txtCountry = new TextBox { Location = new Point(140, 96), Size = new Size(280, 28), Font = new Font("Segoe UI", 10F) };
        _txtCity = new TextBox { Location = new Point(140, 134), Size = new Size(280, 28), Font = new Font("Segoe UI", 10F) };
        _txtStreet = new TextBox { Location = new Point(140, 172), Size = new Size(280, 28), Font = new Font("Segoe UI", 10F) };

        _chkActive = new CheckBox { Text = AppStrings.Active, Location = new Point(140, 210), Size = new Size(200, 28), Font = new Font("Segoe UI", 10F), Checked = true };

        _btnSave = ButtonFactory.Primary(AppStrings.Save);
        _btnSave.Location = new Point(140, 253);
        _btnSave.Size = new Size(120, 40);
        _btnSave.Click += async (_, _) => await SaveAsync();

        _btnCancel = ButtonFactory.Neutral(AppStrings.Cancel);
        _btnCancel.Location = new Point(280, 253);
        _btnCancel.Size = new Size(120, 40);
        _btnCancel.DialogResult = DialogResult.Cancel;

        Controls.AddRange(new Control[]
        {
            _lblName, _txtName,
            _lblPhone, _txtPhone,
            _lblCountry, _txtCountry,
            _lblCity, _txtCity,
            _lblStreet, _txtStreet,
            _chkActive,
            _btnSave, _btnCancel
        });

        if (existing != null)
        {
            _txtName.Text = existing.Name;
            _txtPhone.Text = existing.PhoneNumber ?? "";
            _txtCountry.Text = existing.Country ?? "";
            _txtCity.Text = existing.City ?? "";
            _txtStreet.Text = existing.Street ?? "";
            _chkActive.Checked = existing.IsActive;
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
        Text = _existing == null ? AppStrings.AddSupplier : AppStrings.EditSupplier;
        _lblName.Text = AppStrings.SupplierName;
        _lblPhone.Text = AppStrings.Phone;
        _lblCountry.Text = AppStrings.Country;
        _lblCity.Text = AppStrings.City;
        _lblStreet.Text = AppStrings.Address;
        _chkActive.Text = AppStrings.Active;
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
                CreatedSupplier = await _supplierService.CreateAsync(new CreateSupplierDto
                {
                    Name = _txtName.Text.Trim(),
                    PhoneNumber = _txtPhone.Text.Trim(),
                    Country = _txtCountry.Text.Trim(),
                    City = _txtCity.Text.Trim(),
                    Street = _txtStreet.Text.Trim(),
                    IsActive = _chkActive.Checked
                });
            }
            else
            {
                await _supplierService.UpdateAsync(new UpdateSupplierDto
                {
                    Id = _existing.Id,
                    Name = _txtName.Text.Trim(),
                    PhoneNumber = _txtPhone.Text.Trim(),
                    Country = _txtCountry.Text.Trim(),
                    City = _txtCity.Text.Trim(),
                    Street = _txtStreet.Text.Trim(),
                    IsActive = _chkActive.Checked
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