using DzentoPOS2.WinForms.Controls;
using DzentoPOS2.WinForms.Themes;
using System.Windows.Media.Media3D;

namespace DzentoPOS2.WinForms.Forms.Auth
{
    partial class LoginForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginForm));
            txtUserName = new EZControls.Controls.Inputs.EZTextBox();
            txtPassword = new EZControls.Controls.Inputs.EZTextBox();
            lblCopyRight = new Label();
            ckbRememberMe = new CheckBox();
            lblUserName = new Label();
            lblPassword = new Label();
            labelVersion = new Label();
            labelWelcomeDes = new Label();
            labelWelcomeBack = new Label();
            pictureBox1 = new PictureBox();
            pnlLeft = new Panel();
            label5 = new Label();
            pictureBox3 = new PictureBox();
            pictureBox2 = new PictureBox();
            panel1 = new Panel();
            comboBoxLang = new EZControls.Controls.Inputs.EZComboBox();
            ButtonLogin = new EZControls.Controls.Buttons.EZButton();
            iconPictureBoxlanguage = new FontAwesome.Sharp.IconPictureBox();
            btnIconThemMode = new FontAwesome.Sharp.IconButton();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            pnlLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)iconPictureBoxlanguage).BeginInit();
            SuspendLayout();
            // 
            // txtUserName
            // 
            resources.ApplyResources(txtUserName, "txtUserName");
            txtUserName.BackColor = Color.White;
            txtUserName.BorderColor = Color.FromArgb(64, 64, 64);
            txtUserName.BorderErrorColor = Color.Firebrick;
            txtUserName.BorderFocusColor = Color.FromArgb(0, 192, 192);
            txtUserName.BorderRadius = 10;
            txtUserName.BorderSize = 2;
            txtUserName.ForeColor = Color.FromArgb(0, 39, 49);
            txtUserName.Name = "txtUserName";
            txtUserName.PlaceholderColor = Color.DarkGray;
            txtUserName.PlaceholderText = "Enter username";
            txtUserName.TextAlign = HorizontalAlignment.Center;
            // 
            // txtPassword
            // 
            resources.ApplyResources(txtPassword, "txtPassword");
            txtPassword.BackColor = Color.White;
            txtPassword.BorderColor = Color.FromArgb(64, 64, 64);
            txtPassword.BorderErrorColor = Color.Firebrick;
            txtPassword.BorderFocusColor = Color.FromArgb(0, 192, 192);
            txtPassword.BorderRadius = 10;
            txtPassword.BorderSize = 2;
            txtPassword.ForeColor = Color.FromArgb(0, 39, 49);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = true;
            txtPassword.PlaceholderColor = Color.DarkGray;
            txtPassword.PlaceholderText = "Enter password";
            txtPassword.TextAlign = HorizontalAlignment.Center;
            // 
            // lblCopyRight
            // 
            lblCopyRight.BackColor = Color.Transparent;
            resources.ApplyResources(lblCopyRight, "lblCopyRight");
            lblCopyRight.ForeColor = Color.White;
            lblCopyRight.Name = "lblCopyRight";
            // 
            // ckbRememberMe
            // 
            resources.ApplyResources(ckbRememberMe, "ckbRememberMe");
            ckbRememberMe.BackColor = Color.White;
            ckbRememberMe.ForeColor = Color.FromArgb(0, 39, 49);
            ckbRememberMe.Name = "ckbRememberMe";
            ckbRememberMe.UseVisualStyleBackColor = false;
            ckbRememberMe.CheckedChanged += ckbRememberMe_CheckedChanged;
            // 
            // lblUserName
            // 
            resources.ApplyResources(lblUserName, "lblUserName");
            lblUserName.BackColor = Color.White;
            lblUserName.ForeColor = Color.FromArgb(0, 39, 49);
            lblUserName.Name = "lblUserName";
            // 
            // lblPassword
            // 
            resources.ApplyResources(lblPassword, "lblPassword");
            lblPassword.BackColor = Color.White;
            lblPassword.ForeColor = Color.FromArgb(0, 39, 49);
            lblPassword.Name = "lblPassword";
            // 
            // labelVersion
            // 
            resources.ApplyResources(labelVersion, "labelVersion");
            labelVersion.BackColor = Color.WhiteSmoke;
            labelVersion.ForeColor = Color.Gray;
            labelVersion.Name = "labelVersion";
            // 
            // labelWelcomeDes
            // 
            resources.ApplyResources(labelWelcomeDes, "labelWelcomeDes");
            labelWelcomeDes.BackColor = Color.WhiteSmoke;
            labelWelcomeDes.ForeColor = Color.FromArgb(64, 64, 64);
            labelWelcomeDes.Name = "labelWelcomeDes";
            // 
            // labelWelcomeBack
            // 
            resources.ApplyResources(labelWelcomeBack, "labelWelcomeBack");
            labelWelcomeBack.BackColor = Color.WhiteSmoke;
            labelWelcomeBack.ForeColor = Color.FromArgb(0, 39, 49);
            labelWelcomeBack.Name = "labelWelcomeBack";
            // 
            // pictureBox1
            // 
            resources.ApplyResources(pictureBox1, "pictureBox1");
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.Name = "pictureBox1";
            pictureBox1.TabStop = false;
            // 
            // pnlLeft
            // 
            pnlLeft.BackColor = Color.FromArgb(0, 39, 49);
            resources.ApplyResources(pnlLeft, "pnlLeft");
            pnlLeft.Controls.Add(label5);
            pnlLeft.Controls.Add(lblCopyRight);
            pnlLeft.Controls.Add(pictureBox3);
            pnlLeft.Controls.Add(pictureBox2);
            pnlLeft.Name = "pnlLeft";
            // 
            // label5
            // 
            resources.ApplyResources(label5, "label5");
            label5.BackColor = Color.Transparent;
            label5.ForeColor = Color.White;
            label5.Name = "label5";
            // 
            // pictureBox3
            // 
            resources.ApplyResources(pictureBox3, "pictureBox3");
            pictureBox3.BackColor = Color.Transparent;
            pictureBox3.Name = "pictureBox3";
            pictureBox3.TabStop = false;
            // 
            // pictureBox2
            // 
            resources.ApplyResources(pictureBox2, "pictureBox2");
            pictureBox2.BackColor = Color.Transparent;
            pictureBox2.Name = "pictureBox2";
            pictureBox2.TabStop = false;
            // 
            // panel1
            // 
            panel1.Controls.Add(comboBoxLang);
            panel1.Controls.Add(ButtonLogin);
            panel1.Controls.Add(LinkForgotPassword);
            panel1.Controls.Add(iconPictureBoxlanguage);
            panel1.Controls.Add(btnIconThemMode);
            panel1.Controls.Add(txtUserName);
            panel1.Controls.Add(labelVersion);
            panel1.Controls.Add(ckbRememberMe);
            panel1.Controls.Add(labelWelcomeBack);
            panel1.Controls.Add(lblPassword);
            panel1.Controls.Add(labelWelcomeDes);
            panel1.Controls.Add(pictureBox1);
            panel1.Controls.Add(lblUserName);
            panel1.Controls.Add(txtPassword);
            resources.ApplyResources(panel1, "panel1");
            panel1.Name = "panel1";
            panel1.Paint += panel1_Paint;
            // 
            // comboBoxLang
            // 
            comboBoxLang.BackColor = Color.White;
            comboBoxLang.BorderColor = Color.Black;
            comboBoxLang.BorderFocusColor = Color.Gray;
            comboBoxLang.BorderRadius = 10;
            comboBoxLang.BorderSize = 2;
            comboBoxLang.DropDownBackColor = Color.WhiteSmoke;
            comboBoxLang.DropDownBorderColor = Color.Gainsboro;
            comboBoxLang.DropDownForeColor = Color.FromArgb(0, 64, 64);
            comboBoxLang.DropDownHoverBackColor = Color.FromArgb(245, 248, 250);
            comboBoxLang.DropDownSelectedBackColor = Color.FromArgb(229, 246, 245);
            comboBoxLang.DropDownSelectedForeColor = Color.DimGray;
            comboBoxLang.DropDownStyle = ComboBoxStyle.DropDownList;
            resources.ApplyResources(comboBoxLang, "comboBoxLang");
            comboBoxLang.ForeColor = Color.DimGray;
            comboBoxLang.IconColor = Color.White;
            comboBoxLang.ItemHeight = 28;
            comboBoxLang.Items = new string[]
    {
    "العربية",
    "English",
    "Français"
    };
            comboBoxLang.Name = "comboBoxLang";
            comboBoxLang.SelectedIndex = -1;
            comboBoxLang.SelectedItem = null;
            comboBoxLang.SelectedValue = null;
            comboBoxLang.SelectedIndexChanged += comboBoxLang_SelectedIndexChanged;
            // 
            // ButtonLogin
            // 
            ButtonLogin.BackColor = Color.FromArgb(0, 122, 116);
            ButtonLogin.BackgroundColor = Color.FromArgb(0, 122, 116);
            ButtonLogin.BorderColor = Color.FromArgb(0, 64, 64);
            ButtonLogin.BorderRadius = 8;
            ButtonLogin.BorderSize = 2F;
            ButtonLogin.DisabledColor = Color.LightGray;
            resources.ApplyResources(ButtonLogin, "ButtonLogin");
            ButtonLogin.ForeColor = Color.White;
            ButtonLogin.HoverColor = Color.FromArgb(0, 104, 99);
            ButtonLogin.Name = "ButtonLogin";
            ButtonLogin.PressedColor = Color.FromArgb(0, 86, 82);
            ButtonLogin.TextColor = Color.White;
            ButtonLogin.Click += ButtonLogin_Click;
            // 
            // LinkForgotPassword
            // 
            LinkForgotPassword.ActiveLinkColor = Color.FromArgb(0, 122, 116);
            LinkForgotPassword.LinkColor = Color.FromArgb(0, 122, 116);
            LinkForgotPassword.Name = "LinkForgotPassword";
            resources.ApplyResources(LinkForgotPassword, "LinkForgotPassword");
            LinkForgotPassword.LinkBehavior = LinkBehavior.HoverUnderline;
            LinkForgotPassword.LinkColor = Color.FromArgb(0, 122, 116);
            LinkForgotPassword.VisitedLinkColor = Color.FromArgb(0, 104, 99);
            LinkForgotPassword.Click += LinkForgotPassword_Click;
            // 
            // iconPictureBoxlanguage
            // 
            resources.ApplyResources(iconPictureBoxlanguage, "iconPictureBoxlanguage");
            iconPictureBoxlanguage.BackColor = Color.White;
            iconPictureBoxlanguage.ForeColor = Color.FromArgb(0, 64, 64);
            iconPictureBoxlanguage.IconChar = FontAwesome.Sharp.IconChar.Language;
            iconPictureBoxlanguage.IconColor = Color.FromArgb(0, 64, 64);
            iconPictureBoxlanguage.IconFont = FontAwesome.Sharp.IconFont.Auto;
            iconPictureBoxlanguage.IconSize = 38;
            iconPictureBoxlanguage.Name = "iconPictureBoxlanguage";
            iconPictureBoxlanguage.TabStop = false;
            // 
            // btnIconThemMode
            // 
            resources.ApplyResources(btnIconThemMode, "btnIconThemMode");
            btnIconThemMode.FlatAppearance.BorderSize = 0;
            btnIconThemMode.IconChar = FontAwesome.Sharp.IconChar.Moon;
            btnIconThemMode.IconColor = Color.FromArgb(0, 64, 64);
            btnIconThemMode.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnIconThemMode.IconSize = 28;
            btnIconThemMode.Name = "btnIconThemMode";
            btnIconThemMode.UseVisualStyleBackColor = true;
            // 
            // LoginForm
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(panel1);
            Controls.Add(pnlLeft);
            Name = "LoginForm";
            SizeGripStyle = SizeGripStyle.Hide;
            Load += LoginForm_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            pnlLeft.ResumeLayout(false);
            pnlLeft.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)iconPictureBoxlanguage).EndInit();
            ResumeLayout(false);
        }

        #endregion

        public EZControls.Controls.Inputs.EZTextBox txtUserName;
        public EZControls.Controls.Inputs.EZTextBox txtPassword;
        private Label lblCopyRight;
        private CheckBox ckbRememberMe;
        private Label lblUserName;
        private Label lblPassword;
        private PictureBox pictureBox1;
        private Label labelWelcomeBack; 
        private Label labelWelcomeDes;
        private Label labelVersion;
        private Panel pnlLeft;
        private PictureBox pictureBox2;
        private PictureBox pictureBox3;
        private Panel panel1;
        private Label label5;
        private FontAwesome.Sharp.IconButton btnIconThemMode;
        private FontAwesome.Sharp.IconPictureBox iconPictureBoxlanguage;
        private EZControls.Controls.Buttons.EZButton ButtonLogin;
        private System.Windows.Forms.LinkLabel LinkForgotPassword;
        private EZControls.Controls.Inputs.EZComboBox comboBoxLang;
    }
}