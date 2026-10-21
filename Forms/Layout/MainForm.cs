using DzentoPOS2.App.Session;
using DzentoPOS2.Localization.Constants;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.WinForms.Configurations;
using DzentoPOS2.WinForms.Controls;
using DzentoPOS2.WinForms.Controls.Header;
using DzentoPOS2.WinForms.Events;
using DzentoPOS2.WinForms.Forms.Accounting;
using DzentoPOS2.WinForms.Forms.Customers;
using DzentoPOS2.WinForms.Forms.Dashboard;
using DzentoPOS2.WinForms.Forms.Identity;
using DzentoPOS2.WinForms.Forms.Inventory;
using DzentoPOS2.WinForms.Forms.Purchases;
using DzentoPOS2.WinForms.Forms.Reports;
using DzentoPOS2.WinForms.Forms.Sales;
using DzentoPOS2.WinForms.Forms.Settings;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Models;
using DzentoPOS2.WinForms.Navigation;
using DzentoPOS2.WinForms.Services;
using DzentoPOS2.WinForms.ServicesUI;
using DzentoPOS2.WinForms.Themes;
using FontAwesome.Sharp;
using System.Runtime;
using System.Runtime.InteropServices; 

namespace DzentoPOS2.WinForms.Forms.Layout
{
    public partial class MainForm : Form
    {
        //Fields
        private const int BorderSize = 2;
        private Size formSize;
        private readonly System.Windows.Forms.Timer _collapseTimer;
        private bool _animating;
        private int _targetWidth;

        private readonly INavigationService _navigation;
        private readonly ICurrentUserService _currentUser;

        private readonly ILanguageService _languageService;
        private readonly IUserPreferencesService _preferences;
        private readonly ICurrentSettingsService _currentSettings;

        private AppSettings _settings;
        private MainHeaderBar _mainHeaderBar = null!;
        private IconButton? _activeSidebarButton;
        private Label _storeNameLabel = null!;
        private Image? _defaultLogo;

        public MainForm(
             ICurrentUserService currentUser,
             INavigationService navigation,
             ILanguageService languageService,
             IUserPreferencesService preferences,
             ICurrentSettingsService currentSettings)
        {
            InitializeComponent();

            _currentUser = currentUser;
            _navigation = navigation;
            _languageService = languageService;
            _preferences = preferences;
            _currentSettings = currentSettings;
            _settings = _preferences.Load();

            Padding = new Padding(BorderSize);

            SidebarPanel.Width = AppStyle.SidebarExpandedWidth;

            _collapseTimer = new System.Windows.Forms.Timer { Interval = AppStyle.AnimationInterval };
            _collapseTimer.Tick += AnimateCollapse;

            InitializeMainHeader();

            _navigation.SetHost(contentPanel, _mainHeaderBar.TitleLabel);

            FormLocalizationHelper.ApplyDirection(this, _languageService);

            //bool isRtl = languageService.IsRightToLeft();
            //if (isRtl)
            //    pictureBoxLogo2.Dock = DockStyle.Right;

            ApplySidebarButtonHover();

            WireNavButtons();
            AddExpensesSidebarButton();

            InitializeStoreBranding();

            LoadText();

            ThemeManager.ThemeChanged += OnThemeChanged;
            _currentSettings.Changed += OnSettingsChanged;
        }

        private void OnSettingsChanged(object? sender, EventArgs e)
        {
            if (IsDisposed) return;
            if (InvokeRequired) { BeginInvoke(ApplyStoreBranding); return; }
            ApplyStoreBranding();
        }

        /// <summary>
        /// Adds the store name below the logo in the sidebar and keeps the
        /// custom logo in sync with the settings.
        /// </summary>
        private void InitializeStoreBranding()
        {
            _defaultLogo = pictureBoxLogo.Image;

            _storeNameLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 26,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.Transparent,
                Margin = Padding.Empty,
                Padding = new Padding(6, 0, 6, 0),
                Text = string.Empty
            };

            pnlLogoANDuserInfo.Controls.Add(_storeNameLabel);
            pnlLogoANDuserInfo.Controls.SetChildIndex(_storeNameLabel, 1);

            ApplyStoreBranding();
        }

