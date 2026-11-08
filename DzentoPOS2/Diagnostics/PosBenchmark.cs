using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DzentoPOS2.App;
using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.Core.Configuration;
using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Core.ValueObjects.Common;
using DzentoPOS2.Core.ValueObjects.Inventory;
using DzentoPOS2.Data;
using DzentoPOS2.Data.Context;
using DzentoPOS2.WinForms.Controls.Pos;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DzentoPOS2.WinForms.Diagnostics;

/// <summary>
/// In-app POS performance benchmark. Creates a temporary SQLite database with
/// 5,000+ variants, measures screen-open, scroll, scan and invalidation times,
/// and writes a JSON report to docs/evidence/.
/// Run: DzentoPOS2.WinForms.exe --pos-benchmark [--variants N] [--output path]
/// </summary>
public static class PosBenchmark
{
    public static int Run(string[] args)
    {
        int variantCount = 5000;
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], "--variants", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(args[i + 1], out var v) && v > 0)
            {
                variantCount = v;
            }
        }

        string outputPath = GetOption(args, "--output")
            ?? Path.Combine(Environment.CurrentDirectory, "docs", "evidence",
                $"pos-benchmark-{DateTime.Now:yyyy-MM-dd}.json");

        int products = Math.Max(1, variantCount / 10);
        int variantsPerProduct = Math.Max(1, variantCount / products);

        string dbPath = Path.Combine(Path.GetTempPath(), $"dzento-pos-bench-{Guid.NewGuid():N}.db");

        try
        {
            // 1. Create and seed the database.
            var sw = Stopwatch.StartNew();
            SeedDatabase(dbPath, products, variantsPerProduct);
            long seedMs = sw.ElapsedMilliseconds;

            // 2. Build services.
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = $"Data Source={dbPath}"
                }).Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(config);
            services.AddLogging();
            services.AddDataServices(config);
            services.AddApplicationServices();

            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var notifier = scope.ServiceProvider.GetRequiredService<ICatalogChangeNotifier>();
            var store = new PosProductStore(context, notifier);

            // 3. Cold load.
            sw.Restart();
            var items = store.GetAllAsync().GetAwaiter().GetResult();
            long coldLoadMs = sw.ElapsedMilliseconds;

            // 4. Scroll benchmark (render 100 frames).
            var list = new PosProductList { Items = items, ViewMode = PosViewMode.Cards };
            list.ClientSize = new Size(1440, 800);
            list.CreateControl();

            var frameTimes = new List<double>();
            for (int f = 0; f < 100; f++)
            {
                sw.Restart();
                using var bmp = new Bitmap(list.ClientSize.Width, list.ClientSize.Height);
                list.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
                frameTimes.Add(sw.Elapsed.TotalMilliseconds);
                list.ScrollOffset += 20;
            }
            list.Dispose();

            frameTimes.Sort();
            double p50 = Percentile(frameTimes, 50);
            double p95 = Percentile(frameTimes, 95);
            double p99 = Percentile(frameTimes, 99);
            double maxFrame = frameTimes.Count > 0 ? frameTimes[^1] : 0;

            // 5. Barcode scan benchmark.
            string testBarcode = items.FirstOrDefault()?.PrimaryBarcode ?? "TEST-0000";
            if (string.IsNullOrWhiteSpace(testBarcode))
                testBarcode = items.First().SKU ?? "SKU-0000";

            var scanTimes = new List<double>();
            for (int s = 0; s < 50; s++)
            {
                sw.Restart();
                var found = store.FindByBarcodeAsync(testBarcode).GetAwaiter().GetResult();
                scanTimes.Add(sw.Elapsed.TotalMilliseconds);
            }
            scanTimes.Sort();
            double scanP50 = Percentile(scanTimes, 50);
            double scanP95 = Percentile(scanTimes, 95);

            // 6. Invalidation benchmark.
            sw.Restart();
            store.Invalidate();
            var reloaded = store.GetAllAsync().GetAwaiter().GetResult();
            long invalidateReloadMs = sw.ElapsedMilliseconds;

            // 7. Build report.
            var report = new PosBenchmarkReport(
                DateTimeOffset.Now,
                products * variantsPerProduct,
                products,
                variantsPerProduct,
                seedMs,
                coldLoadMs,
                p50, p95, p99, maxFrame,
                scanP50, scanP95,
                invalidateReloadMs,
                new Thresholds(
                    ColdLoadMs: 500,
                    ScrollP95Ms: 16,
                    ScanP95Ms: 100,
                    InvalidateReloadMs: 500),
                new PassFail(
                    ColdLoad: coldLoadMs <= 500,
                    ScrollP95: p95 <= 16,
                    ScanP95: scanP95 <= 100,
                    InvalidateReload: invalidateReloadMs <= 500));

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            File.WriteAllText(outputPath,
                JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));

            Console.WriteLine($"POS benchmark: {report.TotalVariants} variants");
            Console.WriteLine($"  Seed: {seedMs} ms | Cold load: {coldLoadMs} ms");
            Console.WriteLine($"  Scroll p50/p95/p99/max: {p50:F1}/{p95:F1}/{p99:F1}/{maxFrame:F1} ms");
            Console.WriteLine($"  Scan p50/p95: {scanP50:F1}/{scanP95:F1} ms");
            Console.WriteLine($"  Invalidate+reload: {invalidateReloadMs} ms");
            Console.WriteLine($"  Report: {outputPath}");
            Console.WriteLine($"  Result: {(report.PassFail.AllPassed ? "PASSED" : "FAILED")}");

            return report.PassFail.AllPassed ? 0 : 1;
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); }
            catch (Exception) { /* best effort cleanup */ }
        }
    }

    private static void SeedDatabase(string dbPath, int products, int variantsPerProduct)
    {
        if (File.Exists(dbPath)) File.Delete(dbPath);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={dbPath}"
            }).Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging();
        services.AddDataServices(config);

        using var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        ctx.Database.Migrate();

        var category = new ProductCategory { Name = "Cat", IsActive = true };
        var unit = new ProductUnit { Name = "pc", Symbol = "pc", IsBaseUnit = true };
        var branch = new Branch { Name = "Main", IsMainBranch = true, IsActive = true };
        var warehouse = new Warehouse { Name = "WH", BranchId = branch.Id, IsActive = true };

        ctx.Branches.Add(branch);
        ctx.Warehouses.Add(warehouse);
        ctx.ProductCategories.Add(category);
        ctx.ProductUnits.Add(unit);
        ctx.SaveChanges();

        var random = new Random(42);

        for (int p = 0; p < products; p++)
        {
            var product = new Product
            {
                Name = $"Product {p}",
                SKU = $"SKU-{p:D4}",
                CategoryId = category.Id,
                UnitId = unit.Id,
                CostPrice = new Money(5 + (decimal)(random.NextDouble() * 20), "DZD"),
                SalePrice = new Money(10 + (decimal)(random.NextDouble() * 30), "DZD"),
                MinimumStock = new Quantity(0),
                TrackStock = true,
                IsActive = true
            };
            ctx.Products.Add(product);
            ctx.SaveChanges();

            for (int v = 0; v < variantsPerProduct; v++)
            {
                var variant = new ProductVariant
                {
                    ProductId = product.Id,
                    Name = $"Variant {v}",
                    SKU = $"SKU-{p:D4}-V{v}",
                    CostPrice = new Money(product.CostPrice.Amount, "DZD"),
                    SalePrice = new Money(product.SalePrice.Amount, "DZD"),
                    MinimumStock = new Quantity(0),
                    TrackStock = true,
                    IsActive = true,
                    IsDefault = (v == 0)
                };
                ctx.ProductVariants.Add(variant);
                ctx.SaveChanges();

                // Add a barcode for the first variant of each product.
                if (v == 0)
                {
                    ctx.ProductBarcodes.Add(new ProductBarcode
                    {
                        VariantId = variant.Id,
                        Barcode = $"BC-{p:D6}",
                        IsPrimary = true
                    });
                    ctx.SaveChanges();
                }

                var stock = new ProductStock
                {
                    ProductId = product.Id,
                    VariantId = variant.Id,
                    WarehouseId = warehouse.Id,
                    Quantity = new Quantity(random.Next(0, 100))
                };
                ctx.ProductStocks.Add(stock);
                ctx.SaveChanges();
            }
        }
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

    private static double Percentile(List<double> sorted, int pct)
    {
        if (sorted.Count == 0) return 0;
        int idx = (int)Math.Ceiling(pct / 100.0 * sorted.Count) - 1;
        idx = Math.Clamp(idx, 0, sorted.Count - 1);
        return sorted[idx];
    }

    // Report types
    private sealed record PosBenchmarkReport(
        DateTimeOffset GeneratedAt,
        int TotalVariants,
        int Products,
        int VariantsPerProduct,
        long SeedMs,
        long ColdLoadMs,
        double ScrollP50Ms,
        double ScrollP95Ms,
        double ScrollP99Ms,
        double ScrollMaxMs,
        double ScanP50Ms,
        double ScanP95Ms,
        long InvalidateReloadMs,
        Thresholds Thresholds,
        PassFail PassFail);

    private sealed record Thresholds(
        long ColdLoadMs,
        long ScrollP95Ms,
        long ScanP95Ms,
        long InvalidateReloadMs);

    private sealed record PassFail(
        bool ColdLoad,
        bool ScrollP95,
        bool ScanP95,
        bool InvalidateReload)
    {
        public bool AllPassed => ColdLoad && ScrollP95 && ScanP95 && InvalidateReload;
    }
}
