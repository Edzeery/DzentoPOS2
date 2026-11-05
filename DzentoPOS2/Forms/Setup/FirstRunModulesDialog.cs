using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;
using Microsoft.Extensions.DependencyInjection;

namespace DzentoPOS2.WinForms.Forms.Setup;

public class FirstRunModulesDialog : ThemedDialog
{
    private readonly IModuleService _moduleService;
    private readonly CheckBox _chkGrocery = new() { Text = "بقالة (وزن/دفعات/صلاحية)", AutoSize = true };
    private readonly CheckBox _chkPhones = new() { Text = "هواتف / إلكترونيات (IMEI/ضمان)", AutoSize = true };
    private readonly CheckBox _chkDigital = new() { Text = "منتجات رقمية (أكواد/شحن)", AutoSize = true };
    private readonly CheckBox _chkGeneral = new() { Text = "تجزئة عامة", AutoSize = true, Checked = true };
    private readonly Button _btnOk = new() { Text = "حفظ ومتابعة", AutoSize = true };

    public FirstRunModulesDialog(IModuleService moduleService)
    {
        _moduleService = moduleService;
        Text = "اختيار وحدات النشاط";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        Width = 460;
        Height = 300;
        BackColor = AppColors.Surface;
        ForeColor = AppColors.Ink;
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(20), BackColor = AppColors.Surface };
        var lbl = new Label { Text = "اختر الوحدات المفعّلة (يمكن تعديلها لاحقًا في الإعدادات):", AutoSize = true, ForeColor = AppColors.Ink, Font = AppStyle.BodyFont };
        _chkGrocery.ForeColor = AppColors.Ink;
        _chkPhones.ForeColor = AppColors.Ink;
        _chkDigital.ForeColor = AppColors.Ink;
        _chkGeneral.ForeColor = AppColors.Ink;
        _btnOk.BackColor = AppColors.Primary;
        _btnOk.ForeColor = AppColors.OnDark;
        _btnOk.FlatStyle = FlatStyle.Flat;
        panel.Controls.Add(lbl);
        panel.Controls.Add(_chkGrocery);
        panel.Controls.Add(_chkPhones);
        panel.Controls.Add(_chkDigital);
        panel.Controls.Add(_chkGeneral);
        panel.Controls.Add(_btnOk);
        Controls.Add(panel);
        _btnOk.Click += async (_, _) => await SaveAsync();
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