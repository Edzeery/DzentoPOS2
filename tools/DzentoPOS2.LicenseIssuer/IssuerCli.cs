using System.Globalization;
using System.Text;
using DzentoPOS2.Licensing;
using DzentoPOS2.Licensing.Signing;

namespace DzentoPOS2.LicenseIssuer;

/// <summary>
/// Command-line surface of the vendor issuer, so issuing can be scripted and
/// tested without the GUI:
///
/// <code>
/// DzentoPOS2.LicenseIssuer --generate --private key.pem --public key.pub.pem --password ****
/// DzentoPOS2.LicenseIssuer --issue --private key.pem --password **** --plan Standard --device &lt;id&gt; [--days 365] [--users 5] [--branches 2] [--to "Acme"] [--serial S1]
/// DzentoPOS2.LicenseIssuer --verify --public key.pub.pem --key &lt;token&gt; [--device &lt;id&gt;]
/// </code>
/// </summary>
internal static class IssuerCli
{
    public static int Run(string[] args)
    {
        if (HasFlag(args, "--generate"))
            return Generate(args);

        if (HasFlag(args, "--issue"))
            return Issue(args);

        if (HasFlag(args, "--verify"))
            return Verify(args);

        Console.Error.WriteLine(
            "Usage: --generate | --issue | --verify (see docs/licensing.md).");
        return 2;
    }

    private static int Generate(string[] args)
    {
        var privatePath = Option(args, "--private");
        var publicPath = Option(args, "--public");
        var password = Option(args, "--password");

        if (string.IsNullOrWhiteSpace(privatePath) ||
            string.IsNullOrWhiteSpace(publicPath) ||
            string.IsNullOrEmpty(password))
        {
            Console.Error.WriteLine(
                "Usage: --generate --private <pem> --public <pem> --password <pwd> [--bits 2048]");
            return 2;
        }

        int bits = ParseInt(args, "--bits", 2048);
        if (bits < 2048)
            bits = 2048;

        var (encryptedPrivateKey, publicKey) =
            LicenseSigner.GenerateEncryptedKeyPair(password, bits);

        WriteFile(privatePath, encryptedPrivateKey);
        WriteFile(publicPath, publicKey);

        Console.WriteLine($"Private key (password-protected): {Path.GetFullPath(privatePath)}");
        Console.WriteLine($"Public key                     : {Path.GetFullPath(publicPath)}");
        Console.WriteLine();
        Console.WriteLine("Replace LicensePublicKey.Pem in the customer application with:");
        Console.WriteLine(publicKey);
        return 0;
    }

    private static int Issue(string[] args)
    {
        var privatePath = Option(args, "--private");
        var password = Option(args, "--password");
        var plan = Option(args, "--plan");
        var device = Option(args, "--device");

        if (string.IsNullOrWhiteSpace(privatePath) || !File.Exists(privatePath) ||
            string.IsNullOrEmpty(password) ||
            string.IsNullOrWhiteSpace(plan) || string.IsNullOrWhiteSpace(device))
        {
            Console.Error.WriteLine(
                "Usage: --issue --private <pem> --password <pwd> --plan <name> --device <id> " +
                "[--days 365] [--users 5] [--branches 2] [--to <name>] [--serial <s>]");
            return 2;
        }

        var encryptedPrivateKey = File.ReadAllText(privatePath);

        string token;
        LicensePayload payload;
        try
        {
            payload = BuildPayload(args, plan!, device!);
            token = LicenseSigner.SignEncrypted(payload, encryptedPrivateKey, password);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to sign: {ex.Message}");
            return 1;
        }

        IssuanceLedger.Append(privatePath, payload, token);

        Console.WriteLine(token);
        Console.Error.WriteLine();
        Console.Error.WriteLine($"Plan    : {payload.Plan}");
        Console.Error.WriteLine($"Customer: {payload.LicensedTo ?? "-"}");
        Console.Error.WriteLine($"Device  : {payload.DeviceId}");
        Console.Error.WriteLine($"Expires : {payload.ExpiresAt:u}");
        Console.Error.WriteLine($"Limits  : {payload.MaxUsers} user(s), {payload.MaxBranches} branch(es)");
        return 0;
    }

    private static int Verify(string[] args)
    {
        var key = Option(args, "--key");
        if (string.IsNullOrWhiteSpace(key))
        {
            Console.Error.WriteLine("Usage: --verify --public <pem|file> --key <token> [--device <id>]");
            return 2;
        }

        var publicKey = ReadPublicKey(Option(args, "--public"));
        if (string.IsNullOrWhiteSpace(publicKey))
        {
            Console.Error.WriteLine("--public <pem|file> is required.");
            return 2;
        }

        var result = LicenseKeyCodec.Verify(key, publicKey);
        if (!result.IsValid)
        {
            Console.Error.WriteLine($"INVALID ({result.ErrorCode}).");
            return 1;
        }

        var payload = result.Payload!;
        Console.WriteLine("VALID");
        Console.WriteLine($"  Plan    : {payload.Plan}");
        Console.WriteLine($"  Serial  : {payload.Serial}");
        Console.WriteLine($"  Customer: {payload.LicensedTo ?? "-"}");
        Console.WriteLine($"  Device  : {payload.DeviceId}");
        Console.WriteLine($"  Expires : {payload.ExpiresAt:u}");
        Console.WriteLine($"  Limits  : {payload.MaxUsers} user(s), {payload.MaxBranches} branch(es)");

        var device = Option(args, "--device");
        if (!string.IsNullOrWhiteSpace(device) &&
            !string.Equals(payload.DeviceId, device, StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("  WARNING: bound to a different device.");
            return 1;
        }

        return 0;
    }

    internal static LicensePayload BuildPayload(string[] args, string plan, string device)
    {
        var now = DateTime.UtcNow;
        return new LicensePayload
        {
            Serial = Option(args, "--serial")
                ?? now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            Plan = plan,
            DeviceId = device,
            LicensedTo = Option(args, "--to"),
            IssuedAt = now,
            ExpiresAt = now.AddDays(ParseInt(args, "--days", 365)),
            MaxUsers = ParseInt(args, "--users", 1),
            MaxBranches = ParseInt(args, "--branches", 1)
        };
    }

    internal static string? ReadPublicKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (value.Contains("BEGIN PUBLIC KEY", StringComparison.Ordinal))
            return value.Replace("\\n", "\n", StringComparison.Ordinal);

        return File.Exists(value) ? File.ReadAllText(value) : value;
    }

    private static void WriteFile(string path, string content)
    {
        var full = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(full);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(full, content, new UTF8Encoding(false));
    }

    private static int ParseInt(string[] args, string name, int fallback) =>
        int.TryParse(Option(args, name), NumberStyles.Integer,
            CultureInfo.InvariantCulture, out var value) && value > 0
            ? value
            : fallback;

    private static bool HasFlag(string[] args, string name) =>
        args.Any(a => string.Equals(a, name, StringComparison.OrdinalIgnoreCase));

    internal static string? Option(string[] args, string name)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        }

        return null;
    }
}
