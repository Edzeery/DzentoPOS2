namespace DzentoPOS2.Licensing;

/// <summary>
/// The signed content of a licence key. It is serialized to JSON, signed with
/// the vendor's private RSA key and carried inside the key token. Nothing here
/// is secret: the signature is what makes it trustworthy.
/// </summary>
public sealed record LicensePayload
{
    /// <summary>Human-readable licence serial / invoice number.</summary>
    public string Serial { get; init; } = string.Empty;

    /// <summary>Plan name; matched against <c>LicensePlan.Name</c>.</summary>
    public string Plan { get; init; } = string.Empty;

    /// <summary>Fingerprint of the machine the licence is bound to.</summary>
    public string DeviceId { get; init; } = string.Empty;

    /// <summary>Optional customer name printed in the UI.</summary>
    public string? LicensedTo { get; init; }

    public DateTime IssuedAt { get; init; }

    public DateTime ExpiresAt { get; init; }

    public int MaxUsers { get; init; }

    public int MaxBranches { get; init; }
}
