using System.Globalization;
using System.Text;
using DzentoPOS2.Licensing;

namespace DzentoPOS2.LicenseIssuer;

/// <summary>
/// Appends every issued licence to <c>issued-licenses.csv</c> next to the
/// private key file, so the vendor can answer "which key did we issue to whom".
/// </summary>
internal static class IssuanceLedger
{
    public const string FileName = "issued-licenses.csv";

    private static readonly string[] Header =
    [
        "IssuedAtUtc", "Serial", "Customer", "DeviceId", "Plan",
        "ExpiresAtUtc", "MaxUsers", "MaxBranches", "LicenseKey"
    ];

    public static string PathFor(string privateKeyPath) =>
        Path.Combine(
            Path.GetDirectoryName(Path.GetFullPath(privateKeyPath)) ?? ".",
            FileName);

    public static void Append(
        string privateKeyPath,
        LicensePayload payload,
        string token)
    {
        var path = PathFor(privateKeyPath);
        var isNew = !File.Exists(path);

        var builder = new StringBuilder();
        if (isNew)
            builder.AppendLine(string.Join(',', Header));

        builder.AppendLine(string.Join(',',
            Quote(DateTime.UtcNow.ToString("u", CultureInfo.InvariantCulture)),
            Quote(payload.Serial),
            Quote(payload.LicensedTo ?? string.Empty),
            Quote(payload.DeviceId),
            Quote(payload.Plan),
            Quote(payload.ExpiresAt.ToString("u", CultureInfo.InvariantCulture)),
            payload.MaxUsers.ToString(CultureInfo.InvariantCulture),
            payload.MaxBranches.ToString(CultureInfo.InvariantCulture),
            Quote(token)));

        File.AppendAllText(path, builder.ToString(), new UTF8Encoding(false));
    }

    private static string Quote(string value) =>
        $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
