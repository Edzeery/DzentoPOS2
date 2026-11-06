using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;
using EZControls.Controls.Inputs;
using FontAwesome.Sharp;

namespace DzentoPOS2.WinForms.Forms.Inventory;

public partial class ProductUnitsForm : LocalizableFormBase
{
    private readonly IProductUnitService _unitService;
    private readonly TableLayoutPanel _topPanel;
    private readonly IconButton _btnAdd;
    private readonly IconButton _btnEdit;
    private readonly IconButton _btnDelete;
    private readonly EZTextBox _searchBox;
    private readonly Label _lblSearch;
    private readonly DataGridView _grid;
    private List<ProductUnitDto> _units = new();

    public ProductUnitsForm(
        IProductUnitService unitService,
        ILanguageService languageService)
        : base(languageService)
    {
        _unitService = unitService;
        Size = new Size(1100, 650);
        MinimumSize = new Size(320, 300);
        BackColor = AppColors.Background;
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Font;

        _btnAdd = ButtonFactory.ToolAdd("Add");
        _btnAdd.Click += (_, _) => RunSafe(AddUnit);

        _btnEdit = ButtonFactory.ToolEdit("Edit");
        _btnEdit.Click += (_, _) => RunSafe(EditUnit);

        _btnDelete = ButtonFactory.ToolDelete("Delete");
        _btnDelete.Click += (_, _) => RunSafe(DeleteUnit);

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
        buttons.Controls.AddRange(new Control[] { _btnAdd, _btnEdit, _btnDelete });

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
        _grid.CellDoubleClick += (_, _) => RunSafe(EditUnit);

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
        Text = AppStrings.Units;
        _lblSearch.Text = AppStrings.Search + ":";
        _searchBox.PlaceholderText = AppStrings.Search;
    }

    private async Task LoadDataAsync()
    {
        var term = _searchBox.Text?.Trim();
        _units = string.IsNullOrWhiteSpace(term)
            ? await _unitService.GetAllAsync()
            : await _unitService.SearchAsync(term);

        if (IsDisposedOrDisposing) return;

        _grid.DataSource = null;
        _grid.Columns.Clear();
        _grid.AutoGenerateColumns = false;

        _grid.Columns.AddRange(new[]
        {
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.UnitName, DataPropertyName = "Name", Width = 200 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Symbol, DataPropertyName = "Symbol", Width = 100 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.BaseUnit, DataPropertyName = "IsBaseUnit", Width = 100 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Active, DataPropertyName = "IsActive", Width = 80 }
        });

        _grid.DataSource = _units;
    }

    private async Task AddUnit()
    {
        using var dialog = new ProductUnitEditDialog(_unitService, _languageService);
        if (dialog.ShowDialog() == DialogResult.OK)
            await LoadDataAsync();
    }

    private async Task EditUnit()
    {
        if (_grid.CurrentRow?.DataBoundItem is not ProductUnitDto unit) return;
        using var dialog = new ProductUnitEditDialog(_unitService, _languageService, unit);
        if (dialog.ShowDialog() == DialogResult.OK)
            await LoadDataAsync();
    }

    private async Task DeleteUnit()
    {
        if (_grid.CurrentRow?.DataBoundItem is not ProductUnitDto unit) return;
        var result = MessageBox.Show(
            $"{AppStrings.DeleteUnitConfirm}\n{unit.Name}",
            AppStrings.DeleteConfirmation,
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (result == DialogResult.Yes)
        {
            await _unitService.DeleteAsync(unit.Id);
            await LoadDataAsync();
        }
    }
}

