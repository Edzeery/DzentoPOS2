using SkiaSharp;
using Svg.Skia;

namespace EZControls.Core.Base
{

    // ══════════════════════════════════════════════════════════════════════════════
    //  SvgImageSource
    //
    //  Issues #7 and #11 addressed here.
    //
    //  Issue #7 – SVG loading robustness:
    //    SKSvg.Load() can throw FileNotFoundException, UnauthorizedAccessException,
    //    and various SKia decode exceptions.  The original code had no try/catch,
    //    so a bad path would crash the entire control (and in designer context, the
    //    whole VS designer surface).  We catch, record the error, and surface it
    //    via a public LoadError property so the host application can react.
    //
    //  Issue #11 (SVG side) – cache per size:
    //    The SKSvg → Bitmap conversion is expensive (Skia rasterisation + PNG
    //    encode/decode round-trip).  We cache by size and reuse until the source
    //    or the size changes.
    //
    //  The PNG encode/decode round-trip from the original code is also eliminated:
    //    SKBitmap → SKImage → SKData (PNG) → MemoryStream → System.Drawing.Bitmap
    //  is replaced by direct pixel copy using LockBits, which is both faster and
    //  avoids the intermediate PNG compression entirely.
    // Install-Package SkiaSharp
    // Install-Package Svg.Skia
    // Install-Package SkiaSharp.Views.WindowsForms
    // ══════════════════════════════════════════════════════════════════════════════
    internal sealed class SvgImageSource : IDisposable
    {
        private SKSvg? _svg;
        private Bitmap? _cachedBitmap;
        private Size _cachedSize;

        // ── public state ──────────────────────────────────────────────────────────

        public bool IsLoaded => _svg?.Picture != null;
        public string LoadError { get; private set; } = string.Empty;

        // ── loading ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Loads the SVG at <paramref name="path"/>.
        /// Returns true on success.  On failure, IsLoaded is false and
        /// LoadError contains the exception message.
        /// </summary>
        public bool Load(string? path)
        {
            DisposeSkia();
            LoadError = string.Empty;

            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                if (!File.Exists(path))
                {
                    LoadError = $"File not found: {path}";
                    return false;
                }

                SKSvg svg = new();
                svg.Load(path);

                if (svg.Picture == null)
                {
                    svg.Dispose();
                    LoadError = "SVG parsed but produced no picture.";
                    return false;
                }

                _svg = svg;
                return true;
            }
            catch (Exception ex)
            {
                LoadError = ex.Message;
                return false;
            }
        }

        // ── rendering ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a System.Drawing.Bitmap rasterised to <paramref name="size"/>.
        /// The bitmap is cached – repeated calls with the same size are free.
        /// Returns null if no SVG is loaded or if rasterisation fails.
        /// </summary>
        public Bitmap? GetBitmap(Size size)
        {
            if (!IsLoaded || size.Width <= 0 || size.Height <= 0)
                return null;

            if (_cachedBitmap != null &&
                    _cachedSize == size &&
                    !_cachedBitmap.Size.IsEmpty)
            {
                return _cachedBitmap;
            }

            _cachedBitmap?.Dispose();
            _cachedBitmap = null;

            try
            {
                _cachedBitmap = Rasterise(_svg!.Picture!, size);
                _cachedSize = size;
            }
            catch
            {
                // Rasterisation failure is non-fatal – fall back to nothing.
            }

            return _cachedBitmap;
        }

        // ── private helpers ───────────────────────────────────────────────────────

        /// <summary>
        /// Rasterises the SKPicture directly into a System.Drawing.Bitmap using
        /// LockBits – no PNG encode/decode round-trip.
        /// </summary>
        private static Bitmap Rasterise(SKPicture picture, Size size)
        {
            SKRect bounds = picture.CullRect;

            float scale = Math.Min(
                size.Width / bounds.Width,
                size.Height / bounds.Height);

            using SKBitmap skBitmap = new(
                size.Width, size.Height,
                SKColorType.Bgra8888, SKAlphaType.Premul);

            using (SKCanvas canvas = new(skBitmap))
            {
                canvas.Clear(SKColors.Transparent);
                canvas.Scale(scale);
                canvas.DrawPicture(picture);
            }

            // Direct pixel copy via LockBits – no intermediate PNG.
            Bitmap gdiBitmap = new(
                size.Width, size.Height,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            System.Drawing.Imaging.BitmapData bmpData = gdiBitmap.LockBits(
                new Rectangle(Point.Empty, size),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                IntPtr skPtr = skBitmap.GetPixels();
                IntPtr gdiPtr = bmpData.Scan0;
                int bytes = size.Width * size.Height * 4;

                // Safe copy: no /unsafe compiler flag required.
                // Marshal.Copy needs a managed array as intermediate; the array is
                // short-lived and collected in gen-0 – acceptable for a cache-miss
                // path that runs infrequently.
                byte[] pixelBytes = new byte[bytes];
                System.Runtime.InteropServices.Marshal.Copy(skPtr, pixelBytes, 0, bytes);
                System.Runtime.InteropServices.Marshal.Copy(pixelBytes, 0, gdiPtr, bytes);
            }
            finally
            {
                gdiBitmap.UnlockBits(bmpData);
            }

            return gdiBitmap;
        }

        // ── IDisposable ───────────────────────────────────────────────────────────

        private void DisposeSkia()
        {
            _cachedBitmap?.Dispose();
            _cachedBitmap = null;
            _svg?.Dispose();
            _svg = null;
        }

        public void Dispose() => DisposeSkia();
    }
}
