using DzentoPOS2.WinForms.Controls;
using FontAwesome.Sharp;

namespace DzentoPOS2.WinForms.Forms.Accounting
{
    partial class CashSessionForm
    {
        private System.ComponentModel.IContainer components = null;

        private TableLayoutPanel _root;
        private TableLayoutPanel _header;
        private TableLayoutPanel _headerTop;
        private Label _lblTitle;
        private Label _lblStatus;
        private FlowLayoutPanel _headerFields;

        private Panel _pnlCashbox;
        private Label _lblCashbox;
        private ThemedComboBox _cmbCashbox;

        private Panel _pnlOpening;
        private Label _lblOpening;
        private NumericUpDown _numOpening;

        private Panel _pnlClosing;
        private Label _lblClosing;
        private NumericUpDown _numClosing;

        private FlowLayoutPanel _headerButtons;
        private ThemedButton _btnOpen;
        private ThemedButton _btnClose;
        private ThemedButton _btnPayIn;
        private ThemedButton _btnPayOut;

        private Panel _kpiHost;
        private TableLayoutPanel _kpiGrid;
        private KpiCard _kpiTotalSales;
        private KpiCard _kpiCashSales;
        private KpiCard _kpiNonCash;
        private KpiCard _kpiExpected;
        private KpiCard _kpiOpening;
        private KpiCard _kpiPayIn;
        private KpiCard _kpiPayOut;
        private KpiCard _kpiExpenses;

        private DataGridView _grid;

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            _root = new TableLayoutPanel();
            _header = new TableLayoutPanel();
            _headerTop = new TableLayoutPanel();
            _lblTitle = new Label();
            _lblStatus = new Label();
            _headerFields = new FlowLayoutPanel();

            _pnlCashbox = new Panel();
            _lblCashbox = new Label();
            _cmbCashbox = new ThemedComboBox();

            _pnlOpening = new Panel();
            _lblOpening = new Label();
            _numOpening = new NumericUpDown();

            _pnlClosing = new Panel();
            _lblClosing = new Label();
            _numClosing = new NumericUpDown();

            _headerButtons = new FlowLayoutPanel();
            _btnOpen = new ThemedButton();
            _btnClose = new ThemedButton();
            _btnPayIn = new ThemedButton();
            _btnPayOut = new ThemedButton();

            _kpiHost = new Panel();
            _kpiGrid = new TableLayoutPanel();
            _kpiTotalSales = new KpiCard();
            _kpiCashSales = new KpiCard();
            _kpiNonCash = new KpiCard();
            _kpiExpected = new KpiCard();
            _kpiOpening = new KpiCard();
            _kpiPayIn = new KpiCard();
            _kpiPayOut = new KpiCard();
            _kpiExpenses = new KpiCard();

            _grid = new DataGridView();

            SuspendLayout();

            // 
            // _root
            // 
            _root.ColumnCount = 1;
            _root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _root.Controls.Add(_header, 0, 0);
            _root.Controls.Add(_kpiHost, 0, 1);
            _root.Controls.Add(_grid, 0, 2);
            _root.Dock = DockStyle.Fill;
            _root.Name = "_root";
            _root.Padding = new Padding(16);
            _root.RowCount = 3;
            _root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _root.TabIndex = 0;

            // 
            // _header
            // 
            _header.AutoSize = true;
            _header.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _header.ColumnCount = 1;
            _header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _header.Controls.Add(_headerTop, 0, 0);
            _header.Controls.Add(_headerFields, 0, 1);
            _header.Dock = DockStyle.Fill;
            _header.Name = "_header";
            _header.RowCount = 2;
            _header.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _header.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _header.TabIndex = 0;

