using DzentoPOS2.App.Services.Licensing;
using DzentoPOS2.App.Session;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.WinForms.Controls;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Forms.Settings;

/// <summary>
/// Shows the current licence state and lets an administrator activate a signed
/// key or deactivate the current one. The device fingerprint is displayed (and
/// copyable) so the vendor can issue a key bound to this machine.
/// </summary>
public sealed class LicenseActivationDialog : ThemedDialog
{
    private static readonly Color ErrorColor = Color.FromArgb(190, 40, 40);

    private readonly ILicenseService _license;
    private readonly ICurrentUserService _currentUser;

    private readonly Label _lblState = new()
    {
        AutoSize = true,
        Font = AppStyle.SubtitleFont,
        ForeColor = AppColors.Primary
    };

    private readonly Label _lblDetails = new()
    {
        AutoSize = true,
        Font = AppStyle.BodyFont,
        ForeColor = AppColors.InkMuted,
        MaximumSize = new Size(580, 0)
    };

    private readonly Label _lblLimits = new()
    {
        AutoSize = true,
        Font = AppStyle.BodyFont,
        ForeColor = AppColors.InkMuted,
        MaximumSize = new Size(580, 0)
    };

    private readonly Label _lblDeviceCaption = new()
    {
        AutoSize = true,
        Font = AppStyle.BodyFont,
        ForeColor = AppColors.Ink
    };

    private readonly Label _lblDevice = new()
    {
        AutoSize = true,
        Font = new Font("Consolas", 9F),
        ForeColor = AppColors.Ink
    };

    private readonly Label _lblKeyCaption = new()
    {
        AutoSize = true,
        Font = AppStyle.BodyFont,
        ForeColor = AppColors.Ink
    };

    private readonly TextBox _txtKey = new()
    {
        Multiline = true,
        ScrollBars = ScrollBars.Vertical,
        Font = new Font("Consolas", 9F),
        Dock = DockStyle.Fill
    };

    private readonly Label _lblFeedback = new()
    {
        AutoSize = true,
        Font = AppStyle.BodyFont,
        ForeColor = AppColors.InkMuted,
        MaximumSize = new Size(580, 0)
    };

    private readonly ThemedButton _btnActivate =
        ButtonFactory.Primary(AppStrings.Activate, ButtonFactory.ButtonSize.Small);

    private readonly ThemedButton _btnDeactivate =
        ButtonFactory.Neutral(AppStrings.Deactivate, ButtonFactory.ButtonSize.Small);

    private readonly ThemedButton _btnCopy =
        ButtonFactory.Neutral(AppStrings.Copy, ButtonFactory.ButtonSize.Small);

    private readonly ThemedButton _btnClose =
        ButtonFactory.Neutral(AppStrings.Close, ButtonFactory.ButtonSize.Small);

    public LicenseActivationDialog(
        ILicenseService license,
        ICurrentUserService currentUser)
    {
        _license = license;
        _currentUser = currentUser;

        Text = AppStrings.LicenseTitle;
        Size = new Size(660, 580);
        MinimumSize = new Size(520, 500);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;

        _btnCopy.Click += (_, _) => CopyDeviceId();
        _btnActivate.Click += async (_, _) => await ActivateAsync();
        _btnDeactivate.Click += async (_, _) => await DeactivateAsync();
        _btnClose.Click += (_, _) => Close();

        Controls.Add(BuildLayout());

        _ = LoadStatusAsync();
    }

