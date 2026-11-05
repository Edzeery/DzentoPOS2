using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Forms.Inventory;

public class ProductBarcodesDialog : Form
{
    private readonly IBarcodeService _barcodeService;
    private readonly Guid _variantId;
    private readonly ListBox _lst = new() { Dock = DockStyle.Top, Height = 180 };
    private readonly TextBox _txt = new() { Width = 260 };
    private readonly Button _btnAdd = new() { Text = "إضافة", Width = 80 };
    private readonly Button _btnGen = new() { Text = "توليد", Width = 80 };
    private readonly Button _btnRemove = new() { Text = "حذف", Width = 80 };
    private readonly Button _btnClose = new() { Text = "إغلاق", Width = 80, DialogResult = DialogResult.OK };

    public ProductBarcodesDialog(IBarcodeService barcodeService, Guid variantId)
    {
        _barcodeService = barcodeService;
        _variantId = variantId;
        Text = "إدارة الباركودات";
        Size = new Size(420, 320);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = AppColors.Surface;
        ForeColor = AppColors.Ink;
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(12) };
        var row = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
        row.Controls.Add(_txt);
        row.Controls.Add(_btnAdd);
        row.Controls.Add(_btnGen);
        var row2 = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
        row2.Controls.Add(_btnRemove);
        row2.Controls.Add(_btnClose);
        panel.Controls.Add(_lst);
        panel.Controls.Add(row);
        panel.Controls.Add(row2);
        Controls.Add(panel);
        _btnAdd.Click += async (_, _) => await AddAsync();
        _btnGen.Click += async (_, _) => { _txt.Text = _barcodeService.GenerateInternal(); };
        _btnRemove.Click += async (_, _) => await RemoveAsync();
        _btnClose.Click += (_, _) => Close();
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        _lst.Items.Clear();
        foreach (var b in await _barcodeService.GetBarcodesAsync(_variantId))
            _lst.Items.Add(b);
    }

    private async Task AddAsync()
    {
        var code = _txt.Text.Trim();
        if (string.IsNullOrWhiteSpace(code)) return;
        if (!await _barcodeService.IsUniqueAsync(code))
        {
            MessageBox.Show("الباركود مستخدم مسبقًا", AppStrings.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        await _barcodeService.AddAsync(_variantId, code, _lst.Items.Count == 0);
        _txt.Clear();
        await LoadAsync();
    }

    private async Task RemoveAsync()
    {
        if (_lst.SelectedItem is not string code) return;
        await _barcodeService.RemoveByValueAsync(_variantId, code);
        await LoadAsync();
    }
}