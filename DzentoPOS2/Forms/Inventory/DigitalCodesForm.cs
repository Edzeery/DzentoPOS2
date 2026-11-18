using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;
using DzentoPOS2.WinForms.Controls;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;
using EZControls.Controls.Inputs;
using FontAwesome.Sharp;

namespace DzentoPOS2.WinForms.Forms.Inventory;

/// <summary>
/// Management screen for digital codes (prepaid cards / activation keys):
/// import a batch, list and search them, and cancel a code from circulation.
/// Hosted as a tab inside <see cref="ProductsHubForm"/>.
/// </summary>
public partial class DigitalCodesForm : LocalizableFormBase
{
    private readonly IDigitalCodeService _service;

    private readonly TableLayoutPanel _topPanel;
    private readonly FlowLayoutPanel _filters;
    private readonly IconButton _btnImport;
    private readonly IconButton _btnCancel;
    private readonly IconButton _btnRefresh;
    private readonly ThemedComboBox _cmbVariant;
    private readonly ThemedComboBox _cmbStatus;
    private readonly EZTextBox _searchBox;
    private readonly Label _lblVariant;
    private readonly Label _lblStatus;
    private readonly Label _lblSearch;
    private readonly Label _lblAvailable;
    private readonly DataGridView _grid;

    private List<DigitalVariantOptionDto> _variantOptions = new();
    private List<Row> _rows = new();
    private bool _suppressFilter;

    public DigitalCodesForm(
        IDigitalCodeService service,
        ILanguageService languageService)
        : base(languageService)
    {
        _service = service;
        Size = new Size(1100, 650);
        MinimumSize = new Size(320, 300);
        BackColor = AppColors.Background;
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Font;

        _btnImport = ButtonFactory.Tool(IconChar.FileImport, AppStrings.DigitalCodeImport, ButtonTone.Primary);
        _btnImport.Click += (_, _) => RunSafe(ImportAsync);

        _btnCancel = ButtonFactory.Tool(IconChar.Ban, AppStrings.Cancel, ButtonTone.Danger);
        _btnCancel.Click += (_, _) => RunSafe(CancelCodeAsync);

        _btnRefresh = ButtonFactory.Tool(IconChar.Sync, AppStrings.Refresh, ButtonTone.Neutral);
        _btnRefresh.Click += (_, _) => RunSafe(LoadCodesAsync);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = Padding.Empty
        };
        buttons.Controls.AddRange(new Control[] { _btnImport, _btnCancel, _btnRefresh });

        _lblVariant = FilterLabel();
        _lblStatus = FilterLabel();
        _lblSearch = FilterLabel();
        _lblAvailable = new Label
        {
            AutoSize = true,
            Margin = new Padding(12, 10, 0, 0)
        };

        _cmbVariant = FilterCombo();
        _cmbVariant.SelectedIndexChanged += OnVariantChanged;

        _cmbStatus = FilterCombo();
        _cmbStatus.SelectedIndexChanged += OnStatusChanged;

        _searchBox = new EZTextBox
        {
            Size = new Size(220, 36),
            MinimumSize = new Size(0, 32),
            Font = new Font("Segoe UI", 10F),
            PlaceholderText = AppStrings.Search,
            Margin = new Padding(0, 6, 0, 0)
        };
        _searchBox.TextChanged += (_, _) => RunSafe(LoadCodesAsync);

