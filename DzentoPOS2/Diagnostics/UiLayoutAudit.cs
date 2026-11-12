using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using DzentoPOS2.App.Authorization;
using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.App.Services.Businesses;
using DzentoPOS2.App.Services.Partners;
using DzentoPOS2.App.Services.Reports.Interfaces;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.App.Session;
using EZControls.Core.Base;
using DzentoPOS2.Core.Configuration;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;
using DzentoPOS2.Shared.Contracts.DTOs.Businesses;
using DzentoPOS2.Shared.Contracts.DTOs.Partners;
using DzentoPOS2.Shared.Contracts.DTOs.Reports;
using DzentoPOS2.Shared.Helpers;
using DzentoPOS2.WinForms.Configurations;
using DzentoPOS2.WinForms.Forms.Dashboard;
using DzentoPOS2.WinForms.Forms.Businesses;
using DzentoPOS2.WinForms.Forms.Inventory;
using DzentoPOS2.WinForms.Forms.Reports;
using DzentoPOS2.WinForms.Forms.Settings;
using DzentoPOS2.WinForms.Forms.Setup;
using DzentoPOS2.WinForms.Themes;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.WinForms.Diagnostics;

/// <summary>
/// Automated layout audit that runs inside the real WinForms executable.
/// It builds the actual dialogs, lays them out at 375/768/1440 logical pixels
/// in light/dark and RTL/LTR, and fails when a control overflows its parent,
/// overlaps a sibling, clips its text, or is too small to tap.
/// No screenshots and no external harness are involved.
/// </summary>
public static class UiLayoutAudit
{
    private static readonly int[] Widths = { 375, 768, 1440 };
    private const int AuditHeight = 800;

    public static int Run()
    {
        string? themeBackup = null;
        bool themeExisted = File.Exists(AppPaths.ThemeFile);

        if (themeExisted)
            themeBackup = File.ReadAllText(AppPaths.ThemeFile);

        var scenarios = new List<ScenarioResult>();

        try
        {
            using var database = new AuditDatabase();

            foreach (ThemeMode theme in new[] { ThemeMode.Light, ThemeMode.Dark })
            {
                ThemeManager.SetMode(theme);

                foreach (bool rtl in new[] { true, false })
                {
                    try
                    {
                        AuditSmoke(rtl);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"FAIL dashboard smoke [{theme}/{rtl}] {ex.GetType().Name}: {ex.Message}");
                        Console.WriteLine(ex.StackTrace);
                        return 1;
                    }

                    var forms = CreateForms(database.Context);

                    try
                    {
                        foreach (int width in Widths)
                        {
                            foreach (Form form in forms)
                                scenarios.Add(Audit(form, theme, rtl, width));
                        }
                    }
                    finally
                    {
                        foreach (Form form in forms)
                            form.Dispose();
                    }
                }
            }
        }
        finally
        {
            // SetMode writes the real theme.config; leave the user's file as it was.
            ThemeManager.SetMode(ThemeMode.Light);

            if (themeExisted && themeBackup is not null)
                File.WriteAllText(AppPaths.ThemeFile, themeBackup);
            else if (!themeExisted && File.Exists(AppPaths.ThemeFile))
                File.Delete(AppPaths.ThemeFile);
        }

        var violations = scenarios.Sum(s => s.Violations.Count);
        var report = new AuditReport(
            DateTimeOffset.Now,
            scenarios.Count,
            violations,
            scenarios);

