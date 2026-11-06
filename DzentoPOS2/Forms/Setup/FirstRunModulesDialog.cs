using DzentoPOS2.Localization.Resources;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;
using Microsoft.Extensions.DependencyInjection;

namespace DzentoPOS2.WinForms.Forms.Setup;

public class FirstRunModulesDialog : ThemedDialog
{
    private readonly IModuleService _moduleService;
    private readonly IBackupService _backupService;
    private readonly CheckBox _chkGrocery = new() { Text = AppStrings.ModuleGrocery, AutoSize = true, MinimumSize = new Size(0, 28) };
    private readonly CheckBox _chkPhones = new() { Text = AppStrings.ModulePhones, AutoSize = true, MinimumSize = new Size(0, 28) };
    private readonly CheckBox _chkDigital = new() { Text = AppStrings.ModuleDigital, AutoSize = true, MinimumSize = new Size(0, 28) };
    private readonly CheckBox _chkGeneral = new() { Text = AppStrings.ModuleGeneralRetail, AutoSize = true, MinimumSize = new Size(0, 28), Checked = true };
    private readonly TextBox _txtBackupFolder = new() { ReadOnly = true, AutoSize = false, Width = 200, Height = 28, BorderStyle = BorderStyle.FixedSingle };
    private readonly Button _btnChooseBackupFolder = new() { Text = AppStrings.ChooseBackupFolder, AutoSize = true, MinimumSize = new Size(80, 32) };
    private readonly Button _btnOk = new() { Text = AppStrings.SaveAndContinue, AutoSize = true, MinimumSize = new Size(80, 32) };

    public FirstRunModulesDialog(IModuleService moduleService, IBackupService backupService)
    {
        _moduleService = moduleService;
        _backupService = backupService;
        Text = AppStrings.ChooseActivityModules;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        Width = 460;
        Height = 400;
        BackColor = AppColors.Surface;
        ForeColor = AppColors.Ink;
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(20), BackColor = AppColors.Surface };
        var lbl = new Label { Text = AppStrings.SelectEnabledModules, AutoSize = true, MaximumSize = new Size(320, 0), ForeColor = AppColors.Ink, Font = AppStyle.BodyFont };
        _chkGrocery.ForeColor = AppColors.Ink;
        _chkPhones.ForeColor = AppColors.Ink;
        _chkDigital.ForeColor = AppColors.Ink;
        _chkGeneral.ForeColor = AppColors.Ink;
        _btnOk.BackColor = AppColors.Primary;
        _btnOk.ForeColor = AppColors.OnDark;
        _btnOk.FlatStyle = FlatStyle.Flat;

        // Backup location: the first-run wizard lets the user pick where the
        // daily snapshots are written before any data exists.
        var lblBackup = new Label { Text = AppStrings.BackupLocation, AutoSize = true, ForeColor = AppColors.Ink, Font = AppStyle.BodyFont, Margin = new Padding(0, 16, 0, 4) };
        var lblBackupHint = new Label { Text = AppStrings.BackupLocationHint, AutoSize = true, ForeColor = AppColors.InkMuted, MaximumSize = new Size(320, 0), UseMnemonic = false, Margin = new Padding(0, 0, 0, 4) };
        _txtBackupFolder.Text = _backupService.BackupDirectory;
        _txtBackupFolder.Margin = new Padding(0, 0, 8, 0);

        var backupRow = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            BackColor = AppColors.Surface,
            Margin = Padding.Empty
        };
        backupRow.Controls.Add(_txtBackupFolder);
        backupRow.Controls.Add(_btnChooseBackupFolder);
        _btnChooseBackupFolder.Click += (_, _) => ChooseBackupFolder();

        panel.Controls.Add(lbl);
        panel.Controls.Add(_chkGrocery);
        panel.Controls.Add(_chkPhones);
        panel.Controls.Add(_chkDigital);
        panel.Controls.Add(_chkGeneral);
        panel.Controls.Add(lblBackup);
        panel.Controls.Add(lblBackupHint);
        panel.Controls.Add(backupRow);
        panel.Controls.Add(_btnOk);
        Controls.Add(panel);
        _btnOk.Click += async (_, _) => await SaveAsync();
    }

    private void ChooseBackupFolder()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = AppStrings.BackupLocation,
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true,
            SelectedPath = Directory.Exists(_backupService.BackupDirectory)
                ? _backupService.BackupDirectory
                : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            _backupService.SetBackupDirectory(dialog.SelectedPath);
            _txtBackupFolder.Text = _backupService.BackupDirectory;
            AppMessageBox.Success(AppStrings.BackupLocationChanged);
        }
        catch (Exception ex)
        {
            AppMessageBox.Error(ex.Message);
        }
    }

    private async Task SaveAsync()
    {
        await _moduleService.SetEnabledAsync(ActivityModule.Grocery, _chkGrocery.Checked);
        await _moduleService.SetEnabledAsync(ActivityModule.Phones, _chkPhones.Checked);
        await _moduleService.SetEnabledAsync(ActivityModule.Digital, _chkDigital.Checked);
        await _moduleService.SetEnabledAsync(ActivityModule.GeneralRetail, _chkGeneral.Checked);
        await _moduleService.SetConfiguredAsync(true);
        DialogResult = DialogResult.OK;
        Close();
    }
}
