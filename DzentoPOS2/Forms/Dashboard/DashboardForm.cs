using DzentoPOS2.App.Services.Reports.Interfaces;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Reports;
using DzentoPOS2.Shared.Helpers;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Controls;
using DzentoPOS2.WinForms.Modules.Dashboard;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Forms.Dashboard
{
    public partial class DashboardForm : LocalizableFormBase
    {
        private readonly IDashboardService _dashboardService;
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

        public DashboardForm(IDashboardService dashboardService, ILanguageService languageService, RegionalContext regional)
            : base(languageService)
        {
            InitializeComponent();
            _dashboardService = dashboardService;
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
            _cmbDatePreset = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160 };
            _cmbDatePreset.Items.AddRange(new object[] { "Today", "Yesterday", "LastWeek", "Month", "Custom" });
            _cmbDatePreset.SelectedIndex = 3;
            _cmbDatePreset.SelectedIndexChanged += (_, _) => { UpdateFilterVisibility(); RefreshSummarySafe(); };

            _dtFrom = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 120 };
            _dtTo = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 120 };
            _dtFrom.ValueChanged += (_, _) => RefreshSummarySafe();
            _dtTo.ValueChanged += (_, _) => RefreshSummarySafe();

            _cmbCustomer = new ComboBox { DropDownStyle = ComboBoxStyle.DropDown, AutoCompleteMode = AutoCompleteMode.SuggestAppend, AutoCompleteSource = AutoCompleteSource.ListItems, Width = 200 };
            _cmbCustomer.SelectedIndexChanged += (_, _) => RefreshSummarySafe();
            LoadCustomers();

            var filterBar = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 4,
                Margin = new Padding(0, 0, 0, 8)
            };
            filterBar.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            filterBar.Controls.Add(_cmbDatePreset, 0, 0);
            filterBar.Controls.Add(_dtFrom, 1, 0);
            filterBar.Controls.Add(_dtTo, 2, 0);
            filterBar.Controls.Add(_cmbCustomer, 3, 0);

            UpdateFilterVisibility();
            return filterBar;
        }

        private void LoadCustomers()
        {
            // placeholder: load active customers lazily
        }

        private void UpdateFilterVisibility()
        {
            bool custom = _cmbDatePreset.SelectedItem?.ToString() == "Custom";
            _dtFrom.Visible = custom;
            _dtTo.Visible = custom;
        }

        private DashboardFilterDto GetFilter()
        {
            var preset = _cmbDatePreset.SelectedItem?.ToString() switch
            {
                "Today" => DashboardDatePreset.Today,
                "Yesterday" => DashboardDatePreset.Yesterday,
                "LastWeek" => DashboardDatePreset.LastWeek,
                "Custom" => DashboardDatePreset.Custom,
                _ => DashboardDatePreset.Month
            };
            Guid? cust = null;
            if (_cmbCustomer.SelectedValue is Guid g) cust = g;

            DateTime? from = null, to = null;
            if (preset == DashboardDatePreset.Custom)
            {
                from = _regional.ToUtcSafe(_dtFrom.Value.Date);
                to = _regional.ToUtcSafe(_dtTo.Value.Date.AddDays(1));
            }

            return new DashboardFilterDto
            {
                DatePreset = preset,
                CustomFromUtc = from,
                CustomToUtc = to,
                CustomerId = cust
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
