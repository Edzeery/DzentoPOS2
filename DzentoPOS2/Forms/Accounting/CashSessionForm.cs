using System.Globalization;
using DzentoPOS2.App.Diagnostics;
using DzentoPOS2.App.Services.Accounting;
using DzentoPOS2.App.Services.Events;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Accounting;
using DzentoPOS2.Shared.Helpers;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;
using DzentoPOS2.WinForms.Controls;
using Microsoft.VisualBasic;

namespace DzentoPOS2.WinForms.Forms.Accounting;

/// <summary>
/// Cash-drawer dashboard. Layout lives in the designer; this file only contains
/// data loading, live refresh and formatting. Every number goes through
/// <see cref="KpiCard.SetValue"/> so nothing is ever parsed back from text.
/// </summary>
public partial class CashSessionForm : LocalizableFormBase
{
    private readonly ICashSessionService _sessionService;
    private readonly ISalesEventPublisher _events;
    private readonly RegionalContext _regional;

    private CashSessionDto? _openSession;
    private CashSessionSummaryDto? _summary;
    private int _kpiColumns;
    private bool _loading;
    private bool _refreshing;
    private bool _refreshPending;

    public CashSessionForm(
        ICashSessionService sessionService,
        ISalesEventPublisher events,
        RegionalContext regional,
        ILanguageService languageService)
        : base(languageService)
    {
        _sessionService = sessionService;
        _events = events;
        _regional = regional;

        InitializeComponent();

        BackColor = AppColors.Background;
        StartPosition = FormStartPosition.CenterParent;
        KpiCard.CurrencyDigits = _regional.CurrencyDecimalDigits;

        DataGridViewStyleHelper.ApplyModernStyle(_grid);
        _grid.AutoGenerateColumns = false;

        _cmbCashbox.SelectedIndexChanged += async (_, _) =>
        {
            if (!_loading)
                await RefreshAsync();
        };

        _btnOpen.Click += async (_, _) => await OpenAsync();
        _btnClose.Click += async (_, _) => await CloseAsync();
        _btnPayIn.Click += async (_, _) => await RecordMovementAsync(isPayIn: true);
        _btnPayOut.Click += async (_, _) => await RecordMovementAsync(isPayIn: false);

        _kpiHost.Resize += (_, _) => RelayoutKpis();

        _events.SaleCompleted += OnSaleCompleted;
        _events.SaleVoided += OnSaleVoided;
        _events.CashMovementAdded += OnCashMovementEvent;

        ThemeManager.ThemeChanged += OnThemeChanged;
        ApplyTheme();
    }

    protected override async void OnInitialized()
    {
        await LoadCashboxesAsync();
    }

    protected override void LoadText()
    {
        Text = AppStrings.CashSessions;

        _lblTitle.Text = AppStrings.CashDrawer;
        _lblCashbox.Text = AppStrings.Cashbox;
        _lblOpening.Text = AppStrings.OpeningBalance;
        _lblClosing.Text = AppStrings.ClosingBalance;

        _btnOpen.Text = AppStrings.OpenSession;
        _btnClose.Text = AppStrings.CloseSession;
        _btnPayIn.Text = AppStrings.PayIn;
        _btnPayOut.Text = AppStrings.PayOut;

        _kpiTotalSales.CardTitle = AppStrings.TotalSales;
        _kpiCashSales.CardTitle = AppStrings.CashSales;
        _kpiNonCash.CardTitle = AppStrings.NonCashSales;
        _kpiExpected.CardTitle = AppStrings.ExpectedBalance;
        _kpiOpening.CardTitle = AppStrings.OpeningBalance;
        _kpiPayIn.CardTitle = AppStrings.PayIn;
        _kpiPayOut.CardTitle = AppStrings.PayOut;
        _kpiExpenses.CardTitle = AppStrings.Expenses;

        ApplyGridColumns();
        UpdateStatusText();
    }

    protected override void OnLanguageChangedAfterLoadText()
    {
        _ = RefreshAsync();
    }

    // ── Live updates ───────────────────────────────────────────────────────

    private void OnSaleCompleted(object? sender, SaleCompletedEventArgs e) => RequestRefresh();

