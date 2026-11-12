namespace DzentoPOS2.App.Services.Licensing;

/// <summary>
/// Lifecycle state of the product licence on this machine, derived from the
/// stored <c>License</c> row, its device binding and the current clock.
/// </summary>
public enum LicenseState
{
    /// <summary>No licence has been activated yet; the app runs as a trial.</summary>
    Unlicensed,

    /// <summary>Valid, bound to this device and comfortably before expiry.</summary>
    Active,

    /// <summary>Valid but will expire within <see cref="LicenseOptions.ExpiringSoonDays"/>.</summary>
    ExpiringSoon,

    /// <summary>Past the expiry date but still inside the grace period.</summary>
    GracePeriod,

    /// <summary>Past the expiry date and the grace period: read-only.</summary>
    Expired,

    /// <summary>A licence exists but it is not bound to this device.</summary>
    DeviceMismatch,
}

/// <summary>Effective user/branch ceilings for the running installation.</summary>
public sealed record LicenseLimits(int MaxUsers, int MaxBranches)
{
    public static readonly LicenseLimits Unlimited =
        new(int.MaxValue, int.MaxValue);
}

/// <summary>Snapshot of the licence shown by the UI and used by the guards.</summary>
public sealed record LicenseStatus
{
    public LicenseState State { get; init; }

    public string? PlanName { get; init; }

    public string? LicensedTo { get; init; }

    /// <summary>Fingerprint of this machine (shown so a key can be issued).</summary>
    public string? DeviceId { get; init; }

    public DateTime? ExpiryDate { get; init; }

    /// <summary>Whole days until expiry; zero once expired.</summary>
    public int DaysRemaining { get; init; }

    public int MaxUsers { get; init; } = int.MaxValue;

    public int MaxBranches { get; init; } = int.MaxValue;

    /// <summary>Whether write operations are currently allowed.</summary>
    public bool IsWritable { get; init; }

    /// <summary>Whether enforcement is switched on (<c>Licensing:Enforce</c>).</summary>
    public bool IsEnforced { get; init; }

    public LicenseLimits Limits => new(MaxUsers, MaxBranches);
}
