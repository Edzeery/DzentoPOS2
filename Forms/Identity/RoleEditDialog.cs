using DzentoPOS2.App.Services.Identity.Interfaces;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Identity.Permissions;
using DzentoPOS2.Shared.Contracts.DTOs.Identity.Roles;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Forms.Identity;

public class RoleEditDialog : Form
{
    private readonly IRoleService _roleService;
    private readonly IPermissionManagementService _permissionService;
    private readonly ILanguageService _languageService;
    private bool _languageSubscribed;
    private readonly Guid? _roleId;

    private readonly Label _lblName;
    private readonly Label _lblDescription;
    private readonly Label _lblPermissions;
    private readonly TextBox _txtName;
    private readonly TextBox _txtDescription;
    private readonly CheckedListBox _clbPermissions;
    private readonly Button _btnSave;
    private readonly Button _btnCancel;

    public RoleEditDialog(
        IRoleService roleService,
        IPermissionManagementService permissionService,
        ILanguageService languageService,
        Guid? roleId = null)
    {
        _roleService = roleService;
        _permissionService = permissionService;
        _languageService = languageService;
        _roleId = roleId;

        Size = new Size(520, 560);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = AppColors.Surface;

        var lbl = (string text, int y) => new Label
        {
            Text = text,
            Location = new Point(20, y),
            Size = new Size(110, 28),
            Font = new Font("Segoe UI", 10F),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _lblName = lbl(AppStrings.Roles, 20);
        _lblDescription = lbl(AppStrings.Description, 58);
        _lblPermissions = lbl(AppStrings.Permissions, 96);

        _txtName = new TextBox { Location = new Point(140, 20), Size = new Size(330, 28), Font = new Font("Segoe UI", 10F) };
        _txtDescription = new TextBox { Location = new Point(140, 58), Size = new Size(330, 28), Font = new Font("Segoe UI", 10F) };

        _clbPermissions = new CheckedListBox
        {
            Location = new Point(140, 96),
            Size = new Size(330, 340),
            Font = new Font("Segoe UI", 10F),
            CheckOnClick = true
        };

        _btnSave = ButtonFactory.Primary(AppStrings.Save);
        _btnSave.Location = new Point(140, 460);
        _btnSave.Size = new Size(150, 40);
        _btnSave.Click += async (_, _) => await SaveAsync();

        _btnCancel = ButtonFactory.Neutral(AppStrings.Cancel);
        _btnCancel.Location = new Point(300, 460);
        _btnCancel.Size = new Size(150, 40);
        _btnCancel.DialogResult = DialogResult.Cancel;

        Controls.AddRange(new Control[]
        {
            _lblName, _txtName,
            _lblDescription, _txtDescription,
            _lblPermissions, _clbPermissions,
            _btnSave, _btnCancel
        });

        LoadText();
        _languageService.LanguageChanged += OnLanguageChanged;
        _languageSubscribed = true;
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        var permResult = await _permissionService.GetAllAsync();
        if (permResult.Success && permResult.Data != null)
        {
            _clbPermissions.Items.Clear();
            foreach (var perm in permResult.Data)
                _clbPermissions.Items.Add(perm);
            _clbPermissions.DisplayMember = "Name";
        }

        if (_roleId.HasValue)
        {
            var roleResult = await _roleService.GetByIdAsync(_roleId.Value);
            if (roleResult.Success && roleResult.Data != null)
            {
                var r = roleResult.Data;
                _txtName.Text = r.Name;
                _txtDescription.Text = r.Description ?? "";

                for (int i = 0; i < _clbPermissions.Items.Count; i++)
                {
                    if (_clbPermissions.Items[i] is PermissionDto perm && r.PermissionIds.Contains(perm.Id))
                        _clbPermissions.SetItemChecked(i, true);
                }
            }
        }
    }

    private void OnLanguageChanged(object? sender, string culture)
    {
        if (IsDisposed) return;
        if (InvokeRequired) BeginInvoke(LoadText);
        else LoadText();
    }

    private void LoadText()
    {
        Text = _roleId.HasValue ? AppStrings.Edit : AppStrings.Add;
        _lblName.Text = AppStrings.Roles;
        _lblDescription.Text = AppStrings.Description;
        _lblPermissions.Text = AppStrings.Permissions;
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
        if (string.IsNullOrWhiteSpace(_txtName.Text))
        {
            AppMessageBox.Warning(AppStrings.ErrorMsg);
            return;
        }

        var permissionIds = _clbPermissions.CheckedItems
            .Cast<PermissionDto>()
            .Select(p => p.Id)
            .ToList();

        try
        {
            if (_roleId.HasValue)
            {
                var result = await _roleService.UpdateAsync(new UpdateRoleDto
                {
                    Id = _roleId.Value,
                    Name = _txtName.Text.Trim(),
                    Description = _txtDescription.Text.Trim(),
                    PermissionIds = permissionIds
                });

                if (!result.Success)
                {
                    AppMessageBox.Error(result.Message ?? AppStrings.ErrorMsg);
                    return;
                }
            }
            else
            {
                var result = await _roleService.CreateAsync(new CreateRoleDto
                {
                    Name = _txtName.Text.Trim(),
                    Description = _txtDescription.Text.Trim(),
                    PermissionIds = permissionIds
                });

                if (!result.Success)
                {
                    AppMessageBox.Error(result.Message ?? AppStrings.ErrorMsg);
                    return;
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            AppMessageBox.Error(ex.Message);
        }
    }
}
