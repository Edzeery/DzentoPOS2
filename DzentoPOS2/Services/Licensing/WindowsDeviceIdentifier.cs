using System.Security.Cryptography;
using System.Text;
using DzentoPOS2.App.Services.Licensing;
using Microsoft.Win32;

namespace DzentoPOS2.WinForms.Services.Licensing;

/// <summary>
/// Windows device fingerprint derived from the registry
/// <c>HKLM\SOFTWARE\Microsoft\Cryptography\MachineGuid</c>, which is created
/// once at installation and stays stable across reboots and updates. The raw
/// GUID is hashed before it is stored or displayed, so the licence never leaks
/// it. When the registry is unavailable (locked-down machine) the portable
/// <see cref="DefaultDeviceIdentifier"/> is used instead.
/// </summary>
public sealed class WindowsDeviceIdentifier : IDeviceIdentifier
{
    private readonly string _id = Compute();

    public string GetDeviceId() => _id;

    private static string Compute()
    {
        try
        {
            // Force the 64-bit view: a 32-bit process would otherwise read a
            // redirected view where MachineGuid may be absent.
            using var baseKey = RegistryKey.OpenBaseKey(
                RegistryHive.LocalMachine, RegistryView.Registry64);
            using var cryptography = baseKey.OpenSubKey(
                @"SOFTWARE\Microsoft\Cryptography");

            if (cryptography?.GetValue("MachineGuid") is string machineGuid &&
                !string.IsNullOrWhiteSpace(machineGuid))
            {
                var hash = SHA256.HashData(Encoding.UTF8.GetBytes(machineGuid));
                return Convert.ToHexString(hash);
            }
        }
        catch (Exception)
        {
            // Registry access can be denied; fall back to the portable hash.
        }

        return new DefaultDeviceIdentifier().GetDeviceId();
    }
}