using DzentoPOS2.App.Services.Reports.Interfaces;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Reports;
using DzentoPOS2.Shared.Helpers;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Controls;
using DzentoPOS2.App.Services.Partners;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.WinForms.Modules.Dashboard;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Forms.Dashboard
{
    public partial class DashboardForm : LocalizableFormBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ICustomerService _customerService;
        private readonly RegionalContext _regional;
        private readonly KpiCardsModule _kpiCards;
        private readonly ProfitSummaryModule _profitSummary;
        private readonly ChartsModule _charts;
        private readonly InsightsModule _insights;
        private readonly RecentTransactionsModule _recentTransactions;
        private readonly TableLayoutPanel _layout;
        private ComboBox _cmbDatePreset = null!;
        private DateTimePicker _dtFrom = null!;
        private DateTimePicker _dtTo = null!;
        private ComboBox _cmbCustomer = null!;
        private System.Windows.Forms.Timer _debounceTimer = null!;
        private Guid? _selectedCustomerId;
        private int _selectedPresetIndex = 3;

        public DashboardForm(IDashboardService dashboardService, ICustomerService customerService, ILanguageService languageService, RegionalContext regional)
            : base(languageService)
        {
            InitializeComponent();
            _dashboardService = dashboardService;
            _customerService = customerService;
            _regional = regional;
            _kpiCards = new KpiCardsModule();
            _profitSummary = new ProfitSummaryModule();
            _recentTransactions = new RecentTransactionsModule();
            _charts = new ChartsModule(_recentTransactions);
            _insights = new InsightsModule();

            AutoScroll = true;
            BackColor = AppColors.Background;

            _layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(20, 16, 20, 20),
                BackColor = Color.Transparent,
                Width = ClientSize.Width - 40
            };

            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _layout.RowCount = 5;
            for (int i = 0; i < 5; i++)
                if (i >= _layout.RowStyles.Count) _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Controls.Add(_layout);

            Resize += (_, _) => _layout.Width = ClientSize.Width - 40;
        }

        protected override void LoadText()
        {
            Text = AppStrings.Dashboard;
            if (_cmbDatePreset != null) _selectedPresetIndex = _cmbDatePreset.SelectedIndex;
            if (_cmbCustomer != null && _cmbCustomer.SelectedItem is CustomerOption opt) _selectedCustomerId = opt.Id;

            SuspendLayout();

            while (_layout.Controls.Count > 0)
            {
                var c = _layout.Controls[0];
                _layout.Controls.RemoveAt(0);
                c.Dispose();
            }

            _layout.Controls.Add(BuildFilterBar(), 0, 0);
            _layout.Controls.Add(_kpiCards.Build(), 0, 1);
            _layout.Controls.Add(_profitSummary.Build(), 0, 2);
            _layout.Controls.Add(_insights.Build(), 0, 3);
            _layout.Controls.Add(_charts.Build(), 0, 4);

            ResumeLayout();
        }

        protected override void OnLanguageChangedAfterLoadText() => RunSafe(RefreshSummaryAsync);

        protected override void OnInitialized() => RunSafe(RefreshSummaryAsync);

        private TableLayoutPanel BuildFilterBar()
        {
            var lblPeriod = new Label { Text = AppStrings.Period, AutoSize = true, ForeColor = AppColors.Ink, Margin = new Padding(0, 8, 8, 0), Height = 36 };
            // EZSegmentedControl replaces the preset combo; EZDatePicker replaces
            // the raw DateTimePickers (filter bar controls).
            var seg = new EZSegmentedControl();
            seg.SetItems(AppStrings.DatePresetToday, AppStrings.DatePresetYesterday,
                AppStrings.DatePresetLast7Days, AppStrings.DatePresetThisMonth, AppStrings.DatePresetCustom);
            seg.SelectedIndex = _selectedPresetIndex < 5 ? _selectedPresetIndex : 3;
            seg.SelectedIndexChanged += (_, idx) =>
            {
                _selectedPresetIndex = idx;
                UpdateFilterVisibility();
                ScheduleRefresh();
            };
            _cmbDatePreset = new ComboBox { Visible = false };

            _dtFrom = new EZDatePicker { Width = 120 };
            _dtTo = new EZDatePicker { Width = 120 };
            _dtFrom.ValueChanged += (_, _) => { ValidateDateRange(); ScheduleRefresh(); };
            _dtTo.ValueChanged += (_, _) => { ValidateDateRange(); ScheduleRefresh(); };

            var lblCustomer = new Label { Text = AppStrings.Customers, AutoSize = true, ForeColor = AppColors.Ink, Margin = new Padding(0, 8, 8, 0), Height = 36 };
            _cmbCustomer = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDown,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems,
                Width = 200,
                Height = 36
            };
            _cmbCustomer.SelectedIndexChanged += (_, _) =>
            {
                if (_cmbCustomer.SelectedItem is CustomerOption opt) _selectedCustomerId = opt.Id;
                ScheduleRefresh();
            };

            _debounceTimer = new System.Windows.Forms.Timer { Interval = 300 };
            _debounceTimer.Tick += (_, _) => { _debounceTimer.Stop(); RefreshSummarySafe(); };

            LoadCustomers();

            var filterBar = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 6,
                Margin = new Padding(0, 0, 0, 8)
            };
            filterBar.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            filterBar.Controls.Add(lblPeriod, 0, 0);
            filterBar.Controls.Add(seg, 1, 0);
            filterBar.Controls.Add(_dtFrom, 2, 0);
            filterBar.Controls.Add(_dtTo, 3, 0);
            filterBar.Controls.Add(lblCustomer, 4, 0);
            filterBar.Controls.Add(_cmbCustomer, 5, 0);

            UpdateFilterVisibility();
            return filterBar;
        }

        private void LoadCustomers()
        {
            _cmbCustomer.Items.Clear();
            _cmbCustomer.Items.Add(new CustomerOption(null, AppStrings.AllCustomers));
            try
            {
                var customers = _customerService.GetAllAsync().GetAwaiter().GetResult();
                foreach (var c in customers)
                    _cmbCustomer.Items.Add(new CustomerOption(c.Id, c.FullName));
            }
            catch { /* ignore load errors */ }

            // restore selection
            int idx = 0;
            if (_selectedCustomerId.HasValue)
            {
                for (int i = 0; i < _cmbCustomer.Items.Count; i++)
                {
                    if (_cmbCustomer.Items[i] is CustomerOption o && o.Id == _selectedCustomerId)
                    { idx = i; break; }
                }
            }
            _cmbCustomer.SelectedIndex = idx;
        }

        private void ValidateDateRange()
        {
            if (_dtFrom.Value.Date > _dtTo.Value.Date)
                _dtTo.Value = _dtFrom.Value;
        }

        private void ScheduleRefresh()
        {
            _debounceTimer?.Stop();
            _debounceTimer?.Start();
        }

        private void UpdateFilterVisibility()
        {
            bool custom = _selectedPresetIndex == 4; // Custom is last
            _dtFrom.Visible = custom;
            _dtTo.Visible = custom;
        }

        private class CustomerOption
        {
            public Guid? Id { get; }
            public string Name { get; }
            public CustomerOption(Guid? id, string name) { Id = id; Name = name; }
            public override string ToString() => Name;
        }

        private DashboardFilterDto GetFilter()
        {
            var preset = _selectedPresetIndex switch
            {
                0 => DashboardDatePreset.Today,
                1 => DashboardDatePreset.Yesterday,
                2 => DashboardDatePreset.Last7Days,
                3 => DashboardDatePreset.ThisMonth,
                4 => DashboardDatePreset.Custom,
                _ => DashboardDatePreset.Today
            };

            DateTime? from = null, to = null;
            if (preset == DashboardDatePreset.Custom)
            {
                from = _dtFrom.Value.Date;
                to = _dtTo.Value.Date;
            }

            return new DashboardFilterDto
            {
                Preset = preset,
                CustomFrom = from,
                CustomTo = to,
                UserId = null
            };
        }

        private void RefreshSummarySafe()
        {
            RunSafe(async () =>
            {
                var data = await _dashboardService.GetSummaryAsync(GetFilter());
                if (IsDisposedOrDisposing) return;
                _kpiCards.Update(data);
                _profitSummary.Update(data, KpiCard.CurrencyDigits);
                _insights.Update(data);
                _charts.Update(data);
            });
        }

        private Task RefreshSummaryAsync()
        {
            RefreshSummarySafe();
            return Task.CompletedTask;
        }
    }
}