    private void OnSaleVoided(object? sender, SaleVoidedEventArgs e) => RequestRefresh();

    private void OnCashMovementEvent(object? sender, CashMovementEventArgs e) => RequestRefresh();

    private void RequestRefresh()
    {
        if (IsDisposed || Disposing) return;

        if (InvokeRequired)
        {
            BeginInvoke(new Action(RequestRefresh));
            return;
        }

        _ = RefreshAsync();
    }

    // ── Theming ────────────────────────────────────────────────────────────

    private void OnThemeChanged(object? sender, EventArgs e) => ApplyTheme();

    private void ApplyTheme()
    {
        if (IsDisposed) return;

        ThemeManager.ApplyToControl(this);

        BackColor = AppColors.Background;
        ForeColor = AppColors.Ink;

        _lblTitle.ForeColor = AppColors.Ink;
        _lblCashbox.ForeColor = AppColors.InkMuted;
        _lblOpening.ForeColor = AppColors.InkMuted;
        _lblClosing.ForeColor = AppColors.InkMuted;

        _numOpening.BackColor = AppColors.InputBackground;
        _numOpening.ForeColor = AppColors.Ink;
        _numClosing.BackColor = AppColors.InputBackground;
        _numClosing.ForeColor = AppColors.Ink;

        _kpiHost.BackColor = AppColors.Background;
        _kpiGrid.BackColor = AppColors.Background;

        _grid.BackgroundColor = AppColors.Surface;
        _cmbCashbox.ApplyAppStyle();

        _btnOpen.ApplyTheme();
        _btnClose.ApplyTheme();
        _btnPayIn.ApplyTheme();
        _btnPayOut.ApplyTheme();

        UpdateStatusText();
    }

    // ── Data ───────────────────────────────────────────────────────────────

