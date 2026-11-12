using DzentoPOS2.App.Authorization;
using DzentoPOS2.App.Services.Reports.Interfaces;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.Core.Constants;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Reports;
using DzentoPOS2.WinForms.Controls;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;
using FontAwesome.Sharp;

namespace DzentoPOS2.WinForms.Forms.Reports;

/// <summary>
/// Unified cross-branch report. One row per branch for the selected period
/// (sales, returns, net sales, purchases, expenses, gross profit, net profit)
/// with a bold grand total. A user without <c>BranchesViewAll</c> only sees the
/// active branch; the report is read-only.
/// </summary>
public class CrossBranchReportForm : LocalizableFormBase, IBranchAware
{
    private readonly ICrossBranchReportService _reportService;
    private readonly IAuthorizationGuard _guard;

    private readonly TableLayoutPanel _root;
    private readonly FlowLayoutPanel _toolbar;
    private readonly Label _lblPeriod;
    private readonly ThemedComboBox _cmbPreset;
    private readonly EZDatePicker _from;
    private readonly EZDatePicker _to;
    private readonly CheckBox _chkAllBranches;
    private readonly ThemedIconButton _btnRefresh;
    private readonly DataGridView _grid;

    private bool _canViewAll;
    private bool _suppressRefresh;

    public CrossBranchReportForm(
        ICrossBranchReportService reportService,
        IAuthorizationGuard guard,
        ILanguageService languageService)
        : base(languageService)
    {
        _reportService = reportService;
        _guard = guard;

        Size = new Size(1200, 720);
        MinimumSize = new Size(320, 300);
        BackColor = AppColors.Background;
        AutoScaleMode = AutoScaleMode.Font;

        _lblPeriod = new Label
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 8, 6, 0)
        };

        _cmbPreset = new ThemedComboBox { Width = 160, Margin = new Padding(0, 2, 10, 2) };
        _cmbPreset.SelectedIndexChanged += (_, _) =>
        {
            UpdateCustomVisibility();
            if (!_suppressRefresh) RunSafe(LoadAsync);
        };

        _from = new EZDatePicker { Width = 120, Margin = new Padding(0, 3, 8, 2) };
        _to = new EZDatePicker { Width = 120, Margin = new Padding(0, 3, 10, 2) };
        _from.ValueChanged += (_, _) => { if (!_suppressRefresh) RunSafe(LoadAsync); };
        _to.ValueChanged += (_, _) => { if (!_suppressRefresh) RunSafe(LoadAsync); };

        _chkAllBranches = new CheckBox
        {
            AutoSize = true,
            // Keep the checkbox at the shared 30px tap-target height even
            // though it auto-sizes to its text.
            MinimumSize = new Size(0, 30),
            Margin = new Padding(0, 6, 10, 0)
        };
        _chkAllBranches.CheckedChanged += (_, _) => { if (!_suppressRefresh) RunSafe(LoadAsync); };

        _btnRefresh = ButtonFactory.Tool(IconChar.Sync, AppStrings.Refresh, ButtonTone.Primary);
        _btnRefresh.AutoSize = true;
        _btnRefresh.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _btnRefresh.MinimumSize = new Size(110, 34);
        _btnRefresh.Margin = new Padding(0, 2, 0, 2);
        _btnRefresh.Click += (_, _) => RunSafe(LoadAsync);

        _toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = AppColors.Surface,
            Padding = new Padding(12, 8, 12, 8),
            Margin = new Padding(0, 0, 0, 8)
        };
        _toolbar.Controls.AddRange(new Control[]
        {
            _lblPeriod, _cmbPreset, _from, _to, _chkAllBranches, _btnRefresh
        });

        _grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        DataGridViewStyleHelper.ApplyModernStyle(_grid);

        _grid.Columns.AddRange(new DataGridViewColumn[]
        {
            new DataGridViewTextBoxColumn { DataPropertyName = "BranchName", FillWeight = 160 },
            new DataGridViewTextBoxColumn { DataPropertyName = "SalesCount", FillWeight = 70, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { DataPropertyName = "SalesTotal", FillWeight = 95, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { DataPropertyName = "ReturnsTotal", FillWeight = 85, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { DataPropertyName = "NetSales", FillWeight = 95, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { DataPropertyName = "PurchasesCount", FillWeight = 70, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { DataPropertyName = "PurchasesTotal", FillWeight = 95, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { DataPropertyName = "ExpensesTotal", FillWeight = 95, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { DataPropertyName = "GrossProfit", FillWeight = 95, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { DataPropertyName = "NetProfit", FillWeight = 95, DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } }
        });

        _grid.CellFormatting += (_, e) =>
        {
            if (e.RowIndex < 0 || e.RowIndex >= _grid.Rows.Count)
                return;

            if (_grid.Rows[e.RowIndex].DataBoundItem is BranchReportRowDto row
                && row.BranchId == Guid.Empty)
            {
                e.CellStyle.Font = new Font(_grid.Font, FontStyle.Bold);
            }
        };

        _root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = Padding.Empty,
            Margin = Padding.Empty
        };
        _root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _root.Controls.Add(_toolbar, 0, 0);
        _root.Controls.Add(_grid, 0, 1);

        Controls.Add(_root);
    }

    protected override void OnInitialized() => RunSafe(LoadAsync);

    public Task OnBranchChangedAsync(Guid branchId)
    {
        RunSafe(LoadAsync);
        return Task.CompletedTask;
    }

    protected override void LoadText()
    {
        Text = AppStrings.CrossBranchReport;
        _lblPeriod.Text = AppStrings.Period + ":";
        _chkAllBranches.Text = AppStrings.ViewAllBranches;
        _btnRefresh.Text = "  " + AppStrings.Refresh;

        _suppressRefresh = true;
        int previous = _cmbPreset.SelectedIndex;
        _cmbPreset.Items = new[]
        {
            AppStrings.DatePresetToday,
            AppStrings.DatePresetYesterday,
            AppStrings.DatePresetLast7Days,
            AppStrings.DatePresetThisMonth,
            AppStrings.DatePresetCustom
        };
        _cmbPreset.SelectedIndex = previous >= 0 && previous < 5 ? previous : 3;
        _suppressRefresh = false;

        _grid.Columns[0].HeaderText = AppStrings.Branch;
        _grid.Columns[1].HeaderText = AppStrings.SalesInvoicesCount;
        _grid.Columns[2].HeaderText = AppStrings.Sales;
        _grid.Columns[3].HeaderText = AppStrings.Returns;
        _grid.Columns[4].HeaderText = AppStrings.NetSales;
        _grid.Columns[5].HeaderText = AppStrings.PurchaseInvoicesCount;
        _grid.Columns[6].HeaderText = AppStrings.Purchases;
        _grid.Columns[7].HeaderText = AppStrings.Expenses;
        _grid.Columns[8].HeaderText = AppStrings.GrossProfit;
        _grid.Columns[9].HeaderText = AppStrings.NetProfit;

        UpdateCustomVisibility();
    }

    private void UpdateCustomVisibility()
    {
        bool custom = _cmbPreset.SelectedIndex == 4;
        _from.Visible = custom;
        _to.Visible = custom;
    }

    private async Task LoadAsync()
    {
        _canViewAll = await _guard.IsAllowedAsync(Permissions.BranchesViewAll);

        if (IsDisposedOrDisposing) return;

        _chkAllBranches.Enabled = _canViewAll;
        if (!_canViewAll && _chkAllBranches.Checked)
        {
            _suppressRefresh = true;
            _chkAllBranches.Checked = false;
            _suppressRefresh = false;
        }

        var report = await _reportService.GetAsync(BuildFilter());
        if (IsDisposedOrDisposing) return;

        var data = new List<BranchReportRowDto>(report.Rows) { report.Total };
        _grid.DataSource = null;
        _grid.DataSource = data;
    }

    private CrossBranchReportFilterDto BuildFilter()
    {
        var preset = _cmbPreset.SelectedIndex switch
        {
            0 => DashboardDatePreset.Today,
            1 => DashboardDatePreset.Yesterday,
            2 => DashboardDatePreset.Last7Days,
            3 => DashboardDatePreset.ThisMonth,
            4 => DashboardDatePreset.Custom,
            _ => DashboardDatePreset.ThisMonth
        };

        DateTime? from = null, to = null;
        if (preset == DashboardDatePreset.Custom)
        {
            from = _from.Value.Date;
            to = _to.Value.Date;
        }

        return new CrossBranchReportFilterDto
        {
            Preset = preset,
            CustomFrom = from,
            CustomTo = to,
            ViewAllBranches = _chkAllBranches.Checked && _canViewAll
        };
    }
}
