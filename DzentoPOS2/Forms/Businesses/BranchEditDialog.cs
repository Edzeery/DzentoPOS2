using DzentoPOS2.App.Services.Businesses;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Businesses;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Forms.Businesses;

/// <summary>
/// Create/edit dialog for a branch. Only the descriptive fields are edited here;
/// the default warehouse and cashbox are provisioned by <see cref="IBranchService"/>
/// when a branch is created, and activation is managed from the list screen.
/// </summary>
public class BranchEditDialog : ThemedDialog
{
    private readonly IBranchService _branchService;
    private readonly ILanguageService _languageService;
    private bool _languageSubscribed;
    private readonly BranchDto? _existing;
    private readonly Label _lblName;
    private readonly Label _lblCity;
    private readonly Label _lblStreet;
    private readonly Label _lblPhone;
    private readonly TextBox _txtName;
    private readonly TextBox _txtCity;
    private readonly TextBox _txtStreet;
    private readonly TextBox _txtPhone;
    private readonly CheckBox _chkActive;
    private readonly Button _btnSave;
    private readonly Button _btnCancel;

    public BranchEditDialog(
        IBranchService branchService,
        ILanguageService languageService,
        BranchDto? existing = null)
    {
        _branchService = branchService;
        _languageService = languageService;
        _existing = existing;

        Size = new Size(560, 460);
        MinimumSize = new Size(340, 360);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = AppColors.Surface;
        AutoScaleMode = AutoScaleMode.Font;

        _lblName = FieldLabel(AppStrings.BranchName);
        _lblCity = FieldLabel(AppStrings.City);
        _lblStreet = FieldLabel(AppStrings.BranchStreet);
        _lblPhone = FieldLabel(AppStrings.BranchPhone);

        _txtName = FieldBox();
        _txtCity = FieldBox();
        _txtStreet = FieldBox();
        _txtPhone = FieldBox();

        _chkActive = new CheckBox
        {
            Text = AppStrings.Active,
            Dock = DockStyle.Fill,
            AutoSize = false,
            Height = 32,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 8, 0, 8)
        };

        _btnSave = ButtonFactory.Primary(AppStrings.Save);
        _btnSave.AutoSize = true;
        _btnSave.MinimumSize = new Size(120, 40);
        _btnSave.Margin = new Padding(0, 0, 8, 0);
        _btnSave.Click += async (_, _) => await SaveAsync();

        _btnCancel = ButtonFactory.Neutral(AppStrings.Cancel);
        _btnCancel.AutoSize = true;
        _btnCancel.MinimumSize = new Size(120, 40);
        _btnCancel.DialogResult = DialogResult.Cancel;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 7,
            Padding = new Padding(20, 16, 20, 16),
            Margin = Padding.Empty
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        root.Controls.Add(_lblName, 0, 0);
        root.Controls.Add(_txtName, 1, 0);
        root.Controls.Add(_lblCity, 0, 1);
        root.Controls.Add(_txtCity, 1, 1);
        root.Controls.Add(_lblStreet, 0, 2);
        root.Controls.Add(_txtStreet, 1, 2);
        root.Controls.Add(_lblPhone, 0, 3);
        root.Controls.Add(_txtPhone, 1, 3);
        root.Controls.Add(_chkActive, 0, 4);
        root.SetColumnSpan(_chkActive, 2);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = true,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 8, 0, 0)
        };
        buttons.Controls.Add(_btnCancel);
        buttons.Controls.Add(_btnSave);
        root.Controls.Add(buttons, 0, 6);
        root.SetColumnSpan(buttons, 2);

        Controls.Add(root);

        if (existing != null)
        {
            _txtName.Text = existing.Name;
            _txtCity.Text = existing.City ?? string.Empty;
            _txtStreet.Text = existing.Street ?? string.Empty;
            _txtPhone.Text = existing.PhoneNumber ?? string.Empty;
            _chkActive.Checked = existing.IsActive;
        }
        else
        {
            _chkActive.Checked = true;
        }

        LoadText();
        _languageService.LanguageChanged += OnLanguageChanged;
        _languageSubscribed = true;
    }

    private static Label FieldLabel(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Anchor = AnchorStyles.Left,
        Margin = new Padding(0, 6, 12, 0)
    };

    private static TextBox FieldBox() => new()
    {
        Dock = DockStyle.Fill,
        MinimumSize = new Size(0, 28),
        Margin = new Padding(0, 6, 0, 6)
    };

    private void OnLanguageChanged(object? sender, string culture)
    {
        if (IsDisposed) return;
        if (InvokeRequired) BeginInvoke(LoadText);
        else LoadText();
    }

    private void LoadText()
    {
        Text = _existing == null ? AppStrings.AddBranch : AppStrings.EditBranch;
        _lblName.Text = AppStrings.BranchName;
        _lblCity.Text = AppStrings.City;
        _lblStreet.Text = AppStrings.BranchStreet;
        _lblPhone.Text = AppStrings.BranchPhone;
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
            MessageBox.Show(
                AppStrings.BranchNameRequiredMsg,
                AppStrings.Warning,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        try
        {
            if (_existing == null)
            {
                await _branchService.CreateAsync(new CreateBranchDto
                {
                    Name = _txtName.Text.Trim(),
                    City = _txtCity.Text.Trim(),
                    Street = _txtStreet.Text.Trim(),
                    PhoneNumber = _txtPhone.Text.Trim(),
                    IsActive = _chkActive.Checked
                });
            }
            else
            {
                await _branchService.UpdateAsync(new UpdateBranchDto
                {
                    Id = _existing.Id,
                    Name = _txtName.Text.Trim(),
                    City = _txtCity.Text.Trim(),
                    Street = _txtStreet.Text.Trim(),
                    PhoneNumber = _txtPhone.Text.Trim()
                });
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            AppMessageBox.Error(ex);
        }
    }
}