    private async Task LoadCashboxesAsync()
    {
        _loading = true;
        try
        {
            var boxes = await _sessionService.GetCashboxesAsync();

            _cmbCashbox.DataSource = null;
            _cmbCashbox.DisplayMember = nameof(CashboxDto.Name);
            _cmbCashbox.ValueMember = nameof(CashboxDto.Id);
            _cmbCashbox.DataSource = boxes;

            if (boxes.Count > 0)
                _cmbCashbox.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
        finally
        {
            _loading = false;
        }

        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        if (IsDisposed) return;

        // Coalesce bursts of events: one refresh at a time, then one more if
        // something arrived while the query was running.
        if (_refreshing)
        {
            _refreshPending = true;
            return;
        }

        _refreshing = true;
        try
        {
            if (_cmbCashbox.SelectedItem is not CashboxDto box)
            {
                _openSession = null;
                _summary = null;
                _grid.DataSource = null;
                UpdateKpis();
                UpdateState();
                return;
            }

            _openSession = await _sessionService.GetOpenSessionByCashboxAsync(box.Id);
            _summary = _openSession == null
                ? null
                : await _sessionService.GetSessionSummaryAsync(_openSession.Id);

            var sessions = await _sessionService.GetRecentSessionsAsync(box.Id, 20);
            _grid.DataSource = sessions.Select(ToRow).ToList();

            _numClosing.Value = Math.Clamp(
                _summary?.ExpectedBalance ?? box.CurrentBalance,
                _numClosing.Minimum,
                _numClosing.Maximum);

            UpdateKpis();
            UpdateState();
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
        finally
        {
            _refreshing = false;
            if (_refreshPending)
            {
                _refreshPending = false;
                _ = RefreshAsync();
            }
        }
    }

    private void UpdateKpis()
    {
        int digits = _regional.CurrencyDecimalDigits;
        var summary = _summary;

        _kpiTotalSales.SetValue(summary?.TotalSales ?? 0m, KpiValueKind.Money, currencyDigits: digits);
        _kpiCashSales.SetValue(summary?.CashSales ?? 0m, KpiValueKind.Money, currencyDigits: digits);
        _kpiNonCash.SetValue(summary?.NonCashSalesByMethod.Values.Sum() ?? 0m, KpiValueKind.Money, currencyDigits: digits);
        _kpiExpected.SetValue(summary?.ExpectedBalance ?? 0m, KpiValueKind.Money, currencyDigits: digits);
        _kpiOpening.SetValue(summary?.OpeningBalance ?? 0m, KpiValueKind.Money, currencyDigits: digits);
        _kpiPayIn.SetValue(summary?.PayIns ?? 0m, KpiValueKind.Money, currencyDigits: digits);
        _kpiPayOut.SetValue(summary?.PayOuts ?? 0m, KpiValueKind.Money, currencyDigits: digits);
        _kpiExpenses.SetValue(summary?.Expenses ?? 0m, KpiValueKind.Money, currencyDigits: digits);
    }

    private void UpdateState()
    {
        var hasBox = _cmbCashbox.SelectedItem is CashboxDto;
        var isOpen = _openSession != null;

        _btnOpen.Enabled = hasBox && !isOpen;
        _btnClose.Enabled = hasBox && isOpen;
        _btnPayIn.Enabled = hasBox && isOpen;
        _btnPayOut.Enabled = hasBox && isOpen;

        _cmbCashbox.Enabled = !isOpen;
        _numOpening.Enabled = !isOpen;
        _numClosing.Enabled = isOpen;

        UpdateStatusText();
    }

    private void UpdateStatusText()
    {
        if (IsDisposed) return;

        if (_openSession == null)
        {
            _lblStatus.Text = AppStrings.DrawerClosed;
            _lblStatus.ForeColor = AppColors.InkMuted;
            return;
        }

        _lblStatus.Text =
            $"{AppStrings.DrawerOpen}  ·  {_regional.ToLocal(_openSession.OpenedAtUtc):g}  ·  " +
            $"{AppStrings.Invoices}: {_summary?.InvoiceCount ?? 0}  ·  " +
            $"{AppStrings.Voids}: {_summary?.VoidCount ?? 0}";
        _lblStatus.ForeColor = AppColors.Success;
    }

    // ── Responsive KPI grid: 4 → 2 → 1 columns ─────────────────────────────

    private void RelayoutKpis()
    {
        if (IsDisposed) return;

        int width = _kpiHost.ClientSize.Width;
        if (_kpiHost.VerticalScroll.Visible)
            width -= SystemInformation.VerticalScrollBarWidth;

        if (width <= 0) return;

        int columns = width >= 900 ? 4 : width >= 460 ? 2 : 1;

        if (columns != _kpiColumns)
        {
            _kpiColumns = columns;

            var cards = new KpiCard[]
            {
                _kpiTotalSales, _kpiCashSales, _kpiNonCash, _kpiExpected,
                _kpiOpening, _kpiPayIn, _kpiPayOut, _kpiExpenses
            };

            _kpiGrid.SuspendLayout();
            _kpiGrid.Controls.Clear();
            _kpiGrid.ColumnStyles.Clear();
            _kpiGrid.RowStyles.Clear();

            _kpiGrid.ColumnCount = columns;
            for (int i = 0; i < columns; i++)
                _kpiGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / columns));

            int rows = (cards.Length + columns - 1) / columns;
            _kpiGrid.RowCount = rows;
            for (int i = 0; i < rows; i++)
                _kpiGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / rows));

            for (int i = 0; i < cards.Length; i++)
                _kpiGrid.Controls.Add(cards[i], i % columns, i / columns);

