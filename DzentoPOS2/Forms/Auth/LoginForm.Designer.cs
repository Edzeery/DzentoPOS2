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
            txtUserName.Anchor = AnchorStyles.None;
            txtUserName.BackColor = Color.White;
            txtUserName.BorderColor = Color.FromArgb(64, 64, 64);
            txtUserName.BorderErrorColor = Color.Firebrick;
            txtUserName.BorderFocusColor = Color.FromArgb(0, 192, 192);
            txtUserName.BorderRadius = 10;
            txtUserName.BorderSize = 2;
            txtUserName.Font = new Font("Segoe UI", 13F);
            txtUserName.ForeColor = Color.FromArgb(0, 39, 49);
            txtUserName.ImeMode = ImeMode.NoControl;
            txtUserName.Location = new Point(397, 444);
            txtUserName.Margin = new Padding(4);
            txtUserName.Name = "txtUserName";
            txtUserName.Padding = new Padding(10, 6, 10, 6);
            txtUserName.PlaceholderColor = Color.DarkGray;
            txtUserName.PlaceholderText = "Enter username";
            txtUserName.Size = new Size(518, 49);
            txtUserName.TabIndex = 0;
            txtUserName.TextAlign = HorizontalAlignment.Center;
            // 
            // txtPassword
            // 
            txtPassword.Anchor = AnchorStyles.None;
            txtPassword.BackColor = Color.White;
            txtPassword.BorderColor = Color.FromArgb(64, 64, 64);
            txtPassword.BorderErrorColor = Color.Firebrick;
            txtPassword.BorderFocusColor = Color.FromArgb(0, 192, 192);
            txtPassword.BorderRadius = 10;
            txtPassword.BorderSize = 2;
            txtPassword.Font = new Font("Segoe UI", 13F);
            txtPassword.ForeColor = Color.FromArgb(0, 39, 49);
            txtPassword.Location = new Point(397, 546);
            txtPassword.Margin = new Padding(4);
            txtPassword.Name = "txtPassword";
            txtPassword.Padding = new Padding(10, 6, 10, 6);
            txtPassword.PasswordChar = true;
            txtPassword.PlaceholderColor = Color.DarkGray;
            txtPassword.PlaceholderText = "Enter password";
            txtPassword.Size = new Size(518, 49);
            txtPassword.TabIndex = 1;
            txtPassword.TextAlign = HorizontalAlignment.Center;
            // 
            // lblCopyRight
            // 
            lblCopyRight.BackColor = Color.Transparent;
            lblCopyRight.Dock = DockStyle.Bottom;
            lblCopyRight.ForeColor = Color.White;
            lblCopyRight.Location = new Point(0, 737);
            lblCopyRight.Name = "lblCopyRight";
            lblCopyRight.Size = new Size(600, 63);
            lblCopyRight.TabIndex = 3;
            lblCopyRight.Text = "مع تحيات شركة إدزيري كود © جميع الحقوق محفوظة 2026";
            lblCopyRight.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // ckbRememberMe
            // 
            ckbRememberMe.Anchor = AnchorStyles.None;
            ckbRememberMe.AutoSize = true;
            ckbRememberMe.BackColor = Color.White;
            ckbRememberMe.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            ckbRememberMe.ForeColor = Color.FromArgb(0, 39, 49);
            ckbRememberMe.Location = new Point(397, 611);
            ckbRememberMe.Name = "ckbRememberMe";
            ckbRememberMe.Size = new Size(140, 25);
            ckbRememberMe.TabIndex = 4;
            ckbRememberMe.Text = "Remember me";
            ckbRememberMe.UseVisualStyleBackColor = false;
            ckbRememberMe.CheckedChanged += ckbRememberMe_CheckedChanged;
            // 
            // lblUserName
            // 
            lblUserName.Anchor = AnchorStyles.Left;
            lblUserName.AutoSize = true;
            lblUserName.BackColor = Color.White;
            lblUserName.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblUserName.ForeColor = Color.FromArgb(0, 39, 49);
            lblUserName.Location = new Point(97, 407);
            lblUserName.Name = "lblUserName";
            lblUserName.Size = new Size(91, 21);
            lblUserName.TabIndex = 5;
            lblUserName.Text = "User name";
            // 
            // lblPassword
            // 
            lblPassword.Anchor = AnchorStyles.Left;
            lblPassword.AutoSize = true;
            lblPassword.BackColor = Color.White;
            lblPassword.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblPassword.ForeColor = Color.FromArgb(0, 39, 49);
            lblPassword.Location = new Point(97, 509);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(82, 21);
            lblPassword.TabIndex = 5;
            lblPassword.Text = "Password";
            // 
            // labelVersion
            // 
            labelVersion.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            labelVersion.AutoSize = true;
            labelVersion.BackColor = Color.WhiteSmoke;
            labelVersion.Font = new Font("Segoe UI", 13F);
            labelVersion.ForeColor = Color.Gray;
            labelVersion.Location = new Point(300, 847);
            labelVersion.Name = "labelVersion";
            labelVersion.Size = new Size(113, 25);
            labelVersion.TabIndex = 8;
            labelVersion.Text = "Version 1.0.0";
            // 
            // labelWelcomeDes
            // 
            labelWelcomeDes.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            labelWelcomeDes.AutoSize = true;
            labelWelcomeDes.BackColor = Color.WhiteSmoke;
            labelWelcomeDes.Font = new Font("Segoe UI", 13F);
            labelWelcomeDes.ForeColor = Color.FromArgb(64, 64, 64);
            labelWelcomeDes.Location = new Point(232, 349);
            labelWelcomeDes.Name = "labelWelcomeDes";
            labelWelcomeDes.Size = new Size(248, 25);
            labelWelcomeDes.TabIndex = 5;
            labelWelcomeDes.Text = "Please sign in to your account";
            // 
            // labelWelcomeBack
            // 
            labelWelcomeBack.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            labelWelcomeBack.AutoSize = true;
            labelWelcomeBack.BackColor = Color.WhiteSmoke;
            labelWelcomeBack.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            labelWelcomeBack.ForeColor = Color.FromArgb(0, 39, 49);
            labelWelcomeBack.Location = new Point(255, 312);
            labelWelcomeBack.Name = "labelWelcomeBack";
            labelWelcomeBack.Size = new Size(203, 37);
            labelWelcomeBack.TabIndex = 5;
            labelWelcomeBack.Text = "Welcome Back";
            // 
            // pictureBox1
            // 
            pictureBox1.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(303, 200);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(706, 93);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 6;
            pictureBox1.TabStop = false;
            // 
            // pnlLeft
            // 
            pnlLeft.BackColor = Color.FromArgb(0, 39, 49);
            pnlLeft.BackgroundImage = (Image)resources.GetObject("pnlLeft.BackgroundImage");
            pnlLeft.BackgroundImageLayout = ImageLayout.Stretch;
            pnlLeft.Controls.Add(label5);
            pnlLeft.Controls.Add(lblCopyRight);
            pnlLeft.Controls.Add(pictureBox3);
            pnlLeft.Controls.Add(pictureBox2);
            pnlLeft.Dock = DockStyle.Left;
            pnlLeft.Location = new Point(0, 0);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Size = new Size(600, 800);
            pnlLeft.TabIndex = 8;
            // 
            // label5
            // 
            label5.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            label5.AutoSize = true;
            label5.BackColor = Color.Transparent;
            label5.Font = new Font("Segoe UI", 13F);
            label5.ForeColor = Color.White;
            label5.Location = new Point(259, 219);
            label5.Name = "label5";
            label5.Size = new Size(248, 25);
            label5.TabIndex = 7;
            label5.Text = "Please sign in to your account";
            // 
            // pictureBox3
            // 
            pictureBox3.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            pictureBox3.BackColor = Color.Transparent;
            pictureBox3.Image = (Image)resources.GetObject("pictureBox3.Image");
            pictureBox3.Location = new Point(207, 142);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(358, 84);
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox3.TabIndex = 6;
            pictureBox3.TabStop = false;
            // 
            // pictureBox2
            // 
            pictureBox2.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            pictureBox2.BackColor = Color.Transparent;
            pictureBox2.Image = (Image)resources.GetObject("pictureBox2.Image");
            pictureBox2.Location = new Point(246, 14);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(242, 147);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.TabIndex = 6;
            pictureBox2.TabStop = false;
            // 
            // panel1
            // 
            panel1.Controls.Add(comboBoxLang);
            panel1.Controls.Add(ButtonLogin);
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
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(1200, 800);
            panel1.TabIndex = 9;
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
            comboBoxLang.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 178);
            comboBoxLang.ForeColor = Color.DimGray;
            comboBoxLang.IconColor = Color.White;
            comboBoxLang.ItemHeight = 28;
            comboBoxLang.Items = new string[]
    {
    "العربية",
    "English",
    "Français"
    };
            comboBoxLang.Location = new Point(544, 26);
            comboBoxLang.MinimumSize = new Size(100, 28);
            comboBoxLang.Name = "comboBoxLang";
            comboBoxLang.SelectedIndex = -1;
            comboBoxLang.SelectedItem = null;
            comboBoxLang.SelectedValue = null;
            comboBoxLang.Size = new Size(136, 38);
            comboBoxLang.TabIndex = 15;
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
            ButtonLogin.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            ButtonLogin.ForeColor = Color.White;
            ButtonLogin.HoverColor = Color.FromArgb(0, 104, 99);
            ButtonLogin.Location = new Point(97, 651);
            ButtonLogin.Name = "ButtonLogin";
            ButtonLogin.PressedColor = Color.FromArgb(0, 86, 82);
            ButtonLogin.Size = new Size(518, 55);
            ButtonLogin.TabIndex = 14;
            ButtonLogin.Text = "Login";
            ButtonLogin.TextColor = Color.White;
            ButtonLogin.Click += ButtonLogin_Click;
            // 
            // iconPictureBoxlanguage
            // 
            iconPictureBoxlanguage.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            iconPictureBoxlanguage.BackColor = Color.White;
            iconPictureBoxlanguage.ForeColor = Color.FromArgb(0, 64, 64);
            iconPictureBoxlanguage.IconChar = FontAwesome.Sharp.IconChar.Language;
            iconPictureBoxlanguage.IconColor = Color.FromArgb(0, 64, 64);
            iconPictureBoxlanguage.IconFont = FontAwesome.Sharp.IconFont.Auto;
            iconPictureBoxlanguage.IconSize = 38;
            iconPictureBoxlanguage.Location = new Point(1106, 26);
            iconPictureBoxlanguage.Name = "iconPictureBoxlanguage";
            iconPictureBoxlanguage.Size = new Size(38, 38);
            iconPictureBoxlanguage.TabIndex = 11;
            iconPictureBoxlanguage.TabStop = false;
            // 
            // btnIconThemMode
            // 
            btnIconThemMode.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnIconThemMode.FlatAppearance.BorderSize = 0;
            btnIconThemMode.FlatStyle = FlatStyle.Flat;
            btnIconThemMode.IconChar = FontAwesome.Sharp.IconChar.Moon;
            btnIconThemMode.IconColor = Color.FromArgb(0, 64, 64);
            btnIconThemMode.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnIconThemMode.IconSize = 28;
            btnIconThemMode.Location = new Point(1280, 26);
            btnIconThemMode.Name = "btnIconThemMode";
            btnIconThemMode.Size = new Size(38, 38);
            btnIconThemMode.TabIndex = 10;
            btnIconThemMode.UseVisualStyleBackColor = true;
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(9F, 21F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1200, 800);
            Controls.Add(pnlLeft);
            Controls.Add(panel1);
            Font = new Font("Segoe UI", 12F);
            Margin = new Padding(4);
            Name = "LoginForm";
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterScreen;
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
        private EZControls.Controls.Inputs.EZComboBox comboBoxLang;
    }
}