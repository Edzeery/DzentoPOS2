using DzentoPOS2.App.Services.Businesses;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Businesses;
using DzentoPOS2.WinForms.Controls;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;
using EZControls.Controls.Inputs;
using FontAwesome.Sharp;

namespace DzentoPOS2.WinForms.Forms.Businesses;

/// <summary>
/// Branch management list. Branches are never deleted from here: an existing
/// branch is only activated or deactivated, and the service refuses to
/// deactivate the main branch, the last active branch, or a branch with an open
/// cash session.
/// </summary>
public partial class BranchesForm : LocalizableFormBase
{
    private readonly IBranchService _branchService;
    private readonly IBranchResourceService _resourceService;
    private readonly TableLayoutPanel _topPanel;
    private readonly ThemedIconButton _btnAdd;
    private readonly ThemedIconButton _btnEdit;
    private readonly ThemedIconButton _btnResources;
    private readonly ThemedIconButton _btnToggle;
    private readonly EZTextBox _searchBox;
    private readonly Label _lblSearch;
    private readonly DataGridView _grid;
    private List<BranchListItemDto> _branches = new();

    public BranchesForm(
        IBranchService branchService,
        IBranchResourceService resourceService,
        ILanguageService languageService)
        : base(languageService)
    {
        _branchService = branchService;
        _resourceService = resourceService;
        Size = new Size(1100, 650);
        MinimumSize = new Size(320, 300);
        BackColor = AppColors.Background;
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Font;

        _btnAdd = ButtonFactory.ToolAdd(AppStrings.AddBranch);
        _btnAdd.Click += (_, _) => RunSafe(AddBranch);

        _btnEdit = ButtonFactory.ToolEdit(AppStrings.EditBranch);
        _btnEdit.Click += (_, _) => RunSafe(EditBranch);

        _btnResources = ButtonFactory.Tool(IconChar.Warehouse, AppStrings.BranchResources, ButtonTone.Info);
        _btnResources.AutoSize = true;
        _btnResources.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _btnResources.MinimumSize = new Size(150, 34);
        _btnResources.Click += (_, _) => RunSafe(OpenResources);

        _btnToggle = ButtonFactory.Tool(IconChar.Ban, AppStrings.Deactivate, ButtonTone.Danger);
        _btnToggle.AutoSize = true;
        _btnToggle.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _btnToggle.MinimumSize = new Size(120, 34);
        _btnToggle.Click += (_, _) => RunSafe(ToggleActiveAsync);

        _lblSearch = new Label
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Font = new Font("Segoe UI", 9F),
            Margin = new Padding(0, 6, 6, 0)
        };

        _searchBox = new EZTextBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F),
            PlaceholderText = AppStrings.Search,
            MinimumSize = new Size(0, 28),
            Margin = new Padding(0, 2, 0, 2)
        };
        _searchBox.TextChanged += (_, _) => RunSafe(LoadDataAsync);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = Padding.Empty
        };
        buttons.Controls.AddRange(new Control[] { _btnAdd, _btnEdit, _btnResources, _btnToggle });

        var search = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            AutoSize = false,
            Height = 36,
            Margin = new Padding(0, 6, 0, 0)
        };
        search.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        search.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        search.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        search.Controls.Add(_lblSearch, 0, 0);
        search.Controls.Add(_searchBox, 1, 0);

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
        _topPanel.Controls.Add(search, 0, 1);

        _grid = new DataGridView { Dock = DockStyle.Fill };
        DataGridViewStyleHelper.ApplyModernStyle(_grid);
        _grid.CellDoubleClick += (_, _) => RunSafe(EditBranch);
        _grid.SelectionChanged += (_, _) => UpdateToggleButton();

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

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        RunSafe(LoadDataAsync);
    }

    protected override void LoadText()
    {
        Text = AppStrings.Branches;
        _lblSearch.Text = AppStrings.Search + ":";
        _searchBox.PlaceholderText = AppStrings.Search;
        _btnAdd.Text = "  " + AppStrings.AddBranch;
        _btnEdit.Text = "  " + AppStrings.EditBranch;
        _btnResources.Text = "  " + AppStrings.BranchResources;
        UpdateToggleButton();
    }

    private async Task LoadDataAsync()
    {
        _branches = await _branchService.GetAllAsync();

        var term = _searchBox.Text?.Trim();
        var rows = string.IsNullOrWhiteSpace(term)
            ? _branches
            : _branches
                .Where(b =>
                    b.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (b.City ?? string.Empty).Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (b.PhoneNumber ?? string.Empty).Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToList();

        if (IsDisposedOrDisposing) return;

        _grid.DataSource = null;
        _grid.Columns.Clear();
        _grid.AutoGenerateColumns = false;

        _grid.Columns.AddRange(new[]
        {
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.BranchName, DataPropertyName = "Name", FillWeight = 160 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.City, DataPropertyName = "City", FillWeight = 100 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.BranchPhone, DataPropertyName = "PhoneNumber", FillWeight = 100 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Warehouses, DataPropertyName = "WarehousesCount", FillWeight = 80 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Cashboxes, DataPropertyName = "CashboxesCount", FillWeight = 80 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Active, DataPropertyName = "IsActive", FillWeight = 60 }
        });

        _grid.DataSource = rows;
        UpdateToggleButton();
    }

    private void UpdateToggleButton()
    {
        var selected = _grid.CurrentRow?.DataBoundItem as BranchListItemDto;
        if (selected is null)
        {
            _btnToggle.Text = "  " + AppStrings.Deactivate;
            return;
        }

        _btnToggle.Text = "  " + (selected.IsActive ? AppStrings.Deactivate : AppStrings.Activate);
    }

    private async Task AddBranch()
    {
        using var dialog = new BranchEditDialog(_branchService, _languageService);
        if (dialog.ShowDialog() == DialogResult.OK)
            await LoadDataAsync();
    }

    private async Task EditBranch()
    {
        if (_grid.CurrentRow?.DataBoundItem is not BranchListItemDto branch) return;
        using var dialog = new BranchEditDialog(_branchService, _languageService, branch);
        if (dialog.ShowDialog() == DialogResult.OK)
            await LoadDataAsync();
    }

    private async Task OpenResources()
    {
        if (_grid.CurrentRow?.DataBoundItem is not BranchListItemDto branch) return;
        using var dialog = new BranchResourcesDialog(_resourceService, _languageService, branch);
        dialog.ShowDialog();
        await LoadDataAsync();
    }

    private async Task ToggleActiveAsync()
    {
        if (_grid.CurrentRow?.DataBoundItem is not BranchListItemDto branch) return;

        if (branch.IsActive)
        {
            if (!AppMessageBox.Confirm(AppStrings.DeactivateBranchConfirm, AppStrings.Deactivate))
                return;

            await _branchService.SetActiveAsync(branch.Id, false);
        }
        else
        {
            await _branchService.SetActiveAsync(branch.Id, true);
        }

        await LoadDataAsync();
    }
}