            // 
            // _headerTop
            // 
            _headerTop.AutoSize = true;
            _headerTop.ColumnCount = 2;
            _headerTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            _headerTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            _headerTop.Controls.Add(_lblTitle, 0, 0);
            _headerTop.Controls.Add(_lblStatus, 1, 0);
            _headerTop.Dock = DockStyle.Fill;
            _headerTop.Name = "_headerTop";
            _headerTop.RowCount = 1;
            _headerTop.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _headerTop.TabIndex = 0;

            // 
            // _lblTitle
            // 
            _lblTitle.AutoSize = true;
            _lblTitle.Dock = DockStyle.Fill;
            _lblTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            _lblTitle.Name = "_lblTitle";
            _lblTitle.TabIndex = 0;
            _lblTitle.Text = "Cash Drawer";
            _lblTitle.TextAlign = ContentAlignment.MiddleLeft;

            // 
            // _lblStatus
            // 
            _lblStatus.AutoSize = true;
            _lblStatus.Dock = DockStyle.Fill;
            _lblStatus.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _lblStatus.Name = "_lblStatus";
            _lblStatus.TabIndex = 1;
            _lblStatus.Text = "Drawer Closed";
            _lblStatus.TextAlign = ContentAlignment.MiddleRight;

            // 
            // _headerFields
            // 
            _headerFields.AutoSize = true;
            _headerFields.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _headerFields.Controls.Add(_pnlCashbox);
            _headerFields.Controls.Add(_pnlOpening);
            _headerFields.Controls.Add(_pnlClosing);
            _headerFields.Controls.Add(_headerButtons);
            _headerFields.Dock = DockStyle.Fill;
            _headerFields.Name = "_headerFields";
            _headerFields.Padding = new Padding(0, 8, 0, 4);
            _headerFields.TabIndex = 1;
            _headerFields.WrapContents = true;

            // 
            // _pnlCashbox
            // 
            _pnlCashbox.Controls.Add(_lblCashbox);
            _pnlCashbox.Controls.Add(_cmbCashbox);
            _pnlCashbox.Margin = new Padding(0, 0, 12, 8);
            _pnlCashbox.Name = "_pnlCashbox";
            _pnlCashbox.Size = new Size(200, 54);
            _pnlCashbox.TabIndex = 0;

            // 
            // _lblCashbox
            // 
            _lblCashbox.AutoSize = true;
            _lblCashbox.Font = new Font("Segoe UI", 8.5F);
            _lblCashbox.Location = new Point(0, 0);
            _lblCashbox.Name = "_lblCashbox";
            _lblCashbox.Size = new Size(55, 15);
            _lblCashbox.TabIndex = 0;
            _lblCashbox.Text = "Cashbox";

            // 
            // _cmbCashbox
            // 
            _cmbCashbox.Location = new Point(0, 18);
            _cmbCashbox.Name = "_cmbCashbox";
            _cmbCashbox.Size = new Size(200, 32);
            _cmbCashbox.TabIndex = 1;

            // 
            // _pnlOpening
            // 
            _pnlOpening.Controls.Add(_lblOpening);
            _pnlOpening.Controls.Add(_numOpening);
            _pnlOpening.Margin = new Padding(0, 0, 12, 8);
            _pnlOpening.Name = "_pnlOpening";
            _pnlOpening.Size = new Size(160, 54);
            _pnlOpening.TabIndex = 1;

            // 
            // _lblOpening
            // 
            _lblOpening.AutoSize = true;
            _lblOpening.Font = new Font("Segoe UI", 8.5F);
            _lblOpening.Location = new Point(0, 0);
            _lblOpening.Name = "_lblOpening";
            _lblOpening.Size = new Size(94, 15);
            _lblOpening.TabIndex = 0;
            _lblOpening.Text = "Opening Balance";

            // 
            // _numOpening
            // 
            _numOpening.DecimalPlaces = 2;
            _numOpening.Location = new Point(0, 18);
            _numOpening.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            _numOpening.Name = "_numOpening";
            _numOpening.Size = new Size(160, 30);
            _numOpening.TabIndex = 1;
            _numOpening.TextAlign = HorizontalAlignment.Right;
            _numOpening.ThousandsSeparator = true;

