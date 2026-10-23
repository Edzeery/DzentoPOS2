using DzentoPOS2.App.Services.Accounting;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Accounting;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Forms.Accounting;

public class ExpenseEditDialog : Form
{
    private readonly IExpenseService _expenseService;
    private readonly ILanguageService _languageService;
    private bool _languageSubscribed;
    private readonly ExpenseDto? _existing;
    private readonly Label _lblTitle;
    private readonly Label _lblCategory;
    private readonly Label _lblAmount;
    private readonly Label _lblDate;
    private readonly Label _lblNotes;
    private readonly TextBox _txtTitle;
    private readonly ComboBox _cmbCategory;
    private readonly NumericUpDown _numAmount;
    private readonly DateTimePicker _dtpDate;
    private readonly TextBox _txtNotes;
    private readonly Button _btnSave;
    private readonly Button _btnCancel;

    public ExpenseEditDialog(IExpenseService expenseService, ILanguageService languageService, List<ExpenseCategoryDto> categories, ExpenseDto? existing = null)
    {
        _expenseService = expenseService;
        _languageService = languageService;
        _existing = existing;

        Size = new Size(480, 340);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = AppColors.Surface;

        var lbl = (string text, int y) => new Label
        {
            Text = text,
            Location = new Point(20, y),
            Size = new Size(100, 28),
            Font = new Font("Segoe UI", 10F),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _lblTitle = lbl(AppStrings.TitleLabel, 20);
        _lblCategory = lbl(AppStrings.CategoryLabel, 58);
        _lblAmount = lbl(AppStrings.AmountLabel, 96);
        _lblDate = lbl(AppStrings.DateLabel, 134);
        _lblNotes = lbl(AppStrings.NotesLabel, 172);

        _txtTitle = new TextBox { Location = new Point(130, 20), Size = new Size(310, 28), Font = new Font("Segoe UI", 10F) };

        var categoryList = categories.Cast<object>().ToList();
        categoryList.Insert(0, new { Id = Guid.Empty, Name = AppStrings.SelectCategory });

        _cmbCategory = new ComboBox
        {
            Location = new Point(130, 58),
            Size = new Size(310, 28),
            Font = new Font("Segoe UI", 10F),
            DropDownStyle = ComboBoxStyle.DropDownList,
            DataSource = categoryList,
            DisplayMember = "Name",
            ValueMember = "Id",
            SelectedIndex = 0
        };

        _numAmount = new NumericUpDown { Location = new Point(130, 96), Size = new Size(310, 28), Font = new Font("Segoe UI", 10F), DecimalPlaces = 2, Maximum = 999999 };
        _dtpDate = new DateTimePicker { Location = new Point(130, 134), Size = new Size(310, 28), Font = new Font("Segoe UI", 10F), Format = DateTimePickerFormat.Short, Value = DateTime.Now };
        _txtNotes = new TextBox { Location = new Point(130, 172), Size = new Size(310, 28), Font = new Font("Segoe UI", 10F) };

        _btnSave = ButtonFactory.Primary(AppStrings.Save);
        _btnSave.Location = new Point(130, 220);
        _btnSave.Size = new Size(120, 40);
        _btnSave.Click += async (_, _) => await SaveAsync();

        _btnCancel = ButtonFactory.Neutral(AppStrings.Cancel);
        _btnCancel.Location = new Point(260, 220);
        _btnCancel.Size = new Size(120, 40);
        _btnCancel.DialogResult = DialogResult.Cancel;

        Controls.AddRange(new Control[]
        {
            _lblTitle, _txtTitle,
            _lblCategory, _cmbCategory,
            _lblAmount, _numAmount,
            _lblDate, _dtpDate,
            _lblNotes, _txtNotes,
            _btnSave, _btnCancel
        });

        if (existing != null)
        {
            _txtTitle.Text = existing.Title;
            _numAmount.Value = existing.Amount;
            _dtpDate.Value = existing.ExpenseDate;
            _txtNotes.Text = existing.Notes;
            for (int i = 0; i < _cmbCategory.Items.Count; i++)
            {
                var item = _cmbCategory.Items[i];
                var prop = item?.GetType().GetProperty("Id");
                if (prop != null && prop.GetValue(item) is Guid id && id == existing.ExpenseCategoryId)
                {
                    _cmbCategory.SelectedIndex = i;
                    break;
                }
            }
        }

        LoadText();
        _languageService.LanguageChanged += OnLanguageChanged;
        _languageSubscribed = true;
    }

    private void OnLanguageChanged(object? sender, string culture)
    {
        if (IsDisposed) return;
        if (InvokeRequired) BeginInvoke(LoadText);
        else LoadText();
    }

    private void LoadText()
    {
        Text = _existing == null ? AppStrings.NewExpense : AppStrings.EditExpenseTitle;
        _lblTitle.Text = AppStrings.TitleLabel;
        _lblCategory.Text = AppStrings.CategoryLabel;
        _lblAmount.Text = AppStrings.AmountLabel;
        _lblDate.Text = AppStrings.DateLabel;
        _lblNotes.Text = AppStrings.NotesLabel;
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
        if (string.IsNullOrWhiteSpace(_txtTitle.Text))
        {
            MessageBox.Show(AppStrings.EnterTitle, AppStrings.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var categoryId = _cmbCategory.SelectedValue as Guid? ?? Guid.Empty;
        if (categoryId == Guid.Empty)
        {
            MessageBox.Show(AppStrings.SelectCategoryMsg, AppStrings.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_existing == null)
        {
            await _expenseService.CreateAsync(new CreateExpenseDto
            {
                Title = _txtTitle.Text.Trim(),
                ExpenseCategoryId = categoryId,
                Amount = _numAmount.Value,
                ExpenseDate = _dtpDate.Value,
                Notes = _txtNotes.Text.Trim()
            });
        }
        else
        {
            await _expenseService.UpdateAsync(new UpdateExpenseDto
            {
                Id = _existing.Id,
                Title = _txtTitle.Text.Trim(),
                ExpenseCategoryId = categoryId,
                Amount = _numAmount.Value,
                ExpenseDate = _dtpDate.Value,
                Notes = _txtNotes.Text.Trim()
            });
        }

        DialogResult = DialogResult.OK;
        Close();
    }
}
