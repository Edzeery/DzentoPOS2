using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;
using EZControls.Controls.Inputs;
using FontAwesome.Sharp;

namespace DzentoPOS2.WinForms.Forms.Inventory;

public partial class ProductCategoriesForm : LocalizableFormBase
{
    private readonly IProductCategoryService _categoryService;
    private readonly Panel _topPanel;
    private readonly IconButton _btnAdd;
    private readonly IconButton _btnEdit;
    private readonly IconButton _btnDelete;
    private readonly EZTextBox _searchBox;
    private readonly Label _lblSearch;
    private readonly DataGridView _grid;
    private List<ProductCategoryDto> _categories = new();

    public ProductCategoriesForm(
        IProductCategoryService categoryService,
        ILanguageService languageService)
        : base(languageService)
    {
        _categoryService = categoryService;
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
        _btnAdd.Click += (_, _) => RunSafe(AddCategory);

        _btnEdit = ButtonFactory.ToolEdit("Edit");
        _btnEdit.Location = new Point(124, 12);
        _btnEdit.Click += (_, _) => RunSafe(EditCategory);

        _btnDelete = ButtonFactory.ToolDelete("Delete");
        _btnDelete.Location = new Point(232, 12);
        _btnDelete.Click += (_, _) => RunSafe(DeleteCategory);

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
        _searchBox.TextChanged += (_, _) => RunSafe(LoadDataAsync);

        _topPanel.Controls.AddRange(new Control[] { _btnDelete, _btnEdit, _btnAdd, _lblSearch, _searchBox });

        _grid = new DataGridView { Dock = DockStyle.Fill };
        DataGridViewStyleHelper.ApplyModernStyle(_grid);
        _grid.CellDoubleClick += (_, _) => RunSafe(EditCategory);

        Controls.Add(_grid);
        Controls.Add(_topPanel);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        RunSafe(LoadDataAsync);
    }

    protected override void LoadText()
    {
        Text = AppStrings.Categories;
        _lblSearch.Text = AppStrings.Search + ":";
        _searchBox.PlaceholderText = AppStrings.Search;
    }

    private async Task LoadDataAsync()
    {
        var term = _searchBox.Text?.Trim();
        _categories = string.IsNullOrWhiteSpace(term)
            ? await _categoryService.GetAllAsync()
            : await _categoryService.SearchAsync(term);

        if (IsDisposedOrDisposing) return;

        _grid.DataSource = null;
        _grid.Columns.Clear();
        _grid.AutoGenerateColumns = false;

        _grid.Columns.AddRange(new[]
        {
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.CategoryName, DataPropertyName = "Name", Width = 220 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Description, DataPropertyName = "Description", Width = 340 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Active, DataPropertyName = "IsActive", Width = 80 }
        });

        _grid.DataSource = _categories;
    }

    private async Task AddCategory()
    {
        using var dialog = new ProductCategoryEditDialog(_categoryService, _languageService);
        if (dialog.ShowDialog() == DialogResult.OK)
            await LoadDataAsync();
    }

    private async Task EditCategory()
    {
        if (_grid.CurrentRow?.DataBoundItem is not ProductCategoryDto category) return;
        using var dialog = new ProductCategoryEditDialog(_categoryService, _languageService, category);
        if (dialog.ShowDialog() == DialogResult.OK)
            await LoadDataAsync();
    }

    private async Task DeleteCategory()
    {
        if (_grid.CurrentRow?.DataBoundItem is not ProductCategoryDto category) return;
        var result = MessageBox.Show(
            $"{AppStrings.DeleteCategoryConfirm}\n{category.Name}",
            AppStrings.DeleteConfirmation,
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (result == DialogResult.Yes)
        {
            await _categoryService.DeleteAsync(category.Id);
            await LoadDataAsync();
        }
    }
}
