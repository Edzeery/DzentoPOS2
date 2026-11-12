using DzentoPOS2.App.Services.Businesses;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Businesses;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Forms.Businesses;

/// <summary>
/// Manages the warehouses and cashboxes of a single branch. Nothing is ever
/// deleted: entries are added and deactivated/reactivated, and the service
/// refuses to leave a branch without an active warehouse and cashbox or to
/// deactivate a cashbox with an open session.
/// </summary>
public class BranchResourcesDialog : ThemedDialog
{
    private readonly IBranchResourceService _resourceService;
    private readonly ILanguageService _languageService;
    private bool _languageSubscribed;
    private readonly BranchDto _branch;

    private readonly FlowLayoutPanel _warehousesBar;
    private readonly Label _lblWarehouses;
    private readonly TextBox _wNameBox;
    private readonly TextBox _wLocationBox;
    private readonly Button _btnAddWarehouse;
    private readonly Button _btnToggleWarehouse;
    private readonly DataGridView _warehouseGrid;

    private readonly FlowLayoutPanel _cashboxesBar;
    private readonly Label _lblCashboxes;
    private readonly TextBox _cNameBox;
    private readonly Button _btnAddCashbox;
    private readonly Button _btnToggleCashbox;
    private readonly DataGridView _cashboxGrid;

    private readonly Button _btnClose;

    private List<WarehouseListItemDto> _warehouses = new();
    private List<CashboxListItemDto> _cashboxes = new();

