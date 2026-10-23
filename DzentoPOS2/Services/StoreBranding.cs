using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace DzentoPOS2.WinForms.Services;

/// <summary>
/// Manages the custom store logo: importing an image, storing it next to the
/// application and handing out a fresh copy for the UI (picture boxes take
/// ownership of the image they receive, so a clone is always returned).
/// </summary>
public static class StoreBranding
{
    private const int MaxLogoSize = 640;

    /// <summary>Folder that stores the imported logo.</summary>
    public static string LogoDirectory =>
        Path.Combine(Application.StartupPath, "branding");

    /// <summary>Full path of the stored logo file.</summary>
    public static string LogoFile =>
        Path.Combine(LogoDirectory, "store-logo.png");

    /// <summary>True when a custom logo was imported by the user.</summary>
    public static bool HasCustomLogo =>
        File.Exists(LogoFile);

    /// <summary>
    /// Imports an image as the store logo. The image is scaled down to a
    /// reasonable size and stored as PNG so it can be reloaded losslessly.
    /// </summary>
    /// <returns>The path of the stored logo.</returns>
    public static string SaveLogo(string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
            throw new FileNotFoundException("Logo file not found.", sourcePath);

        Directory.CreateDirectory(LogoDirectory);

        using var source = Image.FromFile(sourcePath);
        using var scaled = Scale(source, MaxLogoSize);
        using var canvas = new Bitmap(
            Math.Max(1, scaled.Width),
            Math.Max(1, scaled.Height),
            PixelFormat.Format32bppArgb);

        using (var g = Graphics.FromImage(canvas))
        {
            g.Clear(Color.Transparent);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.DrawImage(scaled, new Rectangle(0, 0, canvas.Width, canvas.Height));
        }

        canvas.Save(LogoFile, ImageFormat.Png);
        return LogoFile;
    }

    /// <summary>Loads the custom logo, or null when none was imported.</summary>
    public static Image? LoadLogo()
    {
        if (!HasCustomLogo) return null;

        try
        {
            using var source = Image.FromFile(LogoFile);
            return new Bitmap(source);
        }
        catch
        {
            // A broken logo must never break the shell.
            return null;
        }
    }

    /// <summary>Removes the custom logo and returns the built-in one.</summary>
    public static void ClearLogo()
    {
        try
        {
            if (File.Exists(LogoFile))
                File.Delete(LogoFile);
        }
        catch
        {
            // Ignore: the logo is optional.
        }
    }

    private static Bitmap Scale(Image source, int maxSize)
    {
        var width = source.Width;
        var height = source.Height;

        if (width <= maxSize && height <= maxSize)
            return new Bitmap(source);

        var ratio = Math.Min((double)maxSize / width, (double)maxSize / height);
        var targetWidth = Math.Max(1, (int)Math.Round(width * ratio));
        var targetHeight = Math.Max(1, (int)Math.Round(height * ratio));

        return new Bitmap(targetWidth, targetHeight, PixelFormat.Format32bppArgb);
    }
}
