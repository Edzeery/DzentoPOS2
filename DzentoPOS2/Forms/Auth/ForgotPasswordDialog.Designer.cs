namespace DzentoPOS2.WinForms.Forms.Auth;

partial class ForgotPasswordDialog
{
    private System.ComponentModel.IContainer components = null;

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        this._lblTitle = new System.Windows.Forms.Label();
        this._lblUserName = new System.Windows.Forms.Label();
        this._txtUserName = new System.Windows.Forms.TextBox();
        this._lblRecoveryKey = new System.Windows.Forms.Label();
        this._txtRecoveryKey = new System.Windows.Forms.TextBox();
        this._lblNewPassword = new System.Windows.Forms.Label();
        this._txtNewPassword = new System.Windows.Forms.TextBox();
        this._lblConfirm = new System.Windows.Forms.Label();
        this._txtConfirm = new System.Windows.Forms.TextBox();
        this._btnReset = new System.Windows.Forms.Button();
        this._btnCancel = new System.Windows.Forms.Button();
        this._lblHint = new System.Windows.Forms.Label();
        this.SuspendLayout();
        // 
        // _lblTitle
        // 
        this._lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
        this._lblTitle.Location = new System.Drawing.Point(24, 16);
        this._lblTitle.Size = new System.Drawing.Size(496, 32);
        this._lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // _lblHint
        // 
        this._lblHint.Font = new System.Drawing.Font("Segoe UI", 9.5F);
        this._lblHint.ForeColor = DzentoPOS2.WinForms.Themes.AppColors.InkMuted;
        this._lblHint.Location = new System.Drawing.Point(24, 52);
        this._lblHint.Size = new System.Drawing.Size(496, 40);
        // 
        // _lblUserName
        // 
        this._lblUserName.Location = new System.Drawing.Point(24, 100);
        this._lblUserName.Size = new System.Drawing.Size(150, 28);
        this._lblUserName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // _txtUserName
        // 
        this._txtUserName.Location = new System.Drawing.Point(190, 100);
        this._txtUserName.Size = new System.Drawing.Size(330, 28);
        // 
        // _lblRecoveryKey
        // 
        this._lblRecoveryKey.Location = new System.Drawing.Point(24, 140);
        this._lblRecoveryKey.Size = new System.Drawing.Size(150, 28);
        this._lblRecoveryKey.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // _txtRecoveryKey
        // 
        this._txtRecoveryKey.Location = new System.Drawing.Point(190, 140);
        this._txtRecoveryKey.Size = new System.Drawing.Size(330, 28);
        // 
        // _lblNewPassword
        // 
        this._lblNewPassword.Location = new System.Drawing.Point(24, 180);
        this._lblNewPassword.Size = new System.Drawing.Size(150, 28);
        this._lblNewPassword.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // _txtNewPassword
        // 
        this._txtNewPassword.Location = new System.Drawing.Point(190, 180);
        this._txtNewPassword.Size = new System.Drawing.Size(330, 28);
        this._txtNewPassword.UseSystemPasswordChar = true;
        // 
        // _lblConfirm
        // 
        this._lblConfirm.Location = new System.Drawing.Point(24, 220);
        this._lblConfirm.Size = new System.Drawing.Size(150, 28);
        this._lblConfirm.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // _txtConfirm
        // 
        this._txtConfirm.Location = new System.Drawing.Point(190, 220);
        this._txtConfirm.Size = new System.Drawing.Size(330, 28);
        this._txtConfirm.UseSystemPasswordChar = true;
        // 
        // _btnReset
        // 
        this._btnReset.Location = new System.Drawing.Point(190, 270);
        this._btnReset.Size = new System.Drawing.Size(160, 40);
        // 
        // _btnCancel
        // 
        this._btnCancel.Location = new System.Drawing.Point(360, 270);
        this._btnCancel.Size = new System.Drawing.Size(160, 40);
        this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        // 
        // ForgotPasswordDialog
        // 
        this.AcceptButton = this._btnReset;
        this.CancelButton = this._btnCancel;
        this.BackColor = DzentoPOS2.WinForms.Themes.AppColors.Surface;
        this.ClientSize = new System.Drawing.Size(560, 340);
        this.Controls.Add(this._lblTitle);
        this.Controls.Add(this._lblHint);
        this.Controls.Add(this._lblUserName);
        this.Controls.Add(this._txtUserName);
        this.Controls.Add(this._lblRecoveryKey);
        this.Controls.Add(this._txtRecoveryKey);
        this.Controls.Add(this._lblNewPassword);
        this.Controls.Add(this._txtNewPassword);
        this.Controls.Add(this._lblConfirm);
        this.Controls.Add(this._txtConfirm);
        this.Controls.Add(this._btnReset);
        this.Controls.Add(this._btnCancel);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion
}