            _kpiGrid.ResumeLayout(true);
        }

        int rowsForHeight = (8 + _kpiColumns - 1) / _kpiColumns;
        _kpiGrid.Height = rowsForHeight * 140;
    }

    // ── Actions ────────────────────────────────────────────────────────────

    private async Task OpenAsync()
    {
        if (_cmbCashbox.SelectedItem is not CashboxDto box)
            return;

        try
        {
            await _sessionService.OpenSessionAsync(new OpenCashSessionDto
            {
                CashboxId = box.Id,
                OpeningBalance = _numOpening.Value
            });

            AppMessageBox.Success(AppStrings.SessionOpenedSuccess);
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }

        await RefreshAsync();
    }

    private async Task CloseAsync()
    {
        if (_openSession == null)
            return;

        if (!AppMessageBox.Confirm(
                $"{AppStrings.ConfirmCloseSession}\r\n" +
                $"{AppStrings.ExpectedBalance}: {_regional.FormatMoney(_summary?.ExpectedBalance ?? 0m)}\r\n" +
                $"{AppStrings.ClosingBalance}: {_numClosing.Value:N2}"))
            return;

        try
        {
            await _sessionService.CloseSessionAsync(new CloseCashSessionDto
            {
                SessionId = _openSession.Id,
                ClosingBalance = _numClosing.Value
            });

            AppMessageBox.Success(AppStrings.SessionClosedSuccess);
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }

        await RefreshAsync();
    }

    private async Task RecordMovementAsync(bool isPayIn)
    {
        if (_openSession == null || _cmbCashbox.SelectedItem is not CashboxDto box)
            return;

        string title = isPayIn ? AppStrings.PayIn : AppStrings.PayOut;
        string? raw = Interaction.InputBox(
            AppStrings.Amount,
            title,
            "0",
            -1,
            -1);

        if (string.IsNullOrWhiteSpace(raw))
            return;

        if (!decimal.TryParse(
                raw.Trim(),
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out var amount) ||
            amount <= 0)
        {
            AppMessageBox.Warning(AppStrings.RequiredField);
            return;
        }

        var dto = new CashMovementDto
        {
            CashboxId = box.Id,
            Amount = amount,
            ReferenceNumber = $"{(isPayIn ? "IN" : "OUT")}-{DateTime.Now:yyyyMMddHHmmss}",
            Reason = title
        };

        try
        {
            if (isPayIn)
                await _sessionService.RecordPayInAsync(dto);
            else
                await _sessionService.RecordPayOutAsync(dto);
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }

        await RefreshAsync();
    }

    private void ShowError(Exception ex)
    {
        AppMessageBox.Error(ExceptionFormatter.FriendlyMessage(ex), AppStrings.CashSessions);
    }

    // ── Grid ───────────────────────────────────────────────────────────────

    private void ApplyGridColumns()
    {
        _grid.Columns.Clear();
        _grid.Columns.AddRange(new[]
        {
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.OpenedAt, DataPropertyName = nameof(SessionRow.Opened), Width = 150 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.ClosedAt, DataPropertyName = nameof(SessionRow.Closed), Width = 150 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.OpeningBalance, DataPropertyName = nameof(SessionRow.Opening), Width = 120 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.ExpectedBalance, DataPropertyName = nameof(SessionRow.Expected), Width = 120 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.ClosingBalance, DataPropertyName = nameof(SessionRow.Closing), Width = 120 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Difference, DataPropertyName = nameof(SessionRow.Difference), Width = 110 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Active, DataPropertyName = nameof(SessionRow.Status), Width = 100 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Notes, DataPropertyName = nameof(SessionRow.Notes), Width = 200 }
        });
    }

    private SessionRow ToRow(CashSessionDto s) => new()
    {
        Opened = _regional.ToLocal(s.OpenedAtUtc).ToString("g"),
        Closed = s.ClosedAtUtc.HasValue ? _regional.ToLocal(s.ClosedAtUtc.Value).ToString("g") : "",
        Opening = s.OpeningBalance,
        Expected = s.ExpectedBalance,
        Closing = s.ClosingBalance,
        Difference = s.Difference,
        Status = s.Status == CashSessionStatus.Open ? AppStrings.SessionOpen : AppStrings.SessionClosed,
        Notes = s.Notes ?? ""
    };

    private sealed class SessionRow
    {
        public string Opened { get; init; } = "";
        public string Closed { get; init; } = "";
        public decimal Opening { get; init; }
        public decimal? Expected { get; init; }
        public decimal? Closing { get; init; }
        public decimal? Difference { get; init; }
        public string Status { get; init; } = "";
        public string Notes { get; init; } = "";
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _events.SaleCompleted -= OnSaleCompleted;
            _events.SaleVoided -= OnSaleVoided;
            _events.CashMovementAdded -= OnCashMovementEvent;
            ThemeManager.ThemeChanged -= OnThemeChanged;
            components?.Dispose();
        }

        base.Dispose(disposing);
    }
}