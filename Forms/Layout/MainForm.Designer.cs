using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Forms.Layout
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }

                DzentoPOS2.WinForms.Themes.ThemeManager.ThemeChanged -= OnThemeChanged;
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            contentPanel = new Panel();
            SidebarPanel = new Panel();
            SidebarPanelM = new Panel();
            btnSettings = new FontAwesome.Sharp.IconButton();
            btnUsersRoles = new FontAwesome.Sharp.IconButton();
            btnReports = new FontAwesome.Sharp.IconButton();
            btnInventory = new FontAwesome.Sharp.IconButton();
            BtnCustomers = new FontAwesome.Sharp.IconButton();
            BtnSales = new FontAwesome.Sharp.IconButton();
            BtnPurchases = new FontAwesome.Sharp.IconButton();
            BtnProducts = new FontAwesome.Sharp.IconButton();
            btnDashboard = new FontAwesome.Sharp.IconButton();
            pnlLogoANDuserInfo = new Panel();
            pnlUserInfo = new Panel();
            iconPictureBox1 = new FontAwesome.Sharp.IconPictureBox();
            lblFullName = new Label();
            lblBranch = new Label();
            lblRole = new Label();
            pictureBoxLogo = new PictureBox();
            SidebarPanelB = new Panel();
            settingsButton1 = new DzentoPOS2.WinForms.Controls.Header.SettingsButton();
            panel2 = new Panel();
            StatusBarPanel = new Panel();
            TopbarPanel = new Panel();
            labelVersion = new Label();
            pictureBoxLogo2 = new PictureBox();
            btnMinimize = new FontAwesome.Sharp.IconButton();
            btnMaximize = new FontAwesome.Sharp.IconButton();
            btnClose = new FontAwesome.Sharp.IconButton();
            HeaderPanel = new Panel();
            SidebarPanel.SuspendLayout();
            SidebarPanelM.SuspendLayout();
            pnlLogoANDuserInfo.SuspendLayout();
            pnlUserInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)iconPictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxLogo).BeginInit();
            SidebarPanelB.SuspendLayout();
            TopbarPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxLogo2).BeginInit();
            SuspendLayout();
            // 
            // contentPanel
            // 
            contentPanel.BackColor = Color.Transparent;
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Location = new Point(303, 111);
            contentPanel.Name = "contentPanel";
            contentPanel.Size = new Size(1236, 797);
            contentPanel.TabIndex = 0;
            // 
            // SidebarPanel
            // 
            SidebarPanel.BackColor = AppColors.SidebarSurface;
            SidebarPanel.Controls.Add(SidebarPanelM);
            SidebarPanel.Controls.Add(pnlLogoANDuserInfo);
            SidebarPanel.Controls.Add(SidebarPanelB);
            SidebarPanel.Dock = DockStyle.Left;
            SidebarPanel.ForeColor = Color.White;
            SidebarPanel.Location = new Point(0, 39);
            SidebarPanel.Name = "SidebarPanel";
            SidebarPanel.Size = new Size(303, 922);
            SidebarPanel.TabIndex = 1;
            // 
            // SidebarPanelM
            // 
            SidebarPanelM.Controls.Add(btnSettings);
            SidebarPanelM.Controls.Add(btnUsersRoles);
            SidebarPanelM.Controls.Add(btnReports);
            SidebarPanelM.Controls.Add(btnInventory);
            SidebarPanelM.Controls.Add(BtnCustomers);
            SidebarPanelM.Controls.Add(BtnSales);
            SidebarPanelM.Controls.Add(BtnPurchases);
            SidebarPanelM.Controls.Add(BtnProducts);
            SidebarPanelM.Controls.Add(btnDashboard);
            SidebarPanelM.Dock = DockStyle.Fill;
            SidebarPanelM.Location = new Point(0, 182);
            SidebarPanelM.Name = "SidebarPanelM";
            SidebarPanelM.Padding = new Padding(12);
            SidebarPanelM.Size = new Size(303, 585);
            SidebarPanelM.TabIndex = 12;
            // 
            // btnSettings
            // 
            btnSettings.Cursor = Cursors.Hand;
            btnSettings.Dock = DockStyle.Top;
            btnSettings.FlatAppearance.BorderSize = 0;
            btnSettings.FlatStyle = FlatStyle.Flat;
            btnSettings.Font = new Font("Segoe UI", 13F);
            btnSettings.ForeColor = Color.White;
            btnSettings.IconChar = FontAwesome.Sharp.IconChar.Cog;
            btnSettings.IconColor = Color.White;
            btnSettings.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnSettings.IconSize = 35;
            btnSettings.ImageAlign = ContentAlignment.MiddleLeft;
            btnSettings.Location = new Point(12, 452);
            btnSettings.Margin = new Padding(5);
            btnSettings.Name = "btnSettings";
            btnSettings.Padding = new Padding(13, 0, 13, 0);
            btnSettings.Size = new Size(279, 55);
            btnSettings.TabIndex = 11;
            btnSettings.Text = "  Settings";
            btnSettings.TextAlign = ContentAlignment.MiddleLeft;
            btnSettings.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnSettings.UseVisualStyleBackColor = true;
            btnSettings.Visible = false;
            // 
            // btnUsersRoles
            // 
            btnUsersRoles.Cursor = Cursors.Hand;
            btnUsersRoles.Dock = DockStyle.Top;
            btnUsersRoles.FlatAppearance.BorderSize = 0;
            btnUsersRoles.FlatStyle = FlatStyle.Flat;
            btnUsersRoles.Font = new Font("Segoe UI", 13F);
            btnUsersRoles.ForeColor = Color.White;
            btnUsersRoles.IconChar = FontAwesome.Sharp.IconChar.UserGear;
            btnUsersRoles.IconColor = Color.White;
            btnUsersRoles.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnUsersRoles.IconSize = 35;
            btnUsersRoles.ImageAlign = ContentAlignment.MiddleLeft;
            btnUsersRoles.Location = new Point(12, 397);
            btnUsersRoles.Margin = new Padding(5);
            btnUsersRoles.Name = "btnUsersRoles";
            btnUsersRoles.Padding = new Padding(13, 0, 13, 0);
            btnUsersRoles.Size = new Size(279, 55);
            btnUsersRoles.TabIndex = 11;
            btnUsersRoles.Text = "  Users ";
            btnUsersRoles.TextAlign = ContentAlignment.MiddleLeft;
            btnUsersRoles.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnUsersRoles.UseVisualStyleBackColor = true;
            // 
            // btnReports
            // 
            btnReports.Cursor = Cursors.Hand;
            btnReports.Dock = DockStyle.Top;
            btnReports.FlatAppearance.BorderSize = 0;
            btnReports.FlatStyle = FlatStyle.Flat;
            btnReports.Font = new Font("Segoe UI", 13F);
            btnReports.ForeColor = Color.White;
            btnReports.IconChar = FontAwesome.Sharp.IconChar.BarChart;
            btnReports.IconColor = Color.White;
            btnReports.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnReports.IconSize = 35;
            btnReports.ImageAlign = ContentAlignment.MiddleLeft;
            btnReports.Location = new Point(12, 342);
            btnReports.Margin = new Padding(5);
            btnReports.Name = "btnReports";
            btnReports.Padding = new Padding(13, 0, 13, 0);
            btnReports.Size = new Size(279, 55);
            btnReports.TabIndex = 11;
            btnReports.Text = "  Reports";
            btnReports.TextAlign = ContentAlignment.MiddleLeft;
            btnReports.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnReports.UseVisualStyleBackColor = true;
            // 
            // btnInventory
            // 
            btnInventory.Cursor = Cursors.Hand;
            btnInventory.Dock = DockStyle.Top;
            btnInventory.FlatAppearance.BorderSize = 0;
            btnInventory.FlatStyle = FlatStyle.Flat;
            btnInventory.Font = new Font("Segoe UI", 13F);
            btnInventory.ForeColor = Color.White;
            btnInventory.IconChar = FontAwesome.Sharp.IconChar.CartFlatbed;
            btnInventory.IconColor = Color.White;
            btnInventory.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnInventory.IconSize = 35;
            btnInventory.ImageAlign = ContentAlignment.MiddleLeft;
            btnInventory.Location = new Point(12, 287);
            btnInventory.Margin = new Padding(5);
            btnInventory.Name = "btnInventory";
            btnInventory.Padding = new Padding(13, 0, 13, 0);
            btnInventory.Size = new Size(279, 55);
            btnInventory.TabIndex = 11;
            btnInventory.Text = "  Inventory";
            btnInventory.TextAlign = ContentAlignment.MiddleLeft;
            btnInventory.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnInventory.UseVisualStyleBackColor = true;
            // 
            // BtnCustomers
            // 
            BtnCustomers.Cursor = Cursors.Hand;
            BtnCustomers.Dock = DockStyle.Top;
            BtnCustomers.FlatAppearance.BorderSize = 0;
            BtnCustomers.FlatStyle = FlatStyle.Flat;
            BtnCustomers.Font = new Font("Segoe UI", 13F);
            BtnCustomers.ForeColor = Color.White;
            BtnCustomers.IconChar = FontAwesome.Sharp.IconChar.UserFriends;
            BtnCustomers.IconColor = Color.White;
            BtnCustomers.IconFont = FontAwesome.Sharp.IconFont.Auto;
            BtnCustomers.IconSize = 35;
            BtnCustomers.ImageAlign = ContentAlignment.MiddleLeft;
            BtnCustomers.Location = new Point(12, 232);
            BtnCustomers.Margin = new Padding(5);
            BtnCustomers.Name = "BtnCustomers";
            BtnCustomers.Padding = new Padding(13, 0, 13, 0);
            BtnCustomers.Size = new Size(279, 55);
            BtnCustomers.TabIndex = 11;
            BtnCustomers.Text = "  Customers";
            BtnCustomers.TextAlign = ContentAlignment.MiddleLeft;
            BtnCustomers.TextImageRelation = TextImageRelation.ImageBeforeText;
            BtnCustomers.UseVisualStyleBackColor = true;
            // 
            // BtnSales
            // 
            BtnSales.Cursor = Cursors.Hand;
            BtnSales.Dock = DockStyle.Top;
            BtnSales.FlatAppearance.BorderSize = 0;
            BtnSales.FlatStyle = FlatStyle.Flat;
            BtnSales.Font = new Font("Segoe UI", 13F);
            BtnSales.ForeColor = Color.White;
            BtnSales.IconChar = FontAwesome.Sharp.IconChar.Opencart;
            BtnSales.IconColor = Color.White;
            BtnSales.IconFont = FontAwesome.Sharp.IconFont.Auto;
            BtnSales.IconSize = 35;
            BtnSales.ImageAlign = ContentAlignment.MiddleLeft;
            BtnSales.Location = new Point(12, 177);
            BtnSales.Margin = new Padding(5);
            BtnSales.Name = "BtnSales";
            BtnSales.Padding = new Padding(13, 0, 13, 0);
            BtnSales.Size = new Size(279, 55);
            BtnSales.TabIndex = 11;
            BtnSales.Text = "  Sales";
            BtnSales.TextAlign = ContentAlignment.MiddleLeft;
            BtnSales.TextImageRelation = TextImageRelation.ImageBeforeText;
            BtnSales.UseVisualStyleBackColor = true;
            // 
            // BtnPurchases
            // 
            BtnPurchases.Cursor = Cursors.Hand;
            BtnPurchases.Dock = DockStyle.Top;
            BtnPurchases.FlatAppearance.BorderSize = 0;
            BtnPurchases.FlatStyle = FlatStyle.Flat;
            BtnPurchases.Font = new Font("Segoe UI", 13F);
            BtnPurchases.ForeColor = Color.White;
            BtnPurchases.IconChar = FontAwesome.Sharp.IconChar.Tasks;
            BtnPurchases.IconColor = Color.White;
            BtnPurchases.IconFont = FontAwesome.Sharp.IconFont.Auto;
            BtnPurchases.IconSize = 35;
            BtnPurchases.ImageAlign = ContentAlignment.MiddleLeft;
            BtnPurchases.Location = new Point(12, 122);
            BtnPurchases.Margin = new Padding(5);
            BtnPurchases.Name = "BtnPurchases";
            BtnPurchases.Padding = new Padding(13, 0, 13, 0);
            BtnPurchases.Size = new Size(279, 55);
            BtnPurchases.TabIndex = 11;
            BtnPurchases.Text = "  Purchases";
            BtnPurchases.TextAlign = ContentAlignment.MiddleLeft;
            BtnPurchases.TextImageRelation = TextImageRelation.ImageBeforeText;
            BtnPurchases.UseVisualStyleBackColor = true;
            // 
            // BtnProducts
            // 
            BtnProducts.Cursor = Cursors.Hand;
            BtnProducts.Dock = DockStyle.Top;
            BtnProducts.FlatAppearance.BorderSize = 0;
            BtnProducts.FlatStyle = FlatStyle.Flat;
            BtnProducts.Font = new Font("Segoe UI", 13F);
            BtnProducts.ForeColor = Color.White;
            BtnProducts.IconChar = FontAwesome.Sharp.IconChar.BoxesStacked;
            BtnProducts.IconColor = Color.White;
            BtnProducts.IconFont = FontAwesome.Sharp.IconFont.Auto;
            BtnProducts.IconSize = 35;
            BtnProducts.ImageAlign = ContentAlignment.MiddleLeft;
            BtnProducts.Location = new Point(12, 67);
            BtnProducts.Margin = new Padding(5);
            BtnProducts.Name = "BtnProducts";
            BtnProducts.Padding = new Padding(13, 0, 13, 0);
            BtnProducts.Size = new Size(279, 55);
            BtnProducts.TabIndex = 11;
            BtnProducts.Text = "  Products";
            BtnProducts.TextAlign = ContentAlignment.MiddleLeft;
            BtnProducts.TextImageRelation = TextImageRelation.ImageBeforeText;
            BtnProducts.UseVisualStyleBackColor = true;
            // 
            // btnDashboard
            // 
            btnDashboard.BackColor = AppColors.Primary;
            btnDashboard.Cursor = Cursors.Hand;
            btnDashboard.Dock = DockStyle.Top;
            btnDashboard.FlatAppearance.BorderSize = 0;
            btnDashboard.FlatStyle = FlatStyle.Flat;
            btnDashboard.Font = new Font("Segoe UI", 13F);
            btnDashboard.ForeColor = Color.White;
            btnDashboard.IconChar = FontAwesome.Sharp.IconChar.House;
            btnDashboard.IconColor = Color.White;
            btnDashboard.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnDashboard.IconSize = 35;
            btnDashboard.ImageAlign = ContentAlignment.MiddleLeft;
            btnDashboard.Location = new Point(12, 12);
            btnDashboard.Margin = new Padding(5);
            btnDashboard.Name = "btnDashboard";
            btnDashboard.Padding = new Padding(13, 0, 13, 0);
            btnDashboard.Size = new Size(279, 55);
            btnDashboard.TabIndex = 11;
            btnDashboard.Text = "  Dashboard";
            btnDashboard.TextAlign = ContentAlignment.MiddleLeft;
            btnDashboard.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnDashboard.UseVisualStyleBackColor = false;
            // 
            // pnlLogoANDuserInfo
            // 
            pnlLogoANDuserInfo.Controls.Add(pnlUserInfo);
            pnlLogoANDuserInfo.Controls.Add(pictureBoxLogo);
            pnlLogoANDuserInfo.Dock = DockStyle.Top;
            pnlLogoANDuserInfo.Location = new Point(0, 0);
            pnlLogoANDuserInfo.Name = "pnlLogoANDuserInfo";
            pnlLogoANDuserInfo.Size = new Size(303, 182);
            pnlLogoANDuserInfo.TabIndex = 13;
            // 
            // pnlUserInfo
            // 
            pnlUserInfo.Controls.Add(iconPictureBox1);
            pnlUserInfo.Controls.Add(lblFullName);
            pnlUserInfo.Controls.Add(lblBranch);
            pnlUserInfo.Controls.Add(lblRole);
            pnlUserInfo.Dock = DockStyle.Fill;
            pnlUserInfo.Location = new Point(0, 88);
            pnlUserInfo.Name = "pnlUserInfo";
            pnlUserInfo.Size = new Size(303, 94);
            pnlUserInfo.TabIndex = 1;
            // 
            // iconPictureBox1
            // 
            iconPictureBox1.BackColor = AppColors.SidebarSurface;
            iconPictureBox1.IconChar = FontAwesome.Sharp.IconChar.None;
            iconPictureBox1.IconColor = Color.White;
            iconPictureBox1.IconFont = FontAwesome.Sharp.IconFont.Auto;
            iconPictureBox1.IconSize = 82;
            iconPictureBox1.Location = new Point(16, 6);
            iconPictureBox1.Name = "iconPictureBox1";
            iconPictureBox1.Size = new Size(86, 82);
            iconPictureBox1.TabIndex = 0;
            iconPictureBox1.TabStop = false;
            // 
            // lblFullName
            // 
            lblFullName.AutoSize = true;
            lblFullName.Font = new Font("Segoe UI", 12F);
            lblFullName.Location = new Point(146, 13);
            lblFullName.Name = "lblFullName";
            lblFullName.Size = new Size(94, 21);
            lblFullName.TabIndex = 0;
            lblFullName.Text = "lblFullName";
            // 
            // lblBranch
            // 
            lblBranch.AutoSize = true;
            lblBranch.BackColor = Color.Transparent;
            lblBranch.Font = new Font("Segoe UI", 12F);
            lblBranch.Location = new Point(146, 67);
            lblBranch.Name = "lblBranch";
            lblBranch.Size = new Size(75, 21);
            lblBranch.TabIndex = 2;
            lblBranch.Text = "lblBranch";
            // 
            // lblRole
            // 
            lblRole.AutoSize = true;
            lblRole.BackColor = Color.Transparent;
            lblRole.Font = new Font("Segoe UI", 12F);
            lblRole.Location = new Point(146, 40);
            lblRole.Name = "lblRole";
            lblRole.Size = new Size(58, 21);
            lblRole.TabIndex = 1;
            lblRole.Text = "lblRole";
            // 
            // pictureBoxLogo
            // 
            pictureBoxLogo.BackColor = Color.Transparent;
            pictureBoxLogo.Dock = DockStyle.Top;
            pictureBoxLogo.Image = (Image)resources.GetObject("pictureBoxLogo.Image");
            pictureBoxLogo.Location = new Point(0, 0);
            pictureBoxLogo.Name = "pictureBoxLogo";
            pictureBoxLogo.Size = new Size(303, 88);
            pictureBoxLogo.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxLogo.TabIndex = 0;
            pictureBoxLogo.TabStop = false;
            // 
            // SidebarPanelB
            // 
            SidebarPanelB.Controls.Add(settingsButton1);
            SidebarPanelB.Controls.Add(panel2);
            SidebarPanelB.Dock = DockStyle.Bottom;
            SidebarPanelB.Location = new Point(0, 767);
            SidebarPanelB.Name = "SidebarPanelB";
            SidebarPanelB.Padding = new Padding(12);
            SidebarPanelB.Size = new Size(303, 155);
            SidebarPanelB.TabIndex = 0;
            // 
            // settingsButton1
            // 
            settingsButton1.AccessibleName = "Settings";
            settingsButton1.BackColor = AppColors.SidebarSurface;
            settingsButton1.CanUpgrade = true;
            settingsButton1.Dock = DockStyle.Bottom;
            settingsButton1.FlatAppearance.BorderSize = 0;
            settingsButton1.FlatStyle = FlatStyle.Flat;
            settingsButton1.Font = new Font("Segoe UI", 13F);
            settingsButton1.ForeColor = Color.White;
            settingsButton1.HoverBackColor = AppColors.SidebarAccentSoft;
            settingsButton1.IconChar = FontAwesome.Sharp.IconChar.Cog;
            settingsButton1.IconColor = Color.White;
            settingsButton1.IconFont = FontAwesome.Sharp.IconFont.Auto;
            settingsButton1.IconSize = 30;
            settingsButton1.ImageAlign = ContentAlignment.MiddleLeft;
            settingsButton1.Location = new Point(12, 92);
            settingsButton1.LogoutDescription = "End current session";
            settingsButton1.LogoutText = "Logout";
            settingsButton1.Name = "settingsButton1";
            settingsButton1.NormalBackColor = AppColors.SidebarSurface;
            settingsButton1.PopupPlacement = WinForms.Controls.Header.HeaderPopupPlacement.Top;
            settingsButton1.SettingsDescription = "Open application settings";
            settingsButton1.SettingsText = "Settings";
            settingsButton1.Size = new Size(279, 51);
            settingsButton1.SubscriptionDescription = "View plan and billing details";
            settingsButton1.SubscriptionText = "Subscription";
            settingsButton1.TabIndex = 13;
            settingsButton1.Text = "   Settings ";
            settingsButton1.TextAlign = ContentAlignment.MiddleLeft;
            settingsButton1.TextImageRelation = TextImageRelation.ImageBeforeText;
            settingsButton1.UpgradeDescription = "Upgrade current plan";
            settingsButton1.UpgradeText = "Upgrade";
            settingsButton1.UseVisualStyleBackColor = false;
            // 
            // panel2
            // 
            panel2.BackColor = AppColors.Primary;
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(12, 12);
            panel2.Margin = new Padding(4);
            panel2.Name = "panel2";
            panel2.Size = new Size(279, 2);
            panel2.TabIndex = 0;
            // 
            // StatusBarPanel
            // 
            StatusBarPanel.BackColor = AppColors.SidebarSurface;
            StatusBarPanel.Dock = DockStyle.Bottom;
            StatusBarPanel.ForeColor = Color.White;
            StatusBarPanel.Location = new Point(303, 908);
            StatusBarPanel.Name = "StatusBarPanel";
            StatusBarPanel.Size = new Size(1236, 53);
            StatusBarPanel.TabIndex = 4;
            // 
            // TopbarPanel
            // 
            TopbarPanel.BackColor = AppColors.SidebarSurface;
            TopbarPanel.Controls.Add(labelVersion);
            TopbarPanel.Controls.Add(pictureBoxLogo2);
            TopbarPanel.Controls.Add(btnMinimize);
            TopbarPanel.Controls.Add(btnMaximize);
            TopbarPanel.Controls.Add(btnClose);
            TopbarPanel.Dock = DockStyle.Top;
            TopbarPanel.Location = new Point(0, 0);
            TopbarPanel.Name = "TopbarPanel";
            TopbarPanel.Size = new Size(1539, 39);
            TopbarPanel.TabIndex = 2;
            TopbarPanel.MouseDown += TopbarPanel_MouseDown;
            // 
            // labelVersion
            // 
            labelVersion.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            labelVersion.AutoSize = true;
            labelVersion.BackColor = AppColors.SidebarSurface;
            labelVersion.Font = new Font("Segoe UI", 8F);
            labelVersion.ForeColor = Color.Gray;
            labelVersion.Location = new Point(98, 13);
            labelVersion.Name = "labelVersion";
            labelVersion.Size = new Size(72, 13);
            labelVersion.TabIndex = 14;
            labelVersion.Text = "Version 1.0.0";
            // 
            // pictureBoxLogo2
            // 
            pictureBoxLogo2.BackColor = Color.Transparent;
            pictureBoxLogo2.Dock = DockStyle.Left;
            pictureBoxLogo2.Image = (Image)resources.GetObject("pictureBoxLogo2.Image");
            pictureBoxLogo2.Location = new Point(0, 0);
            pictureBoxLogo2.Name = "pictureBoxLogo2";
            pictureBoxLogo2.Size = new Size(92, 39);
            pictureBoxLogo2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxLogo2.TabIndex = 1;
            pictureBoxLogo2.TabStop = false;
            // 
            // btnMinimize
            // 
            btnMinimize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.FlatStyle = FlatStyle.Flat;
            btnMinimize.IconChar = FontAwesome.Sharp.IconChar.Subtract;
            btnMinimize.IconColor = Color.White;
            btnMinimize.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnMinimize.IconSize = 25;
            btnMinimize.Location = new Point(1413, 7);
            btnMinimize.Name = "btnMinimize";
            btnMinimize.Size = new Size(25, 25);
            btnMinimize.TabIndex = 13;
            btnMinimize.UseVisualStyleBackColor = true;
            btnMinimize.Click += btnMinimize_Click;
            // 
            // btnMaximize
            // 
            btnMaximize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMaximize.FlatAppearance.BorderSize = 0;
            btnMaximize.FlatStyle = FlatStyle.Flat;
            btnMaximize.IconChar = FontAwesome.Sharp.IconChar.WindowMaximize;
            btnMaximize.IconColor = Color.White;
            btnMaximize.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnMaximize.IconSize = 25;
            btnMaximize.Location = new Point(1455, 7);
            btnMaximize.Name = "btnMaximize";
            btnMaximize.Size = new Size(25, 25);
            btnMaximize.TabIndex = 12;
            btnMaximize.UseVisualStyleBackColor = true;
            btnMaximize.Click += btnMaximize_Click;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.IconChar = FontAwesome.Sharp.IconChar.Close;
            btnClose.IconColor = Color.White;
            btnClose.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnClose.IconSize = 25;
            btnClose.Location = new Point(1497, 7);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(25, 25);
            btnClose.TabIndex = 11;
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // HeaderPanel
            // 
            HeaderPanel.BackColor = Color.White;
            HeaderPanel.Dock = DockStyle.Top;
            HeaderPanel.Location = new Point(303, 39);
            HeaderPanel.Name = "HeaderPanel";
            HeaderPanel.Size = new Size(1236, 72);
            HeaderPanel.TabIndex = 3;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImageLayout = ImageLayout.Center;
            ClientSize = new Size(1539, 961);
            MinimumSize = new Size(1200, 720);
            Controls.Add(contentPanel);
            Controls.Add(StatusBarPanel);
            Controls.Add(HeaderPanel);
            Controls.Add(SidebarPanel);
            Controls.Add(TopbarPanel);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "MainForm";
            Load += MainForm_Load;
            SidebarPanel.ResumeLayout(false);
            SidebarPanelM.ResumeLayout(false);
            pnlLogoANDuserInfo.ResumeLayout(false);
            pnlUserInfo.ResumeLayout(false);
            pnlUserInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)iconPictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxLogo).EndInit();
            SidebarPanelB.ResumeLayout(false);
            TopbarPanel.ResumeLayout(false);
            TopbarPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxLogo2).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel contentPanel;
        private Panel SidebarPanel;
        private Panel TopbarPanel;
        private Panel HeaderPanel;
        private FontAwesome.Sharp.IconButton btnDashboard;
        private Panel SidebarPanelB;
        private Panel panel2;
        private Panel SidebarPanelM;
        private PictureBox pictureBoxLogo;
        private PictureBox pictureBoxLogo2;
        private FontAwesome.Sharp.IconButton btnMinimize;
        private FontAwesome.Sharp.IconButton btnMaximize;
        private FontAwesome.Sharp.IconButton btnClose;
        private FontAwesome.Sharp.IconButton btnSettings;
        private FontAwesome.Sharp.IconButton btnUsersRoles;
        private FontAwesome.Sharp.IconButton btnReports;
        private FontAwesome.Sharp.IconButton btnInventory;
        private FontAwesome.Sharp.IconButton BtnCustomers;
        private FontAwesome.Sharp.IconButton BtnPurchases;
        private FontAwesome.Sharp.IconButton BtnSales;
        private FontAwesome.Sharp.IconButton BtnProducts;
        private Label labelVersion;
        private Panel StatusBarPanel;
        private Panel pnlLogoANDuserInfo;
        private Panel pnlUserInfo;
        private Label lblBranch;
        private Label lblRole;
        private Label lblFullName;
        private FontAwesome.Sharp.IconPictureBox iconPictureBox1;
        private Controls.Header.SettingsButton settingsButton1;
    }
}