            // 
            // _pnlClosing
            // 
            _pnlClosing.Controls.Add(_lblClosing);
            _pnlClosing.Controls.Add(_numClosing);
            _pnlClosing.Margin = new Padding(0, 0, 12, 8);
            _pnlClosing.Name = "_pnlClosing";
            _pnlClosing.Size = new Size(160, 54);
            _pnlClosing.TabIndex = 2;

            // 
            // _lblClosing
            // 
            _lblClosing.AutoSize = true;
            _lblClosing.Font = new Font("Segoe UI", 8.5F);
            _lblClosing.Location = new Point(0, 0);
            _lblClosing.Name = "_lblClosing";
            _lblClosing.Size = new Size(92, 15);
            _lblClosing.TabIndex = 0;
            _lblClosing.Text = "Closing Balance";

            // 
            // _numClosing
            // 
            _numClosing.DecimalPlaces = 2;
            _numClosing.Location = new Point(0, 18);
            _numClosing.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            _numClosing.Minimum = new decimal(new int[] { 999999999, 0, 0, int.MinValue });
            _numClosing.Name = "_numClosing";
            _numClosing.Size = new Size(160, 30);
            _numClosing.TabIndex = 1;
            _numClosing.TextAlign = HorizontalAlignment.Right;
            _numClosing.ThousandsSeparator = true;

            // 
            // _headerButtons
            // 
            _headerButtons.AutoSize = true;
            _headerButtons.Controls.Add(_btnOpen);
            _headerButtons.Controls.Add(_btnClose);
            _headerButtons.Controls.Add(_btnPayIn);
            _headerButtons.Controls.Add(_btnPayOut);
            _headerButtons.Margin = new Padding(0, 16, 0, 0);
            _headerButtons.Name = "_headerButtons";
            _headerButtons.Size = new Size(520, 38);
            _headerButtons.TabIndex = 3;
            _headerButtons.WrapContents = false;

            // 
            // _btnOpen
            // 
            _btnOpen.Margin = new Padding(0, 0, 8, 0);
            _btnOpen.Name = "_btnOpen";
            _btnOpen.Size = new Size(126, 36);
            _btnOpen.TabIndex = 0;
            _btnOpen.Text = "Open Session";
            _btnOpen.Tone = ButtonTone.Primary;

            // 
            // _btnClose
            // 
            _btnClose.Margin = new Padding(0, 0, 8, 0);
            _btnClose.Name = "_btnClose";
            _btnClose.Size = new Size(126, 36);
            _btnClose.TabIndex = 1;
            _btnClose.Text = "Close Session";
            _btnClose.Tone = ButtonTone.Neutral;

            // 
            // _btnPayIn
            // 
            _btnPayIn.Margin = new Padding(0, 0, 8, 0);
            _btnPayIn.Name = "_btnPayIn";
            _btnPayIn.Size = new Size(110, 36);
            _btnPayIn.TabIndex = 2;
            _btnPayIn.Text = "Pay In";
            _btnPayIn.Tone = ButtonTone.Success;

            // 
            // _btnPayOut
            // 
            _btnPayOut.Name = "_btnPayOut";
            _btnPayOut.Size = new Size(110, 36);
            _btnPayOut.TabIndex = 3;
            _btnPayOut.Text = "Pay Out";
            _btnPayOut.Tone = ButtonTone.Warning;

            // 
            // _kpiHost
            // 
            _kpiHost.AutoScroll = true;
            _kpiHost.Controls.Add(_kpiGrid);
            _kpiHost.Dock = DockStyle.Fill;
            _kpiHost.Height = 300;
            _kpiHost.Margin = new Padding(0, 8, 0, 8);
            _kpiHost.MinimumSize = new Size(0, 300);
            _kpiHost.Name = "_kpiHost";
            _kpiHost.TabIndex = 1;

