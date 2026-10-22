using DzentoPOS2.WinForms.Themes;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Controls;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;

namespace DzentoPOS2.WinForms.Forms.Sales;

/// <summary>What the user wants to do with the result of the calculator.</summary>
public enum CalculatorTarget
{
    /// <summary>Add a product line to the cart (result = line total).</summary>
    ProductLine,

    /// <summary>Add a free line to the cart (result = line amount).</summary>
    CustomLine,

    /// <summary>Use the result as the amount paid by the customer.</summary>
    SetPaid,

    /// <summary>Use the result as the discount rate.</summary>
    SetDiscount,

    /// <summary>Use the result as the tax rate.</summary>
    SetTax
}

/// <summary>Outcome of the calculator dialog, consumed by the sales screen.</summary>
public sealed class CalculatorOutcome
{
    public decimal Value { get; init; }
    public CalculatorTarget Target { get; init; }
    public Guid? ProductId { get; init; }
    public decimal UnitPrice { get; init; }
    public string Description { get; init; } = string.Empty;
}

/// <summary>
/// A plain calculator whose result can be routed to the sales screen: as a
/// product line, as a free line, as the paid amount, as the discount rate or
/// as the tax rate. The choice is made by the user, nothing is applied
/// silently.
/// </summary>
public class SalesCalculatorDialog : Form
{
    private readonly ILanguageService _languageService;
    private readonly List<ProductOption> _products;
    private readonly decimal _cartTotal;
    private readonly string _currencySymbol;

    private readonly Label _display = new();
    private readonly Label _hint = new();
    private readonly Label _lblTarget = new();
    private readonly Label _lblProduct = new();
    private readonly Label _lblPrice = new();
    private readonly Label _lblDescription = new();
    private readonly ComboBox _cmbTarget = new();
    private readonly ComboBox _cmbProduct = new();
    private readonly NumericUpDown _numPrice = new();
    private readonly TextBox _txtDescription = new();
    private readonly Panel _productContext = new();
    private readonly Panel _descriptionContext = new();
    private readonly ThemedButton _btnApply;
    private readonly ThemedButton _btnCancel;

    private string _entry = "0";
    private decimal? _accumulator;
    private char _pendingOperator;
    private bool _replaceEntry;
    private bool _languageSubscribed;

    /// <summary>The outcome chosen by the user (null when cancelled).</summary>
    public CalculatorOutcome? Outcome { get; private set; }

    public SalesCalculatorDialog(
        ILanguageService languageService,
        IReadOnlyList<ProductDto> products,
        decimal cartTotal,
        string currencySymbol)
    {
        _languageService = languageService;
        _cartTotal = cartTotal;
        _currencySymbol = currencySymbol;

        _products = products
            .Select(p => new ProductOption(p.Id, p.Name, p.SalePrice))
            .ToList();

        Size = new Size(440, 620);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = AppColors.Surface;

        _btnApply = ButtonFactory.Primary(AppStrings.Save, ButtonFactory.ButtonSize.Large);
        _btnApply.Click += (_, _) => Apply();

        _btnCancel = ButtonFactory.Neutral(AppStrings.Cancel, ButtonFactory.ButtonSize.Large);
        _btnCancel.DialogResult = DialogResult.Cancel;

        BuildLayout();

        LoadText();

        _languageService.LanguageChanged += OnLanguageChanged;
        _languageSubscribed = true;
    }

