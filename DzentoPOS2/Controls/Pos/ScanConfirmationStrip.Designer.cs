namespace DzentoPOS2.WinForms.Controls.Pos
{
    partial class ScanConfirmationStrip
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this._textHost = new System.Windows.Forms.Panel();
            this._title = new System.Windows.Forms.Label();
            this._detail = new System.Windows.Forms.Label();
            this._icon = new System.Windows.Forms.Label();
            this._accent = new System.Windows.Forms.Panel();
            this._hideTimer = new System.Windows.Forms.Timer(this.components);
            this._textHost.SuspendLayout();
            this.SuspendLayout();
            // 
            // _textHost
            // 
            this._textHost.BackColor = System.Drawing.Color.Transparent;
            this._textHost.Controls.Add(this._detail);
            this._textHost.Controls.Add(this._title);
            this._textHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this._textHost.Location = new System.Drawing.Point(1, 1);
            this._textHost.Margin = new System.Windows.Forms.Padding(0);
            this._textHost.Name = "_textHost";
            this._textHost.Padding = new System.Windows.Forms.Padding(8, 3, 8, 3);
            this._textHost.Size = new System.Drawing.Size(558, 44);
            this._textHost.TabIndex = 2;
            // 
            // _title
            // 
            this._title.AutoEllipsis = true;
            this._title.Dock = System.Windows.Forms.DockStyle.Top;
            this._title.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this._title.Location = new System.Drawing.Point(8, 3);
            this._title.Name = "_title";
            this._title.Size = new System.Drawing.Size(542, 19);
            this._title.TabIndex = 0;
            this._title.Text = "Scan confirmed";
            this._title.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _detail
            // 
            this._detail.AutoEllipsis = true;
            this._detail.Dock = System.Windows.Forms.DockStyle.Fill;
            this._detail.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this._detail.Location = new System.Drawing.Point(8, 22);
            this._detail.Name = "_detail";
            this._detail.Size = new System.Drawing.Size(542, 19);
            this._detail.TabIndex = 1;
            this._detail.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _icon
            // 
            this._icon.BackColor = System.Drawing.Color.Transparent;
            this._icon.Dock = System.Windows.Forms.DockStyle.Left;
            this._icon.Font = new System.Drawing.Font("Segoe UI Semibold", 13F);
            this._icon.Location = new System.Drawing.Point(5, 1);
            this._icon.Name = "_icon";
            this._icon.Size = new System.Drawing.Size(34, 44);
            this._icon.TabIndex = 1;
            this._icon.Text = "✓";
            this._icon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _accent
            // 
            this._accent.Dock = System.Windows.Forms.DockStyle.Left;
            this._accent.Location = new System.Drawing.Point(1, 1);
            this._accent.Name = "_accent";
            this._accent.Size = new System.Drawing.Size(4, 44);
            this._accent.TabIndex = 0;
            // 
            // _hideTimer
            // 
            this._hideTimer.Interval = 3000;
            this._hideTimer.Tick += new System.EventHandler(this.OnHideTimerTick);
            // 
            // ScanConfirmationStrip
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.Controls.Add(this._textHost);
            this.Controls.Add(this._icon);
            this.Controls.Add(this._accent);
            this.Name = "ScanConfirmationStrip";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.Size = new System.Drawing.Size(600, 46);
            this.Visible = false;
            this._textHost.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel _textHost;
        private System.Windows.Forms.Label _title;
        private System.Windows.Forms.Label _detail;
        private System.Windows.Forms.Label _icon;
        private System.Windows.Forms.Panel _accent;
        private System.Windows.Forms.Timer _hideTimer;
    }
}
