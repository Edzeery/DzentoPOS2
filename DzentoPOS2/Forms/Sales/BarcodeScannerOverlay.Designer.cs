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
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            AutoScaleMode = AutoScaleMode.Dpi;
            MinimumSize = new Size(560, 480);
            Size = new Size(720, 560);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            KeyPreview = true;
            BackColor = AppColors.Background;

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = AppColors.Background,
                Padding = new Padding(12),
                Margin = Padding.Empty
            };
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _previewHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.SurfaceAlt,
                Margin = new Padding(0, 0, 0, 8)
            };

            _preview = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = AppColors.SurfaceAlt
            };
            _previewHost.Controls.Add(_preview);
            root.Controls.Add(_previewHost, 0, 0);

            _lblStatus = new Label
            {
                Text = AppStrings.ScanHint,
                Font = AppStyle.SubtitleFont,
                ForeColor = AppColors.Ink,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 2),
                TextAlign = ContentAlignment.MiddleLeft
            };
            _lblHint = new Label
            {
                Text = AppStrings.ScanHint,
                Font = AppStyle.BodyFont,
                ForeColor = AppColors.InkMuted,
                AutoSize = true,
                Margin = Padding.Empty,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var statusPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 8)
            };
            statusPanel.Controls.Add(_lblStatus);
            statusPanel.Controls.Add(_lblHint);
            root.Controls.Add(statusPanel, 0, 1);

            _cmbSource = new ThemedComboBox { Width = 160 };
            _cmbDevice = new ThemedComboBox { Width = 200 };
            _txtCameraUrl = new TextBox { Font = AppStyle.BodyFont, Width = 200, Visible = false, BackColor = Color.White };
            _btnTestConnection = ButtonFactory.Neutral(AppStrings.TestConnection, ButtonFactory.ButtonSize.Small);
            _btnTestConnection.Visible = false;
            _btnRefresh = ButtonFactory.Neutral(AppStrings.Refresh, ButtonFactory.ButtonSize.Medium);
            _btnClose = ButtonFactory.Neutral(AppStrings.Close, ButtonFactory.ButtonSize.Medium);

            var footer = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = true,
                AutoSize = true,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 8, 0, 0)
            };
            footer.Controls.Add(_btnClose);
            footer.Controls.Add(_btnRefresh);
            footer.Controls.Add(_btnTestConnection);
            footer.Controls.Add(_cmbDevice);
            footer.Controls.Add(new Label { Text = AppStrings.CameraDevice, AutoSize = true, Margin = new Padding(0, 8, 4, 0) });
            footer.Controls.Add(_txtCameraUrl);
            footer.Controls.Add(_cmbSource);
            footer.Controls.Add(new Label { Text = AppStrings.CameraSource, AutoSize = true, Margin = new Padding(0, 8, 4, 0) });

            root.Controls.Add(footer, 0, 2);

            _btnCloseOverlay = new Button { Visible = false, Size = new Size(0, 0) };
            Controls.Add(_btnCloseOverlay);
            Controls.Add(root);

            ResumeLayout(false);
        }
    }
}