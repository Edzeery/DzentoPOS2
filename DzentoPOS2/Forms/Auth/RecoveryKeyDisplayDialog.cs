using DzentoPOS2.Localization.Resources;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Forms.Auth;

public sealed partial class RecoveryKeyDisplayDialog : ThemedDialog
{
    private readonly string _key;

    private Label _lblTitle = null!;
    private Label _lblKey = null!;
    private Label _lblHint = null!;
    private CheckBox _chkStored = null!;
    private Button _btnCopy = null!;
    private Button _btnClose = null!;

    public RecoveryKeyDisplayDialog(string key)
    {
        _key = key;
        InitializeComponent();
        LoadText();
    }

    private void InitializeComponent()
    {
        this._lblTitle = new System.Windows.Forms.Label();
        this._lblKey = new System.Windows.Forms.Label();
        this._lblHint = new System.Windows.Forms.Label();
        this._chkStored = new System.Windows.Forms.CheckBox();
        this._btnCopy = new System.Windows.Forms.Button();
        this._btnClose = new System.Windows.Forms.Button();
        this.SuspendLayout();
        // 
        // _lblTitle
        // 
        this._lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
        this._lblTitle.Location = new System.Drawing.Point(24, 16);
        this._lblTitle.Size = new System.Drawing.Size(496, 32);
        // 
        // _lblKey
        // 
        this._lblKey.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Bold);
        this._lblKey.Location = new System.Drawing.Point(24, 60);
        this._lblKey.Size = new System.Drawing.Size(496, 40);
        this._lblKey.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        this._lblKey.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        // 
        // _lblHint
        // 
        this._lblHint.Font = new System.Drawing.Font("Segoe UI", 9.5F);
        this._lblHint.ForeColor = DzentoPOS2.WinForms.Themes.AppColors.InkMuted;
        this._lblHint.Location = new System.Drawing.Point(24, 110);
        this._lblHint.Size = new System.Drawing.Size(496, 60);
        // 
        // _chkStored
        // 
        this._chkStored.Location = new System.Drawing.Point(24, 180);
        this._chkStored.Size = new System.Drawing.Size(496, 24);
        // 
        // _btnCopy
        // 
        this._btnCopy.Location = new System.Drawing.Point(190, 220);
        this._btnCopy.Size = new System.Drawing.Size(160, 40);
        this._btnCopy.Click += (_, _) =>
        {
            try { Clipboard.SetText(_key); } catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Copy failed: {ex.Message}"); }
        };
        // 
        // _btnClose
        // 
        this._btnClose.Location = new System.Drawing.Point(360, 220);
        this._btnClose.Size = new System.Drawing.Size(160, 40);
        this._btnClose.Enabled = false;
        this._btnClose.Click += (_, _) => { DialogResult = DialogResult.OK; Close(); };
        // 
        // RecoveryKeyDisplayDialog
        // 
        this.BackColor = DzentoPOS2.WinForms.Themes.AppColors.Surface;
        this.ClientSize = new System.Drawing.Size(560, 280);
        this.Controls.Add(this._lblTitle);
        this.Controls.Add(this._lblKey);
        this.Controls.Add(this._lblHint);
        this.Controls.Add(this._chkStored);
        this.Controls.Add(this._btnCopy);
        this.Controls.Add(this._btnClose);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.ResumeLayout(false);
        this.PerformLayout();

        _chkStored.CheckedChanged += (_, _) => _btnClose.Enabled = _chkStored.Checked;
    }

    private void LoadText()
    {
        Text = AppStrings.RecoveryKey;
        _lblTitle.Text = AppStrings.RecoveryKey;
        _lblKey.Text = _key;
        _lblHint.Text = AppStrings.RecoveryKeyStoreWarning;
        _chkStored.Text = AppStrings.IStoredIt;
        _btnCopy.Text = AppStrings.Copy;
        _btnClose.Text = AppStrings.Close;
    }
}