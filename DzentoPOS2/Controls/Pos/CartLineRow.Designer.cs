namespace DzentoPOS2.WinForms.Controls.Pos
{
    partial class CartLineRow
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
            this._subtitle = new System.Windows.Forms.Label();
            this._title = new System.Windows.Forms.Label();
            this._price = new System.Windows.Forms.Label();
            this._total = new System.Windows.Forms.Label();
            this._qtyHost = new System.Windows.Forms.Panel();
            this._qty = new System.Windows.Forms.Label();
            this._minus = new DzentoPOS2.WinForms.Controls.ThemedIconButton();
            this._plus = new DzentoPOS2.WinForms.Controls.ThemedIconButton();
            this._removeHost = new System.Windows.Forms.Panel();
            this._remove = new DzentoPOS2.WinForms.Controls.ThemedIconButton();
            this._toolTip = new System.Windows.Forms.ToolTip(this.components);
            this._textHost.SuspendLayout();
            this._qtyHost.SuspendLayout();
            this._removeHost.SuspendLayout();
            this.SuspendLayout();
            // 
            // _textHost
            // 
            this._textHost.BackColor = System.Drawing.Color.Transparent;
            this._textHost.Controls.Add(this._subtitle);
            this._textHost.Controls.Add(this._title);
            this._textHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this._textHost.Location = new System.Drawing.Point(10, 8);
            this._textHost.Margin = new System.Windows.Forms.Padding(0);
            this._textHost.Name = "_textHost";
            this._textHost.Padding = new System.Windows.Forms.Padding(0, 2, 8, 2);
            this._textHost.Size = new System.Drawing.Size(174, 48);
            this._textHost.TabIndex = 0;
            // 
            // _subtitle
            // 
            this._subtitle.AutoEllipsis = true;
            this._subtitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this._subtitle.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this._subtitle.Location = new System.Drawing.Point(0, 24);
            this._subtitle.Name = "_subtitle";
            this._subtitle.Size = new System.Drawing.Size(166, 22);
            this._subtitle.TabIndex = 1;
            this._subtitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _title
            // 
            this._title.AutoEllipsis = true;
            this._title.Dock = System.Windows.Forms.DockStyle.Top;
            this._title.Font = new System.Drawing.Font("Segoe UI Semibold", 10F);
            this._title.Location = new System.Drawing.Point(0, 2);
            this._title.Name = "_title";
            this._title.Size = new System.Drawing.Size(166, 22);
            this._title.TabIndex = 0;
            this._title.Text = "Product";
            this._title.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _price
            // 
            this._price.Dock = System.Windows.Forms.DockStyle.Right;
            this._price.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._price.Location = new System.Drawing.Point(184, 8);
            this._price.Name = "_price";
            this._price.Size = new System.Drawing.Size(72, 48);
            this._price.TabIndex = 1;
            this._price.Text = "0.00";
            this._price.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _total
            // 
            this._total.Dock = System.Windows.Forms.DockStyle.Right;
            this._total.Font = new System.Drawing.Font("Segoe UI Semibold", 10F);
            this._total.Location = new System.Drawing.Point(256, 8);
            this._total.Name = "_total";
            this._total.Size = new System.Drawing.Size(88, 48);
            this._total.TabIndex = 2;
            this._total.Text = "0.00";
            this._total.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _qtyHost
            // 
            this._qtyHost.BackColor = System.Drawing.Color.Transparent;
            this._qtyHost.Controls.Add(this._qty);
            this._qtyHost.Controls.Add(this._minus);
            this._qtyHost.Controls.Add(this._plus);
            this._qtyHost.Dock = System.Windows.Forms.DockStyle.Right;
            this._qtyHost.Location = new System.Drawing.Point(344, 8);
            this._qtyHost.Name = "_qtyHost";
            this._qtyHost.Padding = new System.Windows.Forms.Padding(0, 9, 0, 9);
            this._qtyHost.Size = new System.Drawing.Size(96, 48);
            this._qtyHost.TabIndex = 3;
            // 
            // _qty
            // 
            this._qty.Dock = System.Windows.Forms.DockStyle.Fill;
            this._qty.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this._qty.Location = new System.Drawing.Point(26, 9);
            this._qty.Name = "_qty";
            this._qty.Size = new System.Drawing.Size(44, 30);
            this._qty.TabIndex = 1;
            this._qty.Text = "1";
            this._qty.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _minus
            // 
            this._minus.Cursor = System.Windows.Forms.Cursors.Hand;
            this._minus.Dock = System.Windows.Forms.DockStyle.Left;
            this._minus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._minus.IconChar = FontAwesome.Sharp.IconChar.Minus;
            this._minus.IconSize = 12;
            this._minus.Location = new System.Drawing.Point(0, 9);
            this._minus.Name = "_minus";
            this._minus.Size = new System.Drawing.Size(26, 30);
            this._minus.TabIndex = 0;
            this._minus.Tone = DzentoPOS2.WinForms.Controls.ButtonTone.Neutral;
            this._minus.Click += new System.EventHandler(this.OnMinusClick);
            // 
            // _plus
            // 
            this._plus.Cursor = System.Windows.Forms.Cursors.Hand;
            this._plus.Dock = System.Windows.Forms.DockStyle.Right;
            this._plus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._plus.IconChar = FontAwesome.Sharp.IconChar.Plus;
            this._plus.IconSize = 12;
            this._plus.Location = new System.Drawing.Point(70, 9);
            this._plus.Name = "_plus";
            this._plus.Size = new System.Drawing.Size(26, 30);
            this._plus.TabIndex = 2;
            this._plus.Tone = DzentoPOS2.WinForms.Controls.ButtonTone.Neutral;
            this._plus.Click += new System.EventHandler(this.OnPlusClick);
            // 
            // _removeHost
            // 
            this._removeHost.BackColor = System.Drawing.Color.Transparent;
            this._removeHost.Controls.Add(this._remove);
            this._removeHost.Dock = System.Windows.Forms.DockStyle.Right;
            this._removeHost.Location = new System.Drawing.Point(440, 8);
            this._removeHost.Name = "_removeHost";
            this._removeHost.Padding = new System.Windows.Forms.Padding(0, 9, 0, 9);
            this._removeHost.Size = new System.Drawing.Size(40, 48);
            this._removeHost.TabIndex = 4;
            // 
            // _remove
            // 
            this._remove.Cursor = System.Windows.Forms.Cursors.Hand;
            this._remove.Dock = System.Windows.Forms.DockStyle.Fill;
            this._remove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._remove.IconChar = FontAwesome.Sharp.IconChar.Trash;
            this._remove.IconSize = 12;
            this._remove.Location = new System.Drawing.Point(0, 9);
            this._remove.Name = "_remove";
            this._remove.Size = new System.Drawing.Size(40, 30);
            this._remove.TabIndex = 0;
            this._remove.Tone = DzentoPOS2.WinForms.Controls.ButtonTone.Danger;
            this._remove.Click += new System.EventHandler(this.OnRemoveClick);
            // 
            // CartLineRow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this._textHost);
            this.Controls.Add(this._price);
            this.Controls.Add(this._total);
            this.Controls.Add(this._qtyHost);
            this.Controls.Add(this._removeHost);
            this.Name = "CartLineRow";
            this.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.Size = new System.Drawing.Size(490, 64);
            this._textHost.ResumeLayout(false);
            this._qtyHost.ResumeLayout(false);
            this._removeHost.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel _textHost;
        private System.Windows.Forms.Label _subtitle;
        private System.Windows.Forms.Label _title;
        private System.Windows.Forms.Label _price;
        private System.Windows.Forms.Label _total;
        private System.Windows.Forms.Panel _qtyHost;
        private System.Windows.Forms.Label _qty;
        private DzentoPOS2.WinForms.Controls.ThemedIconButton _minus;
        private DzentoPOS2.WinForms.Controls.ThemedIconButton _plus;
        private System.Windows.Forms.Panel _removeHost;
        private DzentoPOS2.WinForms.Controls.ThemedIconButton _remove;
        private System.Windows.Forms.ToolTip _toolTip;
    }
}
