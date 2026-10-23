using DzentoPOS2.WinForms.ServicesUI;
using DzentoPOS2.WinForms.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzentoPOS2.WinForms.Controls
{
    public class LoadingOverlay : Panel
    {
        // =========================
        // CONTROLS
        // =========================

        private Panel contentPanel = null!;

        private ProgressBar progress = null!;

        private Label lblMessage = null!;

        // =========================
        // PROPERTIES
        // =========================

        public string Message
        {
            get { return lblMessage.Text; }
            set { lblMessage.Text = value; }
        }

        // =========================
        // CONSTRUCTOR
        // =========================

        public LoadingOverlay()
        {
            Initialize();

            ThemeManager.ThemeChanged += OnThemeChanged;

            ApplyTheme();
        }

        private void OnThemeChanged(object? sender, EventArgs e)
        {
            if (IsDisposed) return;
            if (InvokeRequired) { BeginInvoke(ApplyTheme); return; }
            ApplyTheme();
        }

        // =========================
        // INITIALIZE
        // =========================

        private void Initialize()
        {
            Dock = DockStyle.Fill;

            Visible = false;

            BackColor = ThemeManager.IsDark
                     ? Color.FromArgb(10, 10, 10, 10)
                     : Color.FromArgb(200, 0, 64, 64);

            Cursor = Cursors.WaitCursor;

            BringToFront();

            // =========================
            // CONTENT PANEL
            // =========================

            contentPanel = new Panel
            {
                Size = new Size(320, 140)
            };

            RadiusControl.Apply(
                contentPanel,
                AppStyle.RadiusLarge);

            ShadowEngine.Enable(
                contentPanel);

            // =========================
            // PROGRESS
            // =========================

            progress = new ProgressBar
            {
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Width = 240,
                Height = 12,
                Left = 40,
                Top = 55
            };

            // =========================
            // LABEL
            // =========================

            lblMessage = new Label
            {
                Text = "Loading...",
                AutoSize = false,
                Width = 260,
                Height = 30,
                Left = 30,
                Top = 85,
                Font = AppStyle.SubtitleFont,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // =========================
            // ADD CONTROLS
            // =========================

            contentPanel.Controls.Add(progress);
            contentPanel.Controls.Add(lblMessage);

            Controls.Add(contentPanel);

            Resize += LoadingOverlay_Resize;
        }

        // =========================
        // CENTER PANEL
        // =========================

        private void LoadingOverlay_Resize(
            object? sender,
            EventArgs e)
        {
            contentPanel.Left =
                (Width - contentPanel.Width) / 2;

            contentPanel.Top =
                (Height - contentPanel.Height) / 2;
        }

        // =========================
        // THEME
        // =========================

        private void ApplyTheme()
        {
            contentPanel.BackColor =
                ThemeManager.SurfaceColor;

            lblMessage.ForeColor =
                ThemeManager.TextColor;
        }

        // =========================
        // SHOW
        // =========================

        public void ShowOverlay(
            string message = "Loading...")
        {
            Message = message;

            Visible = true;

            BringToFront();

            Enabled = true;
        }

        // =========================
        // HIDE
        // =========================

        public void HideOverlay()
        {
            Visible = false;
        }

        // =========================
        // BLOCK INPUT
        // =========================

        protected override void OnMouseDown(
            MouseEventArgs e)
        {
            // منع النقر
        }

        protected override void OnMouseMove(
            MouseEventArgs e)
        {
            // منع التفاعل
        }

        protected override void OnMouseClick(
            MouseEventArgs e)
        {
            // منع التفاعل
        }

        // =========================
        // DISPOSE
        // =========================

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThemeManager.ThemeChanged -= OnThemeChanged;
            }

            base.Dispose(disposing);
        }
    }
}
