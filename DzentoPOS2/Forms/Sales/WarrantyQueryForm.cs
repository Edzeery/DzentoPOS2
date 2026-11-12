using System;
using System.Windows.Forms;
using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Forms.Sales;

public class WarrantyQueryForm : ThemedDialog
{
    private readonly ISerialUnitService _serialUnitService;
    private readonly TextBox _txtQuery = new() { Dock = DockStyle.Fill, MinimumSize = new Size(0, 28) };
    private readonly Button _btnSearch = new() { Text = AppStrings.Search, AutoSize = true, MinimumSize = new Size(80, 32) };
    private readonly Label _lblResult = new() { Dock = DockStyle.Fill, AutoSize = true, ForeColor = AppColors.Ink };

    public WarrantyQueryForm(ISerialUnitService serialUnitService)
    {
        _serialUnitService = serialUnitService;

        Text = AppStrings.WarrantyQuery;
        Size = new Size(480, 320);
        MinimumSize = new Size(360, 260);
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Font;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(12),
            Margin = Padding.Empty
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var inputRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };
        inputRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        inputRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        inputRow.Controls.Add(_txtQuery, 0, 0);
        inputRow.Controls.Add(_btnSearch, 1, 0);

        root.Controls.Add(inputRow, 0, 0);
        root.Controls.Add(_lblResult, 0, 2);

        Controls.Add(root);

        _btnSearch.Click += async (_, _) => await SearchAsync();
        _txtQuery.KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await SearchAsync();
            }
        };
    }

    private async Task SearchAsync()
    {
        var query = _txtQuery.Text?.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            _lblResult.Text = AppStrings.BarcodeNotFound;
            return;
        }

        var info = await _serialUnitService.GetWarrantyInfoAsync(query);
        if (info == null)
        {
            _lblResult.Text = AppStrings.BarcodeNotFound;
            return;
        }

        var statusText = info.IsUnderWarranty ? "Under Warranty" : "Expired";
        _lblResult.Text = $"{info.ProductName} - {info.VariantName}\n" +
                          $"Serial: {info.SerialNumber}\n" +
                          $"IMEI: {info.IMEI}\n" +
                          $"Sale Date: {info.SaleDate:yyyy-MM-dd}\n" +
                          $"Warranty Expires: {info.WarrantyExpiry:yyyy-MM-dd}\n" +
                          $"Status: {statusText}";
    }

    protected override void ApplyTheme()
    {
        base.ApplyTheme();
        _btnSearch.BackColor = AppColors.Primary;
        _btnSearch.ForeColor = AppColors.OnDark;
    }
}
