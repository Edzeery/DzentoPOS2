using DzentoPOS2.App.Services.Accounting;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Accounting;
using DzentoPOS2.WinForms.Controls;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Forms.Accounting;

public partial class CashSessionForm : LocalizableFormBase
{
    private readonly ICashSessionService _sessionService;
    private readonly Panel _topPanel;
    private readonly Label _lblCashbox;
    private readonly ComboBox _cboCashbox;
    private readonly Label _lblOpening;
    private readonly NumericUpDown _numOpening;
    private readonly ThemedButton _btnOpen;
    private readonly Label _lblClosing;
    private readonly NumericUpDown _numClosing;
    private readonly ThemedButton _btnClose;
    private readonly Label _lblStatus;
    private readonly DataGridView _grid;
    private CashSessionDto? _openSession;
    private bool _loading;

    public CashSessionForm(
        ICashSessionService sessionService,
        ILanguageService languageService)
        : base(languageService)
    {
        _sessionService = sessionService;
        Size = new Size(1100, 650);
        BackColor = AppColors.Background;
        StartPosition = FormStartPosition.CenterParent;

        _topPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 112,
            BackColor = AppColors.Surface,
            Padding = new Padding(16, 10, 16, 10)
        };

        _lblCashbox = new Label { Location = new Point(16, 10), Size = new Size(220, 20), Font = new Font("Segoe UI", 9F) };
        _cboCashbox = new ComboBox
        {
            Location = new Point(16, 32),
            Size = new Size(220, 28),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10F),
            Name = "cboCashbox"
        };
        _cboCashbox.SelectedIndexChanged += async (_, _) =>
        {
            if (!_loading)
                await RefreshAsync();
        };

        _lblOpening = new Label { Location = new Point(252, 10), Size = new Size(140, 20), Font = new Font("Segoe UI", 9F) };
        _numOpening = new NumericUpDown
        {
            Location = new Point(252, 32),
            Size = new Size(140, 28),
            DecimalPlaces = 2,
            Maximum = 999999999m,
            ThousandsSeparator = true,
            Font = new Font("Segoe UI", 10F),
            Name = "numOpening"
        };

        _btnOpen = ButtonFactory.Primary(AppStrings.OpenCashbox);
        _btnOpen.Location = new Point(404, 28);
        _btnOpen.Size = new Size(150, 36);
        _btnOpen.Name = "btnOpenCashbox";
        _btnOpen.Click += async (_, _) => await OpenAsync();

        _lblClosing = new Label { Location = new Point(574, 10), Size = new Size(140, 20), Font = new Font("Segoe UI", 9F) };
        _numClosing = new NumericUpDown
        {
            Location = new Point(574, 32),
            Size = new Size(140, 28),
            DecimalPlaces = 2,
            Maximum = 999999999m,
            Minimum = -999999999m,
            ThousandsSeparator = true,
            Font = new Font("Segoe UI", 10F),
            Name = "numClosing"
        };

        _btnClose = ButtonFactory.Neutral(AppStrings.CloseCashbox);
        _btnClose.Location = new Point(726, 28);
        _btnClose.Size = new Size(150, 36);
        _btnClose.Name = "btnCloseCashbox";
        _btnClose.Enabled = false;
        _btnClose.Click += async (_, _) => await CloseAsync();

        _lblStatus = new Label
        {
            Location = new Point(16, 76),
            Size = new Size(1050, 24),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = AppColors.InkMuted,
            Name = "lblSessionStatus"
        };

        _topPanel.Controls.AddRange(new Control[]
        {
            _lblCashbox, _cboCashbox, _lblOpening, _numOpening, _btnOpen,
            _lblClosing, _numClosing, _btnClose, _lblStatus
        });

        _grid = new DataGridView { Dock = DockStyle.Fill, Name = "gridSessions" };
        DataGridViewStyleHelper.ApplyModernStyle(_grid);
        _grid.AutoGenerateColumns = false;

        Controls.Add(_grid);
        Controls.Add(_topPanel);
    }

    protected override async void OnInitialized()
    {
        await LoadCashboxesAsync();
    }

    protected override void LoadText()
    {
        Text = AppStrings.CashSessions;
        _lblCashbox.Text = AppStrings.Cashbox;
        _lblOpening.Text = AppStrings.OpeningBalance;
        _lblClosing.Text = AppStrings.ClosingBalance;
        _btnOpen.Text = AppStrings.OpenCashbox;
        _btnClose.Text = AppStrings.CloseCashbox;
        ApplyGridColumns();
    }

    protected override async void OnLanguageChangedAfterLoadText()
    {
        await RefreshAsync();
    }

    private void ApplyGridColumns()
    {
        _grid.Columns.Clear();
        _grid.Columns.AddRange(new[]
        {
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.OpenedAt, DataPropertyName = "Opened", Width = 150 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.ClosedAt, DataPropertyName = "Closed", Width = 150 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.OpeningBalance, DataPropertyName = "Opening", Width = 120 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.ExpectedBalance, DataPropertyName = "Expected", Width = 120 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.ClosingBalance, DataPropertyName = "Closing", Width = 120 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Difference, DataPropertyName = "Difference", Width = 110 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Active, DataPropertyName = "Status", Width = 100 },
            new DataGridViewTextBoxColumn { HeaderText = AppStrings.Notes, DataPropertyName = "Notes", Width = 200 }
        });
    }

    private async Task LoadCashboxesAsync()
    {
        _loading = true;
        try
        {
            var boxes = await _sessionService.GetCashboxesAsync();
            _cboCashbox.DataSource = null;
            _cboCashbox.DisplayMember = nameof(CashboxDto.Name);
            _cboCashbox.ValueMember = nameof(CashboxDto.Id);
            _cboCashbox.DataSource = boxes;
        }
        finally
        {
            _loading = false;
        }

        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        if (_cboCashbox.SelectedItem is not CashboxDto box)
        {
            _openSession = null;
            _grid.DataSource = null;
            UpdateState();
            return;
        }

        _openSession = await _sessionService.GetOpenSessionByCashboxAsync(box.Id);
        var sessions = await _sessionService.GetRecentSessionsAsync(box.Id, 20);

        _grid.DataSource = sessions.Select(ToRow).ToList();
        _numClosing.Value = Math.Clamp(
            _openSession != null ? box.CurrentBalance : 0m,
            _numClosing.Minimum,
            _numClosing.Maximum);

        UpdateState();
    }

    private void UpdateState()
    {
        var hasBox = _cboCashbox.SelectedItem is CashboxDto;
        var isOpen = _openSession != null;

        _btnOpen.Enabled = hasBox && !isOpen;
        _btnClose.Enabled = hasBox && isOpen;
        _cboCashbox.Enabled = !isOpen;
        _numOpening.Enabled = !isOpen;
        _numClosing.Enabled = isOpen;

        if (!hasBox)
            _lblStatus.Text = AppStrings.NoOpenSession;
        else if (_openSession == null)
            _lblStatus.Text = AppStrings.NoOpenSession;
        else
            _lblStatus.Text = $"{AppStrings.SessionOpen} • {AppStrings.OpeningBalance}: {_openSession.OpeningBalance:N2} • {AppStrings.Date}: {_openSession.OpenedAtUtc.ToLocalTime():g}";
    }

    private async Task OpenAsync()
    {
        if (_cboCashbox.SelectedItem is not CashboxDto box)
            return;

        try
        {
            await _sessionService.OpenSessionAsync(new OpenCashSessionDto
            {
                CashboxId = box.Id,
                OpeningBalance = _numOpening.Value
            });
        }
        catch (InvalidOperationException)
        {
            MessageBox.Show(
                AppStrings.SessionAlreadyOpen,
                AppStrings.CashSessions,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, AppStrings.CashSessions, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        await RefreshAsync();
    }

    private async Task CloseAsync()
    {
        if (_openSession == null)
            return;

        var confirm = MessageBox.Show(
            $"{AppStrings.CloseCashbox}\n{AppStrings.ClosingBalance}: {_numClosing.Value:N2}",
            AppStrings.CashSessions,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes)
            return;

        try
        {
            await _sessionService.CloseSessionAsync(new CloseCashSessionDto
            {
                SessionId = _openSession.Id,
                ClosingBalance = _numClosing.Value
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, AppStrings.CashSessions, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        await RefreshAsync();
    }

    private static SessionRow ToRow(CashSessionDto s) => new()
    {
        Opened = s.OpenedAtUtc.ToLocalTime().ToString("g"),
        Closed = s.ClosedAtUtc?.ToLocalTime().ToString("g") ?? "",
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
}
