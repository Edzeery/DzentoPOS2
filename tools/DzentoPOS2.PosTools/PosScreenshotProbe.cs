using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Core.ValueObjects.Common;
using DzentoPOS2.Core.ValueObjects.Inventory;
using DzentoPOS2.Data;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Sales;
using DzentoPOS2.Shared.Helpers;
using DzentoPOS2.WinForms.Configurations;
using DzentoPOS2.WinForms.Forms.Sales;
using DzentoPOS2.WinForms.Infrastructure;
using DzentoPOS2.WinForms.Themes;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DzentoPOS2.PosTools;

/// <summary>
/// Captures the redesigned POS screen for design review.
///
/// The tool never opens the production database. It creates a temporary SQLite
/// database, seeds a realistic catalog (multiple categories, multi-barcode
/// products, products with and without images, long names, out-of-stock and
/// low-stock rows, more rows than fit on screen) and then builds the real
/// <see cref="SalesForm"/> against it. One PNG is written per width, theme and
/// language combination.
///
/// Run:
///   dotnet run --project tools/DzentoPOS2.PosTools -- capture [--out dir]
///       [--widths 375,768,1440] [--themes light,dark] [--langs en,ar]
/// </summary>
internal static class PosScreenshotProbe
{
    private static readonly (string Name, int Width, int Height)[] Devices =
    {
        ("phone", 375, 812),
        ("tablet", 768, 1024),
        ("desktop", 1440, 900)
    };

