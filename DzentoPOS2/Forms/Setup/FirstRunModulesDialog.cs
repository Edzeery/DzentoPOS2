using DzentoPOS2.Localization.Resources;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;
using Microsoft.Extensions.DependencyInjection;

namespace DzentoPOS2.WinForms.Forms.Setup;

public class FirstRunModulesDialog : ThemedDialog
{
    private readonly ICapabilityService _capabilityService;
    private readonly IBackupService _backupService;
    private readonly IActiveBranchContext _activeBranch;

    private readonly CheckBox _chkGrocery = new() { Text = AppStrings.ActivityGrocery, AutoSize = true, MinimumSize = new Size(0, 28) };
    private readonly CheckBox _chkProduce = new() { Text = AppStrings.ActivityProduce, AutoSize = true, MinimumSize = new Size(0, 28) };
    private readonly CheckBox _chkPhones = new() { Text = AppStrings.ActivityPhones, AutoSize = true, MinimumSize = new Size(0, 28) };
    private readonly CheckBox _chkAppliances = new() { Text = AppStrings.ActivityAppliances, AutoSize = true, MinimumSize = new Size(0, 28) };
    private readonly CheckBox _chkIT = new() { Text = AppStrings.ActivityIT, AutoSize = true, MinimumSize = new Size(0, 28) };
    private readonly CheckBox _chkRepair = new() { Text = AppStrings.ActivityRepair, AutoSize = true, MinimumSize = new Size(0, 28) };
    private readonly CheckBox _chkDigital = new() { Text = AppStrings.ActivityDigital, AutoSize = true, MinimumSize = new Size(0, 28) };
    private readonly CheckBox _chkMulti = new() { Text = AppStrings.ActivityMultiService, AutoSize = true, MinimumSize = new Size(0, 28), Checked = true };

    private readonly TextBox _txtBackupFolder = new() { ReadOnly = true, AutoSize = false, Width = 200, Height = 28, BorderStyle = BorderStyle.FixedSingle };
    private readonly Button _btnChooseBackupFolder = new() { Text = AppStrings.ChooseBackupFolder, AutoSize = true, MinimumSize = new Size(80, 32) };
    private readonly Button _btnOk = new() { Text = AppStrings.SaveAndContinue, AutoSize = true, MinimumSize = new Size(80, 32) };

    public FirstRunModulesDialog(ICapabilityService capabilityService, IBackupService backupService, IActiveBranchContext activeBranch)
    {
        _capabilityService = capabilityService;
        _backupService = backupService;
        _activeBranch = activeBranch;
        Text = AppStrings.ChooseActivityModules;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        Width = 520;
        Height = 520;
        BackColor = AppColors.Surface;
        ForeColor = AppColors.Ink;

        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(20), BackColor = AppColors.Surface };
        var lbl = new Label { Text = AppStrings.SelectEnabledModules, AutoSize = true, MaximumSize = new Size(400, 0), ForeColor = AppColors.Ink, Font = AppStyle.BodyFont };

        foreach (var chk in new[] { _chkGrocery, _chkProduce, _chkPhones, _chkAppliances, _chkIT, _chkRepair, _chkDigital, _chkMulti })
        {
            chk.ForeColor = AppColors.Ink;
            panel.Controls.Add(chk);
        }

        _btnOk.BackColor = AppColors.Primary;
        _btnOk.ForeColor = AppColors.OnDark;
        _btnOk.Click += async (_, _) => await SaveAsync();

        panel.Controls.Add(new Label { Text = AppStrings.BackupLocation, AutoSize = true, ForeColor = AppColors.Ink, Font = AppStyle.BodyFont, Margin = new Padding(0, 12, 0, 4) });
        panel.Controls.Add(_txtBackupFolder);
        panel.Controls.Add(_btnChooseBackupFolder);
        panel.Controls.Add(_btnOk);

        Controls.Add(panel);

        _btnChooseBackupFolder.Click += (_, _) => ChooseBackupFolder();
        _txtBackupFolder.Text = _backupService.BackupDirectory;
    }

    private async Task SaveAsync()
    {
        var caps = BranchCapability.None;
        if (_chkGrocery.Checked) caps |= BranchCapability.ExpiryDate | BranchCapability.WeightUnit | BranchCapability.BatchTracking;
        if (_chkProduce.Checked) caps |= BranchCapability.WeightUnit | BranchCapability.BatchTracking | BranchCapability.ExpiryDate;
        if (_chkPhones.Checked) caps |= BranchCapability.SerialNumber | BranchCapability.IMEI | BranchCapability.Warranty | BranchCapability.RepairTicket;
        if (_chkAppliances.Checked) caps |= BranchCapability.SerialNumber | BranchCapability.Warranty | BranchCapability.RepairTicket;
        if (_chkIT.Checked) caps |= BranchCapability.SerialNumber | BranchCapability.License | BranchCapability.DigitalCode;
        if (_chkRepair.Checked) caps |= BranchCapability.RepairTicket | BranchCapability.SerialNumber | BranchCapability.Warranty;
        if (_chkDigital.Checked) caps |= BranchCapability.DigitalCode | BranchCapability.License | BranchCapability.NoStock;
        if (_chkMulti.Checked) caps |= BranchCapability.SerialNumber | BranchCapability.IMEI | BranchCapability.Warranty |
            BranchCapability.RepairTicket | BranchCapability.ExpiryDate | BranchCapability.WeightUnit |
            BranchCapability.BatchTracking | BranchCapability.DigitalCode | BranchCapability.License | BranchCapability.NoStock;

        if (caps == BranchCapability.None)
            caps = BranchCapability.SerialNumber | BranchCapability.IMEI | BranchCapability.Warranty |
                BranchCapability.RepairTicket | BranchCapability.ExpiryDate | BranchCapability.WeightUnit |
                BranchCapability.BatchTracking | BranchCapability.DigitalCode | BranchCapability.License | BranchCapability.NoStock;

        await _capabilityService.SetCapabilitiesAsync(_activeBranch.ActiveBranchId, caps);

        // Save backup location
        if (!string.IsNullOrWhiteSpace(_txtBackupFolder.Text))
            _backupService.SetBackupDirectory(_txtBackupFolder.Text);

        DialogResult = DialogResult.OK;
        Close();
    }

    private void ChooseBackupFolder()
    {
        using var dlg = new FolderBrowserDialog { Description = AppStrings.ChooseBackupFolder };
        if (dlg.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(dlg.SelectedPath))
            _txtBackupFolder.Text = dlg.SelectedPath;
    }

    protected override void ApplyTheme()
    {
        base.ApplyTheme();
        _btnOk.BackColor = AppColors.Primary;
        _btnOk.ForeColor = AppColors.OnDark;
    }
}
