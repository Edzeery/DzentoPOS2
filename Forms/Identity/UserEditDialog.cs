using DzentoPOS2.App.Services.Identity.Interfaces;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Identity.Roles;
using DzentoPOS2.Shared.Contracts.DTOs.Identity.Users;
using DzentoPOS2.Shared.Contracts.Requests;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Forms.Identity;

public class UserEditDialog : Form
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly ILanguageService _languageService;
    private bool _languageSubscribed;
    private readonly Guid? _userId;

    private readonly Label _lblFullName;
    private readonly Label _lblUserName;
    private readonly Label _lblEmail;
    private readonly Label _lblPhone;
    private readonly Label _lblPassword;
    private readonly Label _lblRoles;
    private readonly TextBox _txtFullName;
    private readonly TextBox _txtUserName;
    private readonly TextBox _txtEmail;
    private readonly TextBox _txtPhone;
    private readonly TextBox _txtPassword;
    private readonly CheckedListBox _clbRoles;
    private readonly CheckBox _chkActive;
    private readonly Button _btnSave;
    private readonly Button _btnCancel;

    public UserEditDialog(
        IUserService userService,
        IRoleService roleService,
        ILanguageService languageService,
        Guid? userId = null)
    {
        _userService = userService;
        _roleService = roleService;
        _languageService = languageService;
        _userId = userId;

        Size = new Size(520, 520);
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

        _lblFullName = lbl(AppStrings.FullName, 20);
        _lblUserName = lbl(AppStrings.Username, 58);
        _lblEmail = lbl(AppStrings.Email, 96);
        _lblPhone = lbl(AppStrings.Phone, 134);
        _lblPassword = lbl(AppStrings.Password, 172);
        _lblRoles = lbl(AppStrings.Roles, 210);

        _txtFullName = new TextBox { Location = new Point(140, 20), Size = new Size(330, 28), Font = new Font("Segoe UI", 10F) };
        _txtUserName = new TextBox { Location = new Point(140, 58), Size = new Size(330, 28), Font = new Font("Segoe UI", 10F) };
        _txtEmail = new TextBox { Location = new Point(140, 96), Size = new Size(330, 28), Font = new Font("Segoe UI", 10F) };
        _txtPhone = new TextBox { Location = new Point(140, 134), Size = new Size(330, 28), Font = new Font("Segoe UI", 10F) };
        _txtPassword = new TextBox { Location = new Point(140, 172), Size = new Size(330, 28), Font = new Font("Segoe UI", 10F), UseSystemPasswordChar = true };

        _clbRoles = new CheckedListBox
        {
            Location = new Point(140, 210),
            Size = new Size(330, 130),
            Font = new Font("Segoe UI", 10F),
            CheckOnClick = true
        };

        _chkActive = new CheckBox
        {
            Text = AppStrings.Active,
            Location = new Point(140, 350),
            Size = new Size(200, 28),
            Font = new Font("Segoe UI", 10F),
            Checked = true
        };

        _btnSave = ButtonFactory.Primary(AppStrings.Save);
        _btnSave.Location = new Point(140, 400);
        _btnSave.Size = new Size(150, 40);
        _btnSave.Click += async (_, _) => await SaveAsync();

        _btnCancel = ButtonFactory.Neutral(AppStrings.Cancel);
        _btnCancel.Location = new Point(300, 400);
        _btnCancel.Size = new Size(150, 40);
        _btnCancel.DialogResult = DialogResult.Cancel;

        Controls.AddRange(new Control[]
        {
            _lblFullName, _txtFullName,
            _lblUserName, _txtUserName,
            _lblEmail, _txtEmail,
            _lblPhone, _txtPhone,
            _lblPassword, _txtPassword,
            _lblRoles, _clbRoles,
            _chkActive,
            _btnSave, _btnCancel
        });

        LoadText();
        _languageService.LanguageChanged += OnLanguageChanged;
        _languageSubscribed = true;
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        var roleResult = await _roleService.GetPagedAsync(
            new PagedRequest { PageSize = int.MaxValue });

        if (roleResult.Success && roleResult.Data != null)
        {
            _clbRoles.Items.Clear();
            foreach (var role in roleResult.Data.Items)
                _clbRoles.Items.Add(role);
            _clbRoles.DisplayMember = "Name";
        }

        if (_userId.HasValue)
        {
            var userResult = await _userService.GetByIdAsync(_userId.Value);
            if (userResult.Success && userResult.Data != null)
            {
                var u = userResult.Data;
                _txtFullName.Text = u.FullName;
                _txtUserName.Text = u.UserName;
                _txtEmail.Text = u.Email ?? "";
                _txtPhone.Text = u.PhoneNumber ?? "";
                _chkActive.Checked = u.IsActive;

                for (int i = 0; i < _clbRoles.Items.Count; i++)
                {
                    if (_clbRoles.Items[i] is RoleDto role && u.RoleIds.Contains(role.Id))
                        _clbRoles.SetItemChecked(i, true);
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
        Text = _userId.HasValue ? AppStrings.Edit : AppStrings.AddUser;
        _lblFullName.Text = AppStrings.FullName;
        _lblUserName.Text = AppStrings.Username;
        _lblEmail.Text = AppStrings.Email;
        _lblPhone.Text = AppStrings.Phone;
        _lblPassword.Text = AppStrings.Password;
        _lblRoles.Text = AppStrings.Roles;
        _chkActive.Text = AppStrings.Active;
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
        if (string.IsNullOrWhiteSpace(_txtFullName.Text) ||
            string.IsNullOrWhiteSpace(_txtUserName.Text))
        {
            AppMessageBox.Warning(AppStrings.ErrorMsg);
            return;
        }

        var roleIds = _clbRoles.CheckedItems
            .Cast<RoleDto>()
            .Select(r => r.Id)
            .ToList();

        try
        {
            if (_userId.HasValue)
            {
                var result = await _userService.UpdateAsync(new UpdateUserDto
                {
                    Id = _userId.Value,
                    FullName = _txtFullName.Text.Trim(),
                    UserName = _txtUserName.Text.Trim(),
                    Email = _txtEmail.Text.Trim(),
                    PhoneNumber = _txtPhone.Text.Trim(),
                    IsActive = _chkActive.Checked,
                    RoleIds = roleIds
                });

                if (!result.Success)
                {
                    AppMessageBox.Error(result.Message ?? AppStrings.ErrorMsg);
                    return;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(_txtPassword.Text))
                {
                    AppMessageBox.Warning(AppStrings.ErrorMsg);
                    return;
                }

                var result = await _userService.CreateAsync(new CreateUserDto
                {
                    FullName = _txtFullName.Text.Trim(),
                    UserName = _txtUserName.Text.Trim(),
                    Email = _txtEmail.Text.Trim(),
                    PhoneNumber = _txtPhone.Text.Trim(),
                    Password = _txtPassword.Text,
                    IsActive = _chkActive.Checked,
                    RoleIds = roleIds
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
