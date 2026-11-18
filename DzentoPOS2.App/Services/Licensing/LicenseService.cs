using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.Core.Entities.Licensing;
using DzentoPOS2.Core.Exceptions;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Licensing;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Shared.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DzentoPOS2.App.Services.Licensing;

/// <summary>
/// Default <see cref="ILicenseService"/>. Reads the single stored licence,
/// validates signed keys against the configured public key and enforces the
/// progressive restriction model: warn → block creation at the limit →
/// read-only once the licence and its grace period have both lapsed.
/// </summary>
public sealed class LicenseService : ILicenseService
{
    private readonly AppDbContext _context;
    private readonly LicenseOptions _options;
    private readonly IDeviceIdentifier _device;
    private readonly IBusinessClock _clock;
    private readonly ILogger<LicenseService>? _logger;

    public LicenseService(
        AppDbContext context,
        LicenseOptions options,
        IDeviceIdentifier device,
        IBusinessClock clock,
        ILogger<LicenseService>? logger = null)
    {
        _context = context;
        _options = options;
        _device = device;
        _clock = clock;
        _logger = logger;
    }

    public async Task<LicenseStatus> GetStatusAsync(CancellationToken ct = default)
    {
        var now = _clock.UtcNow;
        var deviceId = _device.GetDeviceId();

        var license = await _context.Licenses
            .AsNoTracking()
            .Include(l => l.LicensePlan)
            .Where(l => !l.IsDeleted && l.IsActive)
            .OrderByDescending(l => l.ExpiryDate)
            .FirstOrDefaultAsync(ct);

        if (license is null)
        {
            // Nothing activated yet: run as a trial under the default plan.
            var defaultPlan = await _context.LicensePlans
                .AsNoTracking()
                .Where(p => p.Name == _options.DefaultPlanName && !p.IsDeleted)
                .FirstOrDefaultAsync(ct);

            var limits = defaultPlan is null
                ? LicenseLimits.Unlimited
                : new LicenseLimits(defaultPlan.MaxUsers, defaultPlan.MaxBranches);

            return new LicenseStatus
            {
                State = LicenseState.Unlicensed,
                PlanName = defaultPlan?.Name,
                DeviceId = deviceId,
                MaxUsers = limits.MaxUsers,
                MaxBranches = limits.MaxBranches,
                IsWritable = true,
                IsEnforced = _options.Enforce
            };
        }

        var deviceBound = await _context.DeviceActivations
            .AsNoTracking()
            .AnyAsync(
                d => d.LicenseId == license.Id &&
                     d.DeviceIdentifier == deviceId &&
                     !d.IsDeleted,
                ct);

        var plan = license.LicensePlan;
        var maxUsers = plan?.MaxUsers ?? int.MaxValue;
        var maxBranches = plan?.MaxBranches ?? int.MaxValue;
        var daysRemaining = (int)Math.Floor((license.ExpiryDate - now).TotalDays);

        if (!deviceBound)
        {
            return new LicenseStatus
            {
                State = LicenseState.DeviceMismatch,
                PlanName = plan?.Name,
                DeviceId = deviceId,
                ExpiryDate = license.ExpiryDate,
                DaysRemaining = Math.Max(daysRemaining, 0),
                MaxUsers = maxUsers,
                MaxBranches = maxBranches,
                IsWritable = !_options.Enforce,
                IsEnforced = _options.Enforce
            };
        }

        LicenseState state;
        bool withinWriteWindow;

        if (now <= license.ExpiryDate)
        {
            state = daysRemaining <= _options.ExpiringSoonDays
                ? LicenseState.ExpiringSoon
                : LicenseState.Active;
            withinWriteWindow = true;
        }
        else if (now <= license.ExpiryDate.AddDays(_options.GraceDays))
        {
            state = LicenseState.GracePeriod;
            withinWriteWindow = true;
        }
        else
        {
            state = LicenseState.Expired;
            withinWriteWindow = false;
        }

        return new LicenseStatus
        {
            State = state,
            PlanName = plan?.Name,
            DeviceId = deviceId,
            ExpiryDate = license.ExpiryDate,
            DaysRemaining = Math.Max(daysRemaining, 0),
            MaxUsers = maxUsers,
            MaxBranches = maxBranches,
            IsWritable = !_options.Enforce || withinWriteWindow,
            IsEnforced = _options.Enforce
        };
    }

