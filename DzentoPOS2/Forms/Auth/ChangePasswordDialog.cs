using DzentoPOS2.App.Services.Identity.Interfaces;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Identity.Auth;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;
using FluentValidation;

namespace DzentoPOS2.WinForms.Forms.Auth;

/// <summary>
/// Blocking dialog shown right after a sign-in whose account still uses a
/// seeded/assigned password (<see cref="App.Session.UserSession.MustChangePassword"/>).
/// The shell is not opened until the password has been changed.
/// </summary>
public sealed class ChangePasswordDialog : Form
{
    private readonly IAuthService _authService;
    private readonly IValidator<ChangePasswordDto> _validator;
    private readonly ILanguageService _languageService;
    private readonly Guid _userId;
    private bool _languageSubscribed;

    private readonly Label _lblCurrent;
    private readonly Label _lblNew;
    private readonly Label _lblConfirm;
    private readonly Label _lblHint;
    private readonly TextBox _txtCurrent;
    private readonly TextBox _txtNew;
    private readonly TextBox _txtConfirm;
    private readonly Button _btnSave;
    private readonly Button _btnCancel;

    public ChangePasswordDialog(
        IAuthService authService,
        IValidator<ChangePasswordDto> validator,
        ILanguageService languageService,
        Guid userId)
    {
        _authService = authService;
        _validator = validator;
        _languageService = languageService;
        _userId = userId;

        Size = new Size(560, 400);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = AppColors.Surface;

        var lbl = (int y) => new Label
        {
            Location = new Point(24, y),
            Size = new Size(150, 28),
            Font = new Font("Segoe UI", 10F),
            TextAlign = ContentAlignment.MiddleLeft
        };

        var passwordBox = (int y) => new TextBox
        {
            Location = new Point(190, y),
            Size = new Size(330, 28),
            Font = new Font("Segoe UI", 10F),
            UseSystemPasswordChar = true
        };

        _lblHint = new Label
        {
            Location = new Point(24, 16),
            Size = new Size(496, 40),
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = AppColors.InkMuted
        };

        _lblCurrent = lbl(70);
        _txtCurrent = passwordBox(70);

        _lblNew = lbl(114);
        _txtNew = passwordBox(114);

        _lblConfirm = lbl(158);
        _txtConfirm = passwordBox(158);

        _btnSave = ButtonFactory.Primary(AppStrings.Save);
        _btnSave.Location = new Point(190, 214);
        _btnSave.Size = new Size(160, 40);
        _btnSave.Click += async (_, _) => await SaveAsync();

        _btnCancel = ButtonFactory.Neutral(AppStrings.Cancel);
        _btnCancel.Location = new Point(360, 214);
        _btnCancel.Size = new Size(160, 40);
        _btnCancel.DialogResult = DialogResult.Cancel;

        AcceptButton = _btnSave;
        CancelButton = _btnCancel;

        Controls.AddRange(new Control[]
        {
            _lblHint,
            _lblCurrent, _txtCurrent,
            _lblNew, _txtNew,
            _lblConfirm, _txtConfirm,
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
        Text = AppStrings.ChangePassword;
        _lblHint.Text = AppStrings.ChangePasswordDetails;
        _lblCurrent.Text = AppStrings.CurrentPassword;
        _lblNew.Text = AppStrings.NewPassword;
        _lblConfirm.Text = AppStrings.ConfirmPassword;
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
        var dto = new ChangePasswordDto
        {
            UserId = _userId,
            CurrentPassword = _txtCurrent.Text,
            NewPassword = _txtNew.Text,
            ConfirmPassword = _txtConfirm.Text
        };

        var validation = await _validator.ValidateAsync(dto);

        if (!validation.IsValid)
        {
            AppMessageBox.Error(validation.Errors.First().ErrorMessage);
            return;
        }

        _btnSave.Enabled = false;

        try
        {
            var result = await _authService.ChangePasswordAsync(dto);

            if (!result.Success)
            {
                AppMessageBox.Error(result.Message ?? AppStrings.ErrorMsg);
                return;
            }

            AppMessageBox.Success(result.Message ?? AppStrings.PasswordChangedSuccessfully);

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            AppMessageBox.Error(ex.Message);
        }
        finally
        {
            if (!IsDisposed)
                _btnSave.Enabled = true;
        }
    }
}