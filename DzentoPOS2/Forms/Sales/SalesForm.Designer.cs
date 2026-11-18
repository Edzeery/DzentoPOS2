namespace DzentoPOS2.WinForms.Forms.Sales
{
    partial class SalesForm
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
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this._tabs = new System.Windows.Forms.TabControl();
            this._tabNewSale = new System.Windows.Forms.TabPage();
            this._saleLayout = new System.Windows.Forms.TableLayoutPanel();
            this._leftPanel = new System.Windows.Forms.Panel();
            this._productColumn = new System.Windows.Forms.TableLayoutPanel();
            this._productList = new DzentoPOS2.WinForms.Controls.Pos.PosProductList();
            this._scanStrip = new DzentoPOS2.WinForms.Controls.Pos.ScanConfirmationStrip();
            this._categoryBar = new DzentoPOS2.WinForms.Controls.Pos.CategoryChipBar();
            this._searchBar = new System.Windows.Forms.TableLayoutPanel();
            this._btnScanBarcode = new DzentoPOS2.WinForms.Controls.ThemedIconButton();
            this._productSearch = new EZControls.Controls.Inputs.EZTextBox();
            this._rightPanel = new System.Windows.Forms.Panel();
            this._basketRoot = new System.Windows.Forms.TableLayoutPanel();
            this._sessionChip = new DzentoPOS2.WinForms.Controls.Pos.SessionStatusChip();
            this._basketScroll = new System.Windows.Forms.Panel();
            this._basketLayout = new System.Windows.Forms.TableLayoutPanel();
            this._totalsLayout = new System.Windows.Forms.TableLayoutPanel();
            this._heroTotalPanel = new System.Windows.Forms.TableLayoutPanel();
            this._lblTotalValue = new System.Windows.Forms.Label();
            this._lblTotal = new System.Windows.Forms.Label();
            this._paidEditorPanel = new System.Windows.Forms.TableLayoutPanel();
            this._btnPayInFull = new DzentoPOS2.WinForms.Controls.ThemedIconButton();
            this._numPaid = new System.Windows.Forms.NumericUpDown();
            this._lblBalanceValue = new System.Windows.Forms.Label();
            this._lblBalance = new System.Windows.Forms.Label();
            this._lblPaymentMethod = new System.Windows.Forms.Label();
            this._cmbPaymentMethod = new System.Windows.Forms.ComboBox();
            this._lblPaid = new System.Windows.Forms.Label();
            this._numFees = new System.Windows.Forms.NumericUpDown();
            this._lblFees = new System.Windows.Forms.Label();
            this._lblTaxAmountValue = new System.Windows.Forms.Label();
            this._lblTaxAmount = new System.Windows.Forms.Label();
            this._numTax = new System.Windows.Forms.NumericUpDown();
            this._lblTax = new System.Windows.Forms.Label();
            this._lblAfterDiscountValue = new System.Windows.Forms.Label();
            this._lblAfterDiscount = new System.Windows.Forms.Label();
            this._discountEditorPanel = new System.Windows.Forms.TableLayoutPanel();
            this._cmbDiscountMode = new System.Windows.Forms.ComboBox();
            this._numDiscount = new System.Windows.Forms.NumericUpDown();
            this._lblDiscountValue = new System.Windows.Forms.Label();
            this._lblDiscount = new System.Windows.Forms.Label();
            this._lblSubtotalValue = new System.Windows.Forms.Label();
            this._lblSubtotal = new System.Windows.Forms.Label();
            this._cartLines = new System.Windows.Forms.FlowLayoutPanel();
            this._lblEmptyCart = new System.Windows.Forms.Label();
            this._cartHeader = new System.Windows.Forms.Panel();
            this._btnClearCart = new DzentoPOS2.WinForms.Controls.ThemedIconButton();
            this._lblCart = new System.Windows.Forms.Label();
            this._customerRow = new System.Windows.Forms.TableLayoutPanel();
            this._btnAddCustomer = new DzentoPOS2.WinForms.Controls.ThemedIconButton();
            this._cmbCustomer = new System.Windows.Forms.ComboBox();
            this._lblCustomer = new System.Windows.Forms.Label();
            this._actions = new System.Windows.Forms.TableLayoutPanel();
            this._btnCompleteSale = new DzentoPOS2.WinForms.Controls.ThemedIconButton();
            this._btnPrintLast = new DzentoPOS2.WinForms.Controls.ThemedIconButton();
            this._btnCalculator = new DzentoPOS2.WinForms.Controls.ThemedIconButton();
            this._tabInvoices = new System.Windows.Forms.TabPage();
            this._invoiceGrid = new System.Windows.Forms.DataGridView();
            this._invoiceToolbar = new System.Windows.Forms.TableLayoutPanel();
            this._btnDeleteInvoice = new DzentoPOS2.WinForms.Controls.ThemedIconButton();
            this._btnNewSale = new DzentoPOS2.WinForms.Controls.ThemedIconButton();
            this._invoiceSearch = new EZControls.Controls.Inputs.EZTextBox();
            this._toolTip = new System.Windows.Forms.ToolTip(this.components);
            this._tabs.SuspendLayout();
            this._tabNewSale.SuspendLayout();
            this._saleLayout.SuspendLayout();
            this._leftPanel.SuspendLayout();
            this._productColumn.SuspendLayout();
            this._searchBar.SuspendLayout();
            this._rightPanel.SuspendLayout();
            this._basketRoot.SuspendLayout();
            this._basketScroll.SuspendLayout();
            this._basketLayout.SuspendLayout();
            this._totalsLayout.SuspendLayout();
            this._heroTotalPanel.SuspendLayout();
            this._paidEditorPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numPaid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numFees)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numTax)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numDiscount)).BeginInit();
            this._cartHeader.SuspendLayout();
            this._customerRow.SuspendLayout();
            this._actions.SuspendLayout();
            this._tabInvoices.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._invoiceGrid)).BeginInit();
            this._invoiceToolbar.SuspendLayout();
            this.SuspendLayout();
            // 
            // _tabs
            // 
            this._tabs.Controls.Add(this._tabNewSale);
            this._tabs.Controls.Add(this._tabInvoices);
            this._tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tabs.Font = new System.Drawing.Font("Segoe UI", 10F);
            this._tabs.Location = new System.Drawing.Point(0, 0);
            this._tabs.Name = "_tabs";
            this._tabs.Padding = new System.Drawing.Point(18, 8);
            this._tabs.SelectedIndex = 0;
            this._tabs.Size = new System.Drawing.Size(1440, 820);
            this._tabs.SizeMode = System.Windows.Forms.TabSizeMode.FillToRight;
            this._tabs.TabIndex = 0;
            // 
            // _tabNewSale
            // 
            this._tabNewSale.BackColor = System.Drawing.SystemColors.Control;
            this._tabNewSale.Controls.Add(this._saleLayout);
            this._tabNewSale.Location = new System.Drawing.Point(4, 36);
            this._tabNewSale.Name = "_tabNewSale";
            this._tabNewSale.Padding = new System.Windows.Forms.Padding(3);
            this._tabNewSale.Size = new System.Drawing.Size(1432, 780);
            this._tabNewSale.TabIndex = 0;
            this._tabNewSale.Text = "New Sale";
            this._tabNewSale.Resize += new System.EventHandler(this.OnSaleTabResized);
            // 
            // _saleLayout
            // 
            this._saleLayout.ColumnCount = 2;
            this._saleLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._saleLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 460F));
            this._saleLayout.Controls.Add(this._leftPanel, 0, 0);
            this._saleLayout.Controls.Add(this._rightPanel, 1, 0);
            this._saleLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this._saleLayout.Location = new System.Drawing.Point(3, 3);
            this._saleLayout.Margin = new System.Windows.Forms.Padding(0);
            this._saleLayout.Name = "_saleLayout";
            this._saleLayout.Padding = new System.Windows.Forms.Padding(0);
            this._saleLayout.RowCount = 1;
            this._saleLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._saleLayout.Size = new System.Drawing.Size(1426, 774);
            this._saleLayout.TabIndex = 0;
            // 
            // _leftPanel
            // 
            this._leftPanel.Controls.Add(this._productColumn);
            this._leftPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._leftPanel.Location = new System.Drawing.Point(0, 0);
            this._leftPanel.Margin = new System.Windows.Forms.Padding(0);
            this._leftPanel.Name = "_leftPanel";
            this._leftPanel.Padding = new System.Windows.Forms.Padding(12);
            this._leftPanel.Size = new System.Drawing.Size(966, 774);
            this._leftPanel.TabIndex = 0;
            this._leftPanel.Tag = "background";
            // 
            // _productColumn
            // 
            this._productColumn.ColumnCount = 1;
            this._productColumn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._productColumn.Controls.Add(this._productList, 0, 3);
            this._productColumn.Controls.Add(this._scanStrip, 0, 2);
            this._productColumn.Controls.Add(this._categoryBar, 0, 1);
            this._productColumn.Controls.Add(this._searchBar, 0, 0);
            this._productColumn.Dock = System.Windows.Forms.DockStyle.Fill;
            this._productColumn.Location = new System.Drawing.Point(12, 12);
            this._productColumn.Margin = new System.Windows.Forms.Padding(0);
            this._productColumn.Name = "_productColumn";
            this._productColumn.Padding = new System.Windows.Forms.Padding(0);
            this._productColumn.RowCount = 4;
            this._productColumn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this._productColumn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this._productColumn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._productColumn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._productColumn.Size = new System.Drawing.Size(942, 750);
            this._productColumn.TabIndex = 0;
            // 
            // _productList
            // 
            this._productList.Dock = System.Windows.Forms.DockStyle.Fill;
            this._productList.Location = new System.Drawing.Point(0, 92);
            this._productList.Margin = new System.Windows.Forms.Padding(0);
            this._productList.Name = "_productList";
            this._productList.Size = new System.Drawing.Size(942, 658);
            this._productList.TabIndex = 3;
            this._productList.TabStop = true;
            this._productList.ItemDoubleClicked += new System.EventHandler<DzentoPOS2.App.Services.Sales.PosProductItem>(this.OnProductItemActivated);
            // 
            // _scanStrip
            // 
            this._scanStrip.Dock = System.Windows.Forms.DockStyle.Fill;
            this._scanStrip.Location = new System.Drawing.Point(0, 92);
            this._scanStrip.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this._scanStrip.MinimumSize = new System.Drawing.Size(0, 46);
            this._scanStrip.Name = "_scanStrip";
            this._scanStrip.Size = new System.Drawing.Size(942, 46);
            this._scanStrip.TabIndex = 2;
            this._scanStrip.Visible = false;
            // 
            // _categoryBar
            // 
            this._categoryBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this._categoryBar.Location = new System.Drawing.Point(0, 48);
            this._categoryBar.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this._categoryBar.Name = "_categoryBar";
            this._categoryBar.Size = new System.Drawing.Size(942, 36);
            this._categoryBar.TabIndex = 1;
            this._categoryBar.CategorySelected += new System.EventHandler<DzentoPOS2.WinForms.Controls.Pos.PosCategoryFilterChangedEventArgs>(this.OnCategorySelected);
            // 
            // _searchBar
            // 
            this._searchBar.ColumnCount = 2;
            this._searchBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._searchBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this._searchBar.Controls.Add(this._btnScanBarcode, 1, 0);
            this._searchBar.Controls.Add(this._productSearch, 0, 0);
            this._searchBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this._searchBar.Location = new System.Drawing.Point(0, 0);
            this._searchBar.Margin = new System.Windows.Forms.Padding(0);
            this._searchBar.Name = "_searchBar";
            this._searchBar.Padding = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this._searchBar.RowCount = 1;
            this._searchBar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._searchBar.Size = new System.Drawing.Size(942, 48);
            this._searchBar.TabIndex = 0;
            // 
            // _btnScanBarcode
            // 
            this._btnScanBarcode.BackColor = System.Drawing.SystemColors.Control;
            this._btnScanBarcode.Cursor = System.Windows.Forms.Cursors.Hand;
            this._btnScanBarcode.Dock = System.Windows.Forms.DockStyle.Fill;
            this._btnScanBarcode.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlDark;
            this._btnScanBarcode.FlatAppearance.BorderSize = 1;
            this._btnScanBarcode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnScanBarcode.Font = new System.Drawing.Font("Segoe UI Semibold", 10F);
            this._btnScanBarcode.ForeColor = System.Drawing.SystemColors.ControlText;
            this._btnScanBarcode.IconChar = FontAwesome.Sharp.IconChar.Barcode;
            this._btnScanBarcode.IconColor = System.Drawing.SystemColors.ControlText;
            this._btnScanBarcode.IconSize = 17;
            this._btnScanBarcode.Location = new System.Drawing.Point(792, 0);
            this._btnScanBarcode.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this._btnScanBarcode.Name = "_btnScanBarcode";
            this._btnScanBarcode.Size = new System.Drawing.Size(150, 40);
            this._btnScanBarcode.TabIndex = 1;
            this._btnScanBarcode.Text = "  Scan";
            this._btnScanBarcode.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this._btnScanBarcode.Tone = DzentoPOS2.WinForms.Controls.ButtonTone.Primary;
            this._btnScanBarcode.Click += new System.EventHandler(this.OnScanBarcodeClick);
            // 
            // _productSearch
            // 
            this._productSearch.BackColor = System.Drawing.SystemColors.Window;
            this._productSearch.BorderColor = System.Drawing.SystemColors.ControlDark;
            this._productSearch.BorderFocusColor = System.Drawing.SystemColors.Highlight;
            this._productSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this._productSearch.Font = new System.Drawing.Font("Segoe UI", 11F);
            this._productSearch.ForeColor = System.Drawing.SystemColors.WindowText;
            this._productSearch.Location = new System.Drawing.Point(0, 0);
            this._productSearch.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            this._productSearch.Name = "_productSearch";
            this._productSearch.PlaceholderColor = System.Drawing.SystemColors.GrayText;
            this._productSearch.PlaceholderText = "Search products or scan a barcode (F2)";
            this._productSearch.Size = new System.Drawing.Size(784, 40);
            this._productSearch.TabIndex = 0;
            this._productSearch.TextChanged += new System.EventHandler(this.OnProductSearchTextChanged);
            this._productSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnProductSearchKeyDown);
            // 
            // _rightPanel
            // 
            this._rightPanel.Controls.Add(this._basketRoot);
            this._rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._rightPanel.Location = new System.Drawing.Point(966, 0);
            this._rightPanel.Margin = new System.Windows.Forms.Padding(0);
            this._rightPanel.Name = "_rightPanel";
            this._rightPanel.Padding = new System.Windows.Forms.Padding(12);
            this._rightPanel.Size = new System.Drawing.Size(460, 774);
            this._rightPanel.TabIndex = 1;
            // 
            // _basketRoot
            // 
            this._basketRoot.ColumnCount = 1;
            this._basketRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._basketRoot.Controls.Add(this._sessionChip, 0, 0);
            this._basketRoot.Controls.Add(this._basketScroll, 0, 1);
            this._basketRoot.Controls.Add(this._actions, 0, 2);
            this._basketRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this._basketRoot.Location = new System.Drawing.Point(12, 12);
            this._basketRoot.Margin = new System.Windows.Forms.Padding(0);
            this._basketRoot.Name = "_basketRoot";
            this._basketRoot.Padding = new System.Windows.Forms.Padding(0);
            this._basketRoot.RowCount = 3;
            this._basketRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._basketRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._basketRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 62F));
            this._basketRoot.Size = new System.Drawing.Size(436, 750);
            this._basketRoot.TabIndex = 0;
            // 
            // _sessionChip
            // 
            this._sessionChip.Dock = System.Windows.Forms.DockStyle.Fill;
            this._sessionChip.Location = new System.Drawing.Point(0, 0);
            this._sessionChip.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this._sessionChip.MinimumSize = new System.Drawing.Size(0, 38);
            this._sessionChip.Name = "_sessionChip";
            this._sessionChip.Size = new System.Drawing.Size(436, 38);
            this._sessionChip.TabIndex = 0;
            this._sessionChip.ActionClicked += new System.EventHandler(this.OnSessionActionClicked);
            // 
            // _basketScroll
            // 
            this._basketScroll.AutoScroll = true;
            this._basketScroll.Dock = System.Windows.Forms.DockStyle.Fill;
            this._basketScroll.Location = new System.Drawing.Point(0, 46);
            this._basketScroll.Margin = new System.Windows.Forms.Padding(0);
            this._basketScroll.Name = "_basketScroll";
            this._basketScroll.Size = new System.Drawing.Size(436, 642);
            this._basketScroll.TabIndex = 1;
            this._basketScroll.ClientSizeChanged += new System.EventHandler(this.OnBasketScrollSizeChanged);
            this._basketScroll.Controls.Add(this._basketLayout);
            // 
            // _basketLayout
            // 
            this._basketLayout.AutoSize = true;
            this._basketLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._basketLayout.ColumnCount = 1;
            this._basketLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._basketLayout.Controls.Add(this._totalsLayout, 0, 3);
            this._basketLayout.Controls.Add(this._cartLines, 0, 2);
            this._basketLayout.Controls.Add(this._cartHeader, 0, 1);
            this._basketLayout.Controls.Add(this._customerRow, 0, 0);
            this._basketLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this._basketLayout.Location = new System.Drawing.Point(0, 0);
            this._basketLayout.Margin = new System.Windows.Forms.Padding(0);
            this._basketLayout.Name = "_basketLayout";
            this._basketLayout.Padding = new System.Windows.Forms.Padding(0);
            this._basketLayout.RowCount = 4;
            this._basketLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this._basketLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this._basketLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._basketLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._basketLayout.Size = new System.Drawing.Size(436, 354);
            this._basketLayout.TabIndex = 0;
            // 
            // _totalsLayout
            // 
            this._totalsLayout.AutoSize = true;
            this._totalsLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._totalsLayout.ColumnCount = 2;
            this._totalsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 56F));
            this._totalsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 44F));
            this._totalsLayout.Controls.Add(this._lblBalanceValue, 1, 10);
            this._totalsLayout.Controls.Add(this._lblBalance, 0, 10);
            this._totalsLayout.Controls.Add(this._paidEditorPanel, 1, 9);
            this._totalsLayout.Controls.Add(this._lblPaid, 0, 9);
            this._totalsLayout.Controls.Add(this._cmbPaymentMethod, 1, 8);
            this._totalsLayout.Controls.Add(this._lblPaymentMethod, 0, 8);
            this._totalsLayout.Controls.Add(this._heroTotalPanel, 0, 7);
            this._totalsLayout.Controls.Add(this._numFees, 1, 6);
            this._totalsLayout.Controls.Add(this._lblFees, 0, 6);
            this._totalsLayout.Controls.Add(this._lblTaxAmountValue, 1, 5);
            this._totalsLayout.Controls.Add(this._lblTaxAmount, 0, 5);
            this._totalsLayout.Controls.Add(this._numTax, 1, 4);
            this._totalsLayout.Controls.Add(this._lblTax, 0, 4);
            this._totalsLayout.Controls.Add(this._lblAfterDiscountValue, 1, 3);
            this._totalsLayout.Controls.Add(this._lblAfterDiscount, 0, 3);
            this._totalsLayout.Controls.Add(this._discountEditorPanel, 0, 2);
            this._totalsLayout.Controls.Add(this._lblDiscountValue, 1, 1);
            this._totalsLayout.Controls.Add(this._lblDiscount, 0, 1);
            this._totalsLayout.Controls.Add(this._lblSubtotalValue, 1, 0);
            this._totalsLayout.Controls.Add(this._lblSubtotal, 0, 0);
            this._totalsLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this._totalsLayout.Location = new System.Drawing.Point(0, 150);
            this._totalsLayout.Margin = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this._totalsLayout.Name = "_totalsLayout";
            this._totalsLayout.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this._totalsLayout.RowCount = 11;
            this._totalsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._totalsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._totalsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._totalsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._totalsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._totalsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._totalsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._totalsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._totalsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._totalsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._totalsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._totalsLayout.SetColumnSpan(this._heroTotalPanel, 2);
            this._totalsLayout.SetColumnSpan(this._discountEditorPanel, 2);
            this._totalsLayout.Size = new System.Drawing.Size(436, 204);
            this._totalsLayout.TabIndex = 3;
            // 
            // _heroTotalPanel
            // 
            this._heroTotalPanel.ColumnCount = 2;
            this._heroTotalPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._heroTotalPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._heroTotalPanel.Controls.Add(this._lblTotalValue, 1, 0);
            this._heroTotalPanel.Controls.Add(this._lblTotal, 0, 0);
            this._heroTotalPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._heroTotalPanel.Location = new System.Drawing.Point(0, 138);
            this._heroTotalPanel.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this._heroTotalPanel.Name = "_heroTotalPanel";
            this._heroTotalPanel.Padding = new System.Windows.Forms.Padding(0, 6, 0, 4);
            this._heroTotalPanel.RowCount = 1;
            this._heroTotalPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._heroTotalPanel.Size = new System.Drawing.Size(436, 58);
            this._heroTotalPanel.TabIndex = 7;
            // 
            // _lblTotalValue
            // 
            this._lblTotalValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lblTotalValue.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Bold);
            this._lblTotalValue.ForeColor = System.Drawing.SystemColors.ControlText;
            this._lblTotalValue.Location = new System.Drawing.Point(110, 6);
            this._lblTotalValue.Margin = new System.Windows.Forms.Padding(0);
            this._lblTotalValue.MinimumSize = new System.Drawing.Size(0, 60);
            this._lblTotalValue.Name = "_lblTotalValue";
            this._lblTotalValue.Size = new System.Drawing.Size(326, 60);
            this._lblTotalValue.TabIndex = 1;
            this._lblTotalValue.Text = "0.00";
            this._lblTotalValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _lblTotal
            // 
            this._lblTotal.AutoSize = true;
            this._lblTotal.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lblTotal.Font = new System.Drawing.Font("Segoe UI Semibold", 11F);
            this._lblTotal.ForeColor = System.Drawing.SystemColors.ControlText;
            this._lblTotal.Location = new System.Drawing.Point(0, 6);
            this._lblTotal.Margin = new System.Windows.Forms.Padding(0);
            this._lblTotal.Name = "_lblTotal";
            this._lblTotal.Size = new System.Drawing.Size(110, 60);
            this._lblTotal.TabIndex = 0;
            this._lblTotal.Text = "Total:";
            this._lblTotal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this._lblTotal.MinimumSize = new System.Drawing.Size(0, 60);
            // 
            // _paidEditorPanel
            // 
            this._paidEditorPanel.ColumnCount = 2;
            this._paidEditorPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._paidEditorPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this._paidEditorPanel.Controls.Add(this._btnPayInFull, 1, 0);
            this._paidEditorPanel.Controls.Add(this._numPaid, 0, 0);
            this._paidEditorPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._paidEditorPanel.Location = new System.Drawing.Point(244, 196);
            this._paidEditorPanel.Margin = new System.Windows.Forms.Padding(0);
            this._paidEditorPanel.Name = "_paidEditorPanel";
            this._paidEditorPanel.Padding = new System.Windows.Forms.Padding(0);
            this._paidEditorPanel.RowCount = 1;
            this._paidEditorPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._paidEditorPanel.Size = new System.Drawing.Size(192, 30);
            this._paidEditorPanel.TabIndex = 8;
            // 
            // _btnPayInFull
            // 
            this._btnPayInFull.BackColor = System.Drawing.SystemColors.Control;
            this._btnPayInFull.Cursor = System.Windows.Forms.Cursors.Hand;
            this._btnPayInFull.Dock = System.Windows.Forms.DockStyle.Fill;
            this._btnPayInFull.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlDark;
            this._btnPayInFull.FlatAppearance.BorderSize = 1;
            this._btnPayInFull.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnPayInFull.IconChar = FontAwesome.Sharp.IconChar.MoneyBillWave;
            this._btnPayInFull.IconColor = System.Drawing.SystemColors.ControlText;
            this._btnPayInFull.IconSize = 15;
            this._btnPayInFull.Location = new System.Drawing.Point(152, 0);
            this._btnPayInFull.Margin = new System.Windows.Forms.Padding(0);
            this._btnPayInFull.Name = "_btnPayInFull";
            this._btnPayInFull.Size = new System.Drawing.Size(40, 30);
            this._btnPayInFull.TabIndex = 1;
            this._btnPayInFull.Text = "";
            this._btnPayInFull.Tone = DzentoPOS2.WinForms.Controls.ButtonTone.Success;
            this._btnPayInFull.Click += new System.EventHandler(this.OnPayInFullClick);
            // 
            // _numPaid
            // 
            this._numPaid.BackColor = System.Drawing.SystemColors.Window;
            this._numPaid.DecimalPlaces = 2;
            this._numPaid.Dock = System.Windows.Forms.DockStyle.Fill;
            this._numPaid.Font = new System.Drawing.Font("Segoe UI", 10F);
            this._numPaid.ForeColor = System.Drawing.SystemColors.WindowText;
            this._numPaid.Location = new System.Drawing.Point(0, 0);
            this._numPaid.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this._numPaid.Maximum = new decimal(new int[] { 99999999, 0, 0, 0 });
            this._numPaid.Name = "_numPaid";
            this._numPaid.Size = new System.Drawing.Size(148, 30);
            this._numPaid.TabIndex = 0;
            this._numPaid.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this._numPaid.ValueChanged += new System.EventHandler(this.OnPaidValueChanged);
            // 
            // _lblBalanceValue
            // 
            this._lblBalanceValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lblBalanceValue.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this._lblBalanceValue.ForeColor = System.Drawing.SystemColors.ControlText;
            this._lblBalanceValue.Location = new System.Drawing.Point(244, 228);
            this._lblBalanceValue.Margin = new System.Windows.Forms.Padding(0);
            this._lblBalanceValue.MinimumSize = new System.Drawing.Size(0, 22);
            this._lblBalanceValue.Name = "_lblBalanceValue";
            this._lblBalanceValue.Size = new System.Drawing.Size(192, 22);
            this._lblBalanceValue.TabIndex = 9;
            this._lblBalanceValue.Text = "0.00";
            this._lblBalanceValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _lblBalance
            // 
            this._lblBalance.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lblBalance.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this._lblBalance.ForeColor = System.Drawing.SystemColors.ControlText;
            this._lblBalance.Location = new System.Drawing.Point(0, 228);
            this._lblBalance.Margin = new System.Windows.Forms.Padding(0);
            this._lblBalance.MinimumSize = new System.Drawing.Size(0, 22);
            this._lblBalance.Name = "_lblBalance";
            this._lblBalance.Size = new System.Drawing.Size(244, 22);
            this._lblBalance.TabIndex = 9;
            this._lblBalance.Text = "Remaining:";
            this._lblBalance.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _lblPaid
            // 
            this._lblPaid.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lblPaid.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this._lblPaid.ForeColor = System.Drawing.SystemColors.ControlText;
            this._lblPaid.Location = new System.Drawing.Point(0, 196);
            this._lblPaid.Margin = new System.Windows.Forms.Padding(0);
            this._lblPaid.MinimumSize = new System.Drawing.Size(0, 22);
            this._lblPaid.Name = "_lblPaid";
            this._lblPaid.Size = new System.Drawing.Size(244, 30);
            this._lblPaid.TabIndex = 8;
            this._lblPaid.Text = "Paid:";
            this._lblPaid.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _lblPaymentMethod
            // 
            this._lblPaymentMethod.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lblPaymentMethod.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this._lblPaymentMethod.ForeColor = System.Drawing.SystemColors.ControlText;
            this._lblPaymentMethod.Location = new System.Drawing.Point(0, 196);
            this._lblPaymentMethod.Margin = new System.Windows.Forms.Padding(0);
            this._lblPaymentMethod.MinimumSize = new System.Drawing.Size(0, 22);
            this._lblPaymentMethod.Name = "_lblPaymentMethod";
            this._lblPaymentMethod.Size = new System.Drawing.Size(244, 22);
            this._lblPaymentMethod.TabIndex = 11;
            this._lblPaymentMethod.Text = "Payment Method:";
            this._lblPaymentMethod.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _cmbPaymentMethod
            // 
            this._cmbPaymentMethod.BackColor = System.Drawing.SystemColors.Window;
            this._cmbPaymentMethod.Dock = System.Windows.Forms.DockStyle.Fill;
            this._cmbPaymentMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbPaymentMethod.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._cmbPaymentMethod.ForeColor = System.Drawing.SystemColors.WindowText;
            this._cmbPaymentMethod.Location = new System.Drawing.Point(244, 196);
            this._cmbPaymentMethod.Margin = new System.Windows.Forms.Padding(0);
            this._cmbPaymentMethod.Name = "_cmbPaymentMethod";
            this._cmbPaymentMethod.Size = new System.Drawing.Size(192, 23);
            this._cmbPaymentMethod.TabIndex = 12;
            this._cmbPaymentMethod.SelectedIndexChanged += new System.EventHandler(this.OnPaymentMethodChanged);
            // 
            // _numFees
            // 
            this._numFees.BackColor = System.Drawing.SystemColors.Window;
            this._numFees.DecimalPlaces = 2;
            this._numFees.Dock = System.Windows.Forms.DockStyle.Fill;
            this._numFees.Font = new System.Drawing.Font("Segoe UI", 10F);
            this._numFees.ForeColor = System.Drawing.SystemColors.WindowText;
            this._numFees.Location = new System.Drawing.Point(244, 166);
            this._numFees.Margin = new System.Windows.Forms.Padding(0);
            this._numFees.Maximum = new decimal(new int[] { 99999999, 0, 0, 0 });
            this._numFees.Name = "_numFees";
            this._numFees.Size = new System.Drawing.Size(192, 30);
            this._numFees.TabIndex = 6;
            this._numFees.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this._numFees.ValueChanged += new System.EventHandler(this.OnFeesValueChanged);
            // 
            // _lblFees
            // 
            this._lblFees.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lblFees.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this._lblFees.ForeColor = System.Drawing.SystemColors.ControlText;
            this._lblFees.Location = new System.Drawing.Point(0, 166);
            this._lblFees.Margin = new System.Windows.Forms.Padding(0);
            this._lblFees.MinimumSize = new System.Drawing.Size(0, 30);
            this._lblFees.Name = "_lblFees";
            this._lblFees.Size = new System.Drawing.Size(244, 30);
            this._lblFees.TabIndex = 6;
            this._lblFees.Text = "Fees:";
            this._lblFees.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _lblTaxAmountValue
            // 
            this._lblTaxAmountValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lblTaxAmountValue.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this._lblTaxAmountValue.ForeColor = System.Drawing.SystemColors.ControlText;
            this._lblTaxAmountValue.Location = new System.Drawing.Point(244, 136);
            this._lblTaxAmountValue.Margin = new System.Windows.Forms.Padding(0);
            this._lblTaxAmountValue.MinimumSize = new System.Drawing.Size(0, 22);
            this._lblTaxAmountValue.Name = "_lblTaxAmountValue";
            this._lblTaxAmountValue.Size = new System.Drawing.Size(192, 22);
            this._lblTaxAmountValue.TabIndex = 5;
            this._lblTaxAmountValue.Text = "0.00";
            this._lblTaxAmountValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _lblTaxAmount
            // 
            this._lblTaxAmount.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lblTaxAmount.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this._lblTaxAmount.ForeColor = System.Drawing.SystemColors.ControlText;
            this._lblTaxAmount.Location = new System.Drawing.Point(0, 136);
            this._lblTaxAmount.Margin = new System.Windows.Forms.Padding(0);
            this._lblTaxAmount.MinimumSize = new System.Drawing.Size(0, 22);
            this._lblTaxAmount.Name = "_lblTaxAmount";
            this._lblTaxAmount.Size = new System.Drawing.Size(244, 22);
            this._lblTaxAmount.TabIndex = 5;
            this._lblTaxAmount.Text = "Tax amount:";
            this._lblTaxAmount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _numTax
            // 
            this._numTax.BackColor = System.Drawing.SystemColors.Window;
            this._numTax.DecimalPlaces = 2;
            this._numTax.Dock = System.Windows.Forms.DockStyle.Fill;
            this._numTax.Font = new System.Drawing.Font("Segoe UI", 10F);
            this._numTax.ForeColor = System.Drawing.SystemColors.WindowText;
            this._numTax.Location = new System.Drawing.Point(244, 106);
            this._numTax.Margin = new System.Windows.Forms.Padding(0);
            this._numTax.Name = "_numTax";
            this._numTax.Size = new System.Drawing.Size(192, 30);
            this._numTax.TabIndex = 4;
            this._numTax.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this._numTax.ValueChanged += new System.EventHandler(this.OnTaxValueChanged);
            // 
            // _lblTax
            // 
            this._lblTax.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lblTax.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this._lblTax.ForeColor = System.Drawing.SystemColors.ControlText;
            this._lblTax.Location = new System.Drawing.Point(0, 106);
            this._lblTax.Margin = new System.Windows.Forms.Padding(0);
            this._lblTax.MinimumSize = new System.Drawing.Size(0, 30);
            this._lblTax.Name = "_lblTax";
            this._lblTax.Size = new System.Drawing.Size(244, 30);
            this._lblTax.TabIndex = 4;
            this._lblTax.Text = "Tax rate (%):";
            this._lblTax.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _lblAfterDiscountValue
            // 
            this._lblAfterDiscountValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lblAfterDiscountValue.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this._lblAfterDiscountValue.ForeColor = System.Drawing.SystemColors.ControlText;
            this._lblAfterDiscountValue.Location = new System.Drawing.Point(244, 76);
            this._lblAfterDiscountValue.Margin = new System.Windows.Forms.Padding(0);
            this._lblAfterDiscountValue.MinimumSize = new System.Drawing.Size(0, 22);
            this._lblAfterDiscountValue.Name = "_lblAfterDiscountValue";
            this._lblAfterDiscountValue.Size = new System.Drawing.Size(192, 22);
            this._lblAfterDiscountValue.TabIndex = 3;
            this._lblAfterDiscountValue.Text = "0.00";
            this._lblAfterDiscountValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _lblAfterDiscount
            // 
            this._lblAfterDiscount.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lblAfterDiscount.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this._lblAfterDiscount.ForeColor = System.Drawing.SystemColors.ControlText;
            this._lblAfterDiscount.Location = new System.Drawing.Point(0, 76);
            this._lblAfterDiscount.Margin = new System.Windows.Forms.Padding(0);
            this._lblAfterDiscount.MinimumSize = new System.Drawing.Size(0, 22);
            this._lblAfterDiscount.Name = "_lblAfterDiscount";
            this._lblAfterDiscount.Size = new System.Drawing.Size(244, 22);
            this._lblAfterDiscount.TabIndex = 3;
            this._lblAfterDiscount.Text = "After discount:";
            this._lblAfterDiscount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _discountEditorPanel
            // 
            this._discountEditorPanel.ColumnCount = 2;
            this._discountEditorPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 52F));
            this._discountEditorPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 48F));
            this._discountEditorPanel.Controls.Add(this._cmbDiscountMode, 1, 0);
            this._discountEditorPanel.Controls.Add(this._numDiscount, 0, 0);
            this._discountEditorPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._discountEditorPanel.Location = new System.Drawing.Point(0, 46);
            this._discountEditorPanel.Margin = new System.Windows.Forms.Padding(0);
            this._discountEditorPanel.Name = "_discountEditorPanel";
            this._discountEditorPanel.Padding = new System.Windows.Forms.Padding(0);
            this._discountEditorPanel.RowCount = 1;
            this._discountEditorPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._discountEditorPanel.Size = new System.Drawing.Size(436, 30);
            this._discountEditorPanel.TabIndex = 2;
            // 
            // _cmbDiscountMode
            // 
            this._cmbDiscountMode.BackColor = System.Drawing.SystemColors.Window;
            this._cmbDiscountMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this._cmbDiscountMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbDiscountMode.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._cmbDiscountMode.ForeColor = System.Drawing.SystemColors.WindowText;
            this._cmbDiscountMode.Location = new System.Drawing.Point(226, 0);
            this._cmbDiscountMode.Margin = new System.Windows.Forms.Padding(0);
            this._cmbDiscountMode.Name = "_cmbDiscountMode";
            this._cmbDiscountMode.Size = new System.Drawing.Size(210, 23);
            this._cmbDiscountMode.TabIndex = 1;
            this._cmbDiscountMode.SelectedIndexChanged += new System.EventHandler(this.OnDiscountModeChanged);
            // 
            // _numDiscount
            // 
            this._numDiscount.BackColor = System.Drawing.SystemColors.Window;
            this._numDiscount.DecimalPlaces = 2;
            this._numDiscount.Dock = System.Windows.Forms.DockStyle.Fill;
            this._numDiscount.Font = new System.Drawing.Font("Segoe UI", 10F);
            this._numDiscount.ForeColor = System.Drawing.SystemColors.WindowText;
            this._numDiscount.Location = new System.Drawing.Point(0, 0);
            this._numDiscount.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this._numDiscount.Maximum = new decimal(new int[] { 99999999, 0, 0, 0 });
            this._numDiscount.Name = "_numDiscount";
            this._numDiscount.Size = new System.Drawing.Size(222, 30);
            this._numDiscount.TabIndex = 0;
            this._numDiscount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this._numDiscount.ValueChanged += new System.EventHandler(this.OnDiscountValueChanged);
            // 
            // _lblDiscountValue
            // 
            this._lblDiscountValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lblDiscountValue.Font = new System.Drawing.Font("Segoe UI", 26F, System.Drawing.FontStyle.Bold);
            this._lblDiscountValue.ForeColor = System.Drawing.SystemColors.ControlText;
            this._lblDiscountValue.Location = new System.Drawing.Point(244, 0);
            this._lblDiscountValue.Margin = new System.Windows.Forms.Padding(0);
            this._lblDiscountValue.MinimumSize = new System.Drawing.Size(0, 46);
            this._lblDiscountValue.Name = "_lblDiscountValue";
            this._lblDiscountValue.Size = new System.Drawing.Size(192, 46);
            this._lblDiscountValue.TabIndex = 1;
            this._lblDiscountValue.Text = "0.00";
            this._lblDiscountValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _lblDiscount
            // 
            this._lblDiscount.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lblDiscount.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this._lblDiscount.ForeColor = System.Drawing.SystemColors.ControlText;
            this._lblDiscount.Location = new System.Drawing.Point(0, 0);
            this._lblDiscount.Margin = new System.Windows.Forms.Padding(0);
            this._lblDiscount.MinimumSize = new System.Drawing.Size(0, 46);
            this._lblDiscount.Name = "_lblDiscount";
            this._lblDiscount.Size = new System.Drawing.Size(244, 46);
            this._lblDiscount.TabIndex = 0;
            this._lblDiscount.Text = "Discount:";
            this._lblDiscount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _lblSubtotalValue
            // 
            this._lblSubtotalValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lblSubtotalValue.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this._lblSubtotalValue.ForeColor = System.Drawing.SystemColors.ControlText;
            this._lblSubtotalValue.Location = new System.Drawing.Point(244, 0);
            this._lblSubtotalValue.Margin = new System.Windows.Forms.Padding(0);
            this._lblSubtotalValue.MinimumSize = new System.Drawing.Size(0, 22);
            this._lblSubtotalValue.Name = "_lblSubtotalValue";
            this._lblSubtotalValue.Size = new System.Drawing.Size(192, 22);
            this._lblSubtotalValue.TabIndex = 1;
            this._lblSubtotalValue.Text = "0.00";
            this._lblSubtotalValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _lblSubtotal
            // 
            this._lblSubtotal.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lblSubtotal.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this._lblSubtotal.ForeColor = System.Drawing.SystemColors.ControlText;
            this._lblSubtotal.Location = new System.Drawing.Point(0, 0);
            this._lblSubtotal.Margin = new System.Windows.Forms.Padding(0);
            this._lblSubtotal.MinimumSize = new System.Drawing.Size(0, 22);
            this._lblSubtotal.Name = "_lblSubtotal";
            this._lblSubtotal.Size = new System.Drawing.Size(244, 22);
            this._lblSubtotal.TabIndex = 0;
            this._lblSubtotal.Text = "Subtotal:";
            this._lblSubtotal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _cartLines
            // 
            this._cartLines.AutoSize = true;
            this._cartLines.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._cartLines.Controls.Add(this._lblEmptyCart);
            this._cartLines.Dock = System.Windows.Forms.DockStyle.Fill;
            this._cartLines.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this._cartLines.Location = new System.Drawing.Point(0, 80);
            this._cartLines.Margin = new System.Windows.Forms.Padding(0);
            this._cartLines.Name = "_cartLines";
            this._cartLines.Padding = new System.Windows.Forms.Padding(0);
            this._cartLines.Size = new System.Drawing.Size(436, 64);
            this._cartLines.TabIndex = 2;
            this._cartLines.WrapContents = false;
            this._cartLines.SizeChanged += new System.EventHandler(this.OnCartLinesSizeChanged);
            // 
            // _lblEmptyCart
            // 
            this._lblEmptyCart.Font = new System.Drawing.Font("Segoe UI", 10F);
            this._lblEmptyCart.ForeColor = System.Drawing.SystemColors.GrayText;
            this._lblEmptyCart.Location = new System.Drawing.Point(0, 0);
            this._lblEmptyCart.Margin = new System.Windows.Forms.Padding(0);
            this._lblEmptyCart.Name = "_lblEmptyCart";
            this._lblEmptyCart.Size = new System.Drawing.Size(436, 56);
            this._lblEmptyCart.TabIndex = 0;
            this._lblEmptyCart.Text = "The cart is empty. Scan a barcode or pick a product.";
            this._lblEmptyCart.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _cartHeader
            // 
            this._cartHeader.Controls.Add(this._lblCart);
            this._cartHeader.Controls.Add(this._btnClearCart);
            this._cartHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this._cartHeader.Location = new System.Drawing.Point(0, 44);
            this._cartHeader.Margin = new System.Windows.Forms.Padding(0);
            this._cartHeader.Name = "_cartHeader";
            this._cartHeader.Padding = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this._cartHeader.Size = new System.Drawing.Size(436, 36);
            this._cartHeader.TabIndex = 1;
            // 
            // _btnClearCart
            // 
            this._btnClearCart.BackColor = System.Drawing.SystemColors.Control;
            this._btnClearCart.Cursor = System.Windows.Forms.Cursors.Hand;
            this._btnClearCart.Dock = System.Windows.Forms.DockStyle.Right;
            this._btnClearCart.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlDark;
            this._btnClearCart.FlatAppearance.BorderSize = 1;
            this._btnClearCart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnClearCart.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this._btnClearCart.ForeColor = System.Drawing.SystemColors.ControlText;
            this._btnClearCart.IconChar = FontAwesome.Sharp.IconChar.Trash;
            this._btnClearCart.IconColor = System.Drawing.SystemColors.ControlText;
            this._btnClearCart.IconSize = 14;
            this._btnClearCart.Location = new System.Drawing.Point(336, 0);
            this._btnClearCart.Margin = new System.Windows.Forms.Padding(0);
            this._btnClearCart.Name = "_btnClearCart";
            this._btnClearCart.Size = new System.Drawing.Size(100, 30);
            this._btnClearCart.TabIndex = 1;
            this._btnClearCart.Text = "  Clear";
            this._btnClearCart.Tone = DzentoPOS2.WinForms.Controls.ButtonTone.Neutral;
            this._btnClearCart.Click += new System.EventHandler(this.OnClearCartClick);
            // 
            // _lblCart
            // 
            this._lblCart.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lblCart.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F);
            this._lblCart.ForeColor = System.Drawing.SystemColors.ControlText;
            this._lblCart.Location = new System.Drawing.Point(0, 0);
            this._lblCart.Margin = new System.Windows.Forms.Padding(0);
            this._lblCart.Name = "_lblCart";
            this._lblCart.Size = new System.Drawing.Size(436, 30);
            this._lblCart.TabIndex = 0;
            this._lblCart.Text = "Cart";
            this._lblCart.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _customerRow
            // 
            this._customerRow.ColumnCount = 3;
            this._customerRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 26F));
            this._customerRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 74F));
            this._customerRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this._customerRow.Controls.Add(this._btnAddCustomer, 2, 0);
            this._customerRow.Controls.Add(this._cmbCustomer, 1, 0);
            this._customerRow.Controls.Add(this._lblCustomer, 0, 0);
            this._customerRow.Dock = System.Windows.Forms.DockStyle.Fill;
            this._customerRow.Location = new System.Drawing.Point(0, 0);
            this._customerRow.Margin = new System.Windows.Forms.Padding(0);
            this._customerRow.Name = "_customerRow";
            this._customerRow.Padding = new System.Windows.Forms.Padding(0);
            this._customerRow.RowCount = 1;
            this._customerRow.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._customerRow.Size = new System.Drawing.Size(436, 44);
            this._customerRow.TabIndex = 0;
            // 
            // _btnAddCustomer
            // 
            this._btnAddCustomer.BackColor = System.Drawing.SystemColors.Control;
            this._btnAddCustomer.Cursor = System.Windows.Forms.Cursors.Hand;
            this._btnAddCustomer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._btnAddCustomer.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlDark;
            this._btnAddCustomer.FlatAppearance.BorderSize = 1;
            this._btnAddCustomer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnAddCustomer.IconChar = FontAwesome.Sharp.IconChar.UserPlus;
            this._btnAddCustomer.IconColor = System.Drawing.SystemColors.ControlText;
            this._btnAddCustomer.IconSize = 16;
            this._btnAddCustomer.Location = new System.Drawing.Point(396, 0);
            this._btnAddCustomer.Margin = new System.Windows.Forms.Padding(0);
            this._btnAddCustomer.Name = "_btnAddCustomer";
            this._btnAddCustomer.Size = new System.Drawing.Size(40, 44);
            this._btnAddCustomer.TabIndex = 2;
            this._btnAddCustomer.Text = "";
            this._btnAddCustomer.Tone = DzentoPOS2.WinForms.Controls.ButtonTone.Primary;
            this._btnAddCustomer.Click += new System.EventHandler(this.OnAddCustomerClick);
            // 
            // _cmbCustomer
            // 
            this._cmbCustomer.BackColor = System.Drawing.SystemColors.Window;
            this._cmbCustomer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._cmbCustomer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbCustomer.Font = new System.Drawing.Font("Segoe UI", 10F);
            this._cmbCustomer.ForeColor = System.Drawing.SystemColors.WindowText;
            this._cmbCustomer.Location = new System.Drawing.Point(113, 0);
            this._cmbCustomer.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this._cmbCustomer.Name = "_cmbCustomer";
            this._cmbCustomer.Size = new System.Drawing.Size(277, 25);
            this._cmbCustomer.TabIndex = 1;
            // 
            // _lblCustomer
            // 
            this._lblCustomer.AutoEllipsis = true;
            this._lblCustomer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lblCustomer.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this._lblCustomer.ForeColor = System.Drawing.SystemColors.ControlText;
            this._lblCustomer.Location = new System.Drawing.Point(0, 0);
            this._lblCustomer.Margin = new System.Windows.Forms.Padding(0);
            this._lblCustomer.Name = "_lblCustomer";
            this._lblCustomer.Size = new System.Drawing.Size(113, 44);
            this._lblCustomer.TabIndex = 0;
            this._lblCustomer.Text = "Customer:";
            this._lblCustomer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _actions
            // 
            this._actions.ColumnCount = 3;
            this._actions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this._actions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this._actions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this._actions.Controls.Add(this._btnCompleteSale, 2, 0);
            this._actions.Controls.Add(this._btnPrintLast, 1, 0);
            this._actions.Controls.Add(this._btnCalculator, 0, 0);
            this._actions.Dock = System.Windows.Forms.DockStyle.Fill;
            this._actions.Location = new System.Drawing.Point(0, 688);
            this._actions.Margin = new System.Windows.Forms.Padding(0);
            this._actions.Name = "_actions";
            this._actions.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this._actions.RowCount = 1;
            this._actions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._actions.Size = new System.Drawing.Size(436, 62);
            this._actions.TabIndex = 2;
            // 
            // _btnCompleteSale
            // 
            this._btnCompleteSale.BackColor = System.Drawing.SystemColors.Control;
            this._btnCompleteSale.Cursor = System.Windows.Forms.Cursors.Hand;
            this._btnCompleteSale.Dock = System.Windows.Forms.DockStyle.Fill;
            this._btnCompleteSale.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlDark;
            this._btnCompleteSale.FlatAppearance.BorderSize = 0;
            this._btnCompleteSale.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnCompleteSale.Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold);
            this._btnCompleteSale.ForeColor = System.Drawing.SystemColors.ControlText;
            this._btnCompleteSale.IconChar = FontAwesome.Sharp.IconChar.CheckCircle;
            this._btnCompleteSale.IconColor = System.Drawing.SystemColors.ControlText;
            this._btnCompleteSale.IconSize = 18;
            this._btnCompleteSale.Location = new System.Drawing.Point(261, 6);
            this._btnCompleteSale.Margin = new System.Windows.Forms.Padding(0);
            this._btnCompleteSale.Name = "_btnCompleteSale";
            this._btnCompleteSale.Size = new System.Drawing.Size(175, 56);
            this._btnCompleteSale.Solid = true;
            this._btnCompleteSale.TabIndex = 2;
            this._btnCompleteSale.Text = "  Complete sale";
            this._btnCompleteSale.Tone = DzentoPOS2.WinForms.Controls.ButtonTone.Primary;
            this._btnCompleteSale.Click += new System.EventHandler(this.OnCompleteSaleClick);
            // 
            // _btnPrintLast
            // 
            this._btnPrintLast.BackColor = System.Drawing.SystemColors.Control;
            this._btnPrintLast.Cursor = System.Windows.Forms.Cursors.Hand;
            this._btnPrintLast.Dock = System.Windows.Forms.DockStyle.Fill;
            this._btnPrintLast.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlDark;
            this._btnPrintLast.FlatAppearance.BorderSize = 1;
            this._btnPrintLast.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnPrintLast.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this._btnPrintLast.ForeColor = System.Drawing.SystemColors.ControlText;
            this._btnPrintLast.IconChar = FontAwesome.Sharp.IconChar.Print;
            this._btnPrintLast.IconColor = System.Drawing.SystemColors.ControlText;
            this._btnPrintLast.IconSize = 16;
            this._btnPrintLast.Location = new System.Drawing.Point(130, 6);
            this._btnPrintLast.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this._btnPrintLast.Name = "_btnPrintLast";
            this._btnPrintLast.Size = new System.Drawing.Size(125, 56);
            this._btnPrintLast.TabIndex = 1;
            this._btnPrintLast.Text = "  Print";
            this._btnPrintLast.Tone = DzentoPOS2.WinForms.Controls.ButtonTone.Neutral;
            this._btnPrintLast.Enabled = false;
            this._btnPrintLast.Click += new System.EventHandler(this.OnPrintLastClick);
            // 
            // _btnCalculator
            // 
            this._btnCalculator.BackColor = System.Drawing.SystemColors.Control;
            this._btnCalculator.Cursor = System.Windows.Forms.Cursors.Hand;
            this._btnCalculator.Dock = System.Windows.Forms.DockStyle.Fill;
            this._btnCalculator.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlDark;
            this._btnCalculator.FlatAppearance.BorderSize = 1;
            this._btnCalculator.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnCalculator.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this._btnCalculator.ForeColor = System.Drawing.SystemColors.ControlText;
            this._btnCalculator.IconChar = FontAwesome.Sharp.IconChar.Calculator;
            this._btnCalculator.IconColor = System.Drawing.SystemColors.ControlText;
            this._btnCalculator.IconSize = 16;
            this._btnCalculator.Location = new System.Drawing.Point(0, 6);
            this._btnCalculator.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this._btnCalculator.Name = "_btnCalculator";
            this._btnCalculator.Size = new System.Drawing.Size(124, 56);
            this._btnCalculator.TabIndex = 0;
            this._btnCalculator.Text = "  Calculator";
            this._btnCalculator.Tone = DzentoPOS2.WinForms.Controls.ButtonTone.Primary;
            this._btnCalculator.Click += new System.EventHandler(this.OnCalculatorClick);
            // 
            // _tabInvoices
            // 
            this._tabInvoices.BackColor = System.Drawing.SystemColors.Control;
            this._tabInvoices.Controls.Add(this._invoiceGrid);
            this._tabInvoices.Controls.Add(this._invoiceToolbar);
            this._tabInvoices.Location = new System.Drawing.Point(4, 36);
            this._tabInvoices.Name = "_tabInvoices";
            this._tabInvoices.Padding = new System.Windows.Forms.Padding(3);
            this._tabInvoices.Size = new System.Drawing.Size(1432, 780);
            this._tabInvoices.TabIndex = 1;
            this._tabInvoices.Text = "Sales";
            // 
            // _invoiceGrid
            // 
            this._invoiceGrid.AllowUserToAddRows = false;
            this._invoiceGrid.AllowUserToDeleteRows = false;
            this._invoiceGrid.BackgroundColor = System.Drawing.SystemColors.Window;
            this._invoiceGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._invoiceGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._invoiceGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this._invoiceGrid.Location = new System.Drawing.Point(3, 61);
            this._invoiceGrid.MultiSelect = false;
            this._invoiceGrid.Name = "_invoiceGrid";
            this._invoiceGrid.ReadOnly = true;
            this._invoiceGrid.RowHeadersVisible = false;
            this._invoiceGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._invoiceGrid.Size = new System.Drawing.Size(1426, 716);
            this._invoiceGrid.TabIndex = 1;
            // 
            // _invoiceToolbar
            // 
            this._invoiceToolbar.ColumnCount = 3;
            this._invoiceToolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._invoiceToolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this._invoiceToolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this._invoiceToolbar.Controls.Add(this._btnDeleteInvoice, 2, 0);
            this._invoiceToolbar.Controls.Add(this._btnNewSale, 1, 0);
            this._invoiceToolbar.Controls.Add(this._invoiceSearch, 0, 0);
            this._invoiceToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this._invoiceToolbar.Location = new System.Drawing.Point(3, 3);
            this._invoiceToolbar.Name = "_invoiceToolbar";
            this._invoiceToolbar.Padding = new System.Windows.Forms.Padding(12, 10, 12, 10);
            this._invoiceToolbar.RowCount = 1;
            this._invoiceToolbar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._invoiceToolbar.Size = new System.Drawing.Size(1426, 58);
            this._invoiceToolbar.TabIndex = 0;
            // 
            // _btnDeleteInvoice
            // 
            this._btnDeleteInvoice.BackColor = System.Drawing.SystemColors.Control;
            this._btnDeleteInvoice.Cursor = System.Windows.Forms.Cursors.Hand;
            this._btnDeleteInvoice.Dock = System.Windows.Forms.DockStyle.Fill;
            this._btnDeleteInvoice.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlDark;
            this._btnDeleteInvoice.FlatAppearance.BorderSize = 1;
            this._btnDeleteInvoice.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnDeleteInvoice.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this._btnDeleteInvoice.ForeColor = System.Drawing.SystemColors.ControlText;
            this._btnDeleteInvoice.IconChar = FontAwesome.Sharp.IconChar.Trash;
            this._btnDeleteInvoice.IconColor = System.Drawing.SystemColors.ControlText;
            this._btnDeleteInvoice.IconSize = 16;
            this._btnDeleteInvoice.Location = new System.Drawing.Point(1284, 10);
            this._btnDeleteInvoice.Margin = new System.Windows.Forms.Padding(0);
            this._btnDeleteInvoice.Name = "_btnDeleteInvoice";
            this._btnDeleteInvoice.Size = new System.Drawing.Size(130, 38);
            this._btnDeleteInvoice.TabIndex = 2;
            this._btnDeleteInvoice.Text = "  Delete";
            this._btnDeleteInvoice.Tone = DzentoPOS2.WinForms.Controls.ButtonTone.Danger;
            this._btnDeleteInvoice.Click += new System.EventHandler(this.OnDeleteInvoiceClick);
            // 
            // _btnNewSale
            // 
            this._btnNewSale.BackColor = System.Drawing.SystemColors.Control;
            this._btnNewSale.Cursor = System.Windows.Forms.Cursors.Hand;
            this._btnNewSale.Dock = System.Windows.Forms.DockStyle.Fill;
            this._btnNewSale.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlDark;
            this._btnNewSale.FlatAppearance.BorderSize = 1;
            this._btnNewSale.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnNewSale.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this._btnNewSale.ForeColor = System.Drawing.SystemColors.ControlText;
            this._btnNewSale.IconChar = FontAwesome.Sharp.IconChar.Plus;
            this._btnNewSale.IconColor = System.Drawing.SystemColors.ControlText;
            this._btnNewSale.IconSize = 16;
            this._btnNewSale.Location = new System.Drawing.Point(1144, 10);
            this._btnNewSale.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this._btnNewSale.Name = "_btnNewSale";
            this._btnNewSale.Size = new System.Drawing.Size(132, 38);
            this._btnNewSale.TabIndex = 1;
            this._btnNewSale.Text = "  New sale";
            this._btnNewSale.Tone = DzentoPOS2.WinForms.Controls.ButtonTone.Primary;
            this._btnNewSale.Click += new System.EventHandler(this.OnNewSaleClick);
            // 
            // _invoiceSearch
            // 
            this._invoiceSearch.BackColor = System.Drawing.SystemColors.Window;
            this._invoiceSearch.BorderColor = System.Drawing.SystemColors.ControlDark;
            this._invoiceSearch.BorderFocusColor = System.Drawing.SystemColors.Highlight;
            this._invoiceSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this._invoiceSearch.Font = new System.Drawing.Font("Segoe UI", 11F);
            this._invoiceSearch.ForeColor = System.Drawing.SystemColors.WindowText;
            this._invoiceSearch.Location = new System.Drawing.Point(12, 10);
            this._invoiceSearch.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this._invoiceSearch.Name = "_invoiceSearch";
            this._invoiceSearch.PlaceholderColor = System.Drawing.SystemColors.GrayText;
            this._invoiceSearch.PlaceholderText = "Search invoices";
            this._invoiceSearch.Size = new System.Drawing.Size(1124, 38);
            this._invoiceSearch.TabIndex = 0;
            this._invoiceSearch.TextChanged += new System.EventHandler(this.OnInvoiceSearchTextChanged);
            // 
            // _toolTip
            // 
            this._toolTip.ShowAlways = true;
            // 
            // SalesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1440, 820);
            this.Controls.Add(this._tabs);
            this.MinimumSize = new System.Drawing.Size(360, 600);
            this.Name = "SalesForm";
            this.Text = "Sales";
            this._tabs.ResumeLayout(false);
            this._tabNewSale.ResumeLayout(false);
            this._saleLayout.ResumeLayout(false);
            this._leftPanel.ResumeLayout(false);
            this._productColumn.ResumeLayout(false);
            this._productColumn.PerformLayout();
            this._searchBar.ResumeLayout(false);
            this._rightPanel.ResumeLayout(false);
            this._basketRoot.ResumeLayout(false);
            this._basketRoot.PerformLayout();
            this._basketScroll.ResumeLayout(false);
            this._basketScroll.PerformLayout();
            this._basketLayout.ResumeLayout(false);
            this._basketLayout.PerformLayout();
            this._totalsLayout.ResumeLayout(false);
            this._totalsLayout.PerformLayout();
            this._heroTotalPanel.ResumeLayout(false);
            this._heroTotalPanel.PerformLayout();
            this._paidEditorPanel.ResumeLayout(false);
            this._paidEditorPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numPaid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numFees)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numTax)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numDiscount)).EndInit();
            this._cartHeader.ResumeLayout(false);
            this._customerRow.ResumeLayout(false);
            this._actions.ResumeLayout(false);
            this._tabInvoices.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._invoiceGrid)).EndInit();
            this._invoiceToolbar.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl _tabs;
        private System.Windows.Forms.TabPage _tabNewSale;
        private System.Windows.Forms.TabPage _tabInvoices;
        private System.Windows.Forms.TableLayoutPanel _saleLayout;
        private System.Windows.Forms.Panel _leftPanel;
        private System.Windows.Forms.TableLayoutPanel _productColumn;
        private DzentoPOS2.WinForms.Controls.Pos.PosProductList _productList;
        private DzentoPOS2.WinForms.Controls.Pos.ScanConfirmationStrip _scanStrip;
        private DzentoPOS2.WinForms.Controls.Pos.CategoryChipBar _categoryBar;
        private System.Windows.Forms.TableLayoutPanel _searchBar;
        private DzentoPOS2.WinForms.Controls.ThemedIconButton _btnScanBarcode;
        private EZControls.Controls.Inputs.EZTextBox _productSearch;
        private System.Windows.Forms.Panel _rightPanel;
        private System.Windows.Forms.TableLayoutPanel _basketRoot;
        private DzentoPOS2.WinForms.Controls.Pos.SessionStatusChip _sessionChip;
        private System.Windows.Forms.Panel _basketScroll;
        private System.Windows.Forms.TableLayoutPanel _basketLayout;
        private System.Windows.Forms.TableLayoutPanel _totalsLayout;
        private System.Windows.Forms.TableLayoutPanel _heroTotalPanel;
        private System.Windows.Forms.Label _lblTotalValue;
        private System.Windows.Forms.Label _lblTotal;
        private System.Windows.Forms.TableLayoutPanel _paidEditorPanel;
        private DzentoPOS2.WinForms.Controls.ThemedIconButton _btnPayInFull;
        private System.Windows.Forms.NumericUpDown _numPaid;
        private System.Windows.Forms.Label _lblBalanceValue;
        private System.Windows.Forms.Label _lblBalance;
        private System.Windows.Forms.Label _lblPaid;
    private System.Windows.Forms.Label _lblPaymentMethod;
    private System.Windows.Forms.ComboBox _cmbPaymentMethod;
        private System.Windows.Forms.NumericUpDown _numFees;
        private System.Windows.Forms.Label _lblFees;
        private System.Windows.Forms.Label _lblTaxAmountValue;
        private System.Windows.Forms.Label _lblTaxAmount;
        private System.Windows.Forms.NumericUpDown _numTax;
        private System.Windows.Forms.Label _lblTax;
        private System.Windows.Forms.Label _lblAfterDiscountValue;
        private System.Windows.Forms.Label _lblAfterDiscount;
        private System.Windows.Forms.TableLayoutPanel _discountEditorPanel;
        private System.Windows.Forms.ComboBox _cmbDiscountMode;
        private System.Windows.Forms.NumericUpDown _numDiscount;
        private System.Windows.Forms.Label _lblDiscountValue;
        private System.Windows.Forms.Label _lblDiscount;
        private System.Windows.Forms.Label _lblSubtotalValue;
        private System.Windows.Forms.Label _lblSubtotal;
        private System.Windows.Forms.FlowLayoutPanel _cartLines;
        private System.Windows.Forms.Label _lblEmptyCart;
        private System.Windows.Forms.Panel _cartHeader;
        private DzentoPOS2.WinForms.Controls.ThemedIconButton _btnClearCart;
        private System.Windows.Forms.Label _lblCart;
        private System.Windows.Forms.TableLayoutPanel _customerRow;
        private DzentoPOS2.WinForms.Controls.ThemedIconButton _btnAddCustomer;
        private System.Windows.Forms.ComboBox _cmbCustomer;
        private System.Windows.Forms.Label _lblCustomer;
        private System.Windows.Forms.TableLayoutPanel _actions;
        private DzentoPOS2.WinForms.Controls.ThemedIconButton _btnCompleteSale;
        private DzentoPOS2.WinForms.Controls.ThemedIconButton _btnPrintLast;
        private DzentoPOS2.WinForms.Controls.ThemedIconButton _btnCalculator;
        private System.Windows.Forms.DataGridView _invoiceGrid;
        private System.Windows.Forms.TableLayoutPanel _invoiceToolbar;
        private DzentoPOS2.WinForms.Controls.ThemedIconButton _btnDeleteInvoice;
        private DzentoPOS2.WinForms.Controls.ThemedIconButton _btnNewSale;
        private EZControls.Controls.Inputs.EZTextBox _invoiceSearch;
        private System.Windows.Forms.ToolTip _toolTip;
    }
}
