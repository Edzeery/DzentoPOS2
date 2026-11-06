using DzentoPOS2.App.Services.Identity.Interfaces;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Identity.Users;
using DzentoPOS2.Shared.Contracts.Requests;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;
using FontAwesome.Sharp;

namespace DzentoPOS2.WinForms.Forms.Identity;

public partial class UsersForm : LocalizableFormBase
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly Panel _topPanel;
    private readonly IconButton _btnAdd;
    private readonly IconButton _btnEdit;
    private readonly IconButton _btnDelete;
    private readonly DataGridView _grid;
    private List<UserDto> _users = new();

    public UsersForm(IUserService userService, IRoleService roleService, ILanguageService languageService)
        : base(languageService)
    {
        _userService = userService;
        _roleService = roleService;
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
        _btnAdd.Click += (_, _) => RunSafe(AddUser);

        _btnEdit = ButtonFactory.ToolEdit("Edit");
        _btnEdit.Location = new Point(124, 12);
        _btnEdit.Click += (_, _) => RunSafe(EditUser);

        _btnDelete = ButtonFactory.ToolDelete("Delete");
        _btnDelete.Location = new Point(232, 12);
        _btnDelete.Click += (_, _) => RunSafe(DeleteUser);

        _topPanel.Controls.AddRange(new Control[] { _btnDelete, _btnEdit, _btnAdd });

        _grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, MultiSelect = false };
        DataGridViewStyleHelper.ApplyModernStyle(_grid);
        _grid.CellDoubleClick += (_, _) => RunSafe(EditUser);

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
        Text = AppStrings.Users;
        _btnAdd.Text = "  " + AppStrings.Add;
        _btnEdit.Text = "  " + AppStrings.Edit;
        _btnDelete.Text = "  " + AppStrings.Delete;
    }

    private async Task LoadDataAsync()
    {
        var result = await _userService.GetPagedAsync(
            new PagedRequest { PageSize = int.MaxValue });

        if (IsDisposedOrDisposing) return;

        _users = result.Success && result.Data != null
            ? result.Data.Items.ToList()
            : new List<UserDto>();

        _grid.DataSource = null;
        _grid.Columns.Clear();
        _grid.AutoGenerateColumns = false;
        _grid.Columns.AddRange(new[]
        {
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.FullName, DataPropertyName = "FullName", Width = 180 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Username, DataPropertyName = "UserName", Width = 150 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Email, DataPropertyName = "Email", Width = 180 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Phone, DataPropertyName = "PhoneNumber", Width = 130 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Branch, DataPropertyName = "BranchName", Width = 130 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Statut, DataPropertyName = "IsActive", Width = 80 }
        });
        _grid.DataSource = _users;
    }

    private async Task AddUser()
    {
        using var dialog = new UserEditDialog(_userService, _roleService, _languageService);
        if (dialog.ShowDialog() == DialogResult.OK)
            await LoadDataAsync();
    }

    private async Task EditUser()
    {
        if (_grid.CurrentRow?.DataBoundItem is not UserDto user) return;
        using var dialog = new UserEditDialog(_userService, _roleService, _languageService, user.Id);
        if (dialog.ShowDialog() == DialogResult.OK)
            await LoadDataAsync();
    }

    private async Task DeleteUser()
    {
        if (_grid.CurrentRow?.DataBoundItem is not UserDto user) return;
        var confirm = MessageBox.Show(
            $"{AppStrings.ConfirmDelete}\n{user.FullName}",
            AppStrings.DeleteConfirmation,
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes) return;

        var result = await _userService.DeleteAsync(user.Id);
        if (result.Success)
            await LoadDataAsync();
        else
            AppMessageBox.Error(result.Message ?? AppStrings.ErrorMsg);
    }
}
