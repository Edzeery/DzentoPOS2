using DzentoPOS2.App.Services.Accounting;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Accounting;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;
using FontAwesome.Sharp;

namespace DzentoPOS2.WinForms.Forms.Accounting;

public partial class ExpensesForm : LocalizableFormBase
{
    private readonly IExpenseService _expenseService;
    private readonly Panel _topPanel;
    private readonly Panel _filterPanel;
    private readonly ComboBox _cmbCategory;
    private readonly DateTimePicker _dtpFrom;
    private readonly DateTimePicker _dtpTo;
    private readonly IconButton _btnFilter;
    private readonly IconButton _btnAdd;
    private readonly IconButton _btnEdit;
    private readonly IconButton _btnDelete;
    private readonly Label _lblCategoryFilter;
    private readonly Label _lblFrom;
    private readonly Label _lblTo;
    private readonly DataGridView _grid;
    private List<ExpenseDto> _expenses = new();
    private List<ExpenseCategoryDto> _categories = new();

    public ExpensesForm(IExpenseService expenseService, ILanguageService languageService)
        : base(languageService)
    {
        _expenseService = expenseService;
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
        _btnAdd.Click += (_, _) => RunSafe(AddExpense);

        _btnEdit = ButtonFactory.ToolEdit("Edit");
        _btnEdit.Location = new Point(124, 12);
        _btnEdit.Click += (_, _) => RunSafe(EditExpense);

        _btnDelete = ButtonFactory.ToolDelete("Delete");
        _btnDelete.Location = new Point(232, 12);
        _btnDelete.Click += (_, _) => RunSafe(DeleteExpense);

        _topPanel.Controls.AddRange(new Control[] { _btnDelete, _btnEdit, _btnAdd });

        _filterPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = AppColors.Surface,
            Padding = new Padding(16, 8, 16, 8)
        };

        _cmbCategory = new ComboBox
        {
            Location = new Point(16, 10),
            Size = new Size(180, 28),
            Font = new Font("Segoe UI", 10F),
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        _dtpFrom = new DateTimePicker
        {
            Location = new Point(210, 10),
            Size = new Size(140, 28),
            Font = new Font("Segoe UI", 10F),
            Format = DateTimePickerFormat.Short,
            Value = DateTime.Now.AddMonths(-1)
        };

        _dtpTo = new DateTimePicker
        {
            Location = new Point(360, 10),
            Size = new Size(140, 28),
            Font = new Font("Segoe UI", 10F),
            Format = DateTimePickerFormat.Short,
            Value = DateTime.Now
        };

        _lblCategoryFilter = new Label { Location = new Point(16, 14), Size = new Size(60, 20), Font = new Font("Segoe UI", 9F) };
        _lblFrom = new Label { Location = new Point(210, 14), Size = new Size(30, 20), Font = new Font("Segoe UI", 9F) };
        _lblTo = new Label { Location = new Point(345, 14), Size = new Size(30, 20), Font = new Font("Segoe UI", 9F) };

        _btnFilter = new IconButton
        {
            IconChar = IconChar.Filter,
            IconColor = Color.White,
            ForeColor = Color.White,
            Font = AppStyle.ButtonFont,
            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 0, MouseOverBackColor = AppColors.PrimaryHover, MouseDownBackColor = AppColors.PrimaryPressed },
            BackColor = AppColors.Primary,
            Size = new Size(100, 30),
            Location = new Point(520, 10),
            Cursor = Cursors.Hand,
            TextImageRelation = TextImageRelation.ImageBeforeText
        };
        _btnFilter.Click += (_, _) => RunSafe(LoadDataAsync);

        _filterPanel.Controls.AddRange(new Control[]
        {
            _lblCategoryFilter, _cmbCategory,
            _lblFrom, _dtpFrom,
            _lblTo, _dtpTo,
            _btnFilter
        });

        _grid = new DataGridView { Dock = DockStyle.Fill };
        DataGridViewStyleHelper.ApplyModernStyle(_grid);
        _grid.CellDoubleClick += (_, _) => RunSafe(EditExpense);

        Controls.Add(_grid);
        Controls.Add(_filterPanel);
        Controls.Add(_topPanel);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        RunSafe(LoadLookupsAndDataAsync);
    }

    private async Task LoadLookupsAndDataAsync()
    {
        _categories = await _expenseService.GetCategoriesAsync();
        if (IsDisposedOrDisposing) return;
        _cmbCategory.Items.Clear();
        _cmbCategory.Items.Add(new { Name = AppStrings.All, Id = Guid.Empty });
        foreach (var c in _categories)
            _cmbCategory.Items.Add(new { c.Name, c.Id });
        _cmbCategory.DisplayMember = "Name";
        _cmbCategory.ValueMember = "Id";
        _cmbCategory.SelectedIndex = 0;

        await LoadDataAsync();
    }

    protected override void LoadText()
    {
        Text = AppStrings.Expenses;
        _btnFilter.Text = "  " + AppStrings.Filter;
        _lblCategoryFilter.Text = AppStrings.CategoryFilter + ":";
        _lblFrom.Text = AppStrings.FromLabel;
        _lblTo.Text = AppStrings.ToLabel;
    }

    private async Task LoadDataAsync()
    {
        var from = _dtpFrom.Value.Date;
        var to = _dtpTo.Value.Date.AddDays(1);
        _expenses = await _expenseService.GetByDateRangeAsync(from, to);

        if (IsDisposedOrDisposing) return;

        var selectedId = GetSelectedCategoryId();
        if (selectedId != Guid.Empty)
            _expenses = _expenses.Where(e => e.ExpenseCategoryId == selectedId).ToList();

        _grid.DataSource = null;
        _grid.Columns.Clear();
        _grid.AutoGenerateColumns = false;

        _grid.Columns.AddRange(new[]
        {
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Title, DataPropertyName = "Title", Width = 200 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.CategoryFilter, DataPropertyName = "CategoryName", Width = 130 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Amount, DataPropertyName = "Amount", Width = 120, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.TransactionDate, DataPropertyName = "ExpenseDate", Width = 110, DefaultCellStyle = { Format = "yyyy/MM/dd" } },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Notes, DataPropertyName = "Notes", Width = 200 }
        });

        _grid.DataSource = _expenses;
    }

    private Guid GetSelectedCategoryId()
    {
        if (_cmbCategory.SelectedItem == null) return Guid.Empty;
        var prop = _cmbCategory.SelectedItem.GetType().GetProperty("Id");
        return prop?.GetValue(_cmbCategory.SelectedItem) is Guid id ? id : Guid.Empty;
    }

    private async Task AddExpense()
    {
        using var dialog = new ExpenseEditDialog(_expenseService, _languageService, _categories);
        if (dialog.ShowDialog() == DialogResult.OK)
            await LoadDataAsync();
    }

    private async Task EditExpense()
    {
        if (_grid.CurrentRow?.DataBoundItem is not ExpenseDto expense) return;
        using var dialog = new ExpenseEditDialog(_expenseService, _languageService, _categories, expense);
        if (dialog.ShowDialog() == DialogResult.OK)
            await LoadDataAsync();
    }

    private async Task DeleteExpense()
    {
        if (_grid.CurrentRow?.DataBoundItem is not ExpenseDto expense) return;
        var result = MessageBox.Show($"{AppStrings.ConfirmDeleteExpense} '{expense.Title}'?", AppStrings.DeleteConfirmation,
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (result == DialogResult.Yes)
        {
            await _expenseService.DeleteAsync(expense.Id);
            await LoadDataAsync();
        }
    }
}
