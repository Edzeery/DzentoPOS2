namespace DzentoPOS2.Licensing;

/// <summary>Outcome of verifying/validating a licence token.</summary>
public sealed record LicenseValidationResult(
    bool IsValid,
    LicensePayload? Payload,
    string? ErrorCode)
{
    public static LicenseValidationResult Valid(LicensePayload payload) =>
        new(true, payload, null);

    public static LicenseValidationResult Invalid(string errorCode) =>
        new(false, null, errorCode);
}
