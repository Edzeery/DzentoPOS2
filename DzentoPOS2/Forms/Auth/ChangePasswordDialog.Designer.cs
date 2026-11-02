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
        _lblHint = new Label();
        _lblCurrent = new Label();
        _txtCurrent = new TextBox();
        _lblNew = new Label();
        _txtNew = new TextBox();
        _lblConfirm = new Label();
        _txtConfirm = new TextBox();
        _btnSave = new Button();
        _btnCancel = new Button();
        SuspendLayout();
        // 
        // _lblHint
        // 
        _lblHint.Font = new Font("Segoe UI", 9.5F);
        _lblHint.ForeColor = Color.FromArgb(0, 64, 64);
        _lblHint.Location = new Point(24, 16);
        _lblHint.Name = "_lblHint";
        _lblHint.Size = new Size(496, 40);
        _lblHint.TabIndex = 0;
        // 
        // _lblCurrent
        // 
        _lblCurrent.BackColor = Color.Transparent;
        _lblCurrent.Font = new Font("Segoe UI", 10F);
        _lblCurrent.ForeColor = Color.Teal;
        _lblCurrent.Location = new Point(24, 70);
        _lblCurrent.Name = "_lblCurrent";
        _lblCurrent.Size = new Size(150, 28);
        _lblCurrent.TabIndex = 1;
        _lblCurrent.Text = "111";
        _lblCurrent.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _txtCurrent
        // 
        _txtCurrent.Font = new Font("Segoe UI", 10F);
        _txtCurrent.ForeColor = Color.FromArgb(0, 64, 64);
        _txtCurrent.Location = new Point(190, 70);
        _txtCurrent.Name = "_txtCurrent";
        _txtCurrent.Size = new Size(330, 25);
        _txtCurrent.TabIndex = 2;
        _txtCurrent.UseSystemPasswordChar = true;
        // 
        // _lblNew
        // 
        _lblNew.BackColor = Color.Transparent;
        _lblNew.Font = new Font("Segoe UI", 10F);
        _lblNew.ForeColor = Color.Teal;
        _lblNew.Location = new Point(24, 114);
        _lblNew.Name = "_lblNew";
        _lblNew.Size = new Size(150, 28);
        _lblNew.TabIndex = 3;
        _lblNew.Text = "111";
        _lblNew.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _txtNew
        // 
        _txtNew.Font = new Font("Segoe UI", 10F);
        _txtNew.ForeColor = Color.FromArgb(0, 64, 64);
        _txtNew.Location = new Point(190, 114);
        _txtNew.Name = "_txtNew";
        _txtNew.Size = new Size(330, 25);
        _txtNew.TabIndex = 4;
        _txtNew.UseSystemPasswordChar = true;
        // 
        // _lblConfirm
        // 
        _lblConfirm.BackColor = Color.Transparent;
        _lblConfirm.Font = new Font("Segoe UI", 10F);
        _lblConfirm.ForeColor = Color.Teal;
        _lblConfirm.Location = new Point(24, 158);
        _lblConfirm.Name = "_lblConfirm";
        _lblConfirm.Size = new Size(150, 28);
        _lblConfirm.TabIndex = 5;
        _lblConfirm.Text = "111";
        _lblConfirm.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _txtConfirm
        // 
        _txtConfirm.Font = new Font("Segoe UI", 10F);
        _txtConfirm.ForeColor = Color.FromArgb(0, 64, 64);
        _txtConfirm.Location = new Point(190, 158);
        _txtConfirm.Name = "_txtConfirm";
        _txtConfirm.Size = new Size(330, 25);
        _txtConfirm.TabIndex = 6;
        _txtConfirm.UseSystemPasswordChar = true;
        // 
        // _btnSave
        // 
        _btnSave.ForeColor = Color.FromArgb(0, 64, 64);
        _btnSave.Location = new Point(190, 214);
        _btnSave.Name = "_btnSave";
        _btnSave.Size = new Size(160, 40);
        _btnSave.TabIndex = 7;
        // 
        // _btnCancel
        // 
        _btnCancel.DialogResult = DialogResult.Cancel;
        _btnCancel.ForeColor = Color.FromArgb(0, 64, 64);
        _btnCancel.Location = new Point(360, 214);
        _btnCancel.Name = "_btnCancel";
        _btnCancel.Size = new Size(160, 40);
        _btnCancel.TabIndex = 8;
        // 
        // ChangePasswordDialog
        // 
        AcceptButton = _btnSave;
        BackColor = Color.White;
        CancelButton = _btnCancel;
        ClientSize = new Size(560, 400);
        Controls.Add(_lblHint);
        Controls.Add(_lblCurrent);
        Controls.Add(_txtCurrent);
        Controls.Add(_lblNew);
        Controls.Add(_txtNew);
        Controls.Add(_lblConfirm);
        Controls.Add(_txtConfirm);
        Controls.Add(_btnSave);
        Controls.Add(_btnCancel);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "ChangePasswordDialog";
        StartPosition = FormStartPosition.CenterScreen;
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion
}