    public static int Run(string[] args)
    {
        string outputDir = GetOption(args, "--out")
            ?? Path.Combine(Environment.CurrentDirectory, "docs", "evidence");
        string[] widths = SplitOption(args, "--widths", "375,768,1440");
        string[] themes = SplitOption(args, "--themes", "Light,Dark");
        string[] langs = SplitOption(args, "--langs", "en,ar");
        bool keep = args.Any(a => string.Equals(a, "--keep", StringComparison.OrdinalIgnoreCase));

        string tempDir = Path.Combine(Path.GetTempPath(), $"dzento-pos-capture-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        string dbPath = Path.Combine(tempDir, "DzentoPOS.db");
        string imagesDir = Path.Combine(tempDir, "images");
        Directory.CreateDirectory(imagesDir);

        try
        {
            // 1. Temporary catalog.
            SeedResult seed = SeedDatabase(dbPath, imagesDir);
            Console.WriteLine($"seeded temporary database: {dbPath}");
            Console.WriteLine($"  branch={seed.BranchId} products={seed.ProductCount} categories={seed.CategoryCount}");

            // 2. Build the real WinForms graph against the temporary database.
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = $"Data Source={dbPath}",
                    ["Localization:DefaultLanguage"] = "en",
                    ["Licensing:Enforce"] = "false"
                })
                .Build();

            var services = new ServiceCollection();
            services.AddWinForms(config);

            // The probe has no signed-in user, so the real branch context cannot
            // be initialized. The fixed context points the catalog and the cash
            // session at the seeded branch. Registered last so it wins.
            services.AddSingleton<IActiveBranchContext>(new FixedBranchContext(seed.BranchId));
            services.AddSingleton<ICurrentSettingsService>(new ProbeSettingsService());

            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();

            var languageService = provider.GetRequiredService<ILanguageService>();
            var store = scope.ServiceProvider.GetRequiredService<IPosProductStore>();

            Directory.CreateDirectory(outputDir);

            foreach (string widthText in widths)
            {
                (string device, int width, int height) = ResolveDevice(widthText);

                foreach (string themeText in themes)
                {
                    ThemeMode theme = ParseTheme(themeText);

                    foreach (string lang in langs)
                    {
                        string culture = string.Equals(lang, "ar", StringComparison.OrdinalIgnoreCase)
                            ? "ar"
                            : "en";

                        // Theme and language are process-wide; set them before the
                        // form is constructed so its first paint is already correct.
                        ApplyThemeWithoutSaving(theme);
                        languageService.SetLanguage(culture);

                        string fileName = $"pos-sales-{device}-{width}x{height}-{theme.ToString().ToLowerInvariant()}-{(culture == "ar" ? "rtl" : "ltr")}.png";
                        string path = Path.Combine(outputDir, fileName);

                        using SalesForm form = scope.ServiceProvider.GetRequiredService<SalesForm>();
                        ConfigureForm(form, width, height);
                        form.Show();

                        bool catalogReady = WaitFor(
                            () => ReadCatalog(form).Count > 0,
                            timeoutMs: 15000);

                        bool sessionReady = WaitFor(
                            () => ReadBool(form, "_sessionOpen"),
                            timeoutMs: 5000);

                        Pump(400);
                        PopulateCart(form);
                        SetTotals(form);
                        Pump(500);

                        // Product images decode on a background thread and then
                        // invalidate the list; pump until the cache has settled.
                        WaitFor(() => ReadInt(form, "_productList", "Items", "Count") > 0, timeoutMs: 3000);
                        Pump(700);

                        form.PerformLayout();
                        form.Invalidate(true);
                        form.Update();
                        Pump(200);

                        Save(form, path);
                        Console.WriteLine(
                            $"  {fileName}: catalog={catalogReady} session={sessionReady} " +
                            $"items={ReadInt(form, "_productList", "Items", "Count")} " +
                            $"subtotal='{ReadText(form, "_lblSubtotalValue")}' " +
                            $"discount='{ReadText(form, "_lblDiscountValue")}' " +
                            $"total='{ReadText(form, "_lblTotalValue")}' " +
                            $"discountWidth={ReadSize(form, "_lblDiscountValue")}");

                        form.Close();
                        Pump(100);
                    }
                }
            }

            // A second desktop capture scrolled to the middle of the catalog
            // proves that more than one screen of products exists.
            CaptureScrolled(scope.ServiceProvider, languageService, outputDir);

            Console.WriteLine($"output: {outputDir}");
            return 0;
        }
        finally
        {
            SqliteConnection.ClearAllPools();

            if (!keep)
            {
                try { Directory.Delete(tempDir, recursive: true); }
                catch (IOException) { /* best effort */ }
            }
            else
            {
                Console.WriteLine($"kept temporary files: {tempDir}");
            }
        }
    }

    private static void CaptureScrolled(
        IServiceProvider scopedProvider,
        ILanguageService languageService,
        string outputDir)
    {
        ApplyThemeWithoutSaving(ThemeMode.Light);
        languageService.SetLanguage("en");

        using SalesForm form = scopedProvider.GetRequiredService<SalesForm>();
        ConfigureForm(form, 1440, 900);
        form.Show();

        WaitFor(() => ReadCatalog(form).Count > 0, timeoutMs: 15000);
        WaitFor(() => ReadBool(form, "_sessionOpen"), timeoutMs: 5000);
        Pump(400);
        PopulateCart(form);
        SetTotals(form);
        Pump(600);

        // Scroll two rows down through the product list.
        object? list = ReadField(form, "_productList");
        if (list is not null)
        {
            SetProperty(list, "ScrollOffset", 232);
            Pump(300);
        }

        form.PerformLayout();
        form.Invalidate(true);
        form.Update();
        Pump(300);

        Save(form, Path.Combine(outputDir, "pos-sales-desktop-1440x900-light-ltr-scrolled.png"));
        form.Close();
        Pump(100);
    }

    private static void ConfigureForm(Form form, int width, int height)
    {
        // A borderless form keeps the captured pixel width equal to the tested
        // viewport (375 / 768 / 1440) instead of losing room to window chrome.
        form.FormBorderStyle = FormBorderStyle.None;
        form.StartPosition = FormStartPosition.Manual;
        form.Location = new Point(-8000, 0);
        form.MinimumSize = Size.Empty;
        form.Size = new Size(width, height);
        form.ShowInTaskbar = false;
    }

    private static void PopulateCart(SalesForm form)
    {
        IReadOnlyList<PosProductItem> catalog = ReadCatalog(form);
        if (catalog.Count == 0) return;

        // Pick the interesting rows: a normal product, the long-named product
        // that has no image, and a low-stock product with several barcodes.
        PosProductItem? coffee = catalog.FirstOrDefault(i => i.Name.StartsWith("Arabica", StringComparison.OrdinalIgnoreCase));
        PosProductItem? longName = catalog.FirstOrDefault(i => i.Name.StartsWith("Best Choice", StringComparison.OrdinalIgnoreCase));
        PosProductItem? espresso = catalog.FirstOrDefault(i => i.Name.StartsWith("Espresso", StringComparison.OrdinalIgnoreCase));

        var cart = ReadField(form, "_cart") as List<CreateSaleItemDto>;
        if (cart is null) return;

        cart.Clear();
        AddLine(cart, coffee ?? catalog[0], 1m);
        AddLine(cart, longName ?? catalog[Math.Min(1, catalog.Count - 1)], 1m);
        AddLine(cart, espresso ?? catalog[Math.Min(4, catalog.Count - 1)], 2m);

        Invoke(form, "RefreshCart");
    }

    private static void AddLine(List<CreateSaleItemDto> cart, PosProductItem item, decimal quantity)
    {
        cart.Add(new CreateSaleItemDto
        {
            ProductId = item.ProductId,
            VariantId = item.VariantId,
            VariantName = item.Name,
            Quantity = quantity,
            Price = item.SalePrice,
            OriginalPrice = item.SalePrice,
            Description = item.Name
        });
    }

    private static void SetTotals(SalesForm form)
    {
        // 19 % VAT and a 5 % basket discount exercise the tax row and the
        // success-coloured discount row without hiding the hero total.
        if (ReadField(form, "_numTax") is NumericUpDown tax)
            tax.Value = Math.Min(19m, tax.Maximum);

        if (ReadField(form, "_numDiscount") is NumericUpDown discount)
            discount.Value = Math.Min(5m, discount.Maximum);

        if (ReadField(form, "_cmbDiscountMode") is ComboBox mode && mode.Items.Count > 0)
            mode.SelectedIndex = 0;

        Invoke(form, "UpdateTotals");
    }

    private static void Save(Form form, string path)
    {
        using var bitmap = new Bitmap(form.Width, form.Height);
        form.DrawToBitmap(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
        bitmap.Save(path, ImageFormat.Png);
        Console.WriteLine($"saved: {path}");
    }

    private static IReadOnlyList<PosProductItem> ReadCatalog(SalesForm form) =>
        ReadField(form, "_catalog") as IReadOnlyList<PosProductItem> ?? Array.Empty<PosProductItem>();

    private static (string Device, int Width, int Height) ResolveDevice(string text)
    {
        if (int.TryParse(text, out int width))
        {
            (string Name, int Width, int Height) match = Devices
                .FirstOrDefault(d => d.Width == width);

            if (match.Name is not null)
                return match;

            return ("custom", width, width < 900 ? 812 : 900);
        }

        foreach ((string name, int w, int h) in Devices)
        {
            if (string.Equals(name, text, StringComparison.OrdinalIgnoreCase))
                return (name, w, h);
        }

        return ("custom", 1440, 900);
    }

    private static ThemeMode ParseTheme(string text) =>
        string.Equals(text, "dark", StringComparison.OrdinalIgnoreCase)
            ? ThemeMode.Dark
            : ThemeMode.Light;

    /// <summary>
    /// <see cref="ThemeManager.SetMode(ThemeMode)"/> writes the user's theme
    /// file. The probe must never touch it, so the private overload that skips
    /// persistence is invoked instead.
    /// </summary>
    private static void ApplyThemeWithoutSaving(ThemeMode mode)
    {
        MethodInfo? method = typeof(ThemeManager).GetMethod(
            "SetMode",
            BindingFlags.NonPublic | BindingFlags.Static,
            binder: null,
            types: new[] { typeof(ThemeMode), typeof(bool) },
            modifiers: null);

        method?.Invoke(null, new object[] { mode, false });
    }

    private static void Pump(int milliseconds)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        while (watch.ElapsedMilliseconds < milliseconds)
        {
            Application.DoEvents();
            Thread.Sleep(15);
        }
    }

    private static bool WaitFor(Func<bool> condition, int timeoutMs)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        while (watch.ElapsedMilliseconds < timeoutMs)
        {
            Application.DoEvents();
            if (condition()) return true;
            Thread.Sleep(20);
        }

        return condition();
    }

    // =========================
    // Database seed
    // =========================

    private sealed record SeedResult(Guid BranchId, int ProductCount, int CategoryCount);

    private static SeedResult SeedDatabase(string dbPath, string imagesDir)
    {
        if (File.Exists(dbPath)) File.Delete(dbPath);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={dbPath}"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging();
        services.AddDataServices(config);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        ctx.Database.Migrate();

        string[] imagePaths =
        {
            CreatePlaceholderImage(Path.Combine(imagesDir, "coffee.png"), Color.FromArgb(120, 72, 42), Color.FromArgb(206, 158, 106)),
            CreatePlaceholderImage(Path.Combine(imagesDir, "cheese.png"), Color.FromArgb(214, 168, 46), Color.FromArgb(250, 226, 148)),
            CreatePlaceholderImage(Path.Combine(imagesDir, "cups.png"), Color.FromArgb(44, 96, 124), Color.FromArgb(150, 204, 226)),
            CreatePlaceholderImage(Path.Combine(imagesDir, "bread.png"), Color.FromArgb(180, 120, 60), Color.FromArgb(238, 202, 148))
        };

        string[] categoryNames = { "Beverages", "Bakery", "Electronics", "Household", "Stationery" };

        var branch = new Branch { Name = "Main Branch", IsMainBranch = true, IsActive = true };
        var warehouse = new Warehouse { Name = "Main Warehouse", BranchId = branch.Id, IsActive = true };
        var unit = new ProductUnit { Name = "Piece", Symbol = "pc", IsBaseUnit = true };
        var categories = categoryNames
            .Select(name => new ProductCategory { Name = name, IsActive = true })
            .ToList();

        ctx.Branches.Add(branch);
        ctx.Warehouses.Add(warehouse);
        ctx.ProductUnits.Add(unit);
        ctx.ProductCategories.AddRange(categories);
        ctx.SaveChanges();

        var cashbox = new Cashbox { Name = "Main Drawer", BranchId = branch.Id, CurrentBalance = 0m, IsActive = true };
        ctx.Cashboxes.Add(cashbox);
        ctx.SaveChanges();

        ctx.CashSessions.Add(new CashSession
        {
            CashboxId = cashbox.Id,
            OpenedAtUtc = DateTime.UtcNow.AddHours(-2),
            OpeningBalance = 5000m,
            Status = CashSessionStatus.Open,
            Notes = "Screenshot probe session"
        });
        ctx.SaveChanges();

        // The first six names are fixed so the interesting cards (multi-barcode,
        // long name without image, out of stock, low stock) appear at the top.
        var seeds = new List<ProductSeed>
        {
            new("Arabica Coffee Beans 1kg", 1450m, 24, 3, imagePaths[0], 0),
            new("Best Choice Wireless Noise-Cancelling Over-Ear Headphones - Midnight Black Edition", 18900m, 3, 1, null, 2),
            new("Cheddar Cheese Block 500g", 620m, 0, 1, imagePaths[1], 1),
            new("Dishwashing Liquid Lemon 750ml", 380m, 12, 1, null, 3),
            new("Espresso Cups Set of 4", 2450m, 2, 2, imagePaths[2], 3),
            new("Fresh Baguette", 120m, 40, 1, imagePaths[3], 1)
        };

        string[] suffixes =
        {
            "Classic", "Premium", "Family Pack", "Eco", "Daily", "Organic", "Deluxe"
        };

        for (int i = 6; i < 40; i++)
        {
            string name = $"Product {i + 1:D2} {suffixes[i % suffixes.Length]}";
            decimal price = 150m + (i * 137m);
            int stock = i % 9 == 0 ? 0 : (i % 7 == 0 ? i % 4 : 8 + (i * 3 % 60));
            string? image = i % 3 == 0 ? imagePaths[i % imagePaths.Length] : null;
            seeds.Add(new ProductSeed(name, price, stock, 1, image, i % categoryNames.Length));
        }

        foreach (ProductSeed seed in seeds)
        {
            string sku = $"SKU-{seed.Name.GetHashCode(StringComparison.Ordinal) & 0x7FFFFFFF:D6}";

            var product = new Product
            {
                Name = seed.Name,
                SKU = sku,
                CategoryId = categories[seed.CategoryIndex].Id,
                UnitId = unit.Id,
                CostPrice = new Money(Math.Round(seed.Price * 0.62m, 2), "DZD"),
                SalePrice = new Money(seed.Price, "DZD"),
                MinimumStock = new Quantity(2),
                TrackStock = true,
                IsActive = true,
                ImagePath = seed.ImagePath
            };

            ctx.Products.Add(product);
            ctx.SaveChanges();

            var variant = new ProductVariant
            {
                ProductId = product.Id,
                // Every other product uses the product SKU fallback on purpose.
                Name = seed.Name,
                SKU = seed.CategoryIndex % 2 == 0 ? null : $"{sku}-V1",
                CostPrice = new Money(product.CostPrice.Amount, "DZD"),
                SalePrice = new Money(seed.Price, "DZD"),
                MinimumStock = new Quantity(2),
                TrackStock = true,
                IsActive = true,
                IsDefault = true
            };

            ctx.ProductVariants.Add(variant);
            ctx.SaveChanges();

            for (int b = 0; b < seed.BarcodeCount; b++)
            {
                ctx.ProductBarcodes.Add(new ProductBarcode
                {
                    VariantId = variant.Id,
                    Barcode = b == 0
                        ? $"BC-{seed.Name.GetHashCode(StringComparison.Ordinal) & 0x7FFFFFFF:D10}"
                        : $"BC-{seed.Name.GetHashCode(StringComparison.Ordinal) & 0x7FFFFFFF:D10}-{b}",
                    IsPrimary = b == 0
                });
            }

            ctx.ProductStocks.Add(new ProductStock
            {
                ProductId = product.Id,
                VariantId = variant.Id,
                WarehouseId = warehouse.Id,
                Quantity = new Quantity(seed.Stock)
            });

            ctx.SaveChanges();
        }

        return new SeedResult(branch.Id, seeds.Count, categories.Count);
    }

    private sealed record ProductSeed(
        string Name,
        decimal Price,
        int Stock,
        int BarcodeCount,
        string? ImagePath,
        int CategoryIndex);

    private static string CreatePlaceholderImage(string path, Color start, Color end)
    {
        using var bitmap = new Bitmap(512, 512);
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (var brush = new LinearGradientBrush(
                       new Rectangle(0, 0, 512, 512), start, end, 45f))
            {
                graphics.FillRectangle(brush, 0, 0, 512, 512);
            }

            using (var overlay = new SolidBrush(Color.FromArgb(52, Color.White)))
            {
                graphics.FillEllipse(overlay, 116, 116, 280, 280);
            }

            using (var inner = new SolidBrush(Color.FromArgb(92, Color.White)))
            {
                graphics.FillEllipse(inner, 196, 196, 120, 120);
            }
        }

        bitmap.Save(path, ImageFormat.Png);
        return Path.GetFullPath(path);
    }

    // =========================
    // Probe services
    // =========================

    /// <summary>
    /// The seeded database has a known branch; the real context needs a signed
    /// in user and is therefore replaced for the probe.
    /// </summary>
    private sealed class FixedBranchContext : IActiveBranchContext
    {
        public FixedBranchContext(Guid branchId)
        {
            ActiveBranchId = branchId;
        }

        public Guid ActiveBranchId { get; }
        public string ActiveBranchName => "Main Branch";
        public bool CanSwitchBranch => false;

        public event EventHandler<ActiveBranchChangedEventArgs>? BranchChanged
        {
            add { }
            remove { }
        }

        public Task<bool> TrySwitchBranchAsync(Guid branchId, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task InitializeAsync(DzentoPOS2.App.Session.ICurrentUserService currentUser, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public void InitializeEmpty() { }
    }

    /// <summary>
    /// Deterministic regional settings for the captures. The real service reads
    /// the developer's user-settings.json, which would make screenshots depend
    /// on the machine they were taken on.
    /// </summary>
    private sealed class ProbeSettingsService : ICurrentSettingsService
    {
        public ProbeSettingsService()
        {
            Regional = new RegionalContext
            {
                CurrencyCode = "DZD",
                CountryCode = "DZ",
                TimeZoneId = "Africa/Algiers"
            };

            Current = new AppSettings
            {
                Language = "en",
                CurrencyCode = "DZD",
                CountryCode = "DZ",
                TimeZoneId = "Africa/Algiers"
            };
        }

        public AppSettings Current { get; private set; }
        public RegionalContext Regional { get; }

        public event EventHandler? Changed;

        public void Save(AppSettings settings)
        {
            Current = settings;
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public void Update(Action<AppSettings> mutate)
        {
            mutate(Current);
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    // =========================
    // Reflection helpers
    // =========================

    private static object? ReadField(object target, string name) =>
        target.GetType()
            .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)?
            .GetValue(target);

    private static bool ReadBool(object target, string name) =>
        ReadField(target, name) is true;

    private static string? ReadText(object target, string name) =>
        ReadField(target, name) is Label label ? label.Text : null;

    private static int ReadSize(object target, string name) =>
        ReadField(target, name) is Control control ? control.Width : 0;

    private static int ReadInt(object target, string field, string property, string nested)
    {
        object? value = ReadField(target, field);
        object? inner = value?.GetType().GetProperty(property)?.GetValue(value);
        return inner?.GetType().GetProperty(nested)?.GetValue(inner) as int? ?? 0;
    }

    private static void SetProperty(object target, string name, object? value) =>
        target.GetType().GetProperty(name)?.SetValue(target, value);

    private static object? Invoke(object target, string name, params object?[] args) =>
        target.GetType()
            .GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)?
            .Invoke(target, args);

    private static string? GetOption(string[] args, string name)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        }

        return null;
    }

    private static string[] SplitOption(string[] args, string name, string fallback)
    {
        string? raw = GetOption(args, name);
        if (string.IsNullOrWhiteSpace(raw))
            raw = fallback;

        return raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