        /// <summary>Applies the custom logo and the store name to the shell.</summary>
        private void ApplyStoreBranding()
        {
            if (IsDisposed || _storeNameLabel is null) return;

            string storeName = _currentSettings.Current.StoreName?.Trim() ?? string.Empty;
            _storeNameLabel.Text = storeName;
            _storeNameLabel.Visible = storeName.Length > 0;

            var custom = StoreBranding.LoadLogo();

            if (custom is not null)
            {
                if (!ReferenceEquals(pictureBoxLogo.Image, _defaultLogo))
                    pictureBoxLogo.Image?.Dispose();

                pictureBoxLogo.Image = custom;
            }
            else if (!ReferenceEquals(pictureBoxLogo.Image, _defaultLogo))
            {
                pictureBoxLogo.Image?.Dispose();
                pictureBoxLogo.Image = _defaultLogo;
            }
        }

        private void OnThemeChanged(object? sender, EventArgs e)
        {
            if (IsDisposed) return;
            if (InvokeRequired) { BeginInvoke(ApplyShellTheme); return; }
            ApplyShellTheme();
        }

        /// <summary>
        /// Repaints the application shell (sidebar, top bar, status bar) so the
        /// whole window follows the active light/dark mode.
        /// </summary>
        private void ApplyShellTheme()
        {
            if (IsDisposed) return;

            var sidebar = AppColors.SidebarSurface;
            var accent = AppColors.Primary;

            SidebarPanel.BackColor = sidebar;
            SidebarPanelM.BackColor = sidebar;
            SidebarPanelB.BackColor = sidebar;
            StatusBarPanel.BackColor = sidebar;
            TopbarPanel.BackColor = sidebar;
            pnlLogoANDuserInfo.BackColor = sidebar;
            pnlUserInfo.BackColor = sidebar;
            iconPictureBox1.BackColor = sidebar;
            pictureBoxLogo.BackColor = Color.Transparent;
            labelVersion.BackColor = sidebar;
            labelVersion.ForeColor = AppColors.InkMuted;
            panel2.BackColor = accent;

            var isActive = _activeSidebarButton;

            ApplyToSidebarButtons(btn =>
            {
                btn.FlatAppearance.MouseOverBackColor = accent;
                btn.FlatAppearance.MouseDownBackColor = AppColors.SidebarPressed;

                bool active = ReferenceEquals(btn, isActive);
                btn.BackColor = active ? accent : sidebar;
                btn.ForeColor = Color.White;
                btn.IconColor = Color.White;
            });

            _mainHeaderBar.ApplyTheme();
        }

        private void InitializeMainHeader()
        {
            _mainHeaderBar = new MainHeaderBar();
            _mainHeaderBar.MenuClicked += (_, _) => CollapseMenu();
            _mainHeaderBar.SettingsMenuActionSelected += MainHeaderBar_SettingsMenuActionSelected;
            _mainHeaderBar.UserMenu.MenuActionSelected += UserMenu_MenuActionSelected;
            _mainHeaderBar.BranchSelector.BranchChanged += BranchSelector_BranchChanged;
            _mainHeaderBar.LanguageChanged += (_, _) => ChangeLanguage(_mainHeaderBar.SelectedLanguageIndex);
            settingsButton1.MenuActionSelected += MainHeaderBar_SettingsMenuActionSelected;
            ConfigureSidebarSettingsButton();

            HeaderPanel.SuspendLayout();
            HeaderPanel.Controls.Clear();
            HeaderPanel.Controls.Add(_mainHeaderBar);
            HeaderPanel.ResumeLayout();
        }

        private void ApplySidebarButtonHover()
        {
            var accent = AppColors.Primary;
            var dark = AppColors.SidebarSurface;
            ApplyToSidebarButtons(btn =>
            {
                btn.FlatAppearance.MouseOverBackColor = accent;
                btn.FlatAppearance.MouseDownBackColor = AppColors.SidebarPressed;
                btn.MouseEnter += (_, _) =>
                {
                    if (btn.BackColor != accent)
                        btn.BackColor = AppColors.SidebarHover;
                };
                btn.MouseLeave += (_, _) =>
                {
                    if (btn.BackColor != accent && btn.BackColor != dark)
                        btn.BackColor = dark;
                };
            });
            btnDashboard.BackColor = accent;
        }

