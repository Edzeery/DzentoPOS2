using System;
using System.Drawing;
using System.Windows.Forms;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Forms.Sales;

/// <summary>
/// Small modal used to override the unit price of one cart line. The screen
/// still checks the permission before it opens the dialog, so this class only
/// collects the value.
/// </summary>
public class PriceOverrideDialog : ThemedDialog
{
    private readonly NumericUpDown _numPrice = new()
    {
        DecimalPlaces = 2,
        Minimum = 0,
        Maximum = 99999999,
        Width = 200,
        TextAlign = HorizontalAlignment.Right
    };

    private readonly Label _lblProduct = new()
    {
        AutoSize = false,
        AutoEllipsis = true,
        Height = 24,
        Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold)
    };

    private readonly Label _lblCurrent = new()
    {
        AutoSize = true,
        Font = new Font("Segoe UI", 9F)
    };

    private readonly Button _btnSave = new()
    {
        Text = AppStrings.Save,
        AutoSize = true,
        MinimumSize = new Size(90, 34)
    };

    private readonly Button _btnCancel = new()
    {
        Text = AppStrings.Cancel,
        AutoSize = true,
        MinimumSize = new Size(90, 34),
        DialogResult = DialogResult.Cancel
    };

    public PriceOverrideDialog(string productName, decimal currentPrice, string? currency)
    {
        Text = AppStrings.Edit + " " + AppStrings.Price;
        Size = new Size(380, 250);
        MinimumSize = new Size(340, 230);
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Font;
        ShowInTaskbar = false;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(16),
            Margin = Padding.Empty
        };
        for (int i = 0; i < 5; i++)
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _lblProduct.Text = productName;
        _lblCurrent.Text = $"{AppStrings.UnitPrice}: {currentPrice:N2} {currency}".Trim();

        var priceLabel = new Label
        {
            Text = AppStrings.PriceLabel,
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5F),
            Margin = new Padding(0, 6, 0, 4)
        };

        _numPrice.Value = Math.Min(Math.Max(currentPrice, _numPrice.Minimum), _numPrice.Maximum);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            AutoSize = true,
            Margin = new Padding(0, 12, 0, 0)
        };
        actions.Controls.Add(_btnCancel);
        actions.Controls.Add(_btnSave);

        root.Controls.Add(_lblProduct, 0, 0);
        root.Controls.Add(_lblCurrent, 0, 1);
        root.Controls.Add(priceLabel, 0, 2);
        root.Controls.Add(_numPrice, 0, 3);
        root.Controls.Add(actions, 0, 4);

        Controls.Add(root);

        AcceptButton = _btnSave;
        CancelButton = _btnCancel;

        _btnSave.Click += OnSaveClick;
        Shown += (_, _) =>
        {
            _numPrice.Focus();
            _numPrice.Select(0, _numPrice.Text.Length);
        };
    }

    /// <summary>The price the cashier accepted.</summary>
    public decimal Price { get; private set; }

    private void OnSaveClick(object? sender, EventArgs e)
    {
        Price = _numPrice.Value;
        DialogResult = DialogResult.OK;
        Close();
    }

    protected override void ApplyTheme()
    {
        base.ApplyTheme();

        _lblProduct.ForeColor = AppColors.Ink;
        _lblCurrent.ForeColor = AppColors.InkMuted;

        _numPrice.BackColor = AppColors.InputBackground;
        _numPrice.ForeColor = AppColors.Ink;

        _btnSave.BackColor = AppColors.Primary;
        _btnSave.ForeColor = AppColors.OnDark;
        _btnSave.FlatStyle = FlatStyle.Flat;
        _btnSave.FlatAppearance.BorderSize = 0;

        _btnCancel.BackColor = AppColors.SurfaceAlt;
        _btnCancel.ForeColor = AppColors.Ink;
        _btnCancel.FlatStyle = FlatStyle.Flat;
        _btnCancel.FlatAppearance.BorderColor = AppColors.Border;
        _btnCancel.FlatAppearance.BorderSize = 1;
    }
}
