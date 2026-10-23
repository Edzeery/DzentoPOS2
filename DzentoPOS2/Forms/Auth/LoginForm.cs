 
using DzentoPOS2.App.Services.Identity.Interfaces; 
using DzentoPOS2.App.Session;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Identity.Auth; 
using DzentoPOS2.WinForms.Configurations;
using DzentoPOS2.WinForms.Controls;
using DzentoPOS2.WinForms.Controls.Header;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Services;
using DzentoPOS2.WinForms.Themes; 
using FluentValidation;
using FontAwesome.Sharp;
namespace DzentoPOS2.WinForms.Forms.Auth
{
    public partial class LoginForm : Form, IThemeAware
    {
        private readonly IAuthService _authService;

        private readonly IValidator<LoginDto> _validator;

        public event EventHandler<UserSession>? LoginSuccess;

        private readonly ILanguageService _languageService;
        private readonly IUserPreferencesService? _preferences;
        private AppSettings _settings;
        private TableLayoutPanel? _rootSplit;
        private TableLayoutPanel? _loginContent;
        private Image? _defaultLoginLogo;
        private bool _rememberedLoginRestored;


        public LoginForm(
            IAuthService authService,
            IValidator<LoginDto> validator,
            ILanguageService languageService,
            IUserPreferencesService? preferences)
        {
            InitializeComponent();
            _authService = authService;
            _validator = validator;

            _languageService = languageService;
            _preferences = preferences;
            _settings = preferences?.Load() ?? new AppSettings();

            _defaultLoginLogo = pictureBox2.Image;

            BuildResponsiveLayout();
            FormLocalizationHelper.ApplyDirection(this, _languageService);
            ConfigureLanguageComboBox();

            ApplyStoreBranding();

            LoadText();

            btnIconThemMode.Click += (_, _) => ThemeManager.ToggleMode();

            // Enter submits the form from either field (the custom button is not
            // an IButtonControl, so Form.AcceptButton cannot be used).
            txtUserName.KeyDown += OnLoginFieldKeyDown;
            txtPassword.KeyDown += OnLoginFieldKeyDown;

            ThemeManager.ThemeChanged += OnThemeChanged;
            ApplyTheme();
        }

        /// <summary>
        /// Shows the custom store logo (when one was imported) and the store
        /// name in the branding panel of the login screen.
        /// </summary>
        private void ApplyStoreBranding()
        {
            string storeName = _settings.StoreName?.Trim() ?? string.Empty;
            label5.Text = storeName;

            var custom = StoreBranding.LoadLogo();
            if (custom is null) return;

            if (!ReferenceEquals(pictureBox2.Image, _defaultLoginLogo))
                pictureBox2.Image?.Dispose();

            pictureBox2.Image = custom;
        }

        /// <summary>Submits the login when Enter is pressed in a field.</summary>
        private void OnLoginFieldKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            ButtonLogin_Click(this, EventArgs.Empty);
        }

        private void OnThemeChanged(object? sender, EventArgs e)
        {
            if (IsDisposed) return;
            if (InvokeRequired) { BeginInvoke(ApplyTheme); return; }
            ApplyTheme();
        }

        public void ApplyTheme()
        {
            if (IsDisposed) return;

            bool isDark = ThemeManager.IsDark;

            BackColor = AppColors.Background;
            panel1.BackColor = AppColors.Surface;
            pnlLeft.BackColor = AppColors.SidebarSurface;

            labelWelcomeBack.BackColor = Color.Transparent;
            labelWelcomeBack.ForeColor = AppColors.Ink;
            labelWelcomeDes.BackColor = Color.Transparent;
            labelWelcomeDes.ForeColor = AppColors.InkMuted;
            labelVersion.BackColor = Color.Transparent;
            labelVersion.ForeColor = AppColors.InkMuted;
            lblUserName.BackColor = Color.Transparent;
            lblUserName.ForeColor = AppColors.Ink;
            lblPassword.BackColor = Color.Transparent;
            lblPassword.ForeColor = AppColors.Ink;
            label5.ForeColor = AppColors.OnDark;
            lblCopyRight.BackColor = Color.Transparent;
            lblCopyRight.ForeColor = AppColors.InkMuted;

            ckbRememberMe.BackColor = Color.Transparent;
            ckbRememberMe.ForeColor = AppColors.Ink;

            ApplyInputTheme(txtUserName);
            ApplyInputTheme(txtPassword);

            ButtonLogin.BackColor = AppColors.Primary;
            ButtonLogin.BackgroundColor = AppColors.Primary;
            ButtonLogin.HoverColor = AppColors.PrimaryHover;
            ButtonLogin.PressedColor = AppColors.PrimaryPressed;
            ButtonLogin.BorderColor = AppColors.Primary;
            ButtonLogin.ForeColor = Color.White;
            ButtonLogin.TextColor = Color.White;
            ButtonLogin.DisabledColor = AppColors.DisabledBack;

            iconPictureBoxlanguage.BackColor = Color.Transparent;
            iconPictureBoxlanguage.IconColor = AppColors.InkMuted;

            btnIconThemMode.IconColor = AppColors.InkMuted;
            btnIconThemMode.IconChar = isDark ? IconChar.Sun : IconChar.Moon;

            HeaderComboBoxStyle.Apply(comboBoxLang);
        }

