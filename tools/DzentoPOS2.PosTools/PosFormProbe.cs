using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows.Forms;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.WinForms.Forms.Sales;
using DzentoPOS2.WinForms.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DzentoPOS2.PosTools;

/// <summary>
/// Builds the real <see cref="SalesForm"/> against a temporary copy of the
/// catalog database, lets the first data load finish, reads a few private
/// fields for diagnostics, and saves before/after PNGs.
///
/// The production database is never opened by the form: the tool copies it to
/// the temp folder and points the connection string at the copy.
/// </summary>
internal static class PosFormProbe
{
    private static readonly string ProductionDatabase = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DzentoPOS2", "Database", "DzentoPOS.db");

    public static int Run(string[] args)
    {
        string? explicitDb = GetOption(args, "--db");
        string outputDir = GetOption(args, "--out")
            ?? Path.Combine(Environment.CurrentDirectory, "docs", "evidence");
        int width = GetInt(args, "--width", 1330);
        int height = GetInt(args, "--height", 790);
        int waitMs = GetInt(args, "--wait", 5000);
        bool keep = args.Any(a => string.Equals(a, "--keep", StringComparison.OrdinalIgnoreCase));

        string source = explicitDb ?? ProductionDatabase;
        if (!File.Exists(source))
        {
            Console.Error.WriteLine($"Database not found: {source}");
            return 2;
        }

        string sourceHashBefore = Hash(source);
        string tempDir = Path.Combine(Path.GetTempPath(), $"dzento-pos-probe-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        string dbCopy = Path.Combine(tempDir, "DzentoPOS.db");
        File.Copy(source, dbCopy, overwrite: true);

        try
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = $"Data Source={dbCopy}",
                    ["Localization:DefaultLanguage"] = "ar",
                    ["Licensing:Enforce"] = "false"
                })
                .Build();

            var services = new ServiceCollection();
            services.AddWinForms(config);
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();

            // No signed-in user: an empty branch context keeps invoice queries
            // harmless instead of throwing a modal error dialog.
            provider.GetRequiredService<IActiveBranchContext>().InitializeEmpty();

            using var form = scope.ServiceProvider.GetRequiredService<SalesForm>();
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(-6000, 0);
            form.Size = new Size(width, height);
            form.Show();

            Directory.CreateDirectory(outputDir);
            string stem = $"pos-probe-{width}x{height}";

            Application.DoEvents();
            Save(form, outputDir, $"{stem}-t0.png");
            Console.WriteLine(
                $"[t=0] products={ReadCount(form, "_products")} " +
                $"gridRows={ReadInt(form, "_productGrid", "Rows", "Count")}");

            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < waitMs)
            {
                Application.DoEvents();
                Thread.Sleep(50);
                if ((ReadCount(form, "_products") ?? 0) > 0) break;
            }

            // Let the first layout/scroll settle before the final capture.
            for (int i = 0; i < 10; i++)
            {
                Application.DoEvents();
                Thread.Sleep(30);
            }

            Console.WriteLine(
                $"[t={sw.ElapsedMilliseconds}ms] products={ReadCount(form, "_products")} " +
                $"filtered={ReadCount(form, "_filteredProducts")} " +
                $"gridRows={ReadInt(form, "_productGrid", "Rows", "Count")}");
            Save(form, outputDir, $"{stem}.png");

            form.Close();

            string sourceHashAfter = Hash(source);
            Console.WriteLine($"source: {source}");
            Console.WriteLine($"copy:   {dbCopy}");
            Console.WriteLine($"source sha256 before: {sourceHashBefore}");
            Console.WriteLine($"source sha256 after:  {sourceHashAfter}");
            Console.WriteLine(sourceHashBefore == sourceHashAfter
                ? "source unchanged (probe read the copy only)"
                : "WARNING: source database changed during the probe");
            Console.WriteLine($"output: {outputDir}");
        }
        finally
        {
            if (!keep)
            {
                try { Directory.Delete(tempDir, recursive: true); }
                catch (IOException) { /* best effort */ }
            }
        }

        return 0;
    }

    private static void Save(Form form, string outputDir, string fileName)
    {
        using var bitmap = new Bitmap(form.Width, form.Height);
        form.DrawToBitmap(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
        string path = Path.Combine(outputDir, fileName);
        bitmap.Save(path, ImageFormat.Png);
        Console.WriteLine($"saved: {path}");
    }

    private static string Hash(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    private static object? ReadField(object target, string name) =>
        target.GetType()
            .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)?
            .GetValue(target);

    private static int? ReadCount(object target, string name)
    {
        object? value = ReadField(target, name);
        return value?.GetType().GetProperty("Count")?.GetValue(value) as int?;
    }

    private static int? ReadInt(object target, string field, string property, string nested)
    {
        object? value = ReadField(target, field);
        object? inner = value?.GetType().GetProperty(property)?.GetValue(value);
        return inner?.GetType().GetProperty(nested)?.GetValue(inner) as int?;
    }

    private static string? GetOption(string[] args, string name)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        }
        return null;
    }

    private static int GetInt(string[] args, string name, int fallback)
    {
        string? raw = GetOption(args, name);
        return int.TryParse(raw, out int value) && value > 0 ? value : fallback;
    }
}
