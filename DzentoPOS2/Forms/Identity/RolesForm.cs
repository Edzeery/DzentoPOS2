using DzentoPOS2.App.Services.Identity.Interfaces;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Identity.Roles;
using DzentoPOS2.Shared.Contracts.Requests;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;
using FontAwesome.Sharp;

namespace DzentoPOS2.WinForms.Forms.Identity;

public partial class RolesForm : LocalizableFormBase
{
    private readonly IRoleService _roleService;
    private readonly IPermissionManagementService _permissionService;
    private readonly Panel _topPanel;
    private readonly IconButton _btnAdd;
    private readonly IconButton _btnEdit;
    private readonly IconButton _btnDelete;
    private readonly DataGridView _grid;
    private List<RoleDto> _roles = new();

    public RolesForm(
        IRoleService roleService,
        IPermissionManagementService permissionService,
        ILanguageService languageService)
        : base(languageService)
    {
        _roleService = roleService;
        _permissionService = permissionService;
        Size = new Size(1000, 600);
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
        _btnAdd.Click += (_, _) => RunSafe(AddRole);

        _btnEdit = ButtonFactory.ToolEdit("Edit");
        _btnEdit.Location = new Point(124, 12);
        _btnEdit.Click += (_, _) => RunSafe(EditRole);

        _btnDelete = ButtonFactory.ToolDelete("Delete");
        _btnDelete.Location = new Point(232, 12);
        _btnDelete.Click += (_, _) => RunSafe(DeleteRole);

        _topPanel.Controls.AddRange(new Control[] { _btnDelete, _btnEdit, _btnAdd });

        _grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, MultiSelect = false };
        DataGridViewStyleHelper.ApplyModernStyle(_grid);
        _grid.CellDoubleClick += (_, _) => RunSafe(EditRole);

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
        Text = AppStrings.Roles;
        _btnAdd.Text = "  " + AppStrings.Add;
        _btnEdit.Text = "  " + AppStrings.Edit;
        _btnDelete.Text = "  " + AppStrings.Delete;
    }

    private async Task LoadDataAsync()
    {
        var result = await _roleService.GetPagedAsync(
            new PagedRequest { PageSize = int.MaxValue });

        if (IsDisposedOrDisposing) return;

        _roles = result.Success && result.Data != null
            ? result.Data.Items.ToList()
            : new List<RoleDto>();

        _grid.DataSource = null;
        _grid.Columns.Clear();
        _grid.AutoGenerateColumns = false;
        _grid.Columns.AddRange(new[]
        {
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Roles, DataPropertyName = "Name", Width = 200 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Description, DataPropertyName = "Description", Width = 350 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Permissions, DataPropertyName = "PermissionsCount", Width = 120 }
        });
        _grid.DataSource = _roles;
    }

    private async Task AddRole()
    {
        using var dialog = new RoleEditDialog(_roleService, _permissionService, _languageService);
        if (dialog.ShowDialog() == DialogResult.OK)
            await LoadDataAsync();
    }

    private async Task EditRole()
    {
        if (_grid.CurrentRow?.DataBoundItem is not RoleDto role) return;
        using var dialog = new RoleEditDialog(_roleService, _permissionService, _languageService, role.Id);
        if (dialog.ShowDialog() == DialogResult.OK)
            await LoadDataAsync();
    }

    private async Task DeleteRole()
    {
        if (_grid.CurrentRow?.DataBoundItem is not RoleDto role) return;
        var confirm = MessageBox.Show(
            $"{AppStrings.ConfirmDelete}\n{role.Name}",
            AppStrings.DeleteConfirmation,
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes) return;

        var result = await _roleService.DeleteAsync(role.Id);
        if (result.Success)
            await LoadDataAsync();
        else
            AppMessageBox.Error(result.Message ?? AppStrings.ErrorMsg);
    }
}
