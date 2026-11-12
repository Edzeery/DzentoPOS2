using DzentoPOS2.Shared.Results;

namespace DzentoPOS2.App.Services.Licensing;

/// <summary>
/// Offline licence service: reads the current state, activates signed keys and
/// enforces the plan limits. Every guard is a no-op while
/// <see cref="LicenseOptions.Enforce"/> is <c>false</c>.
/// </summary>
public interface ILicenseService
{
    /// <summary>Current state, plan and remaining days.</summary>
    Task<LicenseStatus> GetStatusAsync(CancellationToken ct = default);

    /// <summary>Effective user/branch ceilings for the running installation.</summary>
    Task<LicenseLimits> GetLimitsAsync(CancellationToken ct = default);

    /// <summary>Verifies, binds and stores a signed licence key.</summary>
    Task<Result> ActivateAsync(string licenseKey, CancellationToken ct = default);

    /// <summary>Deactivates the stored licence and its device binding.</summary>
    Task<Result> DeactivateAsync(CancellationToken ct = default);

    /// <summary>Throws when the licence forbids writing (expired).</summary>
    Task EnsureWritableAsync(CancellationToken ct = default);

    /// <summary>Throws when the branch ceiling has been reached.</summary>
    Task EnsureCanCreateBranchAsync(CancellationToken ct = default);

    /// <summary>Throws when the user ceiling has been reached.</summary>
    Task EnsureCanCreateUserAsync(CancellationToken ct = default);
}
