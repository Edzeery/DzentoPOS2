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
    private readonly Panel _topPanel;
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
        BackColor = AppColors.Background;
        StartPosition = FormStartPosition.CenterParent;

        _topPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = AppColors.Surface,
            Padding = new Padding(16, 10, 16, 10)
        };

        _btnAdd = ButtonFactory.ToolAdd("Add");
        _btnAdd.Location = new Point(16, 12);
        _btnAdd.Click += async (_, _) => await AddUnit();

        _btnEdit = ButtonFactory.ToolEdit("Edit");
        _btnEdit.Location = new Point(124, 12);
        _btnEdit.Click += async (_, _) => await EditUnit();

        _btnDelete = ButtonFactory.ToolDelete("Delete");
        _btnDelete.Location = new Point(232, 12);
        _btnDelete.Click += async (_, _) => await DeleteUnit();

        _lblSearch = new Label
        {
            Location = new Point(350, 18),
            Size = new Size(50, 20),
            Font = new Font("Segoe UI", 9F)
        };

        _searchBox = new EZTextBox
        {
            Location = new Point(400, 14),
            Size = new Size(250, 30),
            Font = new Font("Segoe UI", 10F),
            PlaceholderText = AppStrings.Search
        };
        _searchBox.TextChanged += async (_, _) => await LoadDataAsync();

        _topPanel.Controls.AddRange(new Control[] { _btnDelete, _btnEdit, _btnAdd, _lblSearch, _searchBox });

        _grid = new DataGridView { Dock = DockStyle.Fill };
        DataGridViewStyleHelper.ApplyModernStyle(_grid);
        _grid.CellDoubleClick += async (_, _) => await EditUnit();

        Controls.Add(_grid);
        Controls.Add(_topPanel);
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await LoadDataAsync();
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