        private void AddExpensesSidebarButton()
        {
            var btnExpenses = new IconButton
            {
                IconChar = IconChar.MoneyBillWave,
                Text = "  " + AppStrings.ExpensesButton,
                IconColor = Color.White,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13F),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Dock = DockStyle.Top,
                Size = new Size(279, 55),
                Padding = new Padding(13, 0, 13, 0),
                TextImageRelation = TextImageRelation.ImageBeforeText,
                ImageAlign = ContentAlignment.MiddleLeft,
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand,
                BackColor = AppColors.SidebarSurface,
                IconSize = 35,
                Name = "btnExpenses"
            };

            btnExpenses.Click += (_, _) => NavigateTo<ExpensesForm>(btnExpenses);
            btnExpenses.FlatAppearance.MouseOverBackColor = AppColors.Primary;
            btnExpenses.FlatAppearance.MouseDownBackColor = AppColors.SidebarPressed;
            btnExpenses.MouseEnter += (_, _) =>
            {
                if (btnExpenses.BackColor != AppColors.Primary)
                    btnExpenses.BackColor = AppColors.SidebarHover;
            };
            btnExpenses.MouseLeave += (_, _) =>
            {
                if (btnExpenses.BackColor != AppColors.Primary && btnExpenses.BackColor != AppColors.SidebarSurface)
                    btnExpenses.BackColor = AppColors.SidebarSurface;
            };

