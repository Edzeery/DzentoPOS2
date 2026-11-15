namespace DzentoPOS2.WinForms.Controls.Pos
{
    partial class CategoryChipBar
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
            this._flow = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // _flow
            // 
            this._flow.AutoScroll = true;
            this._flow.BackColor = System.Drawing.Color.Transparent;
            this._flow.Dock = System.Windows.Forms.DockStyle.Fill;
            this._flow.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this._flow.Margin = new System.Windows.Forms.Padding(0);
            this._flow.Name = "_flow";
            this._flow.Padding = new System.Windows.Forms.Padding(0);
            this._flow.Size = new System.Drawing.Size(600, 54);
            this._flow.TabIndex = 0;
            this._flow.WrapContents = false;
            // 
            // CategoryChipBar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this._flow);
            this.Name = "CategoryChipBar";
            this.Size = new System.Drawing.Size(600, 56);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel _flow;
    }
}
