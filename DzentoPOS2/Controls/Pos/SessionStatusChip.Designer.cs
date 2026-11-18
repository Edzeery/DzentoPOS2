namespace DzentoPOS2.WinForms.Controls.Pos
{
    partial class SessionStatusChip
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
            this._text = new System.Windows.Forms.Label();
            this._dot = new System.Windows.Forms.Label();
            this._action = new DzentoPOS2.WinForms.Controls.ThemedIconButton();
            this._toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // _text
            // 
            this._text.AutoEllipsis = true;
            this._text.BackColor = System.Drawing.Color.Transparent;
            this._text.Dock = System.Windows.Forms.DockStyle.Fill;
            this._text.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this._text.Location = new System.Drawing.Point(26, 4);
            this._text.Name = "_text";
            this._text.Size = new System.Drawing.Size(244, 28);
            this._text.TabIndex = 0;
            this._text.Text = "Session";
            this._text.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _dot
            // 
            this._dot.BackColor = System.Drawing.Color.Transparent;
            this._dot.Dock = System.Windows.Forms.DockStyle.Left;
            this._dot.Font = new System.Drawing.Font("Segoe UI", 12F);
            this._dot.ForeColor = System.Drawing.SystemColors.Highlight;
            this._dot.Location = new System.Drawing.Point(10, 4);
            this._dot.Name = "_dot";
            this._dot.Size = new System.Drawing.Size(16, 28);
            this._dot.TabIndex = 1;
            this._dot.Text = "●";
            this._dot.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _action
            // 
            this._action.Cursor = System.Windows.Forms.Cursors.Hand;
            this._action.Dock = System.Windows.Forms.DockStyle.Right;
            this._action.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._action.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            this._action.IconChar = FontAwesome.Sharp.IconChar.CashRegister;
            this._action.IconSize = 14;
            this._action.Location = new System.Drawing.Point(270, 4);
            this._action.Name = "_action";
            this._action.Size = new System.Drawing.Size(110, 28);
            this._action.TabIndex = 2;
            this._action.Text = "Open";
            this._action.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this._action.Tone = DzentoPOS2.WinForms.Controls.ButtonTone.Primary;
            this._action.Visible = false;
            this._action.Click += new System.EventHandler(this.OnActionClick);
            // 
            // SessionStatusChip
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this._text);
            this.Controls.Add(this._action);
            this.Controls.Add(this._dot);
            this.Name = "SessionStatusChip";
            this.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            this.Size = new System.Drawing.Size(390, 36);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label _text;
        private System.Windows.Forms.Label _dot;
        private DzentoPOS2.WinForms.Controls.ThemedIconButton _action;
        private System.Windows.Forms.ToolTip _toolTip;
    }
}
