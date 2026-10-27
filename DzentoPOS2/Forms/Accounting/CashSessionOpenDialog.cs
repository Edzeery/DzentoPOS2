using DzentoPOS2.App.Diagnostics;
using DzentoPOS2.App.Services.Accounting;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Accounting;
using DzentoPOS2.Shared.Helpers;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Forms.Accounting;

/// <summary>
/// Modal shown when the drawer must be opened (after sign-in, from the POS
/// banner or from the cash-drawer page). Layout lives in the designer; this
/// file only contains behaviour and runtime theming/localization.
/// </summary>
public partial class CashSessionOpenDialog : LocalizableFormBase
{
    private readonly ICashSessionService _sessionService;
    private readonly RegionalContext _regional;
    private bool _busy;

    public CashSessionOpenDialog(
        ICashSessionService sessionService,
        ILanguageService languageService,
        RegionalContext regional)
        : base(languageService)
    {
        _sessionService = sessionService;
        _regional = regional;

        InitializeComponent();

        BackColor = AppColors.Background;
        numOpening.Maximum = 999_999_999m;
        numOpening.DecimalPlaces = _regional.CurrencyDecimalDigits;

        btnOpen.Click += async (_, _) => await OpenAsync();
        btnCancel.Click += (_, _) => CloseDialog(DialogResult.Cancel);

        ThemeManager.ThemeChanged += OnThemeChanged;
        ApplyTheme();
    }

    /// <summary>The session that was opened, or <c>null</c> when cancelled.</summary>
    public CashSessionDto? OpenedSession { get; private set; }

    protected override async void OnInitialized()
    {
        await LoadCashboxesAsync();
        numOpening.Select(0, numOpening.Text.Length);
        numOpening.Focus();
    }

    protected override void LoadText()
    {
        Text = AppStrings.OpenCashDrawer;
        lblTitle.Text = AppStrings.OpenCashDrawer;
        lblCashbox.Text = AppStrings.Cashbox;
        lblOpening.Text = AppStrings.OpeningBalance;
        lblNotes.Text = AppStrings.Notes;
        btnOpen.Text = AppStrings.OpenSession;
        btnCancel.Text = AppStrings.Cancel;
    }

    private void OnThemeChanged(object? sender, EventArgs e) => ApplyTheme();

    private void ApplyTheme()
    {
        if (IsDisposed) return;

        ThemeManager.ApplyToControl(this);

        BackColor = AppColors.Background;
        ForeColor = AppColors.Ink;

        lblTitle.ForeColor = AppColors.Ink;
        lblCashbox.ForeColor = AppColors.InkMuted;
        lblOpening.ForeColor = AppColors.InkMuted;
        lblNotes.ForeColor = AppColors.InkMuted;

        pnlFooter.BackColor = AppColors.Surface;

        txtNotes.BackColor = AppColors.InputBackground;
        txtNotes.ForeColor = AppColors.Ink;

        numOpening.BackColor = AppColors.InputBackground;
        numOpening.ForeColor = AppColors.Ink;

        lblError.ForeColor = AppColors.Danger;

        cmbCashbox.ApplyAppStyle();
        btnOpen.ApplyTheme();
        btnCancel.ApplyTheme();
    }

    private async Task LoadCashboxesAsync()
    {
        try
        {
            var boxes = await _sessionService.GetCashboxesAsync();

            cmbCashbox.DataSource = null;
            cmbCashbox.DisplayMember = nameof(CashboxDto.Name);
            cmbCashbox.ValueMember = nameof(CashboxDto.Id);
            cmbCashbox.DataSource = boxes;

            btnOpen.Enabled = boxes.Count > 0;

            if (boxes.Count == 0)
                ShowError(AppStrings.NoDataAvailable);
        }
        catch (Exception ex)
        {
            ShowError(ExceptionFormatter.FriendlyMessage(ex));
        }
    }

    private async Task OpenAsync()
    {
        if (_busy) return;

        lblError.Visible = false;

        if (cmbCashbox.SelectedItem is not CashboxDto box)
        {
            ShowError(AppStrings.RequiredField);
            return;
        }

        _busy = true;
        btnOpen.Enabled = false;
        try
        {
            OpenedSession = await _sessionService.OpenSessionAsync(new OpenCashSessionDto
            {
                CashboxId = box.Id,
                OpeningBalance = numOpening.Value,
                Notes = txtNotes.Text
            });

            CloseDialog(DialogResult.OK);
        }
        catch (Exception ex)
        {
            ShowError(ExceptionFormatter.FriendlyMessage(ex));
        }
        finally
        {
            _busy = false;
            btnOpen.Enabled = true;
        }
    }

    private void ShowError(string message)
    {
        lblError.Text = message;
        lblError.Visible = true;
    }

    private void CloseDialog(DialogResult result)
    {
        DialogResult = result;
        Close();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ThemeManager.ThemeChanged -= OnThemeChanged;
            components?.Dispose();
        }

        base.Dispose(disposing);
    }
}