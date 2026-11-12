using System.Security.Cryptography;
using System.Text.Json;
using DzentoPOS2.Core.Exceptions;

namespace DzentoPOS2.Licensing;

/// <summary>
/// Offline licence key format: parsing and cryptographic <b>verification</b>.
///
/// <para>
/// A key is <c>base64url(payloadJson) + "." + base64url(signature)</c> where the
/// signature is RSA-SHA256 (PKCS#1) over the exact payload bytes. Because the
/// payload bytes are signed verbatim, verification never depends on JSON
/// property ordering.
/// </para>
///
/// <para>
/// This type is compiled into the customer application and holds <b>no secret</b>:
/// it can only verify a key against a public key. Signing and key generation live
/// in the separate, vendor-only <c>DzentoPOS2.Licensing.Signing</c> library, which
/// the customer application does not reference. <see cref="SerializePayload"/>
/// and <see cref="PackToken"/> are shared with that library so the exact bytes
/// that were signed are reproduced at verification time.
/// </para>
/// </summary>
public static class LicenseKeyCodec
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Verifies the signature and parses the payload. Performs no device or
    /// expiry checks; use <see cref="Validate"/> for those.
    /// </summary>
    public static LicenseValidationResult Verify(
        string? token,
        string? publicKeyPem)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(publicKeyPem))
            return LicenseValidationResult.Invalid(DomainErrorCodes.LicenseMalformed);

        var parts = token.Split('.');
        if (parts.Length != 2)
            return LicenseValidationResult.Invalid(DomainErrorCodes.LicenseMalformed);

        byte[] payloadBytes;
        byte[] signature;
        try
        {
            payloadBytes = Base64UrlDecode(parts[0]);
            signature = Base64UrlDecode(parts[1]);
        }
        catch (FormatException)
        {
            return LicenseValidationResult.Invalid(DomainErrorCodes.LicenseMalformed);
        }

        try
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(publicKeyPem);

            if (!rsa.VerifyData(
                    payloadBytes,
                    signature,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1))
            {
                return LicenseValidationResult.Invalid(
                    DomainErrorCodes.LicenseBadSignature);
            }
        }
        catch (CryptographicException)
        {
            return LicenseValidationResult.Invalid(
                DomainErrorCodes.LicenseBadSignature);
        }
        catch (ArgumentException)
        {
            // Malformed PEM in configuration.
            return LicenseValidationResult.Invalid(DomainErrorCodes.LicenseMalformed);
        }

        LicensePayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<LicensePayload>(payloadBytes, JsonOptions);
        }
        catch (JsonException)
        {
            return LicenseValidationResult.Invalid(DomainErrorCodes.LicenseMalformed);
        }

        if (payload is null || string.IsNullOrWhiteSpace(payload.Plan))
            return LicenseValidationResult.Invalid(DomainErrorCodes.LicenseMalformed);

        return LicenseValidationResult.Valid(payload);
    }

    /// <summary>
    /// Verifies the signature and then checks that the licence is bound to
    /// <paramref name="deviceId"/> and has not expired with respect to
    /// <paramref name="nowUtc"/>.
    /// </summary>
    public static LicenseValidationResult Validate(
        string? token,
        string? publicKeyPem,
        string? deviceId,
        DateTime nowUtc)
    {
        var verified = Verify(token, publicKeyPem);
        if (!verified.IsValid)
            return verified;

        var payload = verified.Payload!;

        if (!string.IsNullOrWhiteSpace(payload.DeviceId) &&
            !string.Equals(payload.DeviceId, deviceId, StringComparison.OrdinalIgnoreCase))
        {
            return LicenseValidationResult.Invalid(
                DomainErrorCodes.LicenseDeviceMismatch);
        }

        if (payload.ExpiresAt != default && payload.ExpiresAt < nowUtc)
            return LicenseValidationResult.Invalid(DomainErrorCodes.LicenseExpired);

        return verified;
    }

    /// <summary>
    /// Serializes a payload to the exact UTF-8 bytes that are signed and verified.
    /// Used by the vendor signing library so its signature matches this format.
    /// </summary>
    public static byte[] SerializePayload(LicensePayload payload) =>
        JsonSerializer.SerializeToUtf8Bytes(payload, JsonOptions);

    /// <summary>
    /// Assembles a licence token from signed payload bytes and a signature.
    /// Used by the vendor signing library.
    /// </summary>
    public static string PackToken(byte[] payloadBytes, byte[] signature) =>
        $"{Base64UrlEncode(payloadBytes)}.{Base64UrlEncode(signature)}";

    internal static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static byte[] Base64UrlDecode(string value)
    {
        var normalized = value.Replace('-', '+').Replace('_', '/');
        switch (normalized.Length % 4)
        {
            case 2:
                normalized += "==";
                break;
            case 3:
                normalized += "=";
                break;
        }

        return Convert.FromBase64String(normalized);
    }
}
