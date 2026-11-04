using DzentoPOS2.Localization.Resources;
using DzentoPOS2.WinForms.Controls;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Services;
using DzentoPOS2.WinForms.Themes;
using FontAwesome.Sharp;

namespace DzentoPOS2.WinForms.Forms.Sales
{
    partial class BarcodeScannerOverlay
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            root = new TableLayoutPanel();
            _previewHost = new Panel();
            _preview = new PictureBox();
            statusPanel = new FlowLayoutPanel();
            _lblStatus = new Label();
            _lblHint = new Label();
            footer = new FlowLayoutPanel();
            _cmbSource = new ThemedComboBox();
            _txtCameraUrl = new TextBox();
            _cmbDevice = new ThemedComboBox();
            _btnTestConnection = ButtonFactory.Neutral(AppStrings.TestConnection, ButtonFactory.ButtonSize.Small);
            _btnRefresh = ButtonFactory.Neutral(AppStrings.Refresh, ButtonFactory.ButtonSize.Medium);
            _btnClose = ButtonFactory.Neutral(AppStrings.Close, ButtonFactory.ButtonSize.Medium);
            _btnCloseOverlay = new Button();
            root.SuspendLayout();
            _previewHost.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_preview).BeginInit();
            statusPanel.SuspendLayout();
            footer.SuspendLayout();
            SuspendLayout();
            // 
            // root
            // 
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            root.Controls.Add(_previewHost, 0, 0);
            root.Controls.Add(statusPanel, 0, 1);
            root.Controls.Add(footer, 0, 2);
            root.Dock = DockStyle.Fill;
            root.Location = new Point(0, 0);
            root.Name = "root";
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle());
            root.RowStyles.Add(new RowStyle());
            root.Size = new Size(704, 521);
            root.TabIndex = 1;
            // 
            // _previewHost
            // 
            _previewHost.Controls.Add(_preview);
            _previewHost.Location = new Point(3, 3);
            _previewHost.Name = "_previewHost";
            _previewHost.Size = new Size(194, 1);
            _previewHost.TabIndex = 0;
            // 
            // _preview
            // 
            _preview.Location = new Point(0, 0);
            _preview.Name = "_preview";
            _preview.Size = new Size(100, 50);
            _preview.TabIndex = 0;
            _preview.TabStop = false;
            // 
            // statusPanel
            // 
            statusPanel.Controls.Add(_lblStatus);
            statusPanel.Controls.Add(_lblHint);
            statusPanel.Location = new Point(3, 344);
            statusPanel.Name = "statusPanel";
            statusPanel.Size = new Size(194, 100);
            statusPanel.TabIndex = 1;
            // 
            // _lblStatus
            // 
            _lblStatus.Location = new Point(3, 0);
            _lblStatus.Name = "_lblStatus";
            _lblStatus.Size = new Size(100, 23);
            _lblStatus.TabIndex = 0;
            // 
            // _lblHint
            // 
            _lblHint.Location = new Point(3, 23);
            _lblHint.Name = "_lblHint";
            _lblHint.Size = new Size(100, 23);
            _lblHint.TabIndex = 1;
            // 
            // footer
            // 
            footer.Controls.Add(_cmbSource);
            footer.Controls.Add(_txtCameraUrl);
            footer.Controls.Add(_cmbDevice);
            footer.Controls.Add(_btnClose);
            footer.Controls.Add(_btnRefresh);
            footer.Controls.Add(_btnTestConnection);
            footer.Location = new Point(3, 450);
            footer.Name = "footer";
            footer.Size = new Size(191, 68);
            footer.TabIndex = 2;
            // 
            // _cmbSource
            // 
            _cmbSource.BackColor = Color.White;
            _cmbSource.BorderColor = Color.FromArgb(224, 228, 234);
            _cmbSource.BorderFocusColor = Color.FromArgb(0, 122, 116);
            _cmbSource.DropDownBackColor = Color.White;
            _cmbSource.DropDownBorderColor = Color.FromArgb(224, 228, 234);
            _cmbSource.DropDownBorderRadius = 8;
            _cmbSource.DropDownForeColor = Color.FromArgb(29, 29, 31);
            _cmbSource.DropDownGap = 4;
            _cmbSource.DropDownHoverBackColor = Color.FromArgb(229, 246, 245);
            _cmbSource.DropDownSelectedBackColor = Color.FromArgb(0, 122, 116);
            _cmbSource.DropDownSelectedForeColor = Color.White;
            _cmbSource.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbSource.Font = new Font("Segoe UI", 10F);
            _cmbSource.ForeColor = Color.FromArgb(29, 29, 31);
            _cmbSource.IconColor = Color.FromArgb(110, 110, 115);
            _cmbSource.ItemHeight = 30;
            _cmbSource.Location = new Point(3, 3);
            _cmbSource.MinimumSize = new Size(140, 36);
            _cmbSource.Name = "_cmbSource";
            _cmbSource.SelectedIndex = -1;
            _cmbSource.SelectedItem = null;
            _cmbSource.SelectedValue = null;
            _cmbSource.Size = new Size(250, 38);
            _cmbSource.TabIndex = 4;
            // 
            // _txtCameraUrl
            // 
            _txtCameraUrl.Location = new Point(3, 47);
            _txtCameraUrl.Name = "_txtCameraUrl";
            _txtCameraUrl.Size = new Size(100, 23);
            _txtCameraUrl.TabIndex = 5;
            // 
            // _cmbDevice
            // 
            _cmbDevice.BackColor = Color.White;
            _cmbDevice.BorderColor = Color.FromArgb(224, 228, 234);
            _cmbDevice.BorderFocusColor = Color.FromArgb(0, 122, 116);
            _cmbDevice.DropDownBackColor = Color.White;
            _cmbDevice.DropDownBorderColor = Color.FromArgb(224, 228, 234);
            _cmbDevice.DropDownBorderRadius = 8;
            _cmbDevice.DropDownForeColor = Color.FromArgb(29, 29, 31);
            _cmbDevice.DropDownGap = 4;
            _cmbDevice.DropDownHoverBackColor = Color.FromArgb(229, 246, 245);
            _cmbDevice.DropDownSelectedBackColor = Color.FromArgb(0, 122, 116);
            _cmbDevice.DropDownSelectedForeColor = Color.White;
            _cmbDevice.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbDevice.Font = new Font("Segoe UI", 10F);
            _cmbDevice.ForeColor = Color.FromArgb(29, 29, 31);
            _cmbDevice.IconColor = Color.FromArgb(110, 110, 115);
            _cmbDevice.ItemHeight = 30;
            _cmbDevice.Location = new Point(3, 76);
            _cmbDevice.MinimumSize = new Size(140, 36);
            _cmbDevice.Name = "_cmbDevice";
            _cmbDevice.SelectedIndex = -1;
            _cmbDevice.SelectedItem = null;
            _cmbDevice.SelectedValue = null;
            _cmbDevice.Size = new Size(250, 38);
            _cmbDevice.TabIndex = 7;
            // 
            // _btnCloseOverlay
            // 
            _btnCloseOverlay.Location = new Point(0, 0);
            _btnCloseOverlay.Name = "_btnCloseOverlay";
            _btnCloseOverlay.Size = new Size(75, 23);
            _btnCloseOverlay.TabIndex = 0;
            // 
            // BarcodeScannerOverlay
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(704, 521);
            Controls.Add(_btnCloseOverlay);
            Controls.Add(root);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            KeyPreview = true;
            MaximizeBox = false;
            MinimumSize = new Size(560, 480);
            Name = "BarcodeScannerOverlay";
            StartPosition = FormStartPosition.CenterParent;
            root.ResumeLayout(false);
            _previewHost.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_preview).EndInit();
            statusPanel.ResumeLayout(false);
            footer.ResumeLayout(false);
            footer.PerformLayout();
            ResumeLayout(false);
        }
        private TableLayoutPanel root;
        private FlowLayoutPanel statusPanel;
        private FlowLayoutPanel footer;
    }
}