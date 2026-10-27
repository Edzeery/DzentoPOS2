using DzentoPOS2.WinForms.Controls;

namespace DzentoPOS2.WinForms.Forms.Accounting
{
    partial class CashSessionOpenDialog
    {
        private System.ComponentModel.IContainer components = null;

        private Label lblTitle;
        private Label lblCashbox;
        private ThemedComboBox cmbCashbox;
        private Label lblOpening;
        private NumericUpDown numOpening;
        private Label lblNotes;
        private TextBox txtNotes;
        private Label lblError;
        private Panel pnlFooter;
        private FlowLayoutPanel footerButtons;
        private ThemedButton btnOpen;
        private ThemedButton btnCancel;

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            lblTitle = new Label();
            lblCashbox = new Label();
            cmbCashbox = new ThemedComboBox();
            lblOpening = new Label();
            numOpening = new NumericUpDown();
            lblNotes = new Label();
            txtNotes = new TextBox();
            lblError = new Label();
            pnlFooter = new Panel();
            footerButtons = new FlowLayoutPanel();
            btnOpen = new ThemedButton();
            btnCancel = new ThemedButton();

            SuspendLayout();

            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblTitle.Location = new Point(24, 20);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(472, 34);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Open Cash Session";

            // 
            // lblCashbox
            // 
            lblCashbox.AutoSize = true;
            lblCashbox.Font = new Font("Segoe UI", 9F);
            lblCashbox.Location = new Point(24, 70);
            lblCashbox.Name = "lblCashbox";
            lblCashbox.Size = new Size(60, 15);
            lblCashbox.TabIndex = 1;
            lblCashbox.Text = "Cashbox";

            // 
            // cmbCashbox
            // 
            cmbCashbox.Location = new Point(24, 90);
            cmbCashbox.Name = "cmbCashbox";
            cmbCashbox.Size = new Size(472, 36);
            cmbCashbox.TabIndex = 2;

            // 
            // lblOpening
            // 
            lblOpening.AutoSize = true;
            lblOpening.Font = new Font("Segoe UI", 9F);
            lblOpening.Location = new Point(24, 144);
            lblOpening.Name = "lblOpening";
            lblOpening.Size = new Size(100, 15);
            lblOpening.TabIndex = 3;
            lblOpening.Text = "Opening Balance";

            // 
            // numOpening
            // 
            numOpening.DecimalPlaces = 2;
            numOpening.Font = new Font("Segoe UI", 11F);
            numOpening.Location = new Point(24, 164);
            numOpening.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            numOpening.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numOpening.Name = "numOpening";
            numOpening.Size = new Size(220, 32);
            numOpening.TabIndex = 4;
            numOpening.TextAlign = HorizontalAlignment.Right;
            numOpening.ThousandsSeparator = true;

            // 
            // lblNotes
            // 
            lblNotes.AutoSize = true;
            lblNotes.Font = new Font("Segoe UI", 9F);
            lblNotes.Location = new Point(24, 212);
            lblNotes.Name = "lblNotes";
            lblNotes.Size = new Size(45, 15);
            lblNotes.TabIndex = 5;
            lblNotes.Text = "Notes";

            // 
            // txtNotes
            // 
            txtNotes.BorderStyle = BorderStyle.FixedSingle;
            txtNotes.Font = new Font("Segoe UI", 10F);
            txtNotes.Location = new Point(24, 232);
            txtNotes.MaxLength = 1000;
            txtNotes.Multiline = true;
            txtNotes.Name = "txtNotes";
            txtNotes.ScrollBars = ScrollBars.Vertical;
            txtNotes.Size = new Size(472, 64);
            txtNotes.TabIndex = 6;

            // 
            // lblError
            // 
            lblError.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblError.ForeColor = Color.FromArgb(220, 53, 69);
            lblError.Location = new Point(24, 304);
            lblError.Name = "lblError";
            lblError.Size = new Size(472, 34);
            lblError.TabIndex = 7;
            lblError.Visible = false;

            // 
            // pnlFooter
            // 
            pnlFooter.Controls.Add(footerButtons);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 344);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Padding = new Padding(24, 10, 24, 10);
            pnlFooter.Size = new Size(520, 56);
            pnlFooter.TabIndex = 8;

            // 
            // footerButtons
            // 
            footerButtons.AutoSize = true;
            footerButtons.Dock = DockStyle.Fill;
            footerButtons.FlowDirection = FlowDirection.RightToLeft;
            footerButtons.Name = "footerButtons";
            footerButtons.TabIndex = 0;
            footerButtons.WrapContents = false;

            // 
            // btnOpen
            // 
            btnOpen.Location = new Point(336, 10);
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new Size(160, 36);
            btnOpen.TabIndex = 0;
            btnOpen.Text = "Open Session";
            btnOpen.Tone = ButtonTone.Primary;

            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(216, 10);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(110, 36);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.Tone = ButtonTone.Neutral;

            footerButtons.Controls.Add(btnOpen);
            footerButtons.Controls.Add(btnCancel);

            // 
            // CashSessionOpenDialog
            // 
            AcceptButton = btnOpen;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(520, 400);
            Controls.Add(lblTitle);
            Controls.Add(lblCashbox);
            Controls.Add(cmbCashbox);
            Controls.Add(lblOpening);
            Controls.Add(numOpening);
            Controls.Add(lblNotes);
            Controls.Add(txtNotes);
            Controls.Add(lblError);
            Controls.Add(pnlFooter);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "CashSessionOpenDialog";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Open Cash Session";

            ResumeLayout(false);
            PerformLayout();
        }
    }
}