        _filters = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 6, 0, 0)
        };
        _filters.Controls.AddRange(new Control[]
        {
            _lblVariant, _cmbVariant,
            _lblStatus, _cmbStatus,
            _lblSearch, _searchBox,
            _lblAvailable
        });

        _topPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = AppColors.Surface,
            Padding = new Padding(16, 10, 16, 10),
            Margin = Padding.Empty
        };
        _topPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _topPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _topPanel.Controls.Add(buttons, 0, 0);
        _topPanel.Controls.Add(_filters, 0, 1);

        _grid = new DataGridView { Dock = DockStyle.Fill };
        DataGridViewStyleHelper.ApplyModernStyle(_grid);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.Controls.Add(_topPanel, 0, 0);
        root.Controls.Add(_grid, 0, 1);
        Controls.Add(root);
    }

    private static Label FilterLabel() => new()
    {
        AutoSize = true,
        Margin = new Padding(0, 12, 6, 0)
    };

    private static ThemedComboBox FilterCombo() => new()
    {
        Size = new Size(200, 36),
        MinimumSize = new Size(140, 36),
        Margin = new Padding(0, 6, 12, 0)
    };

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        RunSafe(InitializeAsync);
    }

    protected override void LoadText()
    {
        Text = AppStrings.DigitalCodesTitle;
        _lblVariant.Text = AppStrings.DigitalCodeVariantColumn + ":";
        _lblStatus.Text = AppStrings.DigitalCodeStatusColumn + ":";
        _lblSearch.Text = AppStrings.Search + ":";
        _searchBox.PlaceholderText = AppStrings.Search;
        _btnImport.Text = "  " + AppStrings.DigitalCodeImport;
        _btnCancel.Text = "  " + AppStrings.Cancel;
        _btnRefresh.Text = "  " + AppStrings.Refresh;

        RebuildVariantCombo();
        RebuildStatusCombo();
        BuildGridColumns();
    }

    protected override void OnLanguageChangedAfterLoadText()
    {
        RunSafe(LoadCodesAsync);
    }

    private async Task InitializeAsync()
    {
        try
        {
            _variantOptions = await _service.GetDigitalVariantsAsync();
        }
        catch (Exception ex)
        {
            AppMessageBox.Error(ex, "DigitalCodes.LoadVariants");
            _variantOptions = new List<DigitalVariantOptionDto>();
        }

        RebuildVariantCombo();
        await LoadCodesAsync();
    }

    private void RebuildVariantCombo()
    {
        if (IsDisposedOrDisposing) return;

        var selected = SelectedVariantId();

        var items = new List<string> { AppStrings.DigitalCodeAllVariants };
        items.AddRange(_variantOptions.Select(v => v.DisplayName));

        var index = 0;
        if (selected.HasValue)
        {
            var found = _variantOptions.FindIndex(v => v.VariantId == selected.Value);
            if (found >= 0)
                index = found + 1;
        }

        _suppressFilter = true;
        _cmbVariant.Items = items.ToArray();
        _cmbVariant.SelectedIndex = index;
        _suppressFilter = false;
    }

    private void RebuildStatusCombo()
    {
        if (IsDisposedOrDisposing) return;

        var index = SelectedStatus() switch
        {
            DigitalCodeStatus.Available => 1,
            DigitalCodeStatus.Delivered => 2,
            DigitalCodeStatus.Cancelled => 3,
            _ => 0
        };

        _suppressFilter = true;
        _cmbStatus.Items = new[]
        {
            AppStrings.DigitalCodeAllStatuses,
            AppStrings.DigitalCodeStatusAvailable,
            AppStrings.DigitalCodeStatusDelivered,
            AppStrings.DigitalCodeStatusCancelled
        };
        _cmbStatus.SelectedIndex = index;
        _suppressFilter = false;
    }

    private void OnVariantChanged(object? sender, EventArgs e)
    {
        if (_suppressFilter) return;
        RunSafe(LoadCodesAsync);
    }

    private void OnStatusChanged(object? sender, EventArgs e)
    {
        if (_suppressFilter) return;
        RunSafe(LoadCodesAsync);
    }

    private Guid? SelectedVariantId()
    {
        var index = _cmbVariant.SelectedIndex;
        if (index <= 0 || index > _variantOptions.Count)
            return null;

        return _variantOptions[index - 1].VariantId;
    }

    private DigitalCodeStatus? SelectedStatus() => _cmbStatus.SelectedIndex switch
    {
        1 => DigitalCodeStatus.Available,
        2 => DigitalCodeStatus.Delivered,
        3 => DigitalCodeStatus.Cancelled,
        _ => null
    };

    private void BuildGridColumns()
    {
        _grid.DataSource = null;
        _grid.Columns.Clear();
        _grid.AutoGenerateColumns = false;

        _grid.Columns.AddRange(new[]
        {
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.DigitalCodeColumn, DataPropertyName = "Code", Width = 200 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.DigitalCodeVariantColumn, DataPropertyName = "VariantName", Width = 220 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.DigitalCodeStatusColumn, DataPropertyName = "StatusText", Width = 110 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.DigitalCodeDeliveredAtColumn, DataPropertyName = "DeliveredAt", Width = 150 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.DigitalCodeInvoiceColumn, DataPropertyName = "InvoiceNumber", Width = 130 }
        });
    }

    private async Task LoadCodesAsync()
    {
        var variant = SelectedVariantId();
        var status = SelectedStatus();
        var term = _searchBox.Text?.Trim();

        var codes = await _service.SearchAsync(variant, status, term);

        if (IsDisposedOrDisposing) return;

        _rows = codes.Select(c => new Row
        {
            Id = c.Id,
            Code = c.Code,
            VariantName = c.VariantName,
            Status = c.Status,
            StatusText = StatusText(c.Status),
            DeliveredAt = c.DeliveredAt?.ToLocalTime().ToString("g") ?? string.Empty,
            InvoiceNumber = c.InvoiceNumber ?? string.Empty
        }).ToList();

        if (_grid.Columns.Count == 0)
            BuildGridColumns();

        _grid.DataSource = _rows;
        await UpdateAvailableCountAsync();
    }

    private async Task UpdateAvailableCountAsync()
    {
        var variant = SelectedVariantId();
        if (!variant.HasValue || variant.Value == Guid.Empty)
        {
            _lblAvailable.Text = string.Empty;
            return;
        }

        var count = await _service.GetAvailableCountAsync(variant.Value);
        if (IsDisposedOrDisposing) return;

        _lblAvailable.Text = string.Format(AppStrings.DigitalCodeAvailableCountFormat, count);
    }

    private async Task ImportAsync()
    {
        var variant = SelectedVariantId();

        using var dialog = new DigitalCodeImportDialog(_service, _languageService, variant);
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Result is null)
            return;

        AppMessageBox.Success(string.Format(
            AppStrings.DigitalCodeImportResultFormat,
            dialog.Result.Imported,
            dialog.Result.Skipped));

        await LoadCodesAsync();
    }

    private async Task CancelCodeAsync()
    {
        if (_grid.CurrentRow?.DataBoundItem is not Row row)
            return;

        if (row.Status != DigitalCodeStatus.Available)
        {
            AppMessageBox.Warning(AppStrings.OperationFailed);
            return;
        }

        var result = MessageBox.Show(
            $"{AppStrings.DigitalCodeCancelConfirm}\n{row.Code}",
            AppStrings.DeleteConfirmation,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result != DialogResult.Yes)
            return;

        await _service.CancelAsync(row.Id);
        await LoadCodesAsync();
    }

    private static string StatusText(DigitalCodeStatus status) => status switch
    {
        DigitalCodeStatus.Available => AppStrings.DigitalCodeStatusAvailable,
        DigitalCodeStatus.Delivered => AppStrings.DigitalCodeStatusDelivered,
        DigitalCodeStatus.Cancelled => AppStrings.DigitalCodeStatusCancelled,
        _ => status.ToString()
    };

    private sealed class Row
    {
        public Guid Id { get; init; }
        public string Code { get; init; } = string.Empty;
        public string VariantName { get; init; } = string.Empty;
        public DigitalCodeStatus Status { get; init; }
        public string StatusText { get; init; } = string.Empty;
        public string DeliveredAt { get; init; } = string.Empty;
        public string InvoiceNumber { get; init; } = string.Empty;
    }
}