    public async Task<LicenseLimits> GetLimitsAsync(CancellationToken ct = default)
        => (await GetStatusAsync(ct)).Limits;

    public async Task<Result> ActivateAsync(string licenseKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(licenseKey))
            return Result.Failed(AppStrings.LicenseInvalidKeyMsg);

        if (string.IsNullOrWhiteSpace(_options.PublicKeyPem))
        {
            _logger?.LogWarning(
                "Licence activation attempted but no public key is configured.");
            return Result.Failed(AppStrings.LicenseNotConfiguredMsg);
        }

        var trimmedKey = licenseKey.Trim();
        var deviceId = _device.GetDeviceId();
        var validation = LicenseKeyCodec.Validate(
            trimmedKey, _options.PublicKeyPem, deviceId, _clock.UtcNow);

        if (!validation.IsValid)
            return Result.Failed(MapValidationError(validation.ErrorCode));

        var payload = validation.Payload!;

        var plan = await UpsertPlanAsync(payload, ct);
        var license = await UpsertLicenseAsync(trimmedKey, payload, plan, ct);
        await BindDeviceAsync(license.Id, deviceId, ct);

        await _context.SaveChangesAsync(ct);

        _logger?.LogInformation(
            "Licence {Plan} activated (serial {Serial}, expires {Expiry:u}).",
            payload.Plan, payload.Serial, payload.ExpiresAt);

