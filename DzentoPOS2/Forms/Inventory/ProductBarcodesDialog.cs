using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.WinForms.Helpers;

namespace DzentoPOS2.WinForms.Forms.Inventory;

public class ProductBarcodesDialog : ThemedDialog
{
    private readonly IBarcodeService _barcodeService;
    private readonly Guid _variantId;
    private readonly ListBox _lst = new() { Dock = DockStyle.Fill, IntegralHeight = false };
    private readonly TextBox _txt = new() { Dock = DockStyle.Fill, MinimumSize = new Size(0, 28) };
    private readonly Button _btnAdd = new() { Text = AppStrings.Add, AutoSize = true, MinimumSize = new Size(80, 32) };
    private readonly Button _btnGen = new() { Text = AppStrings.Generate, AutoSize = true, MinimumSize = new Size(80, 32) };
    private readonly Button _btnRemove = new() { Text = AppStrings.Delete, AutoSize = true, MinimumSize = new Size(80, 32) };
    private readonly Button _btnClose = new() { Text = AppStrings.Close, AutoSize = true, MinimumSize = new Size(80, 32), DialogResult = DialogResult.OK };

    public ProductBarcodesDialog(IBarcodeService barcodeService, Guid variantId)
    {
        _barcodeService = barcodeService;
        _variantId = variantId;
        Text = AppStrings.ManageBarcodes;
        Size = new Size(420, 320);
        MinimumSize = new Size(320, 260);
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Font;

        var inputRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 8, 0, 0)
        };
        inputRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        inputRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        inputRow.Controls.Add(_txt, 0, 0);
        inputRow.Controls.Add(_btnAdd, 1, 0);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = true,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 8, 0, 0)
        };
        actions.Controls.Add(_btnRemove);
        actions.Controls.Add(_btnGen);
        actions.Controls.Add(_btnClose);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(12),
            Margin = Padding.Empty
        };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(_lst, 0, 0);
        root.Controls.Add(inputRow, 0, 1);
        root.Controls.Add(actions, 0, 2);
        Controls.Add(root);

        _btnAdd.Click += async (_, _) => await AddAsync();
        _btnGen.Click += (_, _) => { _txt.Text = _barcodeService.GenerateInternal(); };
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
            MessageBox.Show(AppStrings.BarcodeAlreadyUsed, AppStrings.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
