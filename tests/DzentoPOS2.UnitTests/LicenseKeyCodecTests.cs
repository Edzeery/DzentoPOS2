using DzentoPOS2.Licensing;
using DzentoPOS2.Licensing.Signing;
using DzentoPOS2.Core.Exceptions;
using Xunit;

namespace DzentoPOS2.UnitTests;

public sealed class LicenseKeyCodecTests
{
    private static readonly DateTime Now = new(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);

    private static LicensePayload Payload(
        string deviceId = "device-a",
        DateTime? expiresAt = null) => new()
        {
            Serial = "SN-0001",
            Plan = "Standard",
            DeviceId = deviceId,
            LicensedTo = "Acme",
            IssuedAt = Now.AddDays(-1),
            ExpiresAt = expiresAt ?? Now.AddDays(365),
            MaxUsers = 3,
            MaxBranches = 2
        };

    [Fact]
    public void Sign_then_verify_round_trips_the_payload()
    {
        var (privateKey, publicKey) = LicenseSigner.GenerateKeyPair();
        var payload = Payload();

        var token = LicenseSigner.Sign(payload, privateKey);
        var result = LicenseKeyCodec.Verify(token, publicKey);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Payload);
        Assert.Equal(payload.Serial, result.Payload!.Serial);
        Assert.Equal(payload.Plan, result.Payload.Plan);
        Assert.Equal(payload.DeviceId, result.Payload.DeviceId);
        Assert.Equal(payload.MaxUsers, result.Payload.MaxUsers);
        Assert.Equal(payload.MaxBranches, result.Payload.MaxBranches);
    }

    [Fact]
    public void Tampered_signature_is_rejected()
    {
        var (privateKey, publicKey) = LicenseSigner.GenerateKeyPair();
        var token = LicenseSigner.Sign(Payload(), privateKey);

        var parts = token.Split('.');
        var signature = parts[1].ToCharArray();
        signature[0] = signature[0] == 'A' ? 'B' : 'A';
        var tampered = $"{parts[0]}.{new string(signature)}";

        var result = LicenseKeyCodec.Verify(tampered, publicKey);

        Assert.False(result.IsValid);
        Assert.Equal(DomainErrorCodes.LicenseBadSignature, result.ErrorCode);
    }

    [Fact]
    public void A_key_signed_by_another_private_key_is_rejected()
    {
        var (vendorPrivate, _) = LicenseSigner.GenerateKeyPair();
        var (_, customerPublic) = LicenseSigner.GenerateKeyPair();

        var token = LicenseSigner.Sign(Payload(), vendorPrivate);
        var result = LicenseKeyCodec.Verify(token, customerPublic);

        Assert.False(result.IsValid);
        Assert.Equal(DomainErrorCodes.LicenseBadSignature, result.ErrorCode);
    }

    [Fact]
    public void Validate_rejects_a_key_bound_to_another_device()
    {
        var (privateKey, publicKey) = LicenseSigner.GenerateKeyPair();
        var token = LicenseSigner.Sign(Payload(deviceId: "device-a"), privateKey);

        var result = LicenseKeyCodec.Validate(
            token, publicKey, deviceId: "device-b", Now);

        Assert.False(result.IsValid);
        Assert.Equal(DomainErrorCodes.LicenseDeviceMismatch, result.ErrorCode);
    }

    [Fact]
    public void Validate_rejects_an_expired_key()
    {
        var (privateKey, publicKey) = LicenseSigner.GenerateKeyPair();
        var token = LicenseSigner.Sign(
            Payload(expiresAt: Now.AddDays(-1)), privateKey);

        var result = LicenseKeyCodec.Validate(
            token, publicKey, deviceId: "device-a", Now);

        Assert.False(result.IsValid);
        Assert.Equal(DomainErrorCodes.LicenseExpired, result.ErrorCode);
    }

    [Fact]
    public void Validate_accepts_a_valid_key_for_the_matching_device()
    {
        var (privateKey, publicKey) = LicenseSigner.GenerateKeyPair();
        var token = LicenseSigner.Sign(Payload(), privateKey);

        var result = LicenseKeyCodec.Validate(
            token, publicKey, deviceId: "device-a", Now);

        Assert.True(result.IsValid);
        Assert.Equal("Standard", result.Payload!.Plan);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-token")]
    [InlineData("abc.def")]
    public void Malformed_tokens_are_rejected(string token)
    {
        var (_, publicKey) = LicenseSigner.GenerateKeyPair();

        var result = LicenseKeyCodec.Verify(token, publicKey);

        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorCode);
    }

    [Fact]
    public void Sign_without_a_private_key_throws()
    {
        Assert.Throws<ArgumentException>(
            () => LicenseSigner.Sign(Payload(), string.Empty));
    }

    [Fact]
    public void Two_generated_key_pairs_are_different()
    {
        var first = LicenseSigner.GenerateKeyPair();
        var second = LicenseSigner.GenerateKeyPair();

        Assert.NotEqual(first.PublicKeyPem, second.PublicKeyPem);
        Assert.NotEqual(first.PrivateKeyPem, second.PrivateKeyPem);
    }
}