            SidebarPanelM.Controls.Add(btnExpenses);
            SidebarPanelM.Controls.SetChildIndex(btnExpenses, SidebarPanelM.Controls.GetChildIndex(btnInventory) + 1);
        }

        private void WireNavButtons()
        {
            btnDashboard.Click += (s, e) => NavigateTo<DashboardForm>(btnDashboard);
            BtnProducts.Click += (s, e) => NavigateTo<ProductsForm>(BtnProducts);
            BtnPurchases.Click += (s, e) => NavigateTo<PurchasesForm>(BtnPurchases);
            BtnSales.Click += (s, e) => NavigateTo<SalesForm>(BtnSales);
            BtnCustomers.Click += (s, e) => NavigateTo<CustomersForm>(BtnCustomers);
            btnInventory.Click += (s, e) => NavigateTo<InventoryForm>(btnInventory);
            btnReports.Click += (s, e) => NavigateTo<ReportsForm>(btnReports);
            btnUsersRoles.Click += (s, e) => NavigateTo<UsersForm>(btnUsersRoles);
            btnSettings.Click += (s, e) => NavigateTo<SettingsForm>(btnSettings);
        }

        private void NavigateTo<TForm>(IconButton button)
            where TForm : Form
        {
            _activeSidebarButton = button;
            _navigation.SetActiveButton(button);
            _navigation.Navigate<TForm>();
        }


        //Drag Form
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        private void TopbarPanel_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        //Overridden methods
        protected override void WndProc(ref Message m)
        {
            const int WM_NCCALCSIZE = 0x0083;//Standar Title Bar - Snap Window
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MINIMIZE = 0xF020; //Minimize form (Before)
            const int SC_RESTORE = 0xF120; //Restore form (Before)
            const int WM_NCHITTEST = 0x0084;//Win32, Mouse Input Notification: Determine what part of the window corresponds to a point, allows to resize the form.
            const int resizeAreaSize = 10;
            #region Form Resize
            // Resize/WM_NCHITTEST values
            const int HTCLIENT = 1; //Represents the client area of the window
            const int HTLEFT = 10;  //Left border of a window, allows resize horizontally to the left
            const int HTRIGHT = 11; //Right border of a window, allows resize horizontally to the right
            const int HTTOP = 12;   //Upper-horizontal border of a window, allows resize vertically up
            const int HTTOPLEFT = 13;//Upper-left corner of a window border, allows resize diagonally to the left
            const int HTTOPRIGHT = 14;//Upper-right corner of a window border, allows resize diagonally to the right
            const int HTBOTTOM = 15; //Lower-horizontal border of a window, allows resize vertically down
            const int HTBOTTOMLEFT = 16;//Lower-left corner of a window border, allows resize diagonally to the left
            const int HTBOTTOMRIGHT = 17;//Lower-right corner of a window border, allows resize diagonally to the right
            ///<Doc> More Information: https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-nchittest </Doc>
            if (m.Msg == WM_NCHITTEST)
            { //If the windows m is WM_NCHITTEST
                base.WndProc(ref m);
                if (this.WindowState == FormWindowState.Normal)//Resize the form if it is in normal state
                {
                    if ((int)m.Result == HTCLIENT)//If the result of the m (mouse pointer) is in the client area of the window
                    {
                        Point screenPoint = new Point(m.LParam.ToInt32()); //Gets screen point coordinates(X and Y coordinate of the pointer)                           
                        Point clientPoint = this.PointToClient(screenPoint); //Computes the location of the screen point into client coordinates                          
                        if (clientPoint.Y <= resizeAreaSize)//If the pointer is at the top of the form (within the resize area- X coordinate)
                        {
                            if (clientPoint.X <= resizeAreaSize) //If the pointer is at the coordinate X=0 or less than the resizing area(X=10) in 
                                m.Result = (IntPtr)HTTOPLEFT; //Resize diagonally to the left
                            else if (clientPoint.X < (this.Size.Width - resizeAreaSize))//If the pointer is at the coordinate X=11 or less than the width of the form(X=Form.Width-resizeArea)
                                m.Result = (IntPtr)HTTOP; //Resize vertically up
                            else //Resize diagonally to the right
                                m.Result = (IntPtr)HTTOPRIGHT;
                        }
                        else if (clientPoint.Y <= (this.Size.Height - resizeAreaSize)) //If the pointer is inside the form at the Y coordinate(discounting the resize area size)
                        {
                            if (clientPoint.X <= resizeAreaSize)//Resize horizontally to the left
                                m.Result = (IntPtr)HTLEFT;
                            else if (clientPoint.X > (this.Width - resizeAreaSize))//Resize horizontally to the right
                                m.Result = (IntPtr)HTRIGHT;
                        }
                        else
                        {
                            if (clientPoint.X <= resizeAreaSize)//Resize diagonally to the left
                                m.Result = (IntPtr)HTBOTTOMLEFT;
                            else if (clientPoint.X < (this.Size.Width - resizeAreaSize)) //Resize vertically down
                                m.Result = (IntPtr)HTBOTTOM;
                            else //Resize diagonally to the right
                                m.Result = (IntPtr)HTBOTTOMRIGHT;
                        }
                    }
                }
                return;
            }
            #endregion
            //Remove border and keep snap window
            if (m.Msg == WM_NCCALCSIZE && m.WParam.ToInt32() == 1)
            {
                return;
            }
            //Keep form size when it is minimized and restored. Since the form is resized because it takes into account the size of the title bar and borders.
            if (m.Msg == WM_SYSCOMMAND)
            {
                /// <see cref="https://docs.microsoft.com/en-us/windows/win32/menurc/wm-syscommand"/>
                /// Quote:
                /// In WM_SYSCOMMAND messages, the four low - order bits of the wParam parameter 
                /// are used internally by the system.To obtain the correct result when testing 
                /// the value of wParam, an application must combine the value 0xFFF0 with the 
                /// wParam value by using the bitwise AND operator.
                int wParam = (m.WParam.ToInt32() & 0xFFF0);
                if (wParam == SC_MINIMIZE)  //Before
                    formSize = this.ClientSize;
                if (wParam == SC_RESTORE)// Restored form(Before)
                    this.Size = formSize;
            }
            base.WndProc(ref m);
        }



        private void LoadText()
        {
            labelVersion.Text = AppStrings.Version + " " + Application.ProductVersion;
            lblFullName.Text = _currentUser.User?.FullName;
            lblRole.Text = _currentUser.User?.RoleName;
            lblBranch.Text = _currentUser.User?.BranchName;

            _mainHeaderBar.SetUser(new UserInfoViewModel
            {
                Id = _currentUser.User?.UserId ?? Guid.Empty,
                UserName = _currentUser.User?.UserName ?? "user",
                FullName = _currentUser.User?.FullName ?? _currentUser.User?.UserName ?? "User",
                RoleName = _currentUser.User?.RoleName ?? ""
            });

            _mainHeaderBar.SetBranches(new[]
            {
                new BranchViewModel
                {
                    Id = _currentUser.User?.BranchId ?? Guid.Empty,
                    Name = string.IsNullOrWhiteSpace(_currentUser.User?.BranchName)
                        ? "Main Branch"
                        : _currentUser.User.BranchName,
                    IsMainBranch = true,
                    IsActive = true
                }
            }, _currentUser.User?.BranchId);

            _mainHeaderBar.SetNotifications(Array.Empty<NotificationItem>());
            ConfigureSettingsMenus();

            btnDashboard.Text = AppStrings.Dashboard; 

            BtnProducts.Text = AppStrings.Products; 

            BtnPurchases.Text = AppStrings.Purchases; 


            BtnSales.Text = AppStrings.Sales; 
            BtnCustomers.Text = AppStrings.Customers; 

            btnInventory.Text = AppStrings.Inventory; 


            btnReports.Text = AppStrings.Reports; 


            btnUsersRoles.Text = AppStrings.Users; 

            btnSettings.Text = AppStrings.Settings; 
            settingsButton1.Text = "  " + AppStrings.Settings; 

            SetSidebarButtonTags();

        }
        private void btnClose_Click(object sender, EventArgs e) => Close();

        private void btnMaximize_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                formSize = ClientSize;
                WindowState = FormWindowState.Maximized;
                Padding = new Padding(0);
            }
            else
            {
                WindowState = FormWindowState.Normal;
                Size = formSize;
                Padding = new Padding(BorderSize);
            }
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            formSize = this.ClientSize;
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnLogout_Click(object sender, EventArgs e) => Application.Exit();

        private void CollapseMenu()
        {
            bool expanded = SidebarPanel.Width >= AppStyle.SidebarExpandedWidth - 10;
            _targetWidth = expanded ? AppStyle.SidebarCollapsedWidth : AppStyle.SidebarExpandedWidth;

            if (_animating) return;
            _animating = true;
            _collapseTimer.Start();
        }

        private void AnimateCollapse(object? sender, EventArgs e)
        {
            int step = AppStyle.AnimationStep;
            bool expanding = _targetWidth > SidebarPanel.Width;

            if (expanding)
            {
                SidebarPanel.Width += step;
                if (SidebarPanel.Width >= _targetWidth)
                {
                    SidebarPanel.Width = _targetWidth;
                    StopCollapseAnimation();
                }
            }
            else
            {
                SidebarPanel.Width -= step;
                if (SidebarPanel.Width <= _targetWidth)
                {
                    SidebarPanel.Width = _targetWidth;
                    StopCollapseAnimation();
                }
            }

            UpdateUserInfoVisibility(SidebarPanel.Width >= AppStyle.SidebarExpandedWidth - 10);
        }

        private void StopCollapseAnimation()
        {
            _collapseTimer.Stop();
            _animating = false;

            bool expanded = SidebarPanel.Width >= AppStyle.SidebarExpandedWidth - 10;
            ApplyToSidebarButtons(btn =>
            {
                if (!expanded)
                {
                    btn.Text = "";
                    btn.ImageAlign = ContentAlignment.MiddleCenter;
                    btn.Padding = new Padding(0);
                }
                else
                {
                    btn.Text = btn.Tag?.ToString() ?? "";
                    btn.ImageAlign = ContentAlignment.MiddleLeft;
                    btn.Padding = new Padding(13, 0, 0, 0);
                }
            });
            UpdateUserInfoVisibility(expanded);
        }

        private void UpdateUserInfoVisibility(bool expanded)
        {
            pnlUserInfo.Visible = expanded;
            if (_storeNameLabel is not null)
                _storeNameLabel.Visible = expanded && _storeNameLabel.Text.Length > 0;

            var logoHeight = expanded ? 88 : 55;
            if (pictureBoxLogo.Height != logoHeight)
                pictureBoxLogo.Height = logoHeight;
        }

        private void ApplyToSidebarButtons(Action<IconButton> action)
        {
            foreach (IconButton btn in SidebarPanelM.Controls.OfType<IconButton>())
                action(btn);
            foreach (IconButton btn in SidebarPanelB.Controls.OfType<IconButton>())
                action(btn);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _mainHeaderBar.SelectedLanguageIndex = LanguageToIndex(_settings.Language);
            labelVersion.Text = AppStrings.Version + " " + Application.ProductVersion;
            // ✅ افتح Dashboard تلقائياً عند التحميل
            NavigateTo<DashboardForm>(btnDashboard);
        }

        private void btnMenu_Click(object sender, EventArgs e)
        {
            CollapseMenu();
        }

        private void ChangeLanguage(int selectedIndex)
        {
            string culture = selectedIndex switch
            {
                0 => SupportedLanguages.Arabic,
                1 =>   SupportedLanguages.English,
                2 => SupportedLanguages.French,
                _ => SupportedLanguages.Arabic
            };

            if (culture == _settings.Language)
                return;

            _settings.Language = culture;
            _preferences.Save(_settings);

            _languageService.SetLanguage(culture);

            FormLocalizationHelper.ApplyDirection(this, _languageService);
            ConfigureSidebarSettingsButton();
            LoadText();
        }

        private static int LanguageToIndex(string? culture) =>
            culture?.ToLowerInvariant() switch
            {
                SupportedLanguages.English => 1,
                SupportedLanguages.French => 2,
                _ => 0
            };

        private void BranchSelector_BranchChanged(object? sender, BranchChangedEventArgs e)
        {
            lblBranch.Text = e.Branch.Name;
        }

        private void UserMenu_MenuActionSelected(object? sender, UserMenuEventArgs e)
        {
            switch (e.Action)
            {
                case UserMenuAction.Profile:
                    NavigateTo<UsersForm>(btnUsersRoles);
                    break;
                case UserMenuAction.ChangePassword:
                    NavigateTo<SettingsForm>(btnSettings);
                    break;
                case UserMenuAction.Logout:
                    Application.Exit();
                    break;
            }
        }

        private void MainHeaderBar_SettingsMenuActionSelected(object? sender, SettingsMenuEventArgs e)
        {
            switch (e.Action)
            {
                case SettingsMenuAction.OpenSettings:
                    NavigateTo<SettingsForm>(btnSettings);
                    break;
                case SettingsMenuAction.Subscription:
                    NavigateTo<SettingsForm>(btnSettings);
                    break;
                case SettingsMenuAction.Logout:
                    Application.Exit();
                    break;
            }
        }

        private void SetSidebarButtonTags()
        {
            ApplyToSidebarButtons(btn => btn.Tag = btn.Text);
        }

        private void ConfigureSidebarSettingsButton()
        {
            settingsButton1.NormalBackColor = AppColors.SidebarSurface;
            settingsButton1.HoverBackColor = AppColors.Primary;
            settingsButton1.ForeColor = Color.White;
            settingsButton1.IconColor = Color.White;
            settingsButton1.PopupPlacement = HeaderPopupPlacement.Top;
            settingsButton1.RightToLeft = RightToLeft;
        }

        private void ConfigureSettingsMenus()
        {
            ConfigureSettingsMenu(_mainHeaderBar.Settings);
            ConfigureSettingsMenu(settingsButton1);
        }

        private void ConfigureSettingsMenu(SettingsButton button)
        {
            var user = _currentUser.User;
            var fullName = user?.FullName;
            if (string.IsNullOrWhiteSpace(fullName))
                fullName = user?.UserName ?? "User";

            var roleName = string.IsNullOrWhiteSpace(user?.RoleName)
                ? AppStrings.Profile
                : user.RoleName;

            var branchName = string.IsNullOrWhiteSpace(user?.BranchName)
                ? AppStrings.Branch
                : user.BranchName;

            button.SettingsText = AppStrings.Settings;
            button.SettingsDescription = AppStrings.SettingsDescription;
            button.SubscriptionText = AppStrings.Version;
            button.SubscriptionDescription = Application.ProductVersion;
            button.LogoutText = AppStrings.Logout;
            button.LogoutDescription = AppStrings.LogoutDetails;

            button.SetSubscriptionInfo(fullName, $"{roleName} - {branchName}");
            button.ClearCustomMenuItems();
            button.AddInfoMenuItem(AppStrings.Branch, branchName);
            button.AddInfoMenuItem(AppStrings.Language, GetLanguageDisplayName(_settings.Language));
            button.AddInfoMenuItem(AppStrings.RememberMe, _settings.RememberMe ? AppStrings.Yes : AppStrings.No);
            button.AddInfoMenuItem(AppStrings.DarkMode, _settings.DarkMode ? AppStrings.Active : AppStrings.Inactive);
        }

        private string GetLanguageDisplayName(string? culture) =>
            culture?.ToLowerInvariant() switch
            {
                SupportedLanguages.English => "English",
                SupportedLanguages.French => "Français",
                _ => "العربية"
            };
    }
}
