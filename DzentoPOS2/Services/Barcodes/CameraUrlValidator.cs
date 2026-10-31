using System;
using System.Text.RegularExpressions;

namespace DzentoPOS2.WinForms.Services.Barcodes
{
    /// <summary>
    /// Validates MJPEG/IP camera URLs for the barcode scanner.
    /// </summary>
    public static class CameraUrlValidator
    {
        private static readonly Regex UrlPattern = new(
            @"^https?://[a-zA-Z0-9\-\.]+(:\d{1,5})?(/.*)?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Returns true when <paramref name="url"/> is a valid http/https URL
        /// with a host and optional port/path.
        /// </summary>
        public static bool IsValid(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            url = url.Trim();

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return false;

            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                return false;

            if (string.IsNullOrWhiteSpace(uri.Host))
                return false;

            // Validate port range if present.
            if (!uri.IsDefaultPort && (uri.Port < 1 || uri.Port > 65535))
                return false;

            return UrlPattern.IsMatch(url);
        }

        /// <summary>
        /// Returns a normalized (trimmed) URL or null if invalid.
        /// </summary>
        public static string? Normalize(string? url)
        {
            if (!IsValid(url))
                return null;

            return url!.Trim();
        }
    }
}