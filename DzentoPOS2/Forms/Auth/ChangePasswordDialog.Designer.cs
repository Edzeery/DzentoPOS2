namespace DzentoPOS2.WinForms.Forms.Auth;

partial class ChangePasswordDialog
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this._lblHint = new System.Windows.Forms.Label();
        this._lblCurrent = new System.Windows.Forms.Label();
        this._txtCurrent = new System.Windows.Forms.TextBox();
        this._lblNew = new System.Windows.Forms.Label();
        this._txtNew = new System.Windows.Forms.TextBox();
        this._lblConfirm = new System.Windows.Forms.Label();
        this._txtConfirm = new System.Windows.Forms.TextBox();
        this._btnSave = new System.Windows.Forms.Button();
        this._btnCancel = new System.Windows.Forms.Button();
        this.SuspendLayout();
        // 
        // _lblHint
        // 
        this._lblHint.Font = new System.Drawing.Font("Segoe UI", 9.5F);
        this._lblHint.ForeColor = DzentoPOS2.WinForms.Themes.AppColors.InkMuted;
        this._lblHint.Location = new System.Drawing.Point(24, 16);
        this._lblHint.Size = new System.Drawing.Size(496, 40);
        // 
        // _lblCurrent
        // 
        this._lblCurrent.Font = new System.Drawing.Font("Segoe UI", 10F);
        this._lblCurrent.Location = new System.Drawing.Point(24, 70);
        this._lblCurrent.Size = new System.Drawing.Size(150, 28);
        this._lblCurrent.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // _txtCurrent
        // 
        this._txtCurrent.Font = new System.Drawing.Font("Segoe UI", 10F);
        this._txtCurrent.Location = new System.Drawing.Point(190, 70);
        this._txtCurrent.Size = new System.Drawing.Size(330, 28);
        this._txtCurrent.UseSystemPasswordChar = true;
        // 
        // _lblNew
        // 
        this._lblNew.Font = new System.Drawing.Font("Segoe UI", 10F);
        this._lblNew.Location = new System.Drawing.Point(24, 114);
        this._lblNew.Size = new System.Drawing.Size(150, 28);
        this._lblNew.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // _txtNew
        // 
        this._txtNew.Font = new System.Drawing.Font("Segoe UI", 10F);
        this._txtNew.Location = new System.Drawing.Point(190, 114);
        this._txtNew.Size = new System.Drawing.Size(330, 28);
        this._txtNew.UseSystemPasswordChar = true;
        // 
        // _lblConfirm
        // 
        this._lblConfirm.Font = new System.Drawing.Font("Segoe UI", 10F);
        this._lblConfirm.Location = new System.Drawing.Point(24, 158);
        this._lblConfirm.Size = new System.Drawing.Size(150, 28);
        this._lblConfirm.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // _txtConfirm
        // 
        this._txtConfirm.Font = new System.Drawing.Font("Segoe UI", 10F);
        this._txtConfirm.Location = new System.Drawing.Point(190, 158);
        this._txtConfirm.Size = new System.Drawing.Size(330, 28);
        this._txtConfirm.UseSystemPasswordChar = true;
        // 
        // _btnSave
        // 
        this._btnSave.Location = new System.Drawing.Point(190, 214);
        this._btnSave.Size = new System.Drawing.Size(160, 40);
        // 
        // _btnCancel
        // 
        this._btnCancel.Location = new System.Drawing.Point(360, 214);
        this._btnCancel.Size = new System.Drawing.Size(160, 40);
        this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        // 
        // ChangePasswordDialog
        // 
        this.AcceptButton = this._btnSave;
        this.CancelButton = this._btnCancel;
        this.BackColor = DzentoPOS2.WinForms.Themes.AppColors.Surface;
        this.ClientSize = new System.Drawing.Size(560, 400);
        this.Controls.Add(this._lblHint);
        this.Controls.Add(this._lblCurrent);
        this.Controls.Add(this._txtCurrent);
        this.Controls.Add(this._lblNew);
        this.Controls.Add(this._txtNew);
        this.Controls.Add(this._lblConfirm);
        this.Controls.Add(this._txtConfirm);
        this.Controls.Add(this._btnSave);
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