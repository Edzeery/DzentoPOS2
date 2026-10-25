using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Forms.Inventory;

public class StockAdjustDialog : Form
{
    private readonly IProductService _productService;
    private readonly Guid _productId;
    private readonly NumericUpDown _numQuantity;
    private readonly TextBox _txtReason;

    public StockAdjustDialog(
        IProductService productService,
        Guid productId,
        decimal currentQuantity)
    {
        _productService = productService;
        _productId = productId;

        Text = AppStrings.AdjustStock;
        Size = new Size(420, 260);
        MinimumSize = new Size(380, 240);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = AppColors.Surface;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            Padding = new Padding(20),
            BackColor = AppColors.Surface
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));

        var lblQuantity = new Label
        {
            Text = AppStrings.NewQuantity + ":",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = AppColors.Ink
        };

        _numQuantity = new NumericUpDown
        {
            Dock = DockStyle.Fill,
            DecimalPlaces = 0,
            Minimum = 0,
            Maximum = 999999,
            Value = Math.Max(0, currentQuantity),
            Margin = new Padding(0, 8, 0, 8)
        };

        var lblReason = new Label
        {
            Text = AppStrings.AdjustmentReason + ":",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = AppColors.Ink
        };

        _txtReason = new TextBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 8, 0, 8)
        };

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            BackColor = AppColors.Surface
        };

        var btnApply = ButtonFactory.Primary(AppStrings.Apply);
        btnApply.Size = new Size(110, 44);
        btnApply.Click += async (_, _) => await ApplyAsync();

        var btnCancel = ButtonFactory.Neutral(AppStrings.Cancel);
        btnCancel.Size = new Size(110, 44);
        btnCancel.DialogResult = DialogResult.Cancel;

        buttons.Controls.Add(btnApply);
        buttons.Controls.Add(btnCancel);

        layout.Controls.Add(lblQuantity, 0, 0);
        layout.Controls.Add(_numQuantity, 1, 0);
        layout.Controls.Add(lblReason, 0, 1);
        layout.Controls.Add(_txtReason, 1, 1);
        layout.Controls.Add(buttons, 0, 3);
        layout.SetColumnSpan(buttons, 2);

        Controls.Add(layout);
    }

    private async Task ApplyAsync()
    {
        var reason = _txtReason.Text.Trim();
        if (string.IsNullOrWhiteSpace(reason))
        {
            MessageBox.Show(
                AppStrings.AdjustmentReasonRequired,
                AppStrings.Warning,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        try
        {
            await _productService.AdjustStockAsync(
                _productId,
                _numQuantity.Value,
                reason);

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"{AppStrings.ErrorMsg}: {ex.Message}",
                AppStrings.ErrorMsg,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}

