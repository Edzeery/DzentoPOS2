using DzentoPOS2.App.Services.Partners;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Partners;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;
using EZControls.Controls.Inputs;
using FontAwesome.Sharp;

namespace DzentoPOS2.WinForms.Forms.Suppliers;

public partial class SuppliersForm : LocalizableFormBase
{
    private readonly ISupplierService _supplierService;
    private readonly Panel _topPanel;
    private readonly IconButton _btnAdd;
    private readonly IconButton _btnEdit;
    private readonly IconButton _btnDelete;
    private readonly EZTextBox _searchBox;
    private readonly Label _lblSearch;
    private readonly DataGridView _grid;
    private List<SupplierDto> _suppliers = new();

    public SuppliersForm(ISupplierService supplierService, ILanguageService languageService)
        : base(languageService)
    {
        _supplierService = supplierService;
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
        _btnAdd.Click += async (_, _) => await AddSupplier();

        _btnEdit = ButtonFactory.ToolEdit("Edit");
        _btnEdit.Location = new Point(124, 12);
        _btnEdit.Click += async (_, _) => await EditSupplier();

        _btnDelete = ButtonFactory.ToolDelete("Delete");
        _btnDelete.Location = new Point(232, 12);
        _btnDelete.Click += async (_, _) => await DeleteSupplier();

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
        _grid.CellDoubleClick += async (_, _) => await EditSupplier();

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
        Text = AppStrings.Suppliers;
        _lblSearch.Text = AppStrings.Search + ":";
        _searchBox.PlaceholderText = AppStrings.Search;
    }

    private async Task LoadDataAsync()
    {
        var term = _searchBox.Text?.Trim();
        _suppliers = string.IsNullOrWhiteSpace(term)
            ? await _supplierService.GetAllAsync()
            : await _supplierService.SearchAsync(term);

        _grid.DataSource = null;
        _grid.Columns.Clear();
        _grid.AutoGenerateColumns = false;

        _grid.Columns.AddRange(new[]
        {
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.SupplierName, DataPropertyName = "Name", Width = 180 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Phone, DataPropertyName = "PhoneNumber", Width = 130 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Country, DataPropertyName = "Country", Width = 110 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.City, DataPropertyName = "City", Width = 110 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Address, DataPropertyName = "Street", Width = 160 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Balance, DataPropertyName = "Balance", Width = 100, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.StatusFilter, DataPropertyName = "IsActive", Width = 80 }
        });

        _grid.DataSource = _suppliers;
    }

    private async Task AddSupplier()
    {
        using var dialog = new SupplierEditDialog(_supplierService, _languageService);
        if (dialog.ShowDialog() == DialogResult.OK)
            await LoadDataAsync();
    }

    private async Task EditSupplier()
    {
        if (_grid.CurrentRow?.DataBoundItem is not SupplierDto supplier) return;
        using var dialog = new SupplierEditDialog(_supplierService, _languageService, supplier);
        if (dialog.ShowDialog() == DialogResult.OK)
            await LoadDataAsync();
    }

    private async Task DeleteSupplier()
    {
        if (_grid.CurrentRow?.DataBoundItem is not SupplierDto supplier) return;
        var result = MessageBox.Show(
            $"{AppStrings.DeleteSupplierConfirm}\n{supplier.Name}",
            AppStrings.DeleteConfirmation,
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (result == DialogResult.Yes)
        {
            await _supplierService.DeleteAsync(supplier.Id);
            await LoadDataAsync();
        }
    }
}