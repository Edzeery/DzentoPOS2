namespace DzentoPOS2.LicenseIssuer;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        // Visual verification: render every tab to a PNG and exit.
        //   DzentoPOS2.LicenseIssuer.exe --screenshot <dir>
        if (args.Any(a => string.Equals(a, "--screenshot", StringComparison.OrdinalIgnoreCase)))
        {
            ApplicationConfiguration.Initialize();
            var directory = IssuerCli.Option(args, "--out") ?? Path.GetTempPath();
            using var form = new MainForm();
            form.CaptureScreenshots(directory);
            Console.WriteLine($"Screenshots written to {directory}");
            return 0;
        }

        // Scriptable mode for automation / CI; GUI when no arguments are given.
        if (args.Length > 0)
            return IssuerCli.Run(args);

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
        return 0;
    }
}
