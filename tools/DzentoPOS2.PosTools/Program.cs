namespace DzentoPOS2.PosTools;

/// <summary>
/// Developer-only POS diagnostics. Kept outside the product executable so the
/// shipped app has no benchmark, probe or screenshot entry points.
///
///   dotnet run --project tools/DzentoPOS2.PosTools -- benchmark [--variants N] [--output path]
///   dotnet run --project tools/DzentoPOS2.PosTools -- probe [--db path] [--out dir] [--width W] [--height H]
///   dotnet run --project tools/DzentoPOS2.PosTools -- capture [--out dir] [--widths 375,768,1440]
///       [--themes light,dark] [--langs en,ar]
/// </summary>
internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        string command = args.Length > 0 ? args[0].ToLowerInvariant() : "help";
        string[] rest = args.Length > 1 ? args[1..] : Array.Empty<string>();

        return command switch
        {
            "benchmark" or "--pos-benchmark" => PosBenchmark.Run(rest),
            "probe" or "--pos-probe" => PosFormProbe.Run(rest),
            "capture" or "--pos-capture" => PosScreenshotProbe.Run(rest),
            _ => PrintUsage()
        };
    }

    private static int PrintUsage()
    {
        Console.WriteLine("dzento-pos-tools <command> [options]");
        Console.WriteLine("  benchmark [--variants N] [--output path]");
        Console.WriteLine("  probe     [--db path] [--out dir] [--width W] [--height H]");
        Console.WriteLine("  capture   [--out dir] [--widths 375,768,1440] [--themes light,dark] [--langs en,ar]");
        return 1;
    }
}