    private Control BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 0,
            GrowStyle = TableLayoutPanelGrowStyle.AddRows,
            Padding = new Padding(24),
            BackColor = Color.Transparent
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        AddRow(root, _lblState);
        AddRow(root, _lblDetails);
        AddRow(root, _lblLimits);
        AddRow(root, BuildDeviceRow());
        AddRow(root, _lblKeyCaption);
        AddRow(root, _txtKey, 92);
        AddRow(root, _lblFeedback);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            WrapContents = true,
            BackColor = Color.Transparent,
            Margin = Padding.Empty
        };
        buttons.Controls.Add(_btnActivate);
        buttons.Controls.Add(_btnDeactivate);
        buttons.Controls.Add(_btnCopy);
        buttons.Controls.Add(_btnClose);

        AddRow(root, buttons);
        return root;
    }

    private static void AddRow(TableLayoutPanel root, Control control, int height = 0)
    {
        control.Dock = DockStyle.Top;
        control.Margin = new Padding(0, 0, 0, 10);
        root.Controls.Add(control);
        root.RowStyles.Add(height > 0
            ? new RowStyle(SizeType.Absolute, height + 10)
            : new RowStyle(SizeType.AutoSize));
    }

    private Control BuildDeviceRow()
    {
        var row = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 3,
            AutoSize = true,
            BackColor = Color.Transparent,
            Margin = Padding.Empty
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        row.Controls.Add(_lblDeviceCaption, 0, 0);
        row.Controls.Add(_lblDevice, 1, 0);
        row.Controls.Add(_btnCopy, 2, 0);

        return row;
    }

    private async Task LoadStatusAsync()
    {
        try
        {
            var status = await _license.GetStatusAsync();

            _lblState.Text = LicenseStatusText.State(status.State);
            _lblDetails.Text = status.ExpiryDate is { } expiry
                ? $"{AppStrings.LicensePlanLabel}: {status.PlanName ?? "-"}   |   " +
                  $"{AppStrings.LicenseExpiryLabel}: {expiry:yyyy-MM-dd}"
                : $"{AppStrings.LicensePlanLabel}: {status.PlanName ?? "-"}   |   " +
                  $"{AppStrings.LicenseExpiryLabel}: {AppStrings.NoExpiry}";

            _lblLimits.Text = string.Format(
                AppStrings.LicenseLimitsFormat,
                LicenseStatusText.Limit(status.MaxUsers),
                LicenseStatusText.Limit(status.MaxBranches));

            _lblDeviceCaption.Text = AppStrings.LicenseDeviceLabel + ":";
            _lblKeyCaption.Text = AppStrings.LicenseKeyLabel;
            _lblDevice.Text = status.DeviceId ?? string.Empty;

            bool admin = _currentUser.IsAdmin;
            _btnActivate.Enabled = admin;
            _btnDeactivate.Enabled =
                admin && status.State != LicenseState.Unlicensed;

            if (!admin)
                ShowFeedback(AppStrings.AccessDenied, isError: true);
        }
        catch (Exception ex)
        {
            ShowFeedback(ex.Message, isError: true);
        }
    }

    private async Task ActivateAsync()
    {
        if (!_currentUser.IsAdmin)
        {
            ShowFeedback(AppStrings.AccessDenied, isError: true);
            return;
        }

        var key = _txtKey.Text.Trim();
        if (key.Length == 0)
        {
            ShowFeedback(AppStrings.LicenseEnterKey, isError: true);
            return;
        }

        _btnActivate.Enabled = false;
        try
        {
            var result = await _license.ActivateAsync(key);
            if (result.Success)
            {
                _txtKey.Clear();
                await LoadStatusAsync();
                ShowFeedback(
                    result.Message ?? AppStrings.LicenseActivatedMsg,
                    isError: false);
            }
            else
            {
                ShowFeedback(
                    result.Message ?? AppStrings.LicenseInvalidKeyMsg,
                    isError: true);
            }
        }
        catch (Exception ex)
        {
            ShowFeedback(ex.Message, isError: true);
        }
        finally
        {
            _btnActivate.Enabled = _currentUser.IsAdmin;
        }
    }

    private async Task DeactivateAsync()
    {
        if (!_currentUser.IsAdmin)
        {
            ShowFeedback(AppStrings.AccessDenied, isError: true);
            return;
        }

        if (!AppMessageBox.Confirm(AppStrings.LicenseDeactivateConfirm))
            return;

        _btnDeactivate.Enabled = false;
        try
        {
            var result = await _license.DeactivateAsync();
            await LoadStatusAsync();
            ShowFeedback(
                result.Message ??
                (result.Success
                    ? AppStrings.LicenseDeactivatedMsg
                    : AppStrings.LicenseNoneActiveMsg),
                isError: !result.Success);
        }
        catch (Exception ex)
        {
            ShowFeedback(ex.Message, isError: true);
        }
        finally
        {
            _btnDeactivate.Enabled = _currentUser.IsAdmin;
        }
    }

    private void CopyDeviceId()
    {
        if (string.IsNullOrWhiteSpace(_lblDevice.Text))
            return;

        try
        {
            Clipboard.SetText(_lblDevice.Text);
            ShowFeedback(AppStrings.LicenseDeviceCopied, isError: false);
        }
        catch (Exception ex)
        {
            ShowFeedback(ex.Message, isError: true);
        }
    }

    private void ShowFeedback(string message, bool isError)
    {
        _lblFeedback.ForeColor = isError ? ErrorColor : AppColors.Primary;
        _lblFeedback.Text = message;
    }
}