    // ------------------------------------------------------------------ UI

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 7,
            Margin = Padding.Empty,
            Padding = new Padding(AppStyle.SpaceLG),
            BackColor = AppColors.Surface
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));   // target label
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));   // target combo
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));   // context
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));   // display
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));   // hint
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));   // keypad
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));   // actions

        _lblTarget.Dock = DockStyle.Fill;
        _lblTarget.TextAlign = ContentAlignment.MiddleLeft;
        _lblTarget.Font = new Font("Segoe UI", 9.5F);
        _lblTarget.ForeColor = AppColors.InkMuted;

        _cmbTarget.Dock = DockStyle.Fill;
        _cmbTarget.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbTarget.Font = new Font("Segoe UI", 10F);
        _cmbTarget.Items.AddRange(new object[]
        {
            new TargetItem(CalculatorTarget.ProductLine),
            new TargetItem(CalculatorTarget.CustomLine),
            new TargetItem(CalculatorTarget.SetPaid),
            new TargetItem(CalculatorTarget.SetDiscount),
            new TargetItem(CalculatorTarget.SetTax)
        });
        _cmbTarget.SelectedIndex = 0;
        _cmbTarget.SelectedIndexChanged += (_, _) => OnTargetChanged();

        // --- product context (product + unit price) ---
        _productContext.Dock = DockStyle.Fill;
        _productContext.Padding = Padding.Empty;

        var productRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        productRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
        productRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
        productRow.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        productRow.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

        _lblProduct.Dock = DockStyle.Fill;
        _lblProduct.TextAlign = ContentAlignment.MiddleLeft;
        _lblProduct.Font = new Font("Segoe UI", 9F);
        _lblProduct.ForeColor = AppColors.InkMuted;

        _cmbProduct.Dock = DockStyle.Fill;
        _cmbProduct.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbProduct.Font = new Font("Segoe UI", 9.5F);
        _cmbProduct.DisplayMember = nameof(ProductOption.Display);
        _cmbProduct.ValueMember = nameof(ProductOption.Id);
        _cmbProduct.DataSource = _products;
        _cmbProduct.SelectedIndexChanged += (_, _) => OnProductChanged();

        _lblPrice.Dock = DockStyle.Fill;
        _lblPrice.TextAlign = ContentAlignment.MiddleLeft;
        _lblPrice.Font = new Font("Segoe UI", 9F);
        _lblPrice.ForeColor = AppColors.InkMuted;

        _numPrice.Dock = DockStyle.Fill;
        _numPrice.DecimalPlaces = 2;
        _numPrice.Minimum = 0;
        _numPrice.Maximum = 9999999;
        _numPrice.Font = new Font("Segoe UI", 9.5F);
        _numPrice.ValueChanged += (_, _) => UpdateHint();

        productRow.Controls.Add(_lblProduct, 0, 0);
        productRow.Controls.Add(_cmbProduct, 1, 0);
        productRow.Controls.Add(_lblPrice, 0, 1);
        productRow.Controls.Add(_numPrice, 1, 1);
        _productContext.Controls.Add(productRow);

        // --- description context (free line) ---
        _descriptionContext.Dock = DockStyle.Fill;
        _descriptionContext.Padding = Padding.Empty;
        _descriptionContext.Visible = false;

        var descriptionRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        descriptionRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        descriptionRow.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        descriptionRow.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

        _lblDescription.Dock = DockStyle.Fill;
        _lblDescription.TextAlign = ContentAlignment.MiddleLeft;
        _lblDescription.Font = new Font("Segoe UI", 9F);
        _lblDescription.ForeColor = AppColors.InkMuted;

        _txtDescription.Dock = DockStyle.Fill;
        _txtDescription.Font = new Font("Segoe UI", 9.5F);
        _txtDescription.TextChanged += (_, _) => UpdateHint();

        descriptionRow.Controls.Add(_lblDescription, 0, 0);
        descriptionRow.Controls.Add(_txtDescription, 0, 1);
        _descriptionContext.Controls.Add(descriptionRow);

        var contextHost = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty, Padding = Padding.Empty };
        contextHost.Controls.Add(_descriptionContext);
        contextHost.Controls.Add(_productContext);

        _display.Dock = DockStyle.Fill;
        _display.TextAlign = ContentAlignment.MiddleRight;
        _display.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
        _display.ForeColor = AppColors.Ink;
        _display.BackColor = AppColors.Background;
        _display.Padding = new Padding(10, 0, 10, 0);
        _display.Text = "0";

        _hint.Dock = DockStyle.Fill;
        _hint.TextAlign = ContentAlignment.MiddleRight;
        _hint.Font = new Font("Segoe UI", 9F);
        _hint.ForeColor = AppColors.InkMuted;
        _hint.AutoEllipsis = true;

        var keypad = BuildKeypad();

        var actions = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        actions.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        _btnApply.Dock = DockStyle.Fill;
        _btnApply.Margin = new Padding(0, 0, AppStyle.SpaceSM, 0);
        _btnCancel.Dock = DockStyle.Fill;
        _btnCancel.Margin = new Padding(AppStyle.SpaceSM, 0, 0, 0);

        actions.Controls.Add(_btnApply, 0, 0);
        actions.Controls.Add(_btnCancel, 1, 0);

        root.Controls.Add(_lblTarget, 0, 0);
        root.Controls.Add(_cmbTarget, 0, 1);
        root.Controls.Add(contextHost, 0, 2);
        root.Controls.Add(_display, 0, 3);
        root.Controls.Add(_hint, 0, 4);
        root.Controls.Add(keypad, 0, 5);
        root.Controls.Add(actions, 0, 6);

        Controls.Add(root);
    }

    private Control BuildKeypad()
    {
        var pad = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 5,
            Margin = new Padding(0, AppStyle.SpaceSM, 0, 0),
            Padding = Padding.Empty
        };
        for (int i = 0; i < 4; i++) pad.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        for (int i = 0; i < 5; i++) pad.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));

        void Key(string text, char op, int row, int col, int span = 1, ButtonTone tone = ButtonTone.Neutral)
        {
            var button = new ThemedButton
            {
                Text = text,
                Tone = tone,
                Dock = DockStyle.Fill,
                Margin = new Padding(3),
                Font = new Font("Segoe UI", 13F, FontStyle.Bold)
            };
            button.ApplyTheme();
            button.Click += (_, _) => Press(op, text);
            if (span > 1) pad.SetColumnSpan(button, span);
            pad.Controls.Add(button, col, row);
        }

        Key("7", '7', 0, 0);
        Key("8", '8', 0, 1);
        Key("9", '9', 0, 2);
        Key("÷", '/', 0, 3, 1, ButtonTone.Info);

        Key("4", '4', 1, 0);
        Key("5", '5', 1, 1);
        Key("6", '6', 1, 2);
        Key("×", '*', 1, 3, 1, ButtonTone.Info);

        Key("1", '1', 2, 0);
        Key("2", '2', 2, 1);
        Key("3", '3', 2, 2);
        Key("−", '-', 2, 3, 1, ButtonTone.Info);

        Key("0", '0', 3, 0);
        Key(".", '.', 3, 1);
        Key("⌫", '\b', 3, 2, 1, ButtonTone.Warning);
        Key("C", 'c', 3, 3, 1, ButtonTone.Danger);

        Key("+", '+', 4, 0, 3, ButtonTone.Info);
        Key("=", '=', 4, 3, 1, ButtonTone.Success);

        return pad;
    }

    // ------------------------------------------------------------- behavior

    private void Press(char op, string text)
    {
        switch (op)
        {
            case 'c':
                _entry = "0";
                _accumulator = null;
                _pendingOperator = '\0';
                _replaceEntry = true;
                break;

            case '\b':
                if (!_replaceEntry && _entry.Length > 1)
                    _entry = _entry[..^1];
                else if (!_replaceEntry)
                    _entry = "0";
                _replaceEntry = false;
                break;

            case '=':
                Evaluate();
                break;

            case '+':
            case '-':
            case '*':
            case '/':
                ApplyPending();
                _pendingOperator = op;
                _replaceEntry = true;
                break;

            case '.':
                if (_replaceEntry)
                {
                    _entry = "0.";
                    _replaceEntry = false;
                }
                else if (!_entry.Contains('.'))
                {
                    _entry += ".";
                }
                break;

            default:
                if (op >= '0' && op <= '9')
                {
                    _entry = _replaceEntry || _entry == "0"
                        ? op.ToString()
                        : _entry + op;
                    _replaceEntry = false;
                }
                break;
        }

        RefreshDisplay();
    }

    private void ApplyPending()
    {
        if (_accumulator is null || _pendingOperator == '\0')
        {
            _accumulator = CurrentValue;
            return;
        }

        _accumulator = Compute(_accumulator.Value, CurrentValue, _pendingOperator);
    }

    private void Evaluate()
    {
        if (_pendingOperator != '\0')
        {
            _accumulator = Compute(_accumulator ?? CurrentValue, CurrentValue, _pendingOperator);
            _pendingOperator = '\0';
        }
        else
        {
            _accumulator = CurrentValue;
        }

        _entry = Format(_accumulator.Value);
        _replaceEntry = true;
    }

    private static decimal Compute(decimal left, decimal right, char op) => op switch
    {
        '+' => left + right,
        '-' => left - right,
        '*' => left * right,
        '/' => right == 0 ? left : left / right,
        _ => right
    };

    private decimal CurrentValue =>
        decimal.TryParse(_entry, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.CurrentCulture, out decimal value)
            ? value
            : 0m;

    private static string Format(decimal value) =>
        value.ToString("0.##", System.Globalization.CultureInfo.CurrentCulture);

    private void RefreshDisplay()
    {
        _display.Text = _entry;
        UpdateHint();
    }

    private void OnTargetChanged()
    {
        var target = SelectedTarget;

        _productContext.Visible = target == CalculatorTarget.ProductLine;
        _descriptionContext.Visible = target == CalculatorTarget.CustomLine;

        UpdateHint();
    }

    private void OnProductChanged()
    {
        if (_cmbProduct.SelectedItem is ProductOption option)
            _numPrice.Value = Clamp(option.Price);

        UpdateHint();
    }

    private decimal Clamp(decimal value) =>
        value < _numPrice.Minimum ? _numPrice.Minimum :
        value > _numPrice.Maximum ? _numPrice.Maximum : value;

    private CalculatorTarget SelectedTarget =>
        _cmbTarget.SelectedItem is TargetItem item ? item.Target : CalculatorTarget.ProductLine;

    /// <summary>Explains what will happen with the current result.</summary>
    private void UpdateHint()
    {
        if (_hint is null) return;

        decimal value = CurrentValue;

        switch (SelectedTarget)
        {
            case CalculatorTarget.ProductLine:
                decimal price = _numPrice.Value;
                decimal quantity = price > 0 ? Math.Round(value / price, 3) : 0;
                _hint.Text = price > 0
                    ? $"{Format(quantity)} × {Format(price)} = {Format(value)} {_currencySymbol}"
                    : string.Empty;
                break;

            case CalculatorTarget.CustomLine:
                _hint.Text = string.IsNullOrWhiteSpace(_txtDescription.Text)
                    ? string.Empty
                    : $"{_txtDescription.Text.Trim()} — {Format(value)} {_currencySymbol}";
                break;

            case CalculatorTarget.SetPaid:
                decimal remaining = _cartTotal - value;
                _hint.Text = $"{AppStrings.ChangeDue}: {Format(remaining < 0 ? 0 : remaining)} {_currencySymbol}";
                break;

            case CalculatorTarget.SetDiscount:
            case CalculatorTarget.SetTax:
                _hint.Text = $"{Format(value)} %";
                break;
        }
    }

    private void Apply()
    {
        decimal value = CurrentValue;

        var target = SelectedTarget;

        if (target == CalculatorTarget.ProductLine)
        {
            if (_cmbProduct.SelectedItem is not ProductOption option)
            {
                AppMessageBox.Warning(AppStrings.SelectProductToAdd);
                return;
            }

            Outcome = new CalculatorOutcome
            {
                Value = value,
                Target = target,
                ProductId = option.Id,
                UnitPrice = _numPrice.Value
            };
        }
        else if (target == CalculatorTarget.CustomLine)
        {
            if (string.IsNullOrWhiteSpace(_txtDescription.Text))
            {
                AppMessageBox.Warning(AppStrings.RequiredField);
                return;
            }

            Outcome = new CalculatorOutcome
            {
                Value = value,
                Target = target,
                Description = _txtDescription.Text.Trim()
            };
        }
        else
        {
            Outcome = new CalculatorOutcome { Value = value, Target = target };
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    // ---------------------------------------------------------- localization

    private void OnLanguageChanged(object? sender, string culture)
    {
        if (IsDisposed) return;
        if (InvokeRequired) { BeginInvoke(LoadText); return; }
        LoadText();
    }

    private void LoadText()
    {
        Text = AppStrings.CalculatorTitle;

        _lblTarget.Text = AppStrings.CalculatorTarget;

        foreach (TargetItem item in _cmbTarget.Items)
        {
            item.Label = item.Target switch
            {
                CalculatorTarget.ProductLine => AppStrings.CalculatorProductLine,
                CalculatorTarget.CustomLine => AppStrings.CalculatorCustomLine,
                CalculatorTarget.SetPaid => AppStrings.CalculatorSetPaid,
                CalculatorTarget.SetDiscount => AppStrings.CalculatorSetDiscount,
                _ => AppStrings.CalculatorSetTax
            };
        }
        _cmbTarget.Refresh();

        _lblProduct.Text = AppStrings.SelectProductToAdd;
        _lblPrice.Text = AppStrings.UnitPrice;
        _lblDescription.Text = AppStrings.LineDescription;

        if (_btnApply is not null) _btnApply.Text = AppStrings.Save;
        if (_btnCancel is not null) _btnCancel.Text = AppStrings.Cancel;

        UpdateHint();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _languageSubscribed)
        {
            _languageService.LanguageChanged -= OnLanguageChanged;
            _languageSubscribed = false;
        }
        base.Dispose(disposing);
    }

    // --------------------------------------------------------------- models

    private sealed class TargetItem
    {
        public TargetItem(CalculatorTarget target) => Target = target;

        public CalculatorTarget Target { get; }

        public string Label { get; set; } = string.Empty;

        public override string ToString() => Label;
    }

    private sealed record ProductOption(Guid Id, string Name, decimal Price)
    {
        public string Display => $"{Name} — {Price:0.##}";
    }
}
