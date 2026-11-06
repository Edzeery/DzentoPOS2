using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Forms.Inventory;

public class ProductUnitEditDialog : ThemedDialog
{
    private readonly IProductUnitService _unitService;
    private readonly ILanguageService _languageService;
    private bool _languageSubscribed;
    private readonly ProductUnitDto? _existing;
    private readonly Label _lblName;
    private readonly Label _lblSymbol;
    private readonly TextBox _txtName;
    private readonly TextBox _txtSymbol;
    private readonly CheckBox _chkBaseUnit;
    private readonly Button _btnSave;
    private readonly Button _btnCancel;

    public ProductUnitEditDialog(
        IProductUnitService unitService,
        ILanguageService languageService,
        ProductUnitDto? existing = null)
    {
        _unitService = unitService;
        _languageService = languageService;
        _existing = existing;

        Size = new Size(520, 380);
        MinimumSize = new Size(320, 280);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = AppColors.Surface;
        AutoScaleMode = AutoScaleMode.Font;

        _lblName = FieldLabel(AppStrings.UnitName);
        _lblSymbol = FieldLabel(AppStrings.Symbol);

        _txtName = new TextBox { Dock = DockStyle.Fill, MinimumSize = new Size(0, 28), Margin = new Padding(0, 6, 0, 6) };
        _txtSymbol = new TextBox { Dock = DockStyle.Fill, MinimumSize = new Size(0, 28), Margin = new Padding(0, 6, 0, 6) };

        _chkBaseUnit = new CheckBox
        {
            Text = AppStrings.BaseUnit,
            Dock = DockStyle.Fill,
            AutoSize = false,
            Height = 32,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 8, 0, 8)
        };

        _btnSave = ButtonFactory.Primary(AppStrings.Save);
        _btnSave.AutoSize = true;
        _btnSave.MinimumSize = new Size(120, 40);
        _btnSave.Margin = new Padding(0, 0, 8, 0);
        _btnSave.Click += async (_, _) => await SaveAsync();

        _btnCancel = ButtonFactory.Neutral(AppStrings.Cancel);
        _btnCancel.AutoSize = true;
        _btnCancel.MinimumSize = new Size(120, 40);
        _btnCancel.DialogResult = DialogResult.Cancel;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(20, 16, 20, 16),
            Margin = Padding.Empty
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        root.Controls.Add(_lblName, 0, 0);
        root.Controls.Add(_txtName, 1, 0);
        root.Controls.Add(_lblSymbol, 0, 1);
        root.Controls.Add(_txtSymbol, 1, 1);
        root.Controls.Add(_chkBaseUnit, 0, 2);
        root.SetColumnSpan(_chkBaseUnit, 2);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = true,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 8, 0, 0)
        };
        buttons.Controls.Add(_btnCancel);
        buttons.Controls.Add(_btnSave);
        root.Controls.Add(buttons, 0, 4);
        root.SetColumnSpan(buttons, 2);

        Controls.Add(root);

        if (existing != null)
        {
            _txtName.Text = existing.Name;
            _txtSymbol.Text = existing.Symbol;
            _chkBaseUnit.Checked = existing.IsBaseUnit;
        }

        LoadText();
        _languageService.LanguageChanged += OnLanguageChanged;
        _languageSubscribed = true;
    }

    private static Label FieldLabel(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Anchor = AnchorStyles.Left,
        Margin = new Padding(0, 6, 12, 0)
    };

    private void OnLanguageChanged(object? sender, string culture)
    {
        if (IsDisposed) return;
        if (InvokeRequired) BeginInvoke(LoadText);
        else LoadText();
    }

    private void LoadText()
    {
        Text = _existing == null ? AppStrings.AddUnit : AppStrings.EditUnit;
        _lblName.Text = AppStrings.UnitName;
        _lblSymbol.Text = AppStrings.Symbol;
        _chkBaseUnit.Text = AppStrings.BaseUnit;
        _btnSave.Text = AppStrings.Save;
        _btnCancel.Text = AppStrings.Cancel;
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

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_txtName.Text))
        {
            MessageBox.Show(AppStrings.ErrorMsg, AppStrings.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            if (_existing == null)
            {
                await _unitService.CreateAsync(new CreateProductUnitDto
                {
                    Name = _txtName.Text.Trim(),
                    Symbol = _txtSymbol.Text.Trim(),
                    IsBaseUnit = _chkBaseUnit.Checked
                });
            }
            else
            {
                await _unitService.UpdateAsync(new UpdateProductUnitDto
                {
                    Id = _existing.Id,
                    Name = _txtName.Text.Trim(),
                    Symbol = _txtSymbol.Text.Trim(),
                    IsBaseUnit = _chkBaseUnit.Checked
                });
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, AppStrings.ErrorMsg, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
