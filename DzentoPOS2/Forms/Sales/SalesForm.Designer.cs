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
            components = new System.ComponentModel.Container();
            _tabs = new TabControl();
            _tabNewSale = new TabPage();
            _pageLayout = new TableLayoutPanel();
            _heroStrip = new TableLayoutPanel();
            _heroTotalCard = new Panel();
            _lblHeroTotal = new Label();
            _lblHeroTotalCaption = new Label();
            _discountCard = new Panel();
            _lblHeroDiscount = new Label();
            _lblHeroDiscountCaption = new Label();
            _statsCard = new Panel();
            _lblHeroStats = new Label();
            _saleLayout = new TableLayoutPanel();
            _leftPanel = new Panel();
            _productColumn = new TableLayoutPanel();
            _productHeader = new TableLayoutPanel();
            _lblProductCount = new Label();
            _btnViewToggle = new DzentoPOS2.WinForms.Controls.ThemedIconButton();
            _searchBar = new TableLayoutPanel();
            _productSearch = new EZControls.Controls.Inputs.EZTextBox();
            _btnScanBarcode = new DzentoPOS2.WinForms.Controls.ThemedIconButton();
            _categoryBar = new DzentoPOS2.WinForms.Controls.Pos.CategoryChipBar();
            _scanStrip = new DzentoPOS2.WinForms.Controls.Pos.ScanConfirmationStrip();
            _productList = new DzentoPOS2.WinForms.Controls.Pos.PosProductList();
            _rightPanel = new Panel();
            _basketRoot = new TableLayoutPanel();
            _sessionChip = new DzentoPOS2.WinForms.Controls.Pos.SessionStatusChip();
            _basketScroll = new Panel();
            _basketLayout = new TableLayoutPanel();
            _customerRow = new TableLayoutPanel();
            _lblCustomer = new Label();
            _cmbCustomer = new EZControls.Controls.Inputs.EZComboBox();
            _btnAddCustomer = new DzentoPOS2.WinForms.Controls.ThemedIconButton();
            _cartHeader = new TableLayoutPanel();
            _lblCart = new Label();
            _btnClearCart = new DzentoPOS2.WinForms.Controls.ThemedIconButton();
            _cartLines = new FlowLayoutPanel();
            _lblEmptyCart = new Label();
            _totalsLayout = new TableLayoutPanel();
            _lblDiscount = new Label();
            _lblDiscountValue = new Label();
            _discountEditorPanel = new TableLayoutPanel();
            _numDiscount = new EZControls.Controls.Inputs.EZNumericBox();
            _cmbDiscountMode = new EZControls.Controls.Inputs.EZComboBox();
            _lblSubtotal = new Label();
            _lblSubtotalValue = new Label();
            _lblAfterDiscount = new Label();
            _lblAfterDiscountValue = new Label();
            _lblTax = new Label();
            _numTax = new EZControls.Controls.Inputs.EZNumericBox();
            _lblTaxAmount = new Label();
            _lblTaxAmountValue = new Label();
            _lblFees = new Label();
            _numFees = new EZControls.Controls.Inputs.EZNumericBox();
            _lblTotal = new Label();
            _lblTotalValue = new Label();
            _lblPaid = new Label();
            _paidEditorPanel = new TableLayoutPanel();
            _numPaid = new EZControls.Controls.Inputs.EZNumericBox();
            _btnPayInFull = new DzentoPOS2.WinForms.Controls.ThemedIconButton();
            _lblBalance = new Label();
            _lblBalanceValue = new Label();
            _actions = new TableLayoutPanel();
            _btnCompleteSale = new DzentoPOS2.WinForms.Controls.ThemedIconButton();
            _actionRow = new TableLayoutPanel();
            _btnCard = new DzentoPOS2.WinForms.Controls.ThemedIconButton();
            _btnCredit = new DzentoPOS2.WinForms.Controls.ThemedIconButton();
            _btnHold = new DzentoPOS2.WinForms.Controls.ThemedIconButton();
            _btnCancelSale = new DzentoPOS2.WinForms.Controls.ThemedIconButton();
            _utilityRow = new TableLayoutPanel();
            _btnPrintLast = new DzentoPOS2.WinForms.Controls.ThemedIconButton();
            _btnCalculator = new DzentoPOS2.WinForms.Controls.ThemedIconButton();
            _tabInvoices = new TabPage();
            _invoiceGrid = new DataGridView();
            _lblNoInvoices = new Label();
            _invoiceToolbar = new TableLayoutPanel();
            _invoiceSearch = new EZControls.Controls.Inputs.EZTextBox();
            _btnNewSale = new DzentoPOS2.WinForms.Controls.ThemedIconButton();
            _btnDeleteInvoice = new DzentoPOS2.WinForms.Controls.ThemedIconButton();
            _toolTip = new ToolTip(components);
            _tabs.SuspendLayout();
            _tabNewSale.SuspendLayout();
            _pageLayout.SuspendLayout();
            _heroStrip.SuspendLayout();
            _heroTotalCard.SuspendLayout();
            _discountCard.SuspendLayout();
            _statsCard.SuspendLayout();
            _saleLayout.SuspendLayout();
            _leftPanel.SuspendLayout();
            _productColumn.SuspendLayout();
            _productHeader.SuspendLayout();
            _searchBar.SuspendLayout();
            _rightPanel.SuspendLayout();
            _basketRoot.SuspendLayout();
            _basketScroll.SuspendLayout();
            _basketLayout.SuspendLayout();
            _customerRow.SuspendLayout();
            _cartHeader.SuspendLayout();
            _cartLines.SuspendLayout();
            _totalsLayout.SuspendLayout();
            _discountEditorPanel.SuspendLayout();
            _paidEditorPanel.SuspendLayout();
            _actions.SuspendLayout();
            _actionRow.SuspendLayout();
            _utilityRow.SuspendLayout();
            _tabInvoices.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_invoiceGrid).BeginInit();
            _invoiceToolbar.SuspendLayout();
            SuspendLayout();
            // 
            // _tabs
            // 
            _tabs.Controls.Add(_tabNewSale);
            _tabs.Controls.Add(_tabInvoices);
            _tabs.Dock = DockStyle.Fill;
            _tabs.Font = new Font("Segoe UI", 10F);
            _tabs.Location = new Point(0, 0);
            _tabs.Name = "_tabs";
            _tabs.Padding = new Point(18, 8);
            _tabs.SelectedIndex = 0;
            _tabs.Size = new Size(1440, 820);
            _tabs.SizeMode = TabSizeMode.FillToRight;
            _tabs.TabIndex = 0;
            _tabs.SelectedIndexChanged += OnTabSelected;
            // 
            // _tabNewSale
            // 
            _tabNewSale.BackColor = SystemColors.Control;
            _tabNewSale.Controls.Add(_pageLayout);
            _tabNewSale.Location = new Point(4, 36);
            _tabNewSale.Name = "_tabNewSale";
            _tabNewSale.Padding = new Padding(3);
            _tabNewSale.Size = new Size(1432, 780);
            _tabNewSale.TabIndex = 0;
            _tabNewSale.Text = "New Sale";
            _tabNewSale.Resize += OnSaleTabResized;
            // 
            // _pageLayout
            // 
            _pageLayout.ColumnCount = 1;
            _pageLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _pageLayout.Controls.Add(_heroStrip, 0, 0);
            _pageLayout.Controls.Add(_saleLayout, 0, 1);
            _pageLayout.Dock = DockStyle.Fill;
            _pageLayout.Location = new Point(3, 3);
            _pageLayout.Margin = new Padding(0);
            _pageLayout.Name = "_pageLayout";
            _pageLayout.RowCount = 2;
            _pageLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 110F));
            _pageLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _pageLayout.Size = new Size(1426, 774);
            _pageLayout.TabIndex = 0;
            // 
            // _heroStrip
            // 
            _heroStrip.ColumnCount = 2;
            _heroStrip.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            _heroStrip.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            _heroStrip.Controls.Add(_heroTotalCard, 0, 0);
            _heroStrip.Controls.Add(_discountCard, 1, 0);
            _heroStrip.Controls.Add(_statsCard, 1, 1);
            _heroStrip.Dock = DockStyle.Fill;
            _heroStrip.Location = new Point(0, 0);
            _heroStrip.Margin = new Padding(0);
            _heroStrip.Name = "_heroStrip";
            _heroStrip.Padding = new Padding(12, 4, 12, 4);
            _heroStrip.RowCount = 2;
            _heroStrip.RowStyles.Add(new RowStyle(SizeType.Absolute, 66F));
            _heroStrip.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            _heroStrip.Size = new Size(1426, 110);
            _heroStrip.TabIndex = 0;
            // 
            // _heroTotalCard
            // 
            _heroTotalCard.BackColor = SystemColors.Window;
            _heroTotalCard.Controls.Add(_lblHeroTotal);
            _heroTotalCard.Controls.Add(_lblHeroTotalCaption);
            _heroTotalCard.Dock = DockStyle.Fill;
            _heroTotalCard.Location = new Point(12, 4);
            _heroTotalCard.Margin = new Padding(0, 0, 8, 0);
            _heroTotalCard.Name = "_heroTotalCard";
            _heroTotalCard.Padding = new Padding(10, 6, 10, 6);
            _heroStrip.SetRowSpan(_heroTotalCard, 2);
            _heroTotalCard.Size = new Size(763, 102);
            _heroTotalCard.TabIndex = 0;
            // 
            // _lblHeroTotal
            // 
            _lblHeroTotal.Dock = DockStyle.Fill;
            _lblHeroTotal.Font = new Font("Segoe UI", 38F, FontStyle.Bold);
            _lblHeroTotal.ForeColor = SystemColors.HotTrack;
            _lblHeroTotal.Location = new Point(10, 22);
            _lblHeroTotal.Margin = new Padding(0);
            _lblHeroTotal.Name = "_lblHeroTotal";
            _lblHeroTotal.Size = new Size(743, 74);
            _lblHeroTotal.TabIndex = 1;
            _lblHeroTotal.Text = "0.00";
            _lblHeroTotal.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _lblHeroTotalCaption
            // 
            _lblHeroTotalCaption.Dock = DockStyle.Top;
            _lblHeroTotalCaption.Font = new Font("Segoe UI", 9.5F);
            _lblHeroTotalCaption.ForeColor = SystemColors.GrayText;
            _lblHeroTotalCaption.Location = new Point(10, 6);
            _lblHeroTotalCaption.Margin = new Padding(0);
            _lblHeroTotalCaption.Name = "_lblHeroTotalCaption";
            _lblHeroTotalCaption.Size = new Size(743, 16);
            _lblHeroTotalCaption.TabIndex = 0;
            _lblHeroTotalCaption.Text = "Grand total";
            _lblHeroTotalCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _discountCard
            // 
            _discountCard.BackColor = SystemColors.Window;
            _discountCard.Controls.Add(_lblHeroDiscount);
            _discountCard.Controls.Add(_lblHeroDiscountCaption);
            _discountCard.Dock = DockStyle.Fill;
            _discountCard.Location = new Point(791, 4);
            _discountCard.Margin = new Padding(8, 0, 0, 0);
            _discountCard.Name = "_discountCard";
            _discountCard.Padding = new Padding(10, 2, 10, 2);
            _discountCard.Size = new Size(623, 66);
            _discountCard.TabIndex = 1;
            // 
            // _lblHeroDiscount
            // 
            _lblHeroDiscount.Dock = DockStyle.Fill;
            _lblHeroDiscount.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            _lblHeroDiscount.ForeColor = SystemColors.HotTrack;
            _lblHeroDiscount.Location = new Point(10, 14);
            _lblHeroDiscount.Margin = new Padding(0);
            _lblHeroDiscount.Name = "_lblHeroDiscount";
            _lblHeroDiscount.Size = new Size(603, 50);
            _lblHeroDiscount.TabIndex = 1;
            _lblHeroDiscount.Text = "0.00";
            _lblHeroDiscount.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _lblHeroDiscountCaption
            // 
            _lblHeroDiscountCaption.Dock = DockStyle.Top;
            _lblHeroDiscountCaption.Font = new Font("Segoe UI", 8.5F);
            _lblHeroDiscountCaption.ForeColor = SystemColors.GrayText;
            _lblHeroDiscountCaption.Location = new Point(10, 2);
            _lblHeroDiscountCaption.Margin = new Padding(0);
            _lblHeroDiscountCaption.Name = "_lblHeroDiscountCaption";
            _lblHeroDiscountCaption.Size = new Size(603, 12);
            _lblHeroDiscountCaption.TabIndex = 0;
            _lblHeroDiscountCaption.Text = "Discount";
            _lblHeroDiscountCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _statsCard
            // 
            _statsCard.BackColor = SystemColors.Window;
            _statsCard.Controls.Add(_lblHeroStats);
            _statsCard.Dock = DockStyle.Fill;
            _statsCard.Location = new Point(791, 70);
            _statsCard.Margin = new Padding(8, 0, 0, 0);
            _statsCard.Name = "_statsCard";
            _statsCard.Padding = new Padding(10, 0, 10, 0);
            _statsCard.Size = new Size(623, 36);
            _statsCard.TabIndex = 2;
            // 
            // _lblHeroStats
            // 
            _lblHeroStats.Dock = DockStyle.Fill;
            _lblHeroStats.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            _lblHeroStats.ForeColor = SystemColors.ControlText;
            _lblHeroStats.Location = new Point(10, 0);
            _lblHeroStats.Margin = new Padding(0);
            _lblHeroStats.Name = "_lblHeroStats";
            _lblHeroStats.Size = new Size(603, 36);
            _lblHeroStats.TabIndex = 0;
            _lblHeroStats.Text = "0 items";
            _lblHeroStats.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _saleLayout
            // 
            _saleLayout.ColumnCount = 2;
            _saleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _saleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 460F));
            _saleLayout.Controls.Add(_leftPanel, 0, 0);
            _saleLayout.Controls.Add(_rightPanel, 1, 0);
            _saleLayout.Dock = DockStyle.Fill;
            _saleLayout.Location = new Point(0, 110);
            _saleLayout.Margin = new Padding(0);
            _saleLayout.Name = "_saleLayout";
            _saleLayout.RowCount = 1;
            _saleLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _saleLayout.Size = new Size(1426, 664);
            _saleLayout.TabIndex = 1;
            // 
            // _leftPanel
            // 
            _leftPanel.Controls.Add(_productColumn);
            _leftPanel.Dock = DockStyle.Fill;
            _leftPanel.Location = new Point(0, 0);
            _leftPanel.Margin = new Padding(0);
            _leftPanel.Name = "_leftPanel";
            _leftPanel.Padding = new Padding(12);
            _leftPanel.Size = new Size(966, 664);
            _leftPanel.TabIndex = 0;
            _leftPanel.Tag = "background";
            // 
            // _productColumn
            // 
            _productColumn.ColumnCount = 1;
            _productColumn.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _productColumn.Controls.Add(_productHeader, 0, 0);
            _productColumn.Controls.Add(_searchBar, 0, 1);
            _productColumn.Controls.Add(_categoryBar, 0, 2);
            _productColumn.Controls.Add(_scanStrip, 0, 3);
            _productColumn.Controls.Add(_productList, 0, 4);
            _productColumn.Dock = DockStyle.Fill;
            _productColumn.Location = new Point(12, 12);
            _productColumn.Margin = new Padding(0);
            _productColumn.Name = "_productColumn";
            _productColumn.RowCount = 5;
            _productColumn.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            _productColumn.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            _productColumn.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            _productColumn.RowStyles.Add(new RowStyle());
            _productColumn.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _productColumn.Size = new Size(942, 640);
            _productColumn.TabIndex = 0;
            // 
            // _productHeader
            // 
            _productHeader.ColumnCount = 2;
            _productHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _productHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));
            _productHeader.Controls.Add(_lblProductCount, 0, 0);
            _productHeader.Controls.Add(_btnViewToggle, 1, 0);
            _productHeader.Dock = DockStyle.Fill;
            _productHeader.Location = new Point(0, 0);
            _productHeader.Margin = new Padding(0, 0, 0, 4);
            _productHeader.Name = "_productHeader";
            _productHeader.RowCount = 1;
            _productHeader.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _productHeader.Size = new Size(942, 26);
            _productHeader.TabIndex = 0;
            // 
            // _lblProductCount
            // 
            _lblProductCount.Dock = DockStyle.Fill;
            _lblProductCount.Font = new Font("Segoe UI", 9.5F);
            _lblProductCount.ForeColor = SystemColors.GrayText;
            _lblProductCount.Location = new Point(0, 0);
            _lblProductCount.Margin = new Padding(0);
            _lblProductCount.Name = "_lblProductCount";
            _lblProductCount.Size = new Size(902, 26);
            _lblProductCount.TabIndex = 0;
            _lblProductCount.Text = "0 products";
            _lblProductCount.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _btnViewToggle
            // 
            _btnViewToggle.BackColor = SystemColors.Control;
            _btnViewToggle.Cursor = Cursors.Hand;
            _btnViewToggle.Dock = DockStyle.Fill;
            _btnViewToggle.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _btnViewToggle.FlatStyle = FlatStyle.Flat;
            _btnViewToggle.Font = new Font("Segoe UI Semibold", 10F);
            _btnViewToggle.IconChar = FontAwesome.Sharp.IconChar.TableCells;
            _btnViewToggle.IconColor = SystemColors.ControlText;
            _btnViewToggle.IconFont = FontAwesome.Sharp.IconFont.Auto;
            _btnViewToggle.IconSize = 14;
            _btnViewToggle.Location = new Point(902, 0);
            _btnViewToggle.Margin = new Padding(0);
            _btnViewToggle.Name = "_btnViewToggle";
            _btnViewToggle.Size = new Size(40, 26);
            _btnViewToggle.Solid = false;
            _btnViewToggle.TabIndex = 1;
            _btnViewToggle.TextImageRelation = TextImageRelation.ImageBeforeText;
            _btnViewToggle.Tone = WinForms.Controls.ButtonTone.Neutral;
            _btnViewToggle.UseVisualStyleBackColor = false;
            _btnViewToggle.Click += OnViewModeToggleClick;
            // 
            // _searchBar
            // 
            _searchBar.ColumnCount = 2;
            _searchBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _searchBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            _searchBar.Controls.Add(_productSearch, 0, 0);
            _searchBar.Controls.Add(_btnScanBarcode, 1, 0);
            _searchBar.Dock = DockStyle.Fill;
            _searchBar.Location = new Point(0, 30);
            _searchBar.Margin = new Padding(0);
            _searchBar.Name = "_searchBar";
            _searchBar.RowCount = 1;
            _searchBar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _searchBar.Size = new Size(942, 48);
            _searchBar.TabIndex = 1;
            // 
            // _productSearch
            // 
            _productSearch.BackColor = SystemColors.Window;
            _productSearch.BorderColor = SystemColors.ControlDark;
            _productSearch.BorderErrorColor = Color.Firebrick;
            _productSearch.BorderFocusColor = SystemColors.Highlight;
            _productSearch.BorderRadius = 10;
            _productSearch.BorderSize = 2;
            _productSearch.Dock = DockStyle.Fill;
            _productSearch.Font = new Font("Segoe UI", 11F);
            _productSearch.ForeColor = SystemColors.WindowText;
            _productSearch.Location = new Point(0, 0);
            _productSearch.Margin = new Padding(0, 0, 8, 0);
            _productSearch.Name = "_productSearch";
            _productSearch.Padding = new Padding(10, 6, 10, 6);
            _productSearch.PlaceholderColor = SystemColors.GrayText;
            _productSearch.PlaceholderText = "Search products or scan a barcode (F2)";
            _productSearch.Size = new Size(784, 48);
            _productSearch.TabIndex = 0;
            _productSearch.TextChanged += OnProductSearchTextChanged;
            _productSearch.KeyDown += OnProductSearchKeyDown;
            // 
            // _btnScanBarcode
            // 
            _btnScanBarcode.BackColor = SystemColors.Control;
            _btnScanBarcode.Cursor = Cursors.Hand;
            _btnScanBarcode.Dock = DockStyle.Fill;
            _btnScanBarcode.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _btnScanBarcode.FlatStyle = FlatStyle.Flat;
            _btnScanBarcode.Font = new Font("Segoe UI Semibold", 10F);
            _btnScanBarcode.ForeColor = SystemColors.ControlText;
            _btnScanBarcode.IconChar = FontAwesome.Sharp.IconChar.Barcode;
            _btnScanBarcode.IconColor = SystemColors.ControlText;
            _btnScanBarcode.IconFont = FontAwesome.Sharp.IconFont.Auto;
            _btnScanBarcode.IconSize = 17;
            _btnScanBarcode.Location = new Point(792, 0);
            _btnScanBarcode.Margin = new Padding(0);
            _btnScanBarcode.Name = "_btnScanBarcode";
            _btnScanBarcode.Size = new Size(150, 48);
            _btnScanBarcode.Solid = false;
            _btnScanBarcode.TabIndex = 1;
            _btnScanBarcode.Text = "  Scan";
            _btnScanBarcode.TextImageRelation = TextImageRelation.ImageBeforeText;
            _btnScanBarcode.Tone = WinForms.Controls.ButtonTone.Primary;
            _btnScanBarcode.UseVisualStyleBackColor = false;
            _btnScanBarcode.Click += OnScanBarcodeClick;
            // 
            // _categoryBar
            // 
            _categoryBar.BackColor = Color.Transparent;
            _categoryBar.Dock = DockStyle.Fill;
            _categoryBar.Font = new Font("Segoe UI", 10F);
            _categoryBar.Location = new Point(0, 78);
            _categoryBar.Margin = new Padding(0, 0, 0, 8);
            _categoryBar.Name = "_categoryBar";
            _categoryBar.Size = new Size(942, 36);
            _categoryBar.TabIndex = 2;
            _categoryBar.CategorySelected += OnCategorySelected;
            // 
            // _scanStrip
            // 
            _scanStrip.BackColor = Color.FromArgb(234, 246, 237);
            _scanStrip.Dock = DockStyle.Fill;
            _scanStrip.Location = new Point(0, 122);
            _scanStrip.Margin = new Padding(0, 0, 0, 6);
            _scanStrip.MinimumSize = new Size(0, 46);
            _scanStrip.Name = "_scanStrip";
            _scanStrip.Padding = new Padding(1);
            _scanStrip.Size = new Size(942, 46);
            _scanStrip.TabIndex = 3;
            _scanStrip.Visible = false;
            // 
            // _productList
            // 
            _productList.BackColor = Color.FromArgb(245, 247, 250);
            _productList.Dock = DockStyle.Fill;
            _productList.ForeColor = Color.FromArgb(29, 29, 31);
            _productList.Location = new Point(0, 174);
            _productList.Margin = new Padding(0);
            _productList.Name = "_productList";
            _productList.ScrollOffset = 0;
            _productList.Size = new Size(942, 466);
            _productList.TabIndex = 4;
            _productList.ViewMode = App.Services.Sales.PosViewMode.Cards;
            _productList.ItemDoubleClicked += OnProductItemActivated;
            // 
            // _rightPanel
            // 
            _rightPanel.Controls.Add(_basketRoot);
            _rightPanel.Dock = DockStyle.Fill;
            _rightPanel.Location = new Point(966, 0);
            _rightPanel.Margin = new Padding(0);
            _rightPanel.Name = "_rightPanel";
            _rightPanel.Padding = new Padding(12);
            _rightPanel.Size = new Size(460, 664);
            _rightPanel.TabIndex = 1;
            // 
            // _basketRoot
            // 
            _basketRoot.ColumnCount = 1;
            _basketRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _basketRoot.Controls.Add(_sessionChip, 0, 0);
            _basketRoot.Controls.Add(_basketScroll, 0, 1);
            _basketRoot.Controls.Add(_actions, 0, 2);
            _basketRoot.Dock = DockStyle.Fill;
            _basketRoot.Location = new Point(12, 12);
            _basketRoot.Margin = new Padding(0);
            _basketRoot.Name = "_basketRoot";
            _basketRoot.RowCount = 3;
            _basketRoot.RowStyles.Add(new RowStyle());
            _basketRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _basketRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 136F));
            _basketRoot.Size = new Size(436, 640);
            _basketRoot.TabIndex = 0;
            // 
            // _sessionChip
            // 
            _sessionChip.BackColor = Color.FromArgb(243, 243, 244);
            _sessionChip.Dock = DockStyle.Fill;
            _sessionChip.Location = new Point(0, 0);
            _sessionChip.Margin = new Padding(0, 0, 0, 8);
            _sessionChip.MinimumSize = new Size(0, 38);
            _sessionChip.Name = "_sessionChip";
            _sessionChip.Padding = new Padding(10, 4, 10, 4);
            _sessionChip.Size = new Size(436, 38);
            _sessionChip.TabIndex = 0;
            _sessionChip.ActionClicked += OnSessionActionClicked;
            // 
            // _basketScroll
            // 
            _basketScroll.AutoScroll = true;
            _basketScroll.Controls.Add(_basketLayout);
            _basketScroll.Dock = DockStyle.Fill;
            _basketScroll.Location = new Point(0, 46);
            _basketScroll.Margin = new Padding(0);
            _basketScroll.Name = "_basketScroll";
            _basketScroll.Size = new Size(436, 458);
            _basketScroll.TabIndex = 1;
            _basketScroll.ClientSizeChanged += OnBasketScrollSizeChanged;
            // 
            // _basketLayout
            // 
            _basketLayout.AutoSize = true;
            _basketLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _basketLayout.ColumnCount = 1;
            _basketLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _basketLayout.Controls.Add(_customerRow, 0, 0);
            _basketLayout.Controls.Add(_cartHeader, 0, 1);
            _basketLayout.Controls.Add(_cartLines, 0, 2);
            _basketLayout.Controls.Add(_totalsLayout, 0, 3);
            _basketLayout.Dock = DockStyle.Top;
            _basketLayout.Location = new Point(0, 0);
            _basketLayout.Margin = new Padding(0);
            _basketLayout.Name = "_basketLayout";
            _basketLayout.RowCount = 4;
            _basketLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            _basketLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            _basketLayout.RowStyles.Add(new RowStyle());
            _basketLayout.RowStyles.Add(new RowStyle());
            _basketLayout.Size = new Size(436, 446);
            _basketLayout.TabIndex = 0;
            // 
            // _customerRow
            // 
            _customerRow.ColumnCount = 3;
            _customerRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 26F));
            _customerRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 74F));
            _customerRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));
            _customerRow.Controls.Add(_lblCustomer, 0, 0);
            _customerRow.Controls.Add(_cmbCustomer, 1, 0);
            _customerRow.Controls.Add(_btnAddCustomer, 2, 0);
            _customerRow.Dock = DockStyle.Fill;
            _customerRow.Location = new Point(0, 0);
            _customerRow.Margin = new Padding(0);
            _customerRow.Name = "_customerRow";
            _customerRow.RowCount = 1;
            _customerRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _customerRow.Size = new Size(436, 44);
            _customerRow.TabIndex = 0;
            // 
            // _lblCustomer
            // 
            _lblCustomer.AutoEllipsis = true;
            _lblCustomer.Dock = DockStyle.Fill;
            _lblCustomer.Font = new Font("Segoe UI", 9.5F);
            _lblCustomer.ForeColor = SystemColors.ControlText;
            _lblCustomer.Location = new Point(0, 0);
            _lblCustomer.Margin = new Padding(0);
            _lblCustomer.Name = "_lblCustomer";
            _lblCustomer.Size = new Size(102, 44);
            _lblCustomer.TabIndex = 0;
            _lblCustomer.Text = "Customer:";
            _lblCustomer.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _cmbCustomer
            // 
            _cmbCustomer.BackColor = SystemColors.Window;
            _cmbCustomer.BorderRadius = 10;
            _cmbCustomer.BorderSize = 2;
            _cmbCustomer.Dock = DockStyle.Fill;
            _cmbCustomer.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbCustomer.Font = new Font("Segoe UI", 10F);
            _cmbCustomer.ForeColor = SystemColors.WindowText;
            _cmbCustomer.ItemHeight = 36;
            _cmbCustomer.Location = new Point(102, 0);
            _cmbCustomer.Margin = new Padding(0, 2, 6, 2);
            _cmbCustomer.MaxDropDownItems = 6;
            _cmbCustomer.Name = "_cmbCustomer";
            _cmbCustomer.Size = new Size(287, 40);
            _cmbCustomer.TabIndex = 1;
            _cmbCustomer.AccessibleControlName = "Customer";
            // 
            // _btnAddCustomer
            // 
            _btnAddCustomer.BackColor = SystemColors.Control;
            _btnAddCustomer.Cursor = Cursors.Hand;
            _btnAddCustomer.Dock = DockStyle.Fill;
            _btnAddCustomer.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _btnAddCustomer.FlatStyle = FlatStyle.Flat;
            _btnAddCustomer.Font = new Font("Segoe UI Semibold", 10F);
            _btnAddCustomer.IconChar = FontAwesome.Sharp.IconChar.UserPlus;
            _btnAddCustomer.IconColor = SystemColors.ControlText;
            _btnAddCustomer.IconFont = FontAwesome.Sharp.IconFont.Auto;
            _btnAddCustomer.IconSize = 16;
            _btnAddCustomer.Location = new Point(395, 0);
            _btnAddCustomer.Margin = new Padding(0);
            _btnAddCustomer.Name = "_btnAddCustomer";
            _btnAddCustomer.Size = new Size(41, 44);
            _btnAddCustomer.Solid = false;
            _btnAddCustomer.TabIndex = 2;
            _btnAddCustomer.TextImageRelation = TextImageRelation.ImageBeforeText;
            _btnAddCustomer.Tone = WinForms.Controls.ButtonTone.Neutral;
            _btnAddCustomer.UseVisualStyleBackColor = false;
            _btnAddCustomer.Click += OnAddCustomerClick;
            // 
            // _cartHeader
            // 
            _cartHeader.ColumnCount = 2;
            _cartHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _cartHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            _cartHeader.Controls.Add(_lblCart, 0, 0);
            _cartHeader.Controls.Add(_btnClearCart, 1, 0);
            _cartHeader.Dock = DockStyle.Fill;
            _cartHeader.Location = new Point(0, 44);
            _cartHeader.Margin = new Padding(0);
            _cartHeader.Name = "_cartHeader";
            _cartHeader.RowCount = 1;
            _cartHeader.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _cartHeader.Size = new Size(436, 36);
            _cartHeader.TabIndex = 1;
            // 
            // _lblCart
            // 
            _lblCart.Dock = DockStyle.Fill;
            _lblCart.Font = new Font("Segoe UI Semibold", 10.5F);
            _lblCart.ForeColor = SystemColors.ControlText;
            _lblCart.Location = new Point(0, 0);
            _lblCart.Margin = new Padding(0, 0, 0, 4);
            _lblCart.Name = "_lblCart";
            _lblCart.Size = new Size(336, 32);
            _lblCart.TabIndex = 0;
            _lblCart.Text = "Cart";
            _lblCart.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _btnClearCart
            // 
            _btnClearCart.BackColor = SystemColors.Control;
            _btnClearCart.Cursor = Cursors.Hand;
            _btnClearCart.Dock = DockStyle.Fill;
            _btnClearCart.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _btnClearCart.FlatStyle = FlatStyle.Flat;
            _btnClearCart.Font = new Font("Segoe UI Semibold", 9F);
            _btnClearCart.ForeColor = SystemColors.ControlText;
            _btnClearCart.IconChar = FontAwesome.Sharp.IconChar.Trash;
            _btnClearCart.IconColor = SystemColors.ControlText;
            _btnClearCart.IconFont = FontAwesome.Sharp.IconFont.Auto;
            _btnClearCart.IconSize = 13;
            _btnClearCart.Location = new Point(336, 0);
            _btnClearCart.Margin = new Padding(0, 0, 0, 4);
            _btnClearCart.Name = "_btnClearCart";
            _btnClearCart.Size = new Size(100, 32);
            _btnClearCart.Solid = false;
            _btnClearCart.TabIndex = 1;
            _btnClearCart.Text = "  Clear";
            _btnClearCart.TextImageRelation = TextImageRelation.ImageBeforeText;
            _btnClearCart.Tone = WinForms.Controls.ButtonTone.Neutral;
            _btnClearCart.UseVisualStyleBackColor = false;
            _btnClearCart.Click += OnClearCartClick;
            // 
            // _cartLines
            // 
            _cartLines.AutoSize = true;
            _cartLines.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _cartLines.Controls.Add(_lblEmptyCart);
            _cartLines.Dock = DockStyle.Fill;
            _cartLines.FlowDirection = FlowDirection.TopDown;
            _cartLines.Location = new Point(0, 80);
            _cartLines.Margin = new Padding(0);
            _cartLines.Name = "_cartLines";
            _cartLines.Size = new Size(436, 56);
            _cartLines.TabIndex = 2;
            _cartLines.WrapContents = false;
            _cartLines.SizeChanged += OnCartLinesSizeChanged;
            // 
            // _lblEmptyCart
            // 
            _lblEmptyCart.Font = new Font("Segoe UI", 10F);
            _lblEmptyCart.ForeColor = SystemColors.GrayText;
            _lblEmptyCart.Location = new Point(0, 0);
            _lblEmptyCart.Margin = new Padding(0);
            _lblEmptyCart.Name = "_lblEmptyCart";
            _lblEmptyCart.Size = new Size(436, 56);
            _lblEmptyCart.TabIndex = 0;
            _lblEmptyCart.Text = "The cart is empty. Scan a barcode or pick a product.";
            _lblEmptyCart.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // _totalsLayout
            // 
            _totalsLayout.AutoSize = true;
            _totalsLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _totalsLayout.ColumnCount = 2;
            _totalsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
            _totalsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));
            _totalsLayout.Controls.Add(_lblDiscount, 0, 0);
            _totalsLayout.Controls.Add(_lblDiscountValue, 1, 0);
            _totalsLayout.Controls.Add(_discountEditorPanel, 0, 1);
            _totalsLayout.Controls.Add(_lblSubtotal, 0, 2);
            _totalsLayout.Controls.Add(_lblSubtotalValue, 1, 2);
            _totalsLayout.Controls.Add(_lblAfterDiscount, 0, 3);
            _totalsLayout.Controls.Add(_lblAfterDiscountValue, 1, 3);
            _totalsLayout.Controls.Add(_lblTax, 0, 4);
            _totalsLayout.Controls.Add(_numTax, 1, 4);
            _totalsLayout.Controls.Add(_lblTaxAmount, 0, 5);
            _totalsLayout.Controls.Add(_lblTaxAmountValue, 1, 5);
            _totalsLayout.Controls.Add(_lblFees, 0, 6);
            _totalsLayout.Controls.Add(_numFees, 1, 6);
            _totalsLayout.Controls.Add(_lblTotal, 0, 7);
            _totalsLayout.Controls.Add(_lblTotalValue, 1, 7);
            _totalsLayout.Controls.Add(_lblPaid, 0, 8);
            _totalsLayout.Controls.Add(_paidEditorPanel, 1, 8);
            _totalsLayout.Controls.Add(_lblBalance, 0, 9);
            _totalsLayout.Controls.Add(_lblBalanceValue, 1, 9);
            _totalsLayout.Dock = DockStyle.Top;
            _totalsLayout.Location = new Point(0, 142);
            _totalsLayout.Margin = new Padding(0, 6, 0, 0);
            _totalsLayout.Name = "_totalsLayout";
            _totalsLayout.Padding = new Padding(0, 4, 0, 0);
            _totalsLayout.RowCount = 10;
            //  Row 0 discount label | 1 discount editor | 2 subtotal
            //  3 after discount | 4 tax rate | 5 tax amount | 6 fees
            //  7 total | 8 paid | 9 balance.  Editor rows are 40 px so the
            //  styled inputs keep the unified 40 px height; label rows are
            //  trimmed to 26 px to pay for the extra input height.
            _totalsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            _totalsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            _totalsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            _totalsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            _totalsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            _totalsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            _totalsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            _totalsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            _totalsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            _totalsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            _totalsLayout.Size = new Size(436, 324);
            _totalsLayout.TabIndex = 3;
            // 
            // _lblDiscount
            // 
            _lblDiscount.Dock = DockStyle.Fill;
            _lblDiscount.Font = new Font("Segoe UI", 9.5F);
            _lblDiscount.ForeColor = SystemColors.ControlText;
            _lblDiscount.Location = new Point(0, 4);
            _lblDiscount.Margin = new Padding(0);
            _lblDiscount.Name = "_lblDiscount";
            _lblDiscount.Padding = new Padding(6, 0, 6, 0);
            _lblDiscount.Size = new Size(183, 30);
            _lblDiscount.TabIndex = 0;
            _lblDiscount.Text = "Discount:";
            _lblDiscount.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _lblDiscountValue
            // 
            _lblDiscountValue.Dock = DockStyle.Fill;
            _lblDiscountValue.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _lblDiscountValue.ForeColor = SystemColors.ControlText;
            _lblDiscountValue.Location = new Point(183, 4);
            _lblDiscountValue.Margin = new Padding(0);
            _lblDiscountValue.Name = "_lblDiscountValue";
            _lblDiscountValue.Padding = new Padding(6, 0, 6, 0);
            _lblDiscountValue.Size = new Size(253, 30);
            _lblDiscountValue.TabIndex = 1;
            _lblDiscountValue.Text = "0.00";
            _lblDiscountValue.TextAlign = ContentAlignment.MiddleRight;
            // 
            // _discountEditorPanel
            // 
            _discountEditorPanel.ColumnCount = 2;
            _totalsLayout.SetColumnSpan(_discountEditorPanel, 2);
            _discountEditorPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62F));
            _discountEditorPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));
            _discountEditorPanel.Controls.Add(_numDiscount, 0, 0);
            _discountEditorPanel.Controls.Add(_cmbDiscountMode, 1, 0);
            _discountEditorPanel.Dock = DockStyle.Fill;
            _discountEditorPanel.Location = new Point(0, 34);
            _discountEditorPanel.Margin = new Padding(0);
            _discountEditorPanel.Name = "_discountEditorPanel";
            _discountEditorPanel.RowCount = 1;
            _discountEditorPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _discountEditorPanel.Size = new Size(436, 40);
            _discountEditorPanel.TabIndex = 1;
            // 
            // _numDiscount
            // 
            _numDiscount.BackColor = SystemColors.Window;
            _numDiscount.BorderRadius = 10;
            _numDiscount.BorderSize = 2;
            _numDiscount.DecimalPlaces = 2;
            _numDiscount.Dock = DockStyle.Fill;
            _numDiscount.Font = new Font("Segoe UI", 10F);
            _numDiscount.ForeColor = SystemColors.WindowText;
            _numDiscount.Location = new Point(6, 0);
            _numDiscount.Margin = new Padding(6, 0, 4, 0);
            _numDiscount.Maximum = new decimal(new int[] { 99999999, 0, 0, 0 });
            _numDiscount.Name = "_numDiscount";
            _numDiscount.Size = new Size(260, 40);
            _numDiscount.TabIndex = 0;
            _numDiscount.TextAlign = HorizontalAlignment.Right;
            _numDiscount.AccessibleName = "Discount";
            _numDiscount.ValueChanged += OnDiscountValueChanged;
            // 
            // _cmbDiscountMode
            // 
            _cmbDiscountMode.BackColor = SystemColors.Window;
            _cmbDiscountMode.BorderRadius = 10;
            _cmbDiscountMode.BorderSize = 2;
            _cmbDiscountMode.Dock = DockStyle.Fill;
            _cmbDiscountMode.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbDiscountMode.Font = new Font("Segoe UI", 9F);
            _cmbDiscountMode.ForeColor = SystemColors.WindowText;
            _cmbDiscountMode.ItemHeight = 36;
            _cmbDiscountMode.Location = new Point(272, 0);
            _cmbDiscountMode.Margin = new Padding(2, 0, 6, 0);
            _cmbDiscountMode.MaxDropDownItems = 4;
            _cmbDiscountMode.Name = "_cmbDiscountMode";
            _cmbDiscountMode.Size = new Size(158, 40);
            _cmbDiscountMode.TabIndex = 1;
            _cmbDiscountMode.AccessibleControlName = "Discount mode";
            _cmbDiscountMode.SelectedIndexChanged += OnDiscountModeChanged;
            // 
            // _lblSubtotal
            // 
            _lblSubtotal.Dock = DockStyle.Fill;
            _lblSubtotal.Font = new Font("Segoe UI", 9.5F);
            _lblSubtotal.ForeColor = SystemColors.ControlText;
            _lblSubtotal.Location = new Point(0, 64);
            _lblSubtotal.Margin = new Padding(0);
            _lblSubtotal.Name = "_lblSubtotal";
            _lblSubtotal.Padding = new Padding(6, 0, 6, 0);
            _lblSubtotal.Size = new Size(183, 30);
            _lblSubtotal.TabIndex = 2;
            _lblSubtotal.Text = "Subtotal:";
            _lblSubtotal.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _lblSubtotalValue
            // 
            _lblSubtotalValue.Dock = DockStyle.Fill;
            _lblSubtotalValue.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _lblSubtotalValue.ForeColor = SystemColors.ControlText;
            _lblSubtotalValue.Location = new Point(183, 64);
            _lblSubtotalValue.Margin = new Padding(0);
            _lblSubtotalValue.Name = "_lblSubtotalValue";
            _lblSubtotalValue.Padding = new Padding(6, 0, 6, 0);
            _lblSubtotalValue.Size = new Size(253, 30);
            _lblSubtotalValue.TabIndex = 3;
            _lblSubtotalValue.Text = "0.00";
            _lblSubtotalValue.TextAlign = ContentAlignment.MiddleRight;
            // 
            // _lblAfterDiscount
            // 
            _lblAfterDiscount.Dock = DockStyle.Fill;
            _lblAfterDiscount.Font = new Font("Segoe UI", 9.5F);
            _lblAfterDiscount.ForeColor = SystemColors.ControlText;
            _lblAfterDiscount.Location = new Point(0, 94);
            _lblAfterDiscount.Margin = new Padding(0);
            _lblAfterDiscount.Name = "_lblAfterDiscount";
            _lblAfterDiscount.Padding = new Padding(6, 0, 6, 0);
            _lblAfterDiscount.Size = new Size(183, 30);
            _lblAfterDiscount.TabIndex = 4;
            _lblAfterDiscount.Text = "After discount:";
            _lblAfterDiscount.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _lblAfterDiscountValue
            // 
            _lblAfterDiscountValue.Dock = DockStyle.Fill;
            _lblAfterDiscountValue.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _lblAfterDiscountValue.ForeColor = SystemColors.ControlText;
            _lblAfterDiscountValue.Location = new Point(183, 94);
            _lblAfterDiscountValue.Margin = new Padding(0);
            _lblAfterDiscountValue.Name = "_lblAfterDiscountValue";
            _lblAfterDiscountValue.Padding = new Padding(6, 0, 6, 0);
            _lblAfterDiscountValue.Size = new Size(253, 30);
            _lblAfterDiscountValue.TabIndex = 5;
            _lblAfterDiscountValue.Text = "0.00";
            _lblAfterDiscountValue.TextAlign = ContentAlignment.MiddleRight;
            // 
            // _lblTax
            // 
            _lblTax.Dock = DockStyle.Fill;
            _lblTax.Font = new Font("Segoe UI", 9.5F);
            _lblTax.ForeColor = SystemColors.ControlText;
            _lblTax.Location = new Point(0, 124);
            _lblTax.Margin = new Padding(0);
            _lblTax.Name = "_lblTax";
            _lblTax.Padding = new Padding(6, 0, 6, 0);
            _lblTax.Size = new Size(183, 30);
            _lblTax.TabIndex = 6;
            _lblTax.Text = "Tax rate (%):";
            _lblTax.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _numTax
            // 
            _numTax.BackColor = SystemColors.Window;
            _numTax.BorderRadius = 10;
            _numTax.BorderSize = 2;
            _numTax.DecimalPlaces = 2;
            _numTax.Dock = DockStyle.Fill;
            _numTax.Font = new Font("Segoe UI", 10F);
            _numTax.ForeColor = SystemColors.WindowText;
            _numTax.Location = new Point(189, 126);
            _numTax.Margin = new Padding(6, 0, 6, 0);
            _numTax.Name = "_numTax";
            _numTax.Size = new Size(241, 40);
            _numTax.TabIndex = 7;
            _numTax.TextAlign = HorizontalAlignment.Right;
            _numTax.AccessibleName = "Tax rate";
            _numTax.ValueChanged += OnTaxValueChanged;
            // 
            // _lblTaxAmount
            // 
            _lblTaxAmount.Dock = DockStyle.Fill;
            _lblTaxAmount.Font = new Font("Segoe UI", 9.5F);
            _lblTaxAmount.ForeColor = SystemColors.ControlText;
            _lblTaxAmount.Location = new Point(0, 154);
            _lblTaxAmount.Margin = new Padding(0);
            _lblTaxAmount.Name = "_lblTaxAmount";
            _lblTaxAmount.Padding = new Padding(6, 0, 6, 0);
            _lblTaxAmount.Size = new Size(183, 30);
            _lblTaxAmount.TabIndex = 8;
            _lblTaxAmount.Text = "Tax amount:";
            _lblTaxAmount.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _lblTaxAmountValue
            // 
            _lblTaxAmountValue.Dock = DockStyle.Fill;
            _lblTaxAmountValue.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _lblTaxAmountValue.ForeColor = SystemColors.ControlText;
            _lblTaxAmountValue.Location = new Point(183, 154);
            _lblTaxAmountValue.Margin = new Padding(0);
            _lblTaxAmountValue.Name = "_lblTaxAmountValue";
            _lblTaxAmountValue.Padding = new Padding(6, 0, 6, 0);
            _lblTaxAmountValue.Size = new Size(253, 30);
            _lblTaxAmountValue.TabIndex = 9;
            _lblTaxAmountValue.Text = "0.00";
            _lblTaxAmountValue.TextAlign = ContentAlignment.MiddleRight;
            // 
            // _lblFees
            // 
            _lblFees.Dock = DockStyle.Fill;
            _lblFees.Font = new Font("Segoe UI", 9.5F);
            _lblFees.ForeColor = SystemColors.ControlText;
            _lblFees.Location = new Point(0, 184);
            _lblFees.Margin = new Padding(0);
            _lblFees.Name = "_lblFees";
            _lblFees.Padding = new Padding(6, 0, 6, 0);
            _lblFees.Size = new Size(183, 30);
            _lblFees.TabIndex = 10;
            _lblFees.Text = "Fees:";
            _lblFees.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _numFees
            // 
            _numFees.BackColor = SystemColors.Window;
            _numFees.BorderRadius = 10;
            _numFees.BorderSize = 2;
            _numFees.DecimalPlaces = 2;
            _numFees.Dock = DockStyle.Fill;
            _numFees.Font = new Font("Segoe UI", 10F);
            _numFees.ForeColor = SystemColors.WindowText;
            _numFees.Location = new Point(189, 186);
            _numFees.Margin = new Padding(6, 0, 6, 0);
            _numFees.Maximum = new decimal(new int[] { 99999999, 0, 0, 0 });
            _numFees.Name = "_numFees";
            _numFees.Size = new Size(241, 40);
            _numFees.TabIndex = 11;
            _numFees.TextAlign = HorizontalAlignment.Right;
            _numFees.AccessibleName = "Fees";
            _numFees.ValueChanged += OnFeesValueChanged;
            // 
            // _lblTotal
            // 
            _lblTotal.Dock = DockStyle.Fill;
            _lblTotal.Font = new Font("Segoe UI Semibold", 10F);
            _lblTotal.ForeColor = SystemColors.ControlText;
            _lblTotal.Location = new Point(0, 214);
            _lblTotal.Margin = new Padding(0);
            _lblTotal.Name = "_lblTotal";
            _lblTotal.Padding = new Padding(6, 0, 6, 0);
            _lblTotal.Size = new Size(183, 30);
            _lblTotal.TabIndex = 12;
            _lblTotal.Text = "Total:";
            _lblTotal.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _lblTotalValue
            // 
            _lblTotalValue.Dock = DockStyle.Fill;
            _lblTotalValue.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            _lblTotalValue.ForeColor = SystemColors.HotTrack;
            _lblTotalValue.Location = new Point(183, 214);
            _lblTotalValue.Margin = new Padding(0);
            _lblTotalValue.Name = "_lblTotalValue";
            _lblTotalValue.Padding = new Padding(6, 0, 6, 0);
            _lblTotalValue.Size = new Size(253, 30);
            _lblTotalValue.TabIndex = 13;
            _lblTotalValue.Text = "0.00";
            _lblTotalValue.TextAlign = ContentAlignment.MiddleRight;
            // 
            // _lblPaid
            // 
            _lblPaid.Dock = DockStyle.Fill;
            _lblPaid.Font = new Font("Segoe UI", 9.5F);
            _lblPaid.ForeColor = SystemColors.ControlText;
            _lblPaid.Location = new Point(0, 244);
            _lblPaid.Margin = new Padding(0);
            _lblPaid.Name = "_lblPaid";
            _lblPaid.Padding = new Padding(6, 0, 6, 0);
            _lblPaid.Size = new Size(183, 30);
            _lblPaid.TabIndex = 14;
            _lblPaid.Text = "Paid:";
            _lblPaid.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _paidEditorPanel
            // 
            _paidEditorPanel.ColumnCount = 2;
            _paidEditorPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _paidEditorPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 46F));
            _paidEditorPanel.Controls.Add(_numPaid, 0, 0);
            _paidEditorPanel.Controls.Add(_btnPayInFull, 1, 0);
            _paidEditorPanel.Dock = DockStyle.Fill;
            _paidEditorPanel.Location = new Point(183, 244);
            _paidEditorPanel.Margin = new Padding(0);
            _paidEditorPanel.Name = "_paidEditorPanel";
            _paidEditorPanel.RowCount = 1;
            _paidEditorPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _paidEditorPanel.Size = new Size(253, 40);
            _paidEditorPanel.TabIndex = 15;
            // 
            // _numPaid
            // 
            _numPaid.BackColor = SystemColors.Window;
            _numPaid.BorderRadius = 10;
            _numPaid.BorderSize = 2;
            _numPaid.DecimalPlaces = 2;
            _numPaid.Dock = DockStyle.Fill;
            _numPaid.Font = new Font("Segoe UI", 10F);
            _numPaid.ForeColor = SystemColors.WindowText;
            _numPaid.Location = new Point(6, 0);
            _numPaid.Margin = new Padding(6, 0, 4, 0);
            _numPaid.Maximum = new decimal(new int[] { 99999999, 0, 0, 0 });
            _numPaid.Name = "_numPaid";
            _numPaid.Size = new Size(197, 40);
            _numPaid.TabIndex = 0;
            _numPaid.TextAlign = HorizontalAlignment.Right;
            _numPaid.AccessibleName = "Paid amount";
            _numPaid.ValueChanged += OnPaidValueChanged;
            // 
            // _btnPayInFull
            // 
            _btnPayInFull.BackColor = SystemColors.Control;
            _btnPayInFull.Cursor = Cursors.Hand;
            _btnPayInFull.Dock = DockStyle.Fill;
            _btnPayInFull.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _btnPayInFull.FlatStyle = FlatStyle.Flat;
            _btnPayInFull.Font = new Font("Segoe UI Semibold", 10F);
            _btnPayInFull.IconChar = FontAwesome.Sharp.IconChar.MoneyBill;
            _btnPayInFull.IconColor = SystemColors.ControlText;
            _btnPayInFull.IconFont = FontAwesome.Sharp.IconFont.Auto;
            _btnPayInFull.IconSize = 16;
            _btnPayInFull.Location = new Point(209, 2);
            _btnPayInFull.Margin = new Padding(2, 2, 6, 2);
            _btnPayInFull.Name = "_btnPayInFull";
            _btnPayInFull.Size = new Size(38, 26);
            _btnPayInFull.Solid = false;
            _btnPayInFull.TabIndex = 1;
            _btnPayInFull.TextImageRelation = TextImageRelation.ImageBeforeText;
            _btnPayInFull.Tone = WinForms.Controls.ButtonTone.Success;
            _btnPayInFull.UseVisualStyleBackColor = false;
            _btnPayInFull.Click += OnPayInFullClick;
            // 
            // _lblBalance
            // 
            _lblBalance.Dock = DockStyle.Fill;
            _lblBalance.Font = new Font("Segoe UI", 9.5F);
            _lblBalance.ForeColor = SystemColors.ControlText;
            _lblBalance.Location = new Point(0, 274);
            _lblBalance.Margin = new Padding(0);
            _lblBalance.Name = "_lblBalance";
            _lblBalance.Padding = new Padding(6, 0, 6, 0);
            _lblBalance.Size = new Size(183, 30);
            _lblBalance.TabIndex = 16;
            _lblBalance.Text = "Remaining:";
            _lblBalance.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _lblBalanceValue
            // 
            _lblBalanceValue.Dock = DockStyle.Fill;
            _lblBalanceValue.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _lblBalanceValue.ForeColor = SystemColors.ControlText;
            _lblBalanceValue.Location = new Point(183, 274);
            _lblBalanceValue.Margin = new Padding(0);
            _lblBalanceValue.Name = "_lblBalanceValue";
            _lblBalanceValue.Padding = new Padding(6, 0, 6, 0);
            _lblBalanceValue.Size = new Size(253, 30);
            _lblBalanceValue.TabIndex = 17;
            _lblBalanceValue.Text = "0.00";
            _lblBalanceValue.TextAlign = ContentAlignment.MiddleRight;
            // 
            // _actions
            // 
            _actions.ColumnCount = 1;
            _actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _actions.Controls.Add(_btnCompleteSale, 0, 0);
            _actions.Controls.Add(_actionRow, 0, 1);
            _actions.Controls.Add(_utilityRow, 0, 2);
            _actions.Dock = DockStyle.Fill;
            _actions.Location = new Point(0, 504);
            _actions.Margin = new Padding(0);
            _actions.Name = "_actions";
            _actions.Padding = new Padding(0, 6, 0, 0);
            _actions.RowCount = 3;
            _actions.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
            _actions.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            _actions.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            _actions.Size = new Size(436, 136);
            _actions.TabIndex = 2;
            // 
            // _btnCompleteSale
            // 
            _btnCompleteSale.BackColor = SystemColors.Control;
            _btnCompleteSale.Cursor = Cursors.Hand;
            _btnCompleteSale.Dock = DockStyle.Fill;
            _btnCompleteSale.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _btnCompleteSale.FlatAppearance.BorderSize = 0;
            _btnCompleteSale.FlatStyle = FlatStyle.Flat;
            _btnCompleteSale.Font = new Font("Segoe UI Semibold", 11.5F, FontStyle.Bold);
            _btnCompleteSale.ForeColor = SystemColors.ControlText;
            _btnCompleteSale.IconChar = FontAwesome.Sharp.IconChar.CheckCircle;
            _btnCompleteSale.IconColor = SystemColors.ControlText;
            _btnCompleteSale.IconFont = FontAwesome.Sharp.IconFont.Auto;
            _btnCompleteSale.IconSize = 18;
            _btnCompleteSale.Location = new Point(0, 6);
            _btnCompleteSale.Margin = new Padding(0, 0, 0, 4);
            _btnCompleteSale.Name = "_btnCompleteSale";
            _btnCompleteSale.Size = new Size(436, 42);
            _btnCompleteSale.Solid = true;
            _btnCompleteSale.TabIndex = 2;
            _btnCompleteSale.Text = "  Complete sale";
            _btnCompleteSale.TextImageRelation = TextImageRelation.ImageBeforeText;
            _btnCompleteSale.Tone = WinForms.Controls.ButtonTone.Primary;
            _btnCompleteSale.UseVisualStyleBackColor = false;
            _btnCompleteSale.Click += OnCompleteSaleClick;
            // 
            // _actionRow
            // 
            _actionRow.ColumnCount = 4;
            _actionRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            _actionRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            _actionRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            _actionRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            _actionRow.Controls.Add(_btnCard, 0, 0);
            _actionRow.Controls.Add(_btnCredit, 1, 0);
            _actionRow.Controls.Add(_btnHold, 2, 0);
            _actionRow.Controls.Add(_btnCancelSale, 3, 0);
            _actionRow.Dock = DockStyle.Fill;
            _actionRow.Location = new Point(0, 52);
            _actionRow.Margin = new Padding(0);
            _actionRow.Name = "_actionRow";
            _actionRow.RowCount = 1;
            _actionRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _actionRow.Size = new Size(436, 44);
            _actionRow.TabIndex = 3;
            // 
            // _btnCard
            // 
            _btnCard.BackColor = SystemColors.Control;
            _btnCard.Cursor = Cursors.Hand;
            _btnCard.Dock = DockStyle.Fill;
            _btnCard.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _btnCard.FlatStyle = FlatStyle.Flat;
            _btnCard.Font = new Font("Segoe UI Semibold", 9F);
            _btnCard.ForeColor = SystemColors.ControlText;
            _btnCard.IconChar = FontAwesome.Sharp.IconChar.CreditCard;
            _btnCard.IconColor = SystemColors.ControlText;
            _btnCard.IconFont = FontAwesome.Sharp.IconFont.Auto;
            _btnCard.IconSize = 15;
            _btnCard.Location = new Point(0, 0);
            _btnCard.Margin = new Padding(0, 0, 4, 4);
            _btnCard.Name = "_btnCard";
            _btnCard.Size = new Size(105, 40);
            _btnCard.Solid = false;
            _btnCard.TabIndex = 0;
            _btnCard.Text = "  Card";
            _btnCard.TextImageRelation = TextImageRelation.ImageBeforeText;
            _btnCard.Tone = WinForms.Controls.ButtonTone.Primary;
            _btnCard.UseVisualStyleBackColor = false;
            _btnCard.Click += OnCardPaymentClick;
            // 
            // _btnCredit
            // 
            _btnCredit.BackColor = SystemColors.Control;
            _btnCredit.Cursor = Cursors.Hand;
            _btnCredit.Dock = DockStyle.Fill;
            _btnCredit.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _btnCredit.FlatStyle = FlatStyle.Flat;
            _btnCredit.Font = new Font("Segoe UI Semibold", 9F);
            _btnCredit.ForeColor = SystemColors.ControlText;
            _btnCredit.IconChar = FontAwesome.Sharp.IconChar.UserClock;
            _btnCredit.IconColor = SystemColors.ControlText;
            _btnCredit.IconFont = FontAwesome.Sharp.IconFont.Auto;
            _btnCredit.IconSize = 15;
            _btnCredit.Location = new Point(109, 0);
            _btnCredit.Margin = new Padding(0, 0, 4, 4);
            _btnCredit.Name = "_btnCredit";
            _btnCredit.Size = new Size(105, 40);
            _btnCredit.Solid = false;
            _btnCredit.TabIndex = 1;
            _btnCredit.Text = "  Credit";
            _btnCredit.TextImageRelation = TextImageRelation.ImageBeforeText;
            _btnCredit.Tone = WinForms.Controls.ButtonTone.Neutral;
            _btnCredit.UseVisualStyleBackColor = false;
            _btnCredit.Click += OnCreditClick;
            // 
            // _btnHold
            // 
            _btnHold.BackColor = SystemColors.Control;
            _btnHold.Cursor = Cursors.Hand;
            _btnHold.Dock = DockStyle.Fill;
            _btnHold.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _btnHold.FlatStyle = FlatStyle.Flat;
            _btnHold.Font = new Font("Segoe UI Semibold", 9F);
            _btnHold.ForeColor = SystemColors.ControlText;
            _btnHold.IconChar = FontAwesome.Sharp.IconChar.Pause;
            _btnHold.IconColor = SystemColors.ControlText;
            _btnHold.IconFont = FontAwesome.Sharp.IconFont.Auto;
            _btnHold.IconSize = 15;
            _btnHold.Location = new Point(218, 0);
            _btnHold.Margin = new Padding(0, 0, 4, 4);
            _btnHold.Name = "_btnHold";
            _btnHold.Size = new Size(105, 40);
            _btnHold.Solid = false;
            _btnHold.TabIndex = 2;
            _btnHold.Text = "  Hold";
            _btnHold.TextImageRelation = TextImageRelation.ImageBeforeText;
            _btnHold.Tone = WinForms.Controls.ButtonTone.Neutral;
            _btnHold.UseVisualStyleBackColor = false;
            _btnHold.Click += OnHoldClick;
            // 
            // _btnCancelSale
            // 
            _btnCancelSale.BackColor = SystemColors.Control;
            _btnCancelSale.Cursor = Cursors.Hand;
            _btnCancelSale.Dock = DockStyle.Fill;
            _btnCancelSale.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _btnCancelSale.FlatStyle = FlatStyle.Flat;
            _btnCancelSale.Font = new Font("Segoe UI Semibold", 9F);
            _btnCancelSale.ForeColor = SystemColors.ControlText;
            _btnCancelSale.IconChar = FontAwesome.Sharp.IconChar.Close;
            _btnCancelSale.IconColor = SystemColors.ControlText;
            _btnCancelSale.IconFont = FontAwesome.Sharp.IconFont.Auto;
            _btnCancelSale.IconSize = 15;
            _btnCancelSale.Location = new Point(327, 0);
            _btnCancelSale.Margin = new Padding(0, 0, 0, 4);
            _btnCancelSale.Name = "_btnCancelSale";
            _btnCancelSale.Size = new Size(109, 40);
            _btnCancelSale.Solid = false;
            _btnCancelSale.TabIndex = 3;
            _btnCancelSale.Text = "  Cancel";
            _btnCancelSale.TextImageRelation = TextImageRelation.ImageBeforeText;
            _btnCancelSale.Tone = WinForms.Controls.ButtonTone.Danger;
            _btnCancelSale.UseVisualStyleBackColor = false;
            _btnCancelSale.Click += OnCancelSaleClick;
            // 
            // _utilityRow
            // 
            _utilityRow.ColumnCount = 2;
            _utilityRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            _utilityRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            _utilityRow.Controls.Add(_btnPrintLast, 0, 0);
            _utilityRow.Controls.Add(_btnCalculator, 1, 0);
            _utilityRow.Dock = DockStyle.Fill;
            _utilityRow.Location = new Point(0, 96);
            _utilityRow.Margin = new Padding(0);
            _utilityRow.Name = "_utilityRow";
            _utilityRow.RowCount = 1;
            _utilityRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _utilityRow.Size = new Size(436, 40);
            _utilityRow.TabIndex = 4;
            // 
            // _btnPrintLast
            // 
            _btnPrintLast.BackColor = SystemColors.Control;
            _btnPrintLast.Cursor = Cursors.Hand;
            _btnPrintLast.Dock = DockStyle.Fill;
            _btnPrintLast.Enabled = false;
            _btnPrintLast.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _btnPrintLast.FlatStyle = FlatStyle.Flat;
            _btnPrintLast.Font = new Font("Segoe UI Semibold", 9F);
            _btnPrintLast.ForeColor = SystemColors.ControlText;
            _btnPrintLast.IconChar = FontAwesome.Sharp.IconChar.Print;
            _btnPrintLast.IconColor = SystemColors.ControlText;
            _btnPrintLast.IconFont = FontAwesome.Sharp.IconFont.Auto;
            _btnPrintLast.IconSize = 14;
            _btnPrintLast.Location = new Point(0, 0);
            _btnPrintLast.Margin = new Padding(0, 0, 4, 0);
            _btnPrintLast.Name = "_btnPrintLast";
            _btnPrintLast.Size = new Size(214, 40);
            _btnPrintLast.Solid = false;
            _btnPrintLast.TabIndex = 0;
            _btnPrintLast.Text = "  Print";
            _btnPrintLast.TextImageRelation = TextImageRelation.ImageBeforeText;
            _btnPrintLast.Tone = WinForms.Controls.ButtonTone.Neutral;
            _btnPrintLast.UseVisualStyleBackColor = false;
            _btnPrintLast.Click += OnPrintLastClick;
            // 
            // _btnCalculator
            // 
            _btnCalculator.BackColor = SystemColors.Control;
            _btnCalculator.Cursor = Cursors.Hand;
            _btnCalculator.Dock = DockStyle.Fill;
            _btnCalculator.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _btnCalculator.FlatStyle = FlatStyle.Flat;
            _btnCalculator.Font = new Font("Segoe UI Semibold", 9F);
            _btnCalculator.ForeColor = SystemColors.ControlText;
            _btnCalculator.IconChar = FontAwesome.Sharp.IconChar.Calculator;
            _btnCalculator.IconColor = SystemColors.ControlText;
            _btnCalculator.IconFont = FontAwesome.Sharp.IconFont.Auto;
            _btnCalculator.IconSize = 14;
            _btnCalculator.Location = new Point(218, 0);
            _btnCalculator.Margin = new Padding(0);
            _btnCalculator.Name = "_btnCalculator";
            _btnCalculator.Size = new Size(218, 40);
            _btnCalculator.Solid = false;
            _btnCalculator.TabIndex = 1;
            _btnCalculator.Text = "  Calculator";
            _btnCalculator.TextImageRelation = TextImageRelation.ImageBeforeText;
            _btnCalculator.Tone = WinForms.Controls.ButtonTone.Neutral;
            _btnCalculator.UseVisualStyleBackColor = false;
            _btnCalculator.Click += OnCalculatorClick;
            // 
            // _tabInvoices
            // 
            _tabInvoices.BackColor = SystemColors.Control;
            _tabInvoices.Controls.Add(_invoiceGrid);
            _tabInvoices.Controls.Add(_lblNoInvoices);
            _tabInvoices.Controls.Add(_invoiceToolbar);
            _tabInvoices.Location = new Point(4, 36);
            _tabInvoices.Name = "_tabInvoices";
            _tabInvoices.Padding = new Padding(3);
            _tabInvoices.Size = new Size(1432, 780);
            _tabInvoices.TabIndex = 1;
            _tabInvoices.Text = "Sales";
            // 
            // _invoiceGrid
            // 
            _invoiceGrid.AllowUserToAddRows = false;
            _invoiceGrid.AllowUserToDeleteRows = false;
            _invoiceGrid.BackgroundColor = SystemColors.Window;
            _invoiceGrid.BorderStyle = BorderStyle.None;
            _invoiceGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _invoiceGrid.Dock = DockStyle.Fill;
            _invoiceGrid.Location = new Point(3, 61);
            _invoiceGrid.MultiSelect = false;
            _invoiceGrid.Name = "_invoiceGrid";
            _invoiceGrid.ReadOnly = true;
            _invoiceGrid.RowHeadersVisible = false;
            _invoiceGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _invoiceGrid.Size = new Size(1426, 716);
            _invoiceGrid.TabIndex = 1;
            // 
            // _lblNoInvoices
            // 
            _lblNoInvoices.Dock = DockStyle.Fill;
            _lblNoInvoices.Font = new Font("Segoe UI", 12F);
            _lblNoInvoices.ForeColor = SystemColors.GrayText;
            _lblNoInvoices.Location = new Point(3, 61);
            _lblNoInvoices.Name = "_lblNoInvoices";
            _lblNoInvoices.Size = new Size(1426, 716);
            _lblNoInvoices.TabIndex = 2;
            _lblNoInvoices.Text = "No invoices yet.";
            _lblNoInvoices.TextAlign = ContentAlignment.MiddleCenter;
            _lblNoInvoices.Visible = false;
            // 
            // _invoiceToolbar
            // 
            _invoiceToolbar.ColumnCount = 3;
            _invoiceToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _invoiceToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            _invoiceToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            _invoiceToolbar.Controls.Add(_invoiceSearch, 0, 0);
            _invoiceToolbar.Controls.Add(_btnNewSale, 1, 0);
            _invoiceToolbar.Controls.Add(_btnDeleteInvoice, 2, 0);
            _invoiceToolbar.Dock = DockStyle.Top;
            _invoiceToolbar.Location = new Point(3, 3);
            _invoiceToolbar.Name = "_invoiceToolbar";
            _invoiceToolbar.Padding = new Padding(12, 10, 12, 10);
            _invoiceToolbar.RowCount = 1;
            _invoiceToolbar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _invoiceToolbar.Size = new Size(1426, 58);
            _invoiceToolbar.TabIndex = 0;
            // 
            // _invoiceSearch
            // 
            _invoiceSearch.BackColor = SystemColors.Window;
            _invoiceSearch.BorderColor = SystemColors.ControlDark;
            _invoiceSearch.BorderErrorColor = Color.Firebrick;
            _invoiceSearch.BorderFocusColor = SystemColors.Highlight;
            _invoiceSearch.BorderRadius = 10;
            _invoiceSearch.BorderSize = 2;
            _invoiceSearch.Dock = DockStyle.Fill;
            _invoiceSearch.Font = new Font("Segoe UI", 11F);
            _invoiceSearch.ForeColor = SystemColors.WindowText;
            _invoiceSearch.Location = new Point(12, 10);
            _invoiceSearch.Margin = new Padding(0, 0, 8, 0);
            _invoiceSearch.Name = "_invoiceSearch";
            _invoiceSearch.Padding = new Padding(10, 6, 10, 6);
            _invoiceSearch.PlaceholderColor = SystemColors.GrayText;
            _invoiceSearch.PlaceholderText = "Search invoices";
            _invoiceSearch.Size = new Size(1114, 38);
            _invoiceSearch.TabIndex = 0;
            _invoiceSearch.TextChanged += OnInvoiceSearchTextChanged;
            // 
            // _btnNewSale
            // 
            _btnNewSale.BackColor = SystemColors.Control;
            _btnNewSale.Cursor = Cursors.Hand;
            _btnNewSale.Dock = DockStyle.Fill;
            _btnNewSale.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _btnNewSale.FlatStyle = FlatStyle.Flat;
            _btnNewSale.Font = new Font("Segoe UI Semibold", 9.5F);
            _btnNewSale.ForeColor = SystemColors.ControlText;
            _btnNewSale.IconChar = FontAwesome.Sharp.IconChar.Add;
            _btnNewSale.IconColor = SystemColors.ControlText;
            _btnNewSale.IconFont = FontAwesome.Sharp.IconFont.Auto;
            _btnNewSale.IconSize = 16;
            _btnNewSale.Location = new Point(1134, 10);
            _btnNewSale.Margin = new Padding(0, 0, 8, 0);
            _btnNewSale.Name = "_btnNewSale";
            _btnNewSale.Size = new Size(142, 38);
            _btnNewSale.Solid = false;
            _btnNewSale.TabIndex = 1;
            _btnNewSale.Text = "  New sale";
            _btnNewSale.TextImageRelation = TextImageRelation.ImageBeforeText;
            _btnNewSale.Tone = WinForms.Controls.ButtonTone.Primary;
            _btnNewSale.UseVisualStyleBackColor = false;
            _btnNewSale.Click += OnNewSaleClick;
            // 
            // _btnDeleteInvoice
            // 
            _btnDeleteInvoice.BackColor = SystemColors.Control;
            _btnDeleteInvoice.Cursor = Cursors.Hand;
            _btnDeleteInvoice.Dock = DockStyle.Fill;
            _btnDeleteInvoice.FlatAppearance.BorderColor = SystemColors.ControlDark;
            _btnDeleteInvoice.FlatStyle = FlatStyle.Flat;
            _btnDeleteInvoice.Font = new Font("Segoe UI Semibold", 9.5F);
            _btnDeleteInvoice.ForeColor = SystemColors.ControlText;
            _btnDeleteInvoice.IconChar = FontAwesome.Sharp.IconChar.Trash;
            _btnDeleteInvoice.IconColor = SystemColors.ControlText;
            _btnDeleteInvoice.IconFont = FontAwesome.Sharp.IconFont.Auto;
            _btnDeleteInvoice.IconSize = 16;
            _btnDeleteInvoice.Location = new Point(1284, 10);
            _btnDeleteInvoice.Margin = new Padding(0);
            _btnDeleteInvoice.Name = "_btnDeleteInvoice";
            _btnDeleteInvoice.Size = new Size(130, 38);
            _btnDeleteInvoice.Solid = false;
            _btnDeleteInvoice.TabIndex = 2;
            _btnDeleteInvoice.Text = "  Delete";
            _btnDeleteInvoice.TextImageRelation = TextImageRelation.ImageBeforeText;
            _btnDeleteInvoice.Tone = WinForms.Controls.ButtonTone.Danger;
            _btnDeleteInvoice.UseVisualStyleBackColor = false;
            _btnDeleteInvoice.Click += OnDeleteInvoiceClick;
            // 
            // _toolTip
            // 
            _toolTip.ShowAlways = true;
            // 
            // SalesForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(1440, 820);
            Controls.Add(_tabs);
            MinimumSize = new Size(360, 600);
            Name = "SalesForm";
            Text = "Sales";
            _tabs.ResumeLayout(false);
            _tabNewSale.ResumeLayout(false);
            _pageLayout.ResumeLayout(false);
            _heroStrip.ResumeLayout(false);
            _heroTotalCard.ResumeLayout(false);
            _discountCard.ResumeLayout(false);
            _statsCard.ResumeLayout(false);
            _saleLayout.ResumeLayout(false);
            _leftPanel.ResumeLayout(false);
            _productColumn.ResumeLayout(false);
            _productHeader.ResumeLayout(false);
            _searchBar.ResumeLayout(false);
            _rightPanel.ResumeLayout(false);
            _basketRoot.ResumeLayout(false);
            _basketScroll.ResumeLayout(false);
            _basketScroll.PerformLayout();
            _basketLayout.ResumeLayout(false);
            _basketLayout.PerformLayout();
            _customerRow.ResumeLayout(false);
            _cartHeader.ResumeLayout(false);
            _cartLines.ResumeLayout(false);
            _totalsLayout.ResumeLayout(false);
            _discountEditorPanel.ResumeLayout(false);
            _paidEditorPanel.ResumeLayout(false);
            _actions.ResumeLayout(false);
            _actionRow.ResumeLayout(false);
            _utilityRow.ResumeLayout(false);
            _tabInvoices.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_invoiceGrid).EndInit();
            _invoiceToolbar.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl _tabs;
        private System.Windows.Forms.TabPage _tabNewSale;
        private System.Windows.Forms.TableLayoutPanel _pageLayout;
        private System.Windows.Forms.TableLayoutPanel _heroStrip;
        private System.Windows.Forms.Panel _heroTotalCard;
        private System.Windows.Forms.Label _lblHeroTotal;
        private System.Windows.Forms.Label _lblHeroTotalCaption;
        private System.Windows.Forms.Panel _discountCard;
        private System.Windows.Forms.Label _lblHeroDiscount;
        private System.Windows.Forms.Label _lblHeroDiscountCaption;
        private System.Windows.Forms.Panel _statsCard;
        private System.Windows.Forms.Label _lblHeroStats;
        private System.Windows.Forms.TableLayoutPanel _saleLayout;
        private System.Windows.Forms.Panel _leftPanel;
        private System.Windows.Forms.TableLayoutPanel _productColumn;
        private System.Windows.Forms.TableLayoutPanel _productHeader;
        private System.Windows.Forms.Label _lblProductCount;
        private DzentoPOS2.WinForms.Controls.ThemedIconButton _btnViewToggle;
        private System.Windows.Forms.TableLayoutPanel _searchBar;
        private EZControls.Controls.Inputs.EZTextBox _productSearch;
        private DzentoPOS2.WinForms.Controls.ThemedIconButton _btnScanBarcode;
        private DzentoPOS2.WinForms.Controls.Pos.CategoryChipBar _categoryBar;
        private DzentoPOS2.WinForms.Controls.Pos.ScanConfirmationStrip _scanStrip;
        private DzentoPOS2.WinForms.Controls.Pos.PosProductList _productList;
        private System.Windows.Forms.Panel _rightPanel;
        private System.Windows.Forms.TableLayoutPanel _basketRoot;
        private DzentoPOS2.WinForms.Controls.Pos.SessionStatusChip _sessionChip;
        private System.Windows.Forms.Panel _basketScroll;
        private System.Windows.Forms.TableLayoutPanel _basketLayout;
        private System.Windows.Forms.TableLayoutPanel _customerRow;
        private System.Windows.Forms.Label _lblCustomer;
        private EZControls.Controls.Inputs.EZComboBox _cmbCustomer;
        private DzentoPOS2.WinForms.Controls.ThemedIconButton _btnAddCustomer;
        private System.Windows.Forms.TableLayoutPanel _cartHeader;
        private System.Windows.Forms.Label _lblCart;
        private DzentoPOS2.WinForms.Controls.ThemedIconButton _btnClearCart;
        private System.Windows.Forms.FlowLayoutPanel _cartLines;
        private System.Windows.Forms.Label _lblEmptyCart;
        private System.Windows.Forms.TableLayoutPanel _totalsLayout;
        private System.Windows.Forms.Label _lblDiscount;
        private System.Windows.Forms.Label _lblDiscountValue;
        private System.Windows.Forms.TableLayoutPanel _discountEditorPanel;
        private EZControls.Controls.Inputs.EZNumericBox _numDiscount;
        private EZControls.Controls.Inputs.EZComboBox _cmbDiscountMode;
        private System.Windows.Forms.Label _lblSubtotal;
        private System.Windows.Forms.Label _lblSubtotalValue;
        private System.Windows.Forms.Label _lblAfterDiscount;
        private System.Windows.Forms.Label _lblAfterDiscountValue;
        private System.Windows.Forms.Label _lblTax;
        private EZControls.Controls.Inputs.EZNumericBox _numTax;
        private System.Windows.Forms.Label _lblTaxAmount;
        private System.Windows.Forms.Label _lblTaxAmountValue;
        private System.Windows.Forms.Label _lblFees;
        private EZControls.Controls.Inputs.EZNumericBox _numFees;
        private System.Windows.Forms.Label _lblTotal;
        private System.Windows.Forms.Label _lblTotalValue;
        private System.Windows.Forms.Label _lblPaid;
        private System.Windows.Forms.TableLayoutPanel _paidEditorPanel;
        private EZControls.Controls.Inputs.EZNumericBox _numPaid;
        private DzentoPOS2.WinForms.Controls.ThemedIconButton _btnPayInFull;
        private System.Windows.Forms.Label _lblBalance;
        private System.Windows.Forms.Label _lblBalanceValue;
        private System.Windows.Forms.TableLayoutPanel _actions;
        private DzentoPOS2.WinForms.Controls.ThemedIconButton _btnCompleteSale;
        private System.Windows.Forms.TableLayoutPanel _actionRow;
        private DzentoPOS2.WinForms.Controls.ThemedIconButton _btnCard;
        private DzentoPOS2.WinForms.Controls.ThemedIconButton _btnCredit;
        private DzentoPOS2.WinForms.Controls.ThemedIconButton _btnHold;
        private DzentoPOS2.WinForms.Controls.ThemedIconButton _btnCancelSale;
        private System.Windows.Forms.TableLayoutPanel _utilityRow;
        private DzentoPOS2.WinForms.Controls.ThemedIconButton _btnPrintLast;
        private DzentoPOS2.WinForms.Controls.ThemedIconButton _btnCalculator;
        private System.Windows.Forms.TabPage _tabInvoices;
        private System.Windows.Forms.DataGridView _invoiceGrid;
        private System.Windows.Forms.Label _lblNoInvoices;
        private System.Windows.Forms.TableLayoutPanel _invoiceToolbar;
        private DzentoPOS2.WinForms.Controls.ThemedIconButton _btnDeleteInvoice;
        private DzentoPOS2.WinForms.Controls.ThemedIconButton _btnNewSale;
        private EZControls.Controls.Inputs.EZTextBox _invoiceSearch;
        private System.Windows.Forms.ToolTip _toolTip;
    }
}
