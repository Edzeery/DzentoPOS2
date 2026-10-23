using DzentoPOS2.App.Services.Reports.Interfaces;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Modules.Dashboard;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Forms.Dashboard
{
    public partial class DashboardForm : LocalizableFormBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly KpiCardsModule _kpiCards;
        private readonly ChartsModule _charts;
        private readonly InsightsModule _insights;
        private readonly RecentTransactionsModule _recentTransactions;
        private readonly TableLayoutPanel _layout;

        public DashboardForm(IDashboardService dashboardService, ILanguageService languageService)
            : base(languageService)
        {
            InitializeComponent();
            _dashboardService = dashboardService;
            _kpiCards = new KpiCardsModule();
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
                RowCount = 3,
                Padding = new Padding(20, 16, 20, 20),
                BackColor = Color.Transparent,
                Width = ClientSize.Width - 40
            };

            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

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

            _layout.Controls.Add(_kpiCards.Build(), 0, 0);
            _layout.Controls.Add(_insights.Build(), 0, 1);
            _layout.Controls.Add(_charts.Build(), 0, 2);

            ResumeLayout();
        }

        protected override async void OnLanguageChangedAfterLoadText()
        {
            try
            {
                var data = await _dashboardService.GetSummaryAsync();
                _kpiCards.Update(data);
                _insights.Update(data);
                _charts.Update(data);
                _recentTransactions.Update(data);
            }
            catch
            {
            }
        }

        protected override async void OnInitialized()
        {
            try
            {
                var data = await _dashboardService.GetSummaryAsync();
                _kpiCards.Update(data);
                _insights.Update(data);
                _charts.Update(data);
                _recentTransactions.Update(data);
            }
            catch
            {
            }
        }
    }
}