        return Result.Succeeded(AppStrings.LicenseActivatedMsg);
    }

    public async Task<Result> DeactivateAsync(CancellationToken ct = default)
    {
        var licenses = await _context.Licenses
            .Where(l => !l.IsDeleted && l.IsActive)
            .ToListAsync(ct);

        if (licenses.Count == 0)
            return Result.Failed(AppStrings.LicenseNoneActiveMsg);

        foreach (var license in licenses)
        {
            license.IsActive = false;
            license.UpdatedAt = _clock.UtcNow;
        }

        var deviceId = _device.GetDeviceId();
        var activations = await _context.DeviceActivations
            .Where(d => d.DeviceIdentifier == deviceId && !d.IsDeleted)
            .ToListAsync(ct);

        foreach (var activation in activations)
        {
            activation.IsDeleted = true;
            activation.UpdatedAt = _clock.UtcNow;
        }

        await _context.SaveChangesAsync(ct);
        return Result.Succeeded(AppStrings.LicenseDeactivatedMsg);
    }

    public async Task EnsureWritableAsync(CancellationToken ct = default)
    {
        if (!_options.Enforce)
            return;

        var status = await GetStatusAsync(ct);
        if (status.IsWritable)
            return;

        var code = status.State == LicenseState.Expired
            ? DomainErrorCodes.LicenseExpired
            : DomainErrorCodes.LicenseReadOnly;

        throw new CodedDomainException(
            code,
            "The licence does not permit writing operations.");
    }

    public async Task EnsureCanCreateBranchAsync(CancellationToken ct = default)
    {
        if (!_options.Enforce)
            return;

        await EnsureWritableAsync(ct);

        var status = await GetStatusAsync(ct);
        var count = await _context.Branches.CountAsync(b => !b.IsDeleted, ct);

        if (count >= status.MaxBranches)
        {
            throw new CodedDomainException(
                DomainErrorCodes.LicenseBranchLimit,
                $"The licence '{status.PlanName}' allows at most {status.MaxBranches} branch(es).");
        }
    }

    public async Task EnsureCanCreateUserAsync(CancellationToken ct = default)
    {
        if (!_options.Enforce)
            return;

        await EnsureWritableAsync(ct);

        var status = await GetStatusAsync(ct);
        var count = await _context.Users.CountAsync(u => !u.IsDeleted, ct);

        if (count >= status.MaxUsers)
        {
            throw new CodedDomainException(
                DomainErrorCodes.LicenseUserLimit,
                $"The licence '{status.PlanName}' allows at most {status.MaxUsers} user(s).");
        }
    }

    private async Task<LicensePlan> UpsertPlanAsync(
        LicensePayload payload,
        CancellationToken ct)
    {
        var plan = await _context.LicensePlans
            .FirstOrDefaultAsync(p => p.Name == payload.Plan && !p.IsDeleted, ct);

        var durationDays = Math.Max(
            1,
            (int)(payload.ExpiresAt - payload.IssuedAt).TotalDays);

        if (plan is null)
        {
            plan = new LicensePlan
            {
                Name = payload.Plan,
                DurationInDays = durationDays,
                MaxUsers = payload.MaxUsers,
                MaxBranches = payload.MaxBranches,
                IsActive = true
            };
            _context.LicensePlans.Add(plan);
        }
        else
        {
            plan.DurationInDays = durationDays;
            plan.MaxUsers = payload.MaxUsers;
            plan.MaxBranches = payload.MaxBranches;
            plan.IsActive = true;
            plan.UpdatedAt = _clock.UtcNow;
        }

        return plan;
    }

    private async Task<License> UpsertLicenseAsync(
        string licenseKey,
        LicensePayload payload,
        LicensePlan plan,
        CancellationToken ct)
    {
        // A single active licence at a time.
        var activeLicenses = await _context.Licenses
            .Where(l => !l.IsDeleted && l.IsActive)
            .ToListAsync(ct);

        foreach (var active in activeLicenses)
        {
            active.IsActive = false;
            active.UpdatedAt = _clock.UtcNow;
        }

        var license = await _context.Licenses
            .FirstOrDefaultAsync(l => l.LicenseKey == licenseKey && !l.IsDeleted, ct);

        if (license is null)
        {
            license = new License
            {
                LicensePlanId = plan.Id,
                LicenseKey = licenseKey,
                StartDate = payload.IssuedAt,
                ExpiryDate = payload.ExpiresAt,
                IsActive = true
            };
            _context.Licenses.Add(license);
        }
        else
        {
            license.LicensePlanId = plan.Id;
            license.StartDate = payload.IssuedAt;
            license.ExpiryDate = payload.ExpiresAt;
            license.IsActive = true;
            license.UpdatedAt = _clock.UtcNow;
        }

        return license;
    }

    private async Task BindDeviceAsync(
        Guid licenseId,
        string deviceId,
        CancellationToken ct)
    {
        var activation = await _context.DeviceActivations
            .FirstOrDefaultAsync(
                d => d.LicenseId == licenseId && d.DeviceIdentifier == deviceId,
                ct);

        if (activation is null)
        {
            _context.DeviceActivations.Add(new DeviceActivation
            {
                LicenseId = licenseId,
                DeviceName = Environment.MachineName,
                DeviceIdentifier = deviceId,
                ActivatedAt = _clock.UtcNow
            });
        }
        else
        {
            activation.IsDeleted = false;
            activation.ActivatedAt = _clock.UtcNow;
            activation.UpdatedAt = _clock.UtcNow;
        }
    }

    private static string MapValidationError(string? errorCode) => errorCode switch
    {
        DomainErrorCodes.LicenseExpired => AppStrings.LicenseExpiredMsg,
        DomainErrorCodes.LicenseDeviceMismatch => AppStrings.LicenseDeviceMismatchMsg,
        DomainErrorCodes.LicenseBadSignature => AppStrings.LicenseBadSignatureMsg,
        _ => AppStrings.LicenseInvalidKeyMsg
    };
}