        private static void ApplyInputTheme(EZControls.Controls.Inputs.EZTextBox input)
        {
            input.BackColor = AppColors.InputBackground;
            input.ForeColor = AppColors.Ink;
            input.BorderColor = AppColors.InputBorder;
            input.BorderFocusColor = AppColors.InputBorderFocus;
            input.PlaceholderColor = AppColors.InkMuted;
        }

        private void BuildResponsiveLayout()
        {
            BuildBrandingLayout();
            BuildLoginLayout();

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 520F));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));

            pnlLeft.Dock = DockStyle.Fill;
            panel1.Dock = DockStyle.Fill;
            root.Controls.Add(pnlLeft, 0, 0);
            root.Controls.Add(panel1, 1, 0);

            // The copyright lives in its own strip under both columns so it
            // never floats inside the branding panel.
            lblCopyRight.Dock = DockStyle.Fill;
            lblCopyRight.AutoSize = false;
            lblCopyRight.AutoEllipsis = true;
            lblCopyRight.TextAlign = ContentAlignment.MiddleCenter;
            lblCopyRight.Margin = Padding.Empty;
            lblCopyRight.Padding = new Padding(8, 0, 8, 0);
            lblCopyRight.Font = new Font("Segoe UI", 8.5F);
            root.Controls.Add(lblCopyRight, 0, 1);
            root.SetColumnSpan(lblCopyRight, 2);

            Controls.Clear();
            Controls.Add(root);

            _rootSplit = root;

            Resize += (_, _) => ApplyResponsiveSizing();
        }

        private void BuildBrandingLayout()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = Color.Transparent,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            pictureBox2.Dock = DockStyle.Fill;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.Margin = new Padding(0, 8, 0, 0);
            layout.Controls.Add(pictureBox2, 0, 1);

            pictureBox3.Dock = DockStyle.Fill;
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox3.Margin = new Padding(0, 6, 0, 0);
            layout.Controls.Add(pictureBox3, 0, 2);

            label5.Dock = DockStyle.Fill;
            label5.AutoSize = false;
            label5.TextAlign = ContentAlignment.MiddleCenter;
            label5.Margin = Padding.Empty;
            layout.Controls.Add(label5, 0, 3);

            pnlLeft.Controls.Clear();
            pnlLeft.Controls.Add(layout);
        }

        private void BuildLoginLayout()
        {
            var topBar = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = Padding.Empty
            };

            btnIconThemMode.Dock = DockStyle.Right;
            btnIconThemMode.Margin = new Padding(0, 10, 16, 0);
            comboBoxLang.Dock = DockStyle.Right;
            comboBoxLang.Margin = new Padding(0, 13, 8, 0);
            iconPictureBoxlanguage.Dock = DockStyle.Right;
            iconPictureBoxlanguage.Margin = new Padding(0, 14, 6, 0);
            topBar.Controls.Add(iconPictureBoxlanguage);
            topBar.Controls.Add(comboBoxLang);
            topBar.Controls.Add(btnIconThemMode);

            var fields = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 12,
                BackColor = Color.Transparent,
                Margin = Padding.Empty
            };
            // A single column that fills the field area. Without an explicit
            // style the column defaults to AutoSize and the controls (which
            // carry their designer width) overflow the 518px centre column.
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            fields.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            fields.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            labelWelcomeBack.Dock = DockStyle.Fill;
            labelWelcomeBack.AutoSize = false;
            labelWelcomeBack.TextAlign = ContentAlignment.MiddleCenter;
            labelWelcomeBack.Margin = Padding.Empty;
            fields.Controls.Add(labelWelcomeBack, 0, 1);

            labelWelcomeDes.Dock = DockStyle.Fill;
            labelWelcomeDes.AutoSize = false;
            labelWelcomeDes.TextAlign = ContentAlignment.MiddleCenter;
            labelWelcomeDes.Margin = Padding.Empty;
            fields.Controls.Add(labelWelcomeDes, 0, 2);

            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.Margin = Padding.Empty;
            fields.Controls.Add(pictureBox1, 0, 3);

            lblUserName.Dock = DockStyle.Fill;
            lblUserName.AutoSize = false;
            lblUserName.TextAlign = ContentAlignment.MiddleLeft;
            lblUserName.Margin = Padding.Empty;
            fields.Controls.Add(lblUserName, 0, 4);

            txtUserName.Dock = DockStyle.Fill;
            txtUserName.Margin = Padding.Empty;
            fields.Controls.Add(txtUserName, 0, 5);

            lblPassword.Dock = DockStyle.Fill;
            lblPassword.AutoSize = false;
            lblPassword.TextAlign = ContentAlignment.MiddleLeft;
            lblPassword.Margin = Padding.Empty;
            fields.Controls.Add(lblPassword, 0, 6);

            txtPassword.Dock = DockStyle.Fill;
            txtPassword.Margin = Padding.Empty;
            fields.Controls.Add(txtPassword, 0, 7);

            ckbRememberMe.Dock = DockStyle.Top;
            ckbRememberMe.AutoSize = true;
            ckbRememberMe.Margin = Padding.Empty;
            fields.Controls.Add(ckbRememberMe, 0, 8);

            ButtonLogin.Dock = DockStyle.Fill;
            ButtonLogin.Margin = Padding.Empty;
            fields.Controls.Add(ButtonLogin, 0, 9);

            labelVersion.Dock = DockStyle.Fill;
            labelVersion.AutoSize = false;
            labelVersion.TextAlign = ContentAlignment.MiddleCenter;
            labelVersion.Margin = Padding.Empty;
            fields.Controls.Add(labelVersion, 0, 10);

            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = Padding.Empty
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 518F));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            content.Controls.Add(fields, 1, 0);

            _loginContent = content;

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.Controls.Add(topBar, 0, 0);
            root.Controls.Add(content, 0, 1);

            panel1.Controls.Clear();
            panel1.Controls.Add(root);
        }

        private void ApplyResponsiveSizing()
        {
            var area = Screen.FromControl(this).WorkingArea;

            int width = Math.Min(1200, (int)(area.Width * 0.96));
            int height = Math.Min(800, (int)(area.Height * 0.96));

            if (Width != width || Height != height)
            {
                Size = new Size(width, height);
                StartPosition = FormStartPosition.CenterScreen;
            }

            if (_rootSplit != null && _rootSplit.ColumnStyles.Count > 0)
            {
                int brand = Math.Max(240, Math.Min(520, (int)(_rootSplit.ClientSize.Width * 0.42)));
                if (_rootSplit.ColumnStyles[0].Width != brand)
                    _rootSplit.ColumnStyles[0].Width = brand;
            }

            if (_loginContent != null && _loginContent.ColumnStyles.Count >= 3)
            {
                int field = Math.Max(260, Math.Min(518, _loginContent.ClientSize.Width - 24));
                if (_loginContent.ColumnStyles[1].Width != field)
                    _loginContent.ColumnStyles[1].Width = field;
            }
        }

        private void ConfigureLanguageComboBox()
        {
            HeaderComboBoxStyle.Apply(comboBoxLang);
            comboBoxLang.Items = new[] { "العربية", "English", "Français" };
          
            iconPictureBoxlanguage.BackColor = Color.Transparent;
            iconPictureBoxlanguage.IconColor = HeaderColors.Accent;
        }

        private void LoadText()
        {
            txtUserName.Focus();
            labelWelcomeBack.Text = AppStrings.WelcomeBack;
            labelWelcomeDes.Text = AppStrings.WelcomeDes;
            ButtonLogin.Text = AppStrings.Login;
            this.Text = AppStrings.Login;
            lblUserName.Text = AppStrings.Username;
            lblPassword.Text = AppStrings.Password;
            lblCopyRight.Text = AppStrings.CopyRight + Convert.ToString(DateTime.Now.Year);
            ckbRememberMe.Text = AppStrings.RememberMe;

            labelVersion.Text = AppStrings.Version + " " + Application.ProductVersion;
            RadiusControl.Bind(ButtonLogin, AppStyle.RadiusMedium);

            RestoreRememberedLogin();

            ckbRememberMe.Checked = _settings.RememberMe;
            comboBoxLang.SelectedIndex =
                _settings.Language switch
                {
                    "ar" => 0,
                    "en" => 1,
                    "fr" => 2,
                    _ => 0
                };
        }

        /// <summary>
        /// Fills the login form with the credential kept in the Windows
        /// credential manager. The password is never stored in a settings file.
        /// </summary>
        private void RestoreRememberedLogin()
        {
            if (_rememberedLoginRestored) return;
            _rememberedLoginRestored = true;

            txtPassword.Text = string.Empty;

            if (_settings.RememberMe &&
                CredentialStore.TryRead(out string savedUser, out string savedPassword))
            {
                txtUserName.Text = savedUser;
                txtPassword.Text = savedPassword;
                return;
            }

            txtUserName.Text = _settings.LastUserName;
        }

        /// <summary>
        /// Keeps the stored credential in sync with the "remember me" box:
        /// unchecking it removes the password from the credential manager.
        /// </summary>
        private void ckbRememberMe_CheckedChanged(object sender, EventArgs e)
        {
            if (!ckbRememberMe.Checked)
                CredentialStore.Delete();
        }
         

        private async void ButtonLogin_Click(object sender, EventArgs e)
        {
            try
            {

                ButtonLogin.Enabled = false;

                var dto = new LoginDto
                {
                    UserName = txtUserName.Text?.Trim() ?? string.Empty,
                    Password = txtPassword.Text ?? string.Empty
                };

                var validationResult =
                    await _validator.ValidateAsync(dto);

                if (!validationResult.IsValid)
                {
                    AppMessageBox.Error(validationResult.Errors.First().ErrorMessage);
                    return;
                }

                var result = await _authService.LoginAsync(dto);

                if (!result.Success)
                {
                    AppMessageBox.Error(result.Message ?? AppStrings.LoginFailed);
                    return;
                }

                // 🔥 تحويل DTO → Session (مهم جدًا)
                var session = new UserSession
                {
                    UserId = result.Data!.UserId,
                    UserName = result.Data.UserName,
                    FullName = result.Data.FullName,
                    RoleName = result.Data.RoleName,
                    BranchName = result.Data.BranchName,
                    BranchId = result.Data.BranchId,
                    IsAdmin = result.Data.IsAdmin
                };
                _settings.RememberMe = ckbRememberMe.Checked;

                _settings.LastUserName =
                    ckbRememberMe.Checked
                        ? txtUserName.Text ?? string.Empty
                        : "";

                _preferences?.Save(_settings);

                // The password only lives inside the Windows credential manager.
                if (ckbRememberMe.Checked)
                    CredentialStore.Save(txtUserName.Text ?? string.Empty, txtPassword.Text ?? string.Empty);
                else
                    CredentialStore.Delete();

                // 🔥 إطلاق الحدث
                LoginSuccess?.Invoke(this, session);

                this.Close();
            }
            finally
            {
                ButtonLogin.Enabled = true;

            }
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            ApplyResponsiveSizing();

            //string[] countries = { "Egypt", "Saudi Arabia", "UAE", "Morocco" };
            //combo.Add(countries);

            //// Or from a list
            //var items = new List<string> { "One", "Two", "Three" };
            //combo.Add(items.ToArray());


             comboBoxLang.ItemHeight = 30;
             comboBoxLang.MaxDropDownItems = 6; 
        }

        private void comboBoxLang_SelectedIndexChanged(object sender, EventArgs e)
        {
            string culture = comboBoxLang.SelectedIndex switch
            {
                0 => "ar",
                1 => "en",
                2 => "fr",
                _ => "ar"
            };

            if (culture == _settings.Language) return;

            _languageService.SetLanguage(culture);
            _settings.Language = culture;
            _preferences?.Save(_settings);

            FormLocalizationHelper.ApplyDirection(this, _languageService);
            LoadText();
        }
    }
}
