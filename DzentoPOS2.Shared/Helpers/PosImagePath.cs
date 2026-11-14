using System;
using System.IO;
using System.Linq;

namespace DzentoPOS2.Shared.Helpers;

/// <summary>
/// Resolves a stored product image path to an absolute file path.
/// Product rows store paths relative to the application's images folder
/// (for example <c>products/abc.jpg</c>); treating them as absolute was why the
/// POS screen never showed an image.
/// </summary>
public static class PosImagePath
{
    /// <summary>
    /// Returns the absolute path when the file exists, otherwise <c>null</c>.
    /// Relative paths are resolved under <paramref name="imagesRoot"/>; a
    /// leading <c>images/</c> segment is tolerated, and paths that escape the
    /// images folder are rejected.
    /// </summary>
    public static string? Resolve(string? imagesRoot, string? storedPath)
    {
        if (string.IsNullOrWhiteSpace(imagesRoot) || string.IsNullOrWhiteSpace(storedPath))
            return null;

        var normalized = storedPath.Trim().Replace('\\', '/');
        if (Path.IsPathRooted(normalized))
            return File.Exists(normalized) ? Path.GetFullPath(normalized) : null;

        var segments = normalized
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(s => s != ".")
            .ToList();

        if (segments.Count == 0 || segments.Any(s => s == ".."))
            return null;

        if (string.Equals(segments[0], "images", StringComparison.OrdinalIgnoreCase))
            segments.RemoveAt(0);

        if (segments.Count == 0)
            return null;

        var root = Path.GetFullPath(imagesRoot);
        var full = Path.GetFullPath(Path.Combine(new[] { root }.Concat(segments).ToArray()));

        var rootWithSeparator = root.EndsWith(Path.DirectorySeparatorChar)
            ? root
            : root + Path.DirectorySeparatorChar;

        if (!full.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
            return null;

        return File.Exists(full) ? full : null;
    }
}