            // 
            // _kpiGrid
            // 
            _kpiGrid.ColumnCount = 4;
            _kpiGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            _kpiGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            _kpiGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            _kpiGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            _kpiGrid.Controls.Add(_kpiTotalSales, 0, 0);
            _kpiGrid.Controls.Add(_kpiCashSales, 1, 0);
            _kpiGrid.Controls.Add(_kpiNonCash, 2, 0);
            _kpiGrid.Controls.Add(_kpiExpected, 3, 0);
            _kpiGrid.Controls.Add(_kpiOpening, 0, 1);
            _kpiGrid.Controls.Add(_kpiPayIn, 1, 1);
            _kpiGrid.Controls.Add(_kpiPayOut, 2, 1);
            _kpiGrid.Controls.Add(_kpiExpenses, 3, 1);
            _kpiGrid.Dock = DockStyle.Top;
            _kpiGrid.Height = 280;
            _kpiGrid.Name = "_kpiGrid";
            _kpiGrid.RowCount = 2;
            _kpiGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            _kpiGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            _kpiGrid.TabIndex = 0;

            // 
            // _kpiTotalSales
            // 
            ConfigureKpi(_kpiTotalSales, "totalSales", "Total Sales", IconChar.ShoppingCart, Color.FromArgb(0, 122, 116), "0");
            // 
            // _kpiCashSales
            // 
            ConfigureKpi(_kpiCashSales, "cashSales", "Cash Sales", IconChar.MoneyBillWave, Color.FromArgb(52, 168, 83), "1");
            // 
            // _kpiNonCash
            // 
            ConfigureKpi(_kpiNonCash, "nonCash", "Card / Other", IconChar.FileInvoiceDollar, Color.FromArgb(0, 123, 255), "2");
            // 
            // _kpiExpected
            // 
            ConfigureKpi(_kpiExpected, "expected", "Expected Balance", IconChar.CashRegister, Color.FromArgb(240, 173, 41), "3");
            // 
            // _kpiOpening
            // 
            ConfigureKpi(_kpiOpening, "opening", "Opening Balance", IconChar.SackDollar, Color.FromArgb(108, 117, 125), "4");
            // 
            // _kpiPayIn
            // 
            ConfigureKpi(_kpiPayIn, "payIn", "Pay In", IconChar.Plus, Color.FromArgb(52, 168, 83), "5");
            // 
            // _kpiPayOut
            // 
            ConfigureKpi(_kpiPayOut, "payOut", "Pay Out", IconChar.Minus, Color.FromArgb(220, 53, 69), "6");
            // 
            // _kpiExpenses
            // 
            ConfigureKpi(_kpiExpenses, "expenses", "Expenses", IconChar.Receipt, Color.FromArgb(220, 53, 69), "7");

            // 
            // _grid
            // 
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.AutoGenerateColumns = false;
            _grid.BackgroundColor = Color.White;
            _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _grid.Dock = DockStyle.Fill;
            _grid.MultiSelect = false;
            _grid.Name = "_grid";
            _grid.ReadOnly = true;
            _grid.RowHeadersVisible = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.TabIndex = 2;

            // 
            // CashSessionForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 720);
            Controls.Add(_root);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(480, 520);
            Name = "CashSessionForm";
            Text = "Cash Sessions";

            ResumeLayout(false);
            PerformLayout();
        }

        /// <summary>Designer helper: one KPI tile with its stable identity.</summary>
        private static void ConfigureKpi(
            KpiCard card,
            string key,
            string title,
            IconChar icon,
            Color accent,
            string tag)
        {
            card.Name = key;
            card.CardTitle = title;
            card.Icon = icon;
            card.AccentColor = accent;
            card.Dock = DockStyle.Fill;
            card.Margin = new Padding(6);
            card.Tag = tag;
            card.TabIndex = int.Parse(tag);
        }
    }
}