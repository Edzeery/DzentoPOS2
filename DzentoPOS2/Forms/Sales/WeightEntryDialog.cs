using System;
using System.Windows.Forms;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Forms.Sales;

public class WeightEntryDialog : ThemedDialog
{
    private readonly NumericUpDown _numWeight = new() { DecimalPlaces = 3, Minimum = 0, Maximum = 999999, Width = 200 };
    private readonly Label _lblPrice = new() { AutoSize = true, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = AppColors.Primary };
    private readonly Button _btnOk = new() { Text = AppStrings.Save, AutoSize = true, MinimumSize = new Size(80, 32) };
    private readonly Button _btnCancel = new() { Text = AppStrings.Cancel, AutoSize = true, MinimumSize = new Size(80, 32), DialogResult = DialogResult.Cancel };

    private readonly decimal _unitPrice;
    public decimal Weight { get; private set; }
    public decimal CalculatedPrice { get; private set; }

    public WeightEntryDialog(decimal unitPrice, string productName)
    {
        _unitPrice = unitPrice;
        Text = AppStrings.WeightEntryTitle;
        Size = new Size(360, 240);
        MinimumSize = new Size(320, 200);
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Font;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(16),
            Margin = Padding.Empty
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var lblProduct = new Label { Text = productName, Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true, ForeColor = AppColors.Ink };
        var lblWeight = new Label { Text = AppStrings.WeightLabel + ":", AutoSize = true, ForeColor = AppColors.Ink };
        var weightRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, AutoSize = true };
        weightRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        weightRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        weightRow.Controls.Add(lblWeight, 0, 0);
        weightRow.Controls.Add(_numWeight, 1, 0);

        _lblPrice.Text = $"{AppStrings.CalculatedPrice}: 0.00";

        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = true, AutoSize = true };
        actions.Controls.Add(_btnCancel);
        actions.Controls.Add(_btnOk);

        root.Controls.Add(lblProduct, 0, 0);
        root.Controls.Add(weightRow, 0, 1);
        root.Controls.Add(_lblPrice, 0, 2);
        root.Controls.Add(actions, 0, 3);

        Controls.Add(root);

        _numWeight.ValueChanged += (_, _) => UpdatePrice();
        _btnOk.Click += (_, _) =>
        {
            Weight = _numWeight.Value;
            CalculatedPrice = Weight * _unitPrice;
            DialogResult = DialogResult.OK;
            Close();
        };
    }

    private void UpdatePrice()
    {
        CalculatedPrice = _numWeight.Value * _unitPrice;
        _lblPrice.Text = $"{AppStrings.CalculatedPrice}: {CalculatedPrice:N2}";
    }

    protected override void ApplyTheme()
    {
        base.ApplyTheme();
        _btnOk.BackColor = AppColors.Primary;
        _btnOk.ForeColor = AppColors.OnDark;
    }
}
