using System.Security.Cryptography;
using DzentoPOS2.Licensing;

namespace DzentoPOS2.Licensing.Signing;

/// <summary>
/// Licence <b>signing</b> and key generation — the vendor side of the offline
/// licence system.
///
/// <para>
/// This is the only place a licence can be created, and it requires the vendor's
/// RSA private key. The customer application references
/// <see cref="LicenseKeyCodec"/> (verify only) and never this type, so a customer
/// build physically cannot sign a licence. The vendor-only issuer tool
/// (<c>tools/DzentoPOS2.LicenseIssuer</c>) and the automated tests reference it.
/// </para>
/// </summary>
public static class LicenseSigner
{
    // PBKDF2 defaults for the password-protected private key file.
    private const int PrivateKeyIterations = 100_000;

    /// <summary>Generates a fresh RSA key pair as PEM strings.</summary>
    public static (string PrivateKeyPem, string PublicKeyPem) GenerateKeyPair(
        int keySize = 2048)
    {
        using var rsa = RSA.Create(keySize);
        return (rsa.ExportRSAPrivateKeyPem(), rsa.ExportSubjectPublicKeyInfoPem());
    }

    /// <summary>
    /// Generates a fresh RSA key pair where the private half is a
    /// password-protected PKCS#8 PEM, so it can be stored on disk safely.
    /// </summary>
    public static (string EncryptedPrivateKeyPem, string PublicKeyPem)
        GenerateEncryptedKeyPair(string password, int keySize = 2048)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("A password is required.", nameof(password));

        using var rsa = RSA.Create(keySize);
        return (ExportEncryptedPrivateKey(rsa, password),
                rsa.ExportSubjectPublicKeyInfoPem());
    }

    /// <summary>Signs a payload and returns the licence key token.</summary>
    public static string Sign(LicensePayload payload, string privateKeyPem)
    {
        ArgumentNullException.ThrowIfNull(payload);

        if (string.IsNullOrWhiteSpace(privateKeyPem))
            throw new ArgumentException(
                "A private key is required to sign a licence.",
                nameof(privateKeyPem));

        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem);

        return SignCore(payload, rsa);
    }

    /// <summary>
    /// Signs a payload with a password-protected private key (PKCS#8 encrypted
    /// PEM), which is how the vendor issuer stores its key.
    /// </summary>
    public static string SignEncrypted(
        LicensePayload payload,
        string encryptedPrivateKeyPem,
        string password)
    {
        ArgumentNullException.ThrowIfNull(payload);

        if (string.IsNullOrWhiteSpace(encryptedPrivateKeyPem))
            throw new ArgumentException(
                "An encrypted private key is required to sign a licence.",
                nameof(encryptedPrivateKeyPem));

        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("A password is required.", nameof(password));

        using var rsa = RSA.Create();
        rsa.ImportFromEncryptedPem(encryptedPrivateKeyPem, password);

        return SignCore(payload, rsa);
    }

    private static string SignCore(LicensePayload payload, RSA rsa)
    {
        var payloadBytes = LicenseKeyCodec.SerializePayload(payload);
        var signature = rsa.SignData(
            payloadBytes,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        return LicenseKeyCodec.PackToken(payloadBytes, signature);
    }

    private static string ExportEncryptedPrivateKey(RSA rsa, string password) =>
        rsa.ExportEncryptedPkcs8PrivateKeyPem(
            password,
            new PbeParameters(
                PbeEncryptionAlgorithm.Aes256Cbc,
                HashAlgorithmName.SHA256,
                PrivateKeyIterations));
}