    public BranchResourcesDialog(
        IBranchResourceService resourceService,
        ILanguageService languageService,
        BranchDto branch)
    {
        _resourceService = resourceService;
        _languageService = languageService;
        _branch = branch;

        Size = new Size(980, 660);
        MinimumSize = new Size(340, 380);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = AppColors.Surface;
        AutoScaleMode = AutoScaleMode.Font;

        // ——— Warehouses section ———
        _lblWarehouses = SectionLabel();
        _wNameBox = InputBox();
        _wLocationBox = InputBox();
        _btnAddWarehouse = ButtonFactory.Primary(AppStrings.AddWarehouse);
        _btnAddWarehouse.AutoSize = true;
        _btnAddWarehouse.MinimumSize = new Size(110, 34);
        _btnAddWarehouse.Click += async (_, _) => await GuardAsync(AddWarehouseAsync);

        _btnToggleWarehouse = ButtonFactory.Neutral(AppStrings.Deactivate);
        _btnToggleWarehouse.AutoSize = true;
        _btnToggleWarehouse.MinimumSize = new Size(110, 34);
        _btnToggleWarehouse.Click += async (_, _) => await GuardAsync(ToggleWarehouseAsync);

        _warehousesBar = Toolbar();
        _warehousesBar.Controls.AddRange(new Control[]
        {
            _lblWarehouses, _wNameBox, _wLocationBox, _btnAddWarehouse, _btnToggleWarehouse
        });

        _warehouseGrid = Grid();
        _warehouseGrid.SelectionChanged += (_, _) => UpdateToggles();
        _warehouseGrid.Columns.AddRange(new[]
        {
            new DataGridViewTextBoxColumn { DataPropertyName = "Name", FillWeight = 140 },
            new DataGridViewTextBoxColumn { DataPropertyName = "Location", FillWeight = 140 },
            new DataGridViewTextBoxColumn { DataPropertyName = "IsActive", FillWeight = 50 }
        });

        // ——— Cashboxes section ———
        _lblCashboxes = SectionLabel();
        _cNameBox = InputBox();
        _btnAddCashbox = ButtonFactory.Primary(AppStrings.AddCashbox);
        _btnAddCashbox.AutoSize = true;
        _btnAddCashbox.MinimumSize = new Size(110, 34);
        _btnAddCashbox.Click += async (_, _) => await GuardAsync(AddCashboxAsync);

        _btnToggleCashbox = ButtonFactory.Neutral(AppStrings.Deactivate);
        _btnToggleCashbox.AutoSize = true;
        _btnToggleCashbox.MinimumSize = new Size(110, 34);
        _btnToggleCashbox.Click += async (_, _) => await GuardAsync(ToggleCashboxAsync);

        _cashboxesBar = Toolbar();
        _cashboxesBar.Controls.AddRange(new Control[]
        {
            _lblCashboxes, _cNameBox, _btnAddCashbox, _btnToggleCashbox
        });

        _cashboxGrid = Grid();
        _cashboxGrid.SelectionChanged += (_, _) => UpdateToggles();
        _cashboxGrid.Columns.AddRange(new[]
        {
            new DataGridViewTextBoxColumn { DataPropertyName = "Name", FillWeight = 140 },
            new DataGridViewTextBoxColumn { DataPropertyName = "CurrentBalance", FillWeight = 100 },
            new DataGridViewTextBoxColumn { DataPropertyName = "IsActive", FillWeight = 50 }
        });

        _btnClose = ButtonFactory.Primary(AppStrings.Close);
        _btnClose.AutoSize = true;
        _btnClose.MinimumSize = new Size(120, 40);
        _btnClose.DialogResult = DialogResult.Cancel;

        var bottom = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = true,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 8, 0, 0)
        };
        bottom.Controls.Add(_btnClose);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(16, 14, 16, 14),
            Margin = Padding.Empty
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        root.Controls.Add(_warehousesBar, 0, 0);
        root.Controls.Add(_warehouseGrid, 0, 1);
        root.Controls.Add(_cashboxesBar, 0, 2);
        root.Controls.Add(_cashboxGrid, 0, 3);
        root.Controls.Add(bottom, 0, 4);

        Controls.Add(root);

        LoadText();
        _languageService.LanguageChanged += OnLanguageChanged;
        _languageSubscribed = true;
    }

    private static Label SectionLabel() => new()
    {
        AutoSize = true,
        Anchor = AnchorStyles.Left,
        Font = new Font("Segoe UI Semibold", 10F),
        Margin = new Padding(0, 6, 8, 0)
    };

    private static TextBox InputBox() => new()
    {
        Font = new Font("Segoe UI", 10F),
        MinimumSize = new Size(140, 30),
        Width = 160,
        Margin = new Padding(0, 3, 8, 3)
    };

    private static FlowLayoutPanel Toolbar() => new()
    {
        Dock = DockStyle.Fill,
        FlowDirection = FlowDirection.LeftToRight,
        WrapContents = true,
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        BackColor = AppColors.Surface,
        Margin = new Padding(0, 0, 0, 6)
    };

    private static DataGridView Grid()
    {
        var grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false };
        DataGridViewStyleHelper.ApplyModernStyle(grid);
        return grid;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        _ = GuardAsync(LoadDataAsync);
    }

    private void OnLanguageChanged(object? sender, string culture)
    {
        if (IsDisposed) return;
        if (InvokeRequired) BeginInvoke(LoadText);
        else LoadText();
    }

    private void LoadText()
    {
        Text = $"{_branch.Name} — {AppStrings.BranchResources}";
        _lblWarehouses.Text = AppStrings.Warehouses;
        _lblCashboxes.Text = AppStrings.Cashboxes;
        _wNameBox.PlaceholderText = AppStrings.Name;
        _wLocationBox.PlaceholderText = AppStrings.Location;
        _cNameBox.PlaceholderText = AppStrings.Name;
        _btnAddWarehouse.Text = AppStrings.AddWarehouse;
        _btnAddCashbox.Text = AppStrings.AddCashbox;
        _btnClose.Text = AppStrings.Close;

        _warehouseGrid.Columns[0].HeaderText = AppStrings.Name;
        _warehouseGrid.Columns[1].HeaderText = AppStrings.Location;
        _warehouseGrid.Columns[2].HeaderText = AppStrings.Active;
        _cashboxGrid.Columns[0].HeaderText = AppStrings.Name;
        _cashboxGrid.Columns[1].HeaderText = AppStrings.Balance;
        _cashboxGrid.Columns[2].HeaderText = AppStrings.Active;

        UpdateToggles();
    }

    private void UpdateToggles()
    {
        var warehouse = _warehouseGrid.CurrentRow?.DataBoundItem as WarehouseListItemDto;
        _btnToggleWarehouse.Text = warehouse is null
            ? AppStrings.Deactivate
            : warehouse.IsActive ? AppStrings.Deactivate : AppStrings.Activate;

        var cashbox = _cashboxGrid.CurrentRow?.DataBoundItem as CashboxListItemDto;
        _btnToggleCashbox.Text = cashbox is null
            ? AppStrings.Deactivate
            : cashbox.IsActive ? AppStrings.Deactivate : AppStrings.Activate;
    }

    private async Task LoadDataAsync()
    {
        _warehouses = await _resourceService.GetWarehousesAsync(_branch.Id);
        _cashboxes = await _resourceService.GetCashboxesAsync(_branch.Id);

        if (IsDisposed) return;

        _warehouseGrid.DataSource = null;
        _warehouseGrid.DataSource = _warehouses;

        _cashboxGrid.DataSource = null;
        _cashboxGrid.DataSource = _cashboxes;

        UpdateToggles();
    }

    private async Task AddWarehouseAsync()
    {
        await _resourceService.CreateWarehouseAsync(new CreateWarehouseDto
        {
            BranchId = _branch.Id,
            Name = _wNameBox.Text.Trim(),
            Location = _wLocationBox.Text.Trim(),
            IsActive = true
        });

        _wNameBox.Clear();
        _wLocationBox.Clear();
        await LoadDataAsync();
    }

    private async Task AddCashboxAsync()
    {
        await _resourceService.CreateCashboxAsync(new CreateCashboxDto
        {
            BranchId = _branch.Id,
            Name = _cNameBox.Text.Trim(),
            IsActive = true
        });

        _cNameBox.Clear();
        await LoadDataAsync();
    }

    private async Task ToggleWarehouseAsync()
    {
        if (_warehouseGrid.CurrentRow?.DataBoundItem is not WarehouseListItemDto warehouse)
            return;

        if (warehouse.IsActive &&
            !AppMessageBox.Confirm(AppStrings.DeactivateConfirm, AppStrings.Deactivate))
        {
            return;
        }

        await _resourceService.SetWarehouseActiveAsync(warehouse.Id, !warehouse.IsActive);
        await LoadDataAsync();
    }

    private async Task ToggleCashboxAsync()
    {
        if (_cashboxGrid.CurrentRow?.DataBoundItem is not CashboxListItemDto cashbox)
            return;

        if (cashbox.IsActive &&
            !AppMessageBox.Confirm(AppStrings.DeactivateConfirm, AppStrings.Deactivate))
        {
            return;
        }

        await _resourceService.SetCashboxActiveAsync(cashbox.Id, !cashbox.IsActive);
        await LoadDataAsync();
    }

    private async Task GuardAsync(Func<Task> work)
    {
        try
        {
            await work();
        }
        catch (Exception ex)
        {
            if (!IsDisposed)
                AppMessageBox.Error(ex);
        }
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
}
