using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace DzentoPOS2.App.Services.Licensing;

/// <summary>
/// Provides a stable fingerprint for the machine the app runs on. The value is
/// bound into a licence key at issue time and re-checked at activation, so a
/// key copied to another computer is rejected.
/// </summary>
public interface IDeviceIdentifier
{
    string GetDeviceId();
}

/// <summary>
/// Portable fallback used when no platform-specific identifier is registered
/// (for example in the test project). It hashes stable machine properties, so
/// it is constant for the lifetime of the process but is deliberately weaker
/// than <c>WindowsDeviceIdentifier</c>, which uses the registry MachineGuid.
/// </summary>
public sealed class DefaultDeviceIdentifier : IDeviceIdentifier
{
    private readonly string _id = Compute();

    public string GetDeviceId() => _id;

    private static string Compute()
    {
        var raw = string.Join(
            "|",
            Environment.MachineName,
            RuntimeInformation.OSDescription,
            RuntimeInformation.ProcessArchitecture);

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hash);
    }
}
