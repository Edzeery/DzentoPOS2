using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;
using DzentoPOS2.WinForms.Controls;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Forms.Inventory;

/// <summary>
/// Bulk import of digital codes for one variant. The operator pastes codes
/// (one per line) or picks a text file; duplicates already present for the
/// variant are skipped.
/// </summary>
public sealed class DigitalCodeImportDialog : ThemedDialog
{
    private readonly IDigitalCodeService _service;
    private readonly ILanguageService _languageService;
    private readonly Guid? _preselectedVariantId;

    private List<DigitalVariantOptionDto> _variants = new();

    private readonly ThemedComboBox _cmbVariant = new()
    {
        Dock = DockStyle.Fill,
        Margin = new Padding(0, 6, 0, 6)
    };

    private readonly TextBox _txtCodes = new()
    {
        Dock = DockStyle.Fill,
        Multiline = true,
        ScrollBars = ScrollBars.Both,
        WordWrap = false,
        AcceptsReturn = true,
        Font = new Font("Consolas", 10F),
        MinimumSize = new Size(0, 120),
        Margin = new Padding(0, 6, 0, 6)
    };

    private readonly Label _lblVariant = new() { AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 6, 12, 0) };
    private readonly Label _lblHint = new() { AutoSize = true, Margin = new Padding(0, 0, 0, 6) };
    private readonly Button _btnImport = ButtonFactory.Primary(AppStrings.DigitalCodeImport);
    private readonly Button _btnCancel = ButtonFactory.Neutral(AppStrings.Cancel);

    private bool _languageSubscribed;

    /// <summary>Import outcome once the dialog closed with <see cref="DialogResult.OK"/>.</summary>
    public DigitalCodeImportResultDto? Result { get; private set; }

    public DigitalCodeImportDialog(
        IDigitalCodeService service,
        ILanguageService languageService,
        Guid? preselectedVariantId = null)
    {
        _service = service;
        _languageService = languageService;
        _preselectedVariantId = preselectedVariantId;

        Size = new Size(560, 480);
        MinimumSize = new Size(360, 340);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = false;
        MinimizeBox = false;
        AutoScaleMode = AutoScaleMode.Font;

        _btnImport.AutoSize = true;
        _btnImport.MinimumSize = new Size(140, 40);
        _btnImport.Margin = new Padding(0, 0, 8, 0);
        _btnImport.Click += async (_, _) => await ImportAsync();

        _btnCancel.AutoSize = true;
        _btnCancel.MinimumSize = new Size(120, 40);
        _btnCancel.DialogResult = DialogResult.Cancel;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            Padding = new Padding(20, 16, 20, 16),
            Margin = Padding.Empty
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        root.Controls.Add(_lblVariant, 0, 0);
        root.Controls.Add(_cmbVariant, 1, 0);
        root.Controls.Add(_lblHint, 0, 1);
        root.SetColumnSpan(_lblHint, 2);
        root.Controls.Add(_txtCodes, 0, 2);
        root.SetColumnSpan(_txtCodes, 2);

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
        buttons.Controls.Add(_btnImport);
        root.Controls.Add(buttons, 0, 3);
        root.SetColumnSpan(buttons, 2);

        Controls.Add(root);

        LoadText();
        _languageService.LanguageChanged += OnLanguageChanged;
        _languageSubscribed = true;

        Load += async (_, _) => await LoadVariantsAsync();
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

    private void OnLanguageChanged(object? sender, string culture)
    {
        if (IsDisposed) return;
        if (InvokeRequired) BeginInvoke(LoadText);
        else LoadText();
    }

    private void LoadText()
    {
        Text = AppStrings.DigitalCodeImportTitle;
        _lblVariant.Text = AppStrings.DigitalCodeVariantColumn;
        _lblHint.Text = AppStrings.DigitalCodeImportHint;
        _btnImport.Text = AppStrings.DigitalCodeImport;
        _btnCancel.Text = AppStrings.Cancel;
    }

    private async Task LoadVariantsAsync()
    {
        try
        {
            _variants = await _service.GetDigitalVariantsAsync();

            _cmbVariant.Items = _variants.Select(v => v.DisplayName).ToArray();

            if (_variants.Count == 0)
                return;

            var index = 0;
            if (_preselectedVariantId.HasValue)
            {
                var found = _variants.FindIndex(v => v.VariantId == _preselectedVariantId.Value);
                if (found >= 0)
                    index = found;
            }

            _cmbVariant.SelectedIndex = index;
        }
        catch (Exception ex)
        {
            AppMessageBox.Error(ex, "DigitalCodeImport.LoadVariants");
        }
    }

    private DigitalVariantOptionDto? SelectedVariant()
    {
        var index = _cmbVariant.SelectedIndex;
        return index >= 0 && index < _variants.Count ? _variants[index] : null;
    }

    private async Task ImportAsync()
    {
        if (SelectedVariant() is not { } variant)
        {
            MessageBox.Show(
                AppStrings.DigitalCodeImportNoVariant,
                AppStrings.Warning,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        var codes = _txtCodes.Text
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        if (codes.Length == 0)
        {
            MessageBox.Show(
                AppStrings.DigitalCodeImportEmpty,
                AppStrings.Warning,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        try
        {
            Result = await _service.ImportAsync(variant.VariantId, codes);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, AppStrings.ErrorMsg, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
