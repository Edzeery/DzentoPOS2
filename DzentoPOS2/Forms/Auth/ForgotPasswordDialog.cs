using DzentoPOS2.App.Services.Identity.Interfaces;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Results;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Forms.Auth;

public sealed partial class ForgotPasswordDialog : ThemedDialog
{
    private readonly IAuthService _authService;
    private readonly ILanguageService _languageService;
    private bool _languageSubscribed;

    private Label _lblTitle = null!;
    private Label _lblHint = null!;
    private Label _lblUserName = null!;
    private TextBox _txtUserName = null!;
    private Label _lblRecoveryKey = null!;
    private TextBox _txtRecoveryKey = null!;
    private Label _lblNewPassword = null!;
    private TextBox _txtNewPassword = null!;
    private Label _lblConfirm = null!;
    private TextBox _txtConfirm = null!;
    private Button _btnReset = null!;
    private Button _btnCancel = null!;

    public ForgotPasswordDialog(
        IAuthService authService,
        ILanguageService languageService)
    {
        _authService = authService;
        _languageService = languageService;

        InitializeComponent();

        var oldReset = _btnReset;
        var oldCancel = _btnCancel;

        _btnReset = ButtonFactory.Primary(AppStrings.ResetPassword);
        _btnReset.Location = new Point(190, 270);
        _btnReset.Size = new Size(160, 40);
        _btnReset.Click += async (_, _) => await ResetAsync();

        _btnCancel = ButtonFactory.Neutral(AppStrings.Cancel);
        _btnCancel.Location = new Point(360, 270);
        _btnCancel.Size = new Size(160, 40);
        _btnCancel.DialogResult = DialogResult.Cancel;

        if (oldReset is not null) Controls.Remove(oldReset);
        if (oldCancel is not null) Controls.Remove(oldCancel);
        Controls.Add(_btnReset);
        Controls.Add(_btnCancel);

        AcceptButton = _btnReset;
        CancelButton = _btnCancel;

        oldReset?.Dispose();
        oldCancel?.Dispose();

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
        Text = AppStrings.ForgotPassword;
        _lblTitle.Text = AppStrings.ForgotPassword;
        _lblHint.Text = AppStrings.RecoveryKeyHint;
        _lblUserName.Text = AppStrings.User;
        _lblRecoveryKey.Text = AppStrings.RecoveryKey;
        _lblNewPassword.Text = AppStrings.NewPassword;
        _lblConfirm.Text = AppStrings.ConfirmPassword;
        _btnReset.Text = AppStrings.ResetPassword;
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

    private async Task ResetAsync()
    {
        string userName = _txtUserName.Text.Trim();
        string recoveryKey = _txtRecoveryKey.Text.Trim();
        string newPassword = _txtNewPassword.Text;
        string confirm = _txtConfirm.Text;

        if (string.IsNullOrWhiteSpace(userName) ||
            string.IsNullOrWhiteSpace(recoveryKey) ||
            string.IsNullOrWhiteSpace(newPassword))
        {
            AppMessageBox.Error(AppStrings.FillAllFields);
            return;
        }

        if (newPassword != confirm)
        {
            AppMessageBox.Error(AppStrings.PasswordsDoNotMatch);
            return;
        }

        if (newPassword.Length < 8)
        {
            AppMessageBox.Error(AppStrings.PasswordTooShort);
            return;
        }

        _btnReset.Enabled = false;

        try
        {
            var result = await _authService.ResetPasswordWithRecoveryKeyAsync(
                userName, recoveryKey, newPassword);

            if (!result.Success)
            {
                AppMessageBox.Error(result.Message ?? AppStrings.InvalidRecoveryKey);
                return;
            }

            // Show the new recovery key once.
            string newKey = result.Data ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(newKey))
            {
                using var keyDialog = new RecoveryKeyDisplayDialog(newKey);
                keyDialog.ShowDialog(this);
            }

            AppMessageBox.Success(AppStrings.PasswordChangedSuccessfully);
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
                _btnReset.Enabled = true;
        }
    }
}