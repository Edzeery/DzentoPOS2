using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Forms.Inventory;

public class ProductCategoryEditDialog : Form
{
    private readonly IProductCategoryService _categoryService;
    private readonly ILanguageService _languageService;
    private bool _languageSubscribed;
    private readonly ProductCategoryDto? _existing;
    private readonly Label _lblName;
    private readonly Label _lblDescription;
    private readonly TextBox _txtName;
    private readonly TextBox _txtDescription;
    private readonly CheckBox _chkActive;
    private readonly Button _btnSave;
    private readonly Button _btnCancel;

    public ProductCategoryEditDialog(
        IProductCategoryService categoryService,
        ILanguageService languageService,
        ProductCategoryDto? existing = null)
    {
        _categoryService = categoryService;
        _languageService = languageService;
        _existing = existing;

        Size = new Size(520, 330);
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

        _lblName = lbl(AppStrings.CategoryName, 20);
        _lblDescription = lbl(AppStrings.Description, 58);

        _txtName = new TextBox { Location = new Point(140, 20), Size = new Size(340, 28), Font = new Font("Segoe UI", 10F) };
        _txtDescription = new TextBox { Location = new Point(140, 58), Size = new Size(340, 28), Font = new Font("Segoe UI", 10F) };

        _chkActive = new CheckBox { Text = AppStrings.Active, Location = new Point(140, 96), Size = new Size(200, 28), Font = new Font("Segoe UI", 10F), Checked = true };

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
            _lblDescription, _txtDescription,
            _chkActive,
            _btnSave, _btnCancel
        });

        if (existing != null)
        {
            _txtName.Text = existing.Name;
            _txtDescription.Text = existing.Description ?? "";
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
        Text = _existing == null ? AppStrings.AddCategory : AppStrings.EditCategory;
        _lblName.Text = AppStrings.CategoryName;
        _lblDescription.Text = AppStrings.Description;
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
                await _categoryService.CreateAsync(new CreateProductCategoryDto
                {
                    Name = _txtName.Text.Trim(),
                    Description = _txtDescription.Text.Trim(),
                    IsActive = _chkActive.Checked
                });
            }
            else
            {
                await _categoryService.UpdateAsync(new UpdateProductCategoryDto
                {
                    Id = _existing.Id,
                    Name = _txtName.Text.Trim(),
                    Description = _txtDescription.Text.Trim(),
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