        string reportPath = Path.Combine(
            Environment.CurrentDirectory, "docs", "evidence", "ui-layout-audit.json");

        Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
        File.WriteAllText(
            reportPath,
            JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));

        Console.WriteLine($"UI layout audit: {scenarios.Count} scenarios, {violations} violations");
        Console.WriteLine($"Controls measured: {scenarios.Sum(s => s.ControlsMeasured)}");
        Console.WriteLine($"Forms: variants, barcodes, unit edit, unit list, branch edit, branch resources, branch list, cross-branch report, first-run wizard, settings");
        Console.WriteLine($"Smoke (built + refreshed, not scored): dashboard");
        Console.WriteLine($"Widths: {string.Join(", ", Widths)} | heights: {AuditHeight} | themes: Light, Dark | direction: RTL, LTR");
        Console.WriteLine($"Report: {reportPath}");

        foreach (var scenario in scenarios.Where(s => s.Violations.Count > 0))
        {
            Console.WriteLine($"FAIL {scenario.Form} [{scenario.Theme}/{scenario.Direction}/{scenario.Width}px]");
            foreach (var violation in scenario.Violations)
                Console.WriteLine($"  - {violation.Control}: {violation.Kind} ({violation.Detail})");
        }

        return violations == 0 ? 0 : 1;
    }

    private static List<Form> CreateForms(AppDbContext context) =>
    [
        new ProductVariantsDialog(context, Guid.NewGuid()),
        new ProductBarcodesDialog(new StubBarcodeService(), Guid.NewGuid()),
        new ProductUnitEditDialog(new StubUnitService(), new StubLanguageService()),
        new ProductUnitsForm(new StubUnitService(), new StubLanguageService()),
        new DigitalCodesForm(new StubDigitalCodeService(), new StubLanguageService()),
        new BranchEditDialog(new StubBranchService(), new StubLanguageService()),
        new BranchResourcesDialog(new StubBranchResourceService(), new StubLanguageService(), new BranchDto { Name = "Branch" }),
        new BranchesForm(new StubBranchService(), new StubBranchResourceService(), new StubLanguageService()),
        new CrossBranchReportForm(new StubCrossBranchReportService(), new StubAuthorizationGuard(), new StubLanguageService()),
        new FirstRunModulesDialog(new StubCapabilityService(), new StubBackupService(), new StubActiveBranchContext()),
        new SettingsForm(
            new StubCurrentSettingsService(),
            new StubLanguageService(),
            new StubBackupService(),
            new EmptyServiceProvider())
    ];

    /// <summary>
    /// Forms that are built and refreshed (to catch crashes in
    /// <c>Build()</c>/<c>Update()</c>) but are not part of the responsive
    /// layout scoring yet. The dashboard is included here because it exercises
    /// every KPI/chart/insight module; add it to <see cref="CreateForms"/> once
    /// its narrow-width layout is made responsive.
    /// </summary>
    private static List<Form> CreateSmokeForms() =>
    [
        new DashboardForm(
            new StubDashboardService(),
            new StubCustomerService(),
            new StubLanguageService(),
            new RegionalContext())
    ];

    /// <summary>
    /// Runs each smoke form's real load path (which builds and refreshes the
    /// dashboard modules) so any exception in <c>Build()</c>/<c>Update()</c>
    /// fails the audit. <see cref="Form.OnLoad"/> is invoked directly instead of
    /// <see cref="Form.Show"/> on purpose: exceptions raised while a native
    /// window message is being dispatched are swallowed by
    /// <c>Application.ThreadException</c> (and would only pop a modal error
    /// dialog), which would make the audit pass regardless.
    /// </summary>
    private static void AuditSmoke(bool rtl)
    {
        var onLoad = typeof(Form).GetMethod(
            "OnLoad",
            BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Form.OnLoad was not found.");

        foreach (Form form in CreateSmokeForms())
        {
            try
            {
                form.RightToLeft = rtl ? RightToLeft.Yes : RightToLeft.No;
                form.RightToLeftLayout = rtl;

                try
                {
                    onLoad.Invoke(form, [EventArgs.Empty]);
                }
                catch (TargetInvocationException ex) when (ex.InnerException is not null)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                }
            }
            finally
            {
                form.Dispose();
            }
        }
    }

    private static ScenarioResult Audit(Form form, ThemeMode theme, bool rtl, int width)
    {
        var violations = new List<Violation>();

        form.RightToLeft = rtl ? RightToLeft.Yes : RightToLeft.No;
        form.RightToLeftLayout = rtl;
        form.StartPosition = FormStartPosition.Manual;
        form.Location = new Point(-32000, -32000);
        form.ShowInTaskbar = false;
        form.MinimumSize = new Size(320, 240);
        form.ClientSize = new Size(width, AuditHeight);

        // The form must be visible for child.Visible (and therefore the audit)
        // to reflect the real layout. It is shown far off-screen and hidden
        // again immediately; no screenshot and no user-visible window.
        form.CreateControl();
        form.Show();
        form.ClientSize = new Size(width, AuditHeight);
        form.PerformLayout();
        LayoutRecursively(form);
        Application.DoEvents();

        if (form.ClientSize.Width != width)
        {
            violations.Add(new Violation(
                form.GetType().Name,
                "form-clamped",
                $"requested {width}px, actual {form.ClientSize.Width}px"));
        }

        InspectContainer(form, form.GetType().Name, violations);
        CheckSiblingOverlaps(form, violations);

        int controlsMeasured = CountControls(form) + AuditOtherTabPages(form, violations);

        form.Hide();

        return new ScenarioResult(
            form.GetType().Name,
            theme.ToString(),
            rtl ? "RTL" : "LTR",
            width,
            AuditHeight,
            controlsMeasured,
            violations);
    }

    private static void LayoutRecursively(Control control)
    {
        control.PerformLayout();

        foreach (Control child in control.Controls)
            LayoutRecursively(child);
    }

    private static void InspectContainer(Control parent, string formName, List<Violation> violations)
    {
        // Composite EZ inputs own an inner TextBox; it is not an independent
        // tap target, so measuring it would be a false positive.
        if (parent is EZInputBase)
            return;

        foreach (Control child in parent.Controls)
        {
            if (parent is TabControl && child is TabPage)
            {
                InspectContainer(child, formName, violations);
                continue;
            }

            if (!child.Visible || child.Width <= 0 || child.Height <= 0)
                continue;

            if (parent is DataGridView)
                continue;

            var bounds = child.Bounds;

            bool scrollable = parent is ScrollableControl { AutoScroll: true };

            if (!scrollable && bounds.Right > parent.ClientSize.Width + 1)
            {
                violations.Add(new Violation(
                    Describe(child),
                    "overflow-right",
                    $"{bounds.Right} > {parent.ClientSize.Width} in {Describe(parent)}"));
            }

            if (!scrollable && bounds.Bottom > parent.ClientSize.Height + 1)
            {
                violations.Add(new Violation(
                    Describe(child),
                    "overflow-bottom",
                    $"{bounds.Bottom} > {parent.ClientSize.Height} in {Describe(parent)}"));
            }

            if (child is Label or Button or CheckBox or RadioButton && !child.AutoSize)
            {
                int available = child.ClientSize.Width - child.Padding.Horizontal;

                if (available > 0 && !string.IsNullOrEmpty(child.Text))
                {
                    int required = TextRenderer.MeasureText(
                        child.Text,
                        child.Font,
                        new Size(int.MaxValue, int.MaxValue),
                        TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Width;

                    if (required > available + 2)
                    {
                        violations.Add(new Violation(
                            Describe(child),
                            "text-clipped",
                            $"needs {required}px, has {available}px"));
                    }
                }
            }

            if (child is Button or TextBox or NumericUpDown or ComboBox or CheckBox)
            {
                int minHeight = child is Button ? 32 : 28;

                if (child.Height < minHeight)
                {
                    violations.Add(new Violation(
                        Describe(child),
                        "tap-target",
                        $"height {child.Height}px < {minHeight}px in {child.Parent?.GetType().Name}"));
                }

                if (child.Width < 40 && child is not CheckBox)
                {
                    violations.Add(new Violation(
                        Describe(child),
                        "tap-target",
                        $"width {child.Width}px < 40px"));
                }
            }

            if (child is not DataGridView and not TabControl)
                InspectContainer(child, formName, violations);
        }
    }

    private static void CheckSiblingOverlaps(Control parent, List<Violation> violations)
    {
        if (parent is not TableLayoutPanel and not FlowLayoutPanel)
        {
            foreach (Control child in parent.Controls)
                CheckSiblingOverlaps(child, violations);

            return;
        }

        var children = parent.Controls
            .Cast<Control>()
            .Where(c => c.Visible && c.Width > 0 && c.Height > 0)
            .ToList();

        for (int i = 0; i < children.Count; i++)
        {
            for (int j = i + 1; j < children.Count; j++)
            {
                var intersection = Rectangle.Intersect(children[i].Bounds, children[j].Bounds);

                if (intersection.Width * intersection.Height > 8)
                {
                    violations.Add(new Violation(
                        Describe(children[i]),
                        "sibling-overlap",
                        $"overlaps {Describe(children[j])} by {intersection.Width}x{intersection.Height}"));
                }
            }
        }

        foreach (Control child in parent.Controls)
            CheckSiblingOverlaps(child, violations);
    }

    /// <summary>
/// A TabControl lays out only its selected page. Select every other page in
/// turn so a hidden tab cannot pass the audit unmeasured.
/// </summary>
private static int AuditOtherTabPages(Control root, List<Violation> violations)
{
    int controlsMeasured = 0;

    foreach (TabControl tabs in FindAll<TabControl>(root))
    {
        int original = tabs.SelectedIndex;

        for (int index = 0; index < tabs.TabCount; index++)
        {
            if (index == original)
                continue;

            tabs.SelectedIndex = index;
            tabs.PerformLayout();
            Application.DoEvents();

            TabPage page = tabs.TabPages[index];
            InspectContainer(page, tabs.GetType().Name, violations);
            CheckSiblingOverlaps(page, violations);
            controlsMeasured += CountControls(page);
        }

        tabs.SelectedIndex = original;
    }

    return controlsMeasured;
}

private static IEnumerable<T> FindAll<T>(Control root) where T : Control
{
    foreach (Control child in root.Controls)
    {
        if (child is T match)
            yield return match;

        foreach (T nested in FindAll<T>(child))
            yield return nested;
    }
}

private static int CountControls(Control parent)
    {
        if (parent is EZInputBase)
            return 0;

        int count = 0;

        foreach (Control child in parent.Controls)
        {
            if (!child.Visible || child.Width <= 0 || child.Height <= 0)
                continue;

            count++;
            count += CountControls(child);
        }

        return count;
    }

    private static string Describe(Control control)
    {
        string text = string.IsNullOrWhiteSpace(control.Text)
            ? string.Empty
            : $" \"{control.Text}\"";

        return $"{control.GetType().Name}{text}";
    }

    private sealed record AuditReport(
        DateTimeOffset GeneratedAt,
        int ScenarioCount,
        int ViolationCount,
        IReadOnlyList<ScenarioResult> Scenarios);

    private sealed record ScenarioResult(
        string Form,
        string Theme,
        string Direction,
        int Width,
        int Height,
        int ControlsMeasured,
        IReadOnlyList<Violation> Violations);

    private sealed record Violation(string Control, string Kind, string Detail);

    private sealed class AuditDatabase : IDisposable
    {
        public AuditDatabase()
        {
            string path = Path.Combine(
                Path.GetTempPath(),
                $"dzento-layout-audit-{Guid.NewGuid():N}.db");

            FilePath = path;

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={path}")
                .Options;

            Context = new AppDbContext(options);
            Context.Database.EnsureCreated();
        }

        public string FilePath { get; }

        public AppDbContext Context { get; }

        public void Dispose()
        {
            Context.Dispose();

            try
            {
                if (File.Exists(FilePath))
                    File.Delete(FilePath);
            }
            catch (IOException)
            {
            }
        }
    }

    private sealed class StubBarcodeService : IBarcodeService
    {
        public Task<Guid> GetDefaultVariantIdAsync(Guid productId) =>
            Task.FromResult(Guid.NewGuid());

        public Task<List<string>> GetBarcodesAsync(Guid variantId) =>
            Task.FromResult(new List<string>());

        public Task<bool> IsUniqueAsync(string barcode, Guid? excludeId = null) =>
            Task.FromResult(true);

        public Task AddAsync(Guid variantId, string barcode, bool isPrimary) =>
            Task.CompletedTask;

        public Task RemoveAsync(Guid barcodeId) => Task.CompletedTask;

        public Task RemoveByValueAsync(Guid variantId, string barcode) => Task.CompletedTask;

        public string GenerateInternal() => "2000000000000";

        public Task<Guid?> FindVariantIdByBarcodeAsync(string barcode) =>
            Task.FromResult<Guid?>(null);

        public Task<List<Guid>> FindVariantIdsByBarcodeAsync(string barcode) =>
            Task.FromResult(new List<Guid>());
    }

    private sealed class StubDashboardService : IDashboardService
    {
        public Task<DashboardSummaryDto> GetSummaryAsync(DashboardFilterDto? filter = null) =>
            Task.FromResult(new DashboardSummaryDto());
    }

    private sealed class StubCustomerService : ICustomerService
    {
        public Task<List<CustomerDto>> GetAllAsync() =>
            Task.FromResult(new List<CustomerDto>());

        public Task<CustomerDto?> GetByIdAsync(Guid id) =>
            Task.FromResult<CustomerDto?>(null);

        public Task<List<CustomerDto>> SearchAsync(string? term) =>
            Task.FromResult(new List<CustomerDto>());

        public Task<CustomerDto> CreateAsync(CreateCustomerDto dto) =>
            throw new NotSupportedException();

        public Task<CustomerDto> UpdateAsync(UpdateCustomerDto dto) =>
            throw new NotSupportedException();

        public Task DeleteAsync(Guid id) => Task.CompletedTask;
    }

    private sealed class StubUnitService : IProductUnitService
    {
        public Task<List<ProductUnitDto>> GetAllAsync() =>
            Task.FromResult(new List<ProductUnitDto>());

        public Task<ProductUnitDto?> GetByIdAsync(Guid id) =>
            Task.FromResult<ProductUnitDto?>(null);

        public Task<List<ProductUnitDto>> SearchAsync(string? term) =>
            Task.FromResult(new List<ProductUnitDto>());

        public Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null) =>
            Task.FromResult(false);

        public Task<bool> ExistsBySymbolAsync(string symbol, Guid? excludeId = null) =>
            Task.FromResult(false);

        public Task<List<ProductUnitDto>> GetBaseUnitsAsync() =>
            Task.FromResult(new List<ProductUnitDto>());

        public Task<ProductUnitDto> CreateAsync(CreateProductUnitDto dto) =>
            Task.FromResult(new ProductUnitDto { Name = dto.Name, Symbol = dto.Symbol, IsBaseUnit = dto.IsBaseUnit });

        public Task<ProductUnitDto> UpdateAsync(UpdateProductUnitDto dto) =>
            Task.FromResult(new ProductUnitDto { Id = dto.Id, Name = dto.Name, Symbol = dto.Symbol, IsBaseUnit = dto.IsBaseUnit });

        public Task DeleteAsync(Guid id) => Task.CompletedTask;
    }

    private sealed class StubDigitalCodeService : IDigitalCodeService
    {
        public Task<List<DigitalVariantOptionDto>> GetDigitalVariantsAsync(CancellationToken ct = default) =>
            Task.FromResult(new List<DigitalVariantOptionDto>());

        public Task<List<DigitalCodeDto>> SearchAsync(
            Guid? variantId,
            DigitalCodeStatus? status,
            string? term,
            CancellationToken ct = default) =>
            Task.FromResult(new List<DigitalCodeDto>());

        public Task<DigitalCodeImportResultDto> ImportAsync(
            Guid variantId,
            IEnumerable<string> codes,
            CancellationToken ct = default) =>
            Task.FromResult(new DigitalCodeImportResultDto());

        public Task<int> GetAvailableCountAsync(Guid variantId, CancellationToken ct = default) =>
            Task.FromResult(0);

        public Task<List<DigitalCode>> AllocateForSaleAsync(
            Guid variantId,
            Guid salesInvoiceId,
            Guid salesInvoiceItemId,
            int quantity,
            DateTime deliveredAt,
            CancellationToken ct = default) =>
            Task.FromResult(new List<DigitalCode>());

        public Task<int> ReturnForSaleAsync(Guid salesInvoiceId, CancellationToken ct = default) =>
            Task.FromResult(0);

        public Task CancelAsync(Guid id, CancellationToken ct = default) =>
            Task.CompletedTask;
    }

    private sealed class StubBranchService : IBranchService    {
        public Task<List<BranchListItemDto>> GetAllAsync() =>
            Task.FromResult(new List<BranchListItemDto>());

        public Task<BranchDto?> GetByIdAsync(Guid id) =>
            Task.FromResult<BranchDto?>(null);

        public Task<BranchDto> CreateAsync(CreateBranchDto dto) =>
            Task.FromResult(new BranchDto { Name = dto.Name, IsActive = dto.IsActive });

        public Task<BranchDto> UpdateAsync(UpdateBranchDto dto) =>
            Task.FromResult(new BranchDto { Id = dto.Id, Name = dto.Name });

        public Task<BranchDto> SetActiveAsync(Guid id, bool isActive) =>
            Task.FromResult(new BranchDto { Id = id, IsActive = isActive });
    }

    private sealed class StubBranchResourceService : IBranchResourceService
    {
        public Task<List<WarehouseListItemDto>> GetWarehousesAsync(Guid branchId) =>
            Task.FromResult(new List<WarehouseListItemDto>());

        public Task<List<CashboxListItemDto>> GetCashboxesAsync(Guid branchId) =>
            Task.FromResult(new List<CashboxListItemDto>());

        public Task<WarehouseListItemDto> CreateWarehouseAsync(CreateWarehouseDto dto) =>
            Task.FromResult(new WarehouseListItemDto { BranchId = dto.BranchId, Name = dto.Name, Location = dto.Location, IsActive = dto.IsActive });

        public Task<CashboxListItemDto> CreateCashboxAsync(CreateCashboxDto dto) =>
            Task.FromResult(new CashboxListItemDto { BranchId = dto.BranchId, Name = dto.Name, IsActive = dto.IsActive });

        public Task<WarehouseListItemDto> SetWarehouseActiveAsync(Guid id, bool isActive) =>
            Task.FromResult(new WarehouseListItemDto { Id = id, IsActive = isActive });

        public Task<CashboxListItemDto> SetCashboxActiveAsync(Guid id, bool isActive) =>
            Task.FromResult(new CashboxListItemDto { Id = id, IsActive = isActive });
    }

    private sealed class StubCrossBranchReportService : ICrossBranchReportService
    {
        public Task<CrossBranchReportDto> GetAsync(CrossBranchReportFilterDto? filter = null) =>
            Task.FromResult(new CrossBranchReportDto());
    }

    private sealed class StubAuthorizationGuard : IAuthorizationGuard
    {
        public Task<bool> IsAllowedAsync(string permissionCode) => Task.FromResult(true);

        public Task DemandAsync(string permissionCode) => Task.CompletedTask;
    }

    private sealed class StubLanguageService : ILanguageService
    {
        public string CurrentLanguage => "ar";

        public event EventHandler<string>? LanguageChanged;

        public void SetLanguage(string culture) => LanguageChanged?.Invoke(this, culture);

        public bool IsRightToLeft() => true;
    }

    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private sealed class StubModuleService : IModuleService
    {
        public Task<bool> IsEnabledAsync(ActivityModule module) => Task.FromResult(false);

        public Task SetEnabledAsync(ActivityModule module, bool enabled) => Task.CompletedTask;

        public Task<bool> IsConfiguredAsync() => Task.FromResult(false);

        public Task SetConfiguredAsync(bool configured) => Task.CompletedTask;

        public Task<Dictionary<ActivityModule, bool>> GetAllAsync() =>
            Task.FromResult(new Dictionary<ActivityModule, bool>());
    }

    private sealed class StubCapabilityService : ICapabilityService
    {
        public Task<bool> HasCapabilityAsync(BranchCapability capability, CancellationToken cancellationToken = default) =>
            Task.FromResult(true);
        public Task<BranchCapability> GetCapabilitiesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(BranchCapability.None);
        public Task SetCapabilitiesAsync(Guid branchId, BranchCapability capabilities, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class StubActiveBranchContext : IActiveBranchContext
    {
        public Guid ActiveBranchId => Guid.Empty;
        public string ActiveBranchName => "Stub";
        public bool CanSwitchBranch => true;
        public event EventHandler<ActiveBranchChangedEventArgs>? BranchChanged
        {
            add { }
            remove { }
        }
        public Task<bool> TrySwitchBranchAsync(Guid branchId, CancellationToken cancellationToken = default) =>
            Task.FromResult(true);
        public Task InitializeAsync(ICurrentUserService currentUser, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
        public void InitializeEmpty() { }
    }

    private sealed class StubCurrentSettingsService : ICurrentSettingsService
    {
        public AppSettings Current { get; } = new();

        public RegionalContext Regional { get; } = new();

        public event EventHandler? Changed;

        public void Save(AppSettings settings)
        {
            Current.Language = settings.Language;
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public void Update(Action<AppSettings> mutate) => mutate(Current);
    }

    private sealed class StubBackupService : IBackupService
    {
        private readonly string _directory =
            Path.Combine(Path.GetTempPath(), "DzentoPOS2.LayoutAudit.Backups");

        public string BackupDirectory => _directory;

        public void SetBackupDirectory(string directory)
        {
        }

        public Task<BackupFile> CreateBackupAsync(CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<BackupFile?> CreateAutomaticBackupIfDueAsync(
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public IReadOnlyList<BackupFile> ListBackups() => Array.Empty<BackupFile>();

        public int ApplyRetention(int keep = 7) => 0;

        public Task ValidateBackupAsync(
            string backupPath,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<string> RestoreAsync(
            string backupPath,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}