using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.Core.Configuration;
using DzentoPOS2.WinForms.Controls.Pos;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Diagnostics;

/// <summary>
/// Generates PNG screenshots of PosProductList at all combinations of
/// width × mode × direction × theme for visual verification.
/// Run: DzentoPOS2.WinForms.exe --pos-screenshot [--output dir]
/// </summary>
public static class PosScreenshot
{
    public static int Run(string[] args)
    {
        string outputDir = GetOption(args, "--output")
            ?? Path.Combine(Environment.CurrentDirectory, "docs", "evidence");

        Directory.CreateDirectory(outputDir);

        // Sample data
        var items = new List<PosProductItem>();
        for (int i = 0; i < 20; i++)
        {
            items.Add(new PosProductItem(
                Guid.NewGuid(), Guid.NewGuid(),
                $"Product {i}", $"SKU-{i:D4}",
                10 + i * 2.5m, 5 + i,
                true, i % 3 == 0,
                Guid.NewGuid(), "pc", Guid.NewGuid(),
                null, null, $"BC-{i:D6}"));
        }

        int[] widths = { 375, 768, 1440 };
        var modes = new[] { PosViewMode.Cards, PosViewMode.List };
        var directions = new[] { false, true }; // LTR, RTL
        var themes = new[] { ThemeMode.Light, ThemeMode.Dark };

        int count = 0;
        foreach (int w in widths)
        {
            foreach (var mode in modes)
            {
                foreach (bool rtl in directions)
                {
                    foreach (var theme in themes)
                    {
                        ThemeManager.SetMode(theme);

                        var list = new PosProductList
                        {
                            Items = items,
                            ViewMode = mode,
                            RightToLeft = rtl ? RightToLeft.Yes : RightToLeft.No
                        };
                        list.ClientSize = new Size(w, 800);
                        list.CreateControl();

                        using var bmp = new Bitmap(w, 800);
                        list.DrawToBitmap(bmp, new Rectangle(0, 0, w, 800));

                        string modeName = mode == PosViewMode.Cards ? "cards" : "list";
                        string dirName = rtl ? "rtl" : "ltr";
                        string themeName = theme == ThemeMode.Light ? "light" : "dark";
                        string fileName = $"pos-{w}-{modeName}-{dirName}-{themeName}.png";
                        string filePath = Path.Combine(outputDir, fileName);
                        bmp.Save(filePath, ImageFormat.Png);
                        count++;

                        list.Dispose();
                    }
                }
            }
        }

        ThemeManager.SetMode(ThemeMode.Light);

        Console.WriteLine($"Generated {count} PNG screenshots in {outputDir}");
        return 0;
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
}
