using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using DzentoPOS2.Core.Configuration;

namespace DzentoPOS2.WinForms.Services.Images;

public class ImageService : IImageService
{
    private const int MaxImageSize = 1024;
    private const int ThumbSize = 256;
    private static readonly string ImagesRoot = Path.Combine(AppPaths.Root, "images");

    // Simple LRU cache for loaded images (thumbnails).
    private static readonly LinkedList<string> _lru = new();
    private static readonly Dictionary<string, (Image Img, LinkedListNode<string> Node)> _cache = new();
    private const int CacheCapacity = 64;
    private static readonly object _lock = new();

    public ImageService()
    {
        Directory.CreateDirectory(ImagesRoot);
    }

    public Task<(string ImagePath, string ThumbnailPath)> SaveAsync(string sourcePath, string ownerFolder)
    {
        return Task.Run(() =>
        {
            using var src = Image.FromFile(sourcePath);
            var folder = Path.Combine(ImagesRoot, ownerFolder);
            Directory.CreateDirectory(folder);
            var baseName = Guid.NewGuid().ToString("N");
            var imgRel = Path.Combine(ownerFolder, baseName + ".jpg").Replace('\\', '/');
            var thumbRel = Path.Combine(ownerFolder, baseName + "_t.jpg").Replace('\\', '/');
            var imgFull = Path.Combine(ImagesRoot, ownerFolder, baseName + ".jpg");
            var thumbFull = Path.Combine(ImagesRoot, ownerFolder, baseName + "_t.jpg");

            SaveResized(src, imgFull, MaxImageSize);
            SaveResized(src, thumbFull, ThumbSize);

            return (imgRel, thumbRel);
        });
    }

    public Task DeleteAsync(string? relativePath)
    {
        return Task.Run(() =>
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;
            var full = Path.Combine(ImagesRoot, relativePath.Replace('/', '\\'));
            try { if (File.Exists(full)) File.Delete(full); }
            catch { /* best-effort cleanup; a locked file is not fatal */ }
            // thumbnail
            var dir = Path.GetDirectoryName(full)!;
            var name = Path.GetFileNameWithoutExtension(full);
            var thumb = Path.Combine(dir, name + "_t.jpg");
            try { if (File.Exists(thumb)) File.Delete(thumb); }
            catch { /* best-effort cleanup; a locked file is not fatal */ }
        });
    }

    public Task<Image?> LoadAsync(string? relativePath, bool thumbnail = false)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return Task.FromResult<Image?>(null);
        var key = relativePath + (thumbnail ? "|t" : "");
        lock (_lock)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                _lru.Remove(entry.Node);
                _lru.AddFirst(entry.Node);
                return Task.FromResult<Image?>(new Bitmap(entry.Img));
            }
        }
        return Task.Run<Image?>(() =>
        {
            var full = Path.Combine(ImagesRoot, relativePath.Replace('/', '\\'));
            if (thumbnail)
            {
                var dir = Path.GetDirectoryName(full)!;
                var name = Path.GetFileNameWithoutExtension(full);
                full = Path.Combine(dir, name + "_t.jpg");
            }
            if (!File.Exists(full)) return null;
            using var fs = new FileStream(full, FileMode.Open, FileAccess.Read);
            using var img = Image.FromStream(fs);
            var copy = new Bitmap(img);
            lock (_lock)
            {
                if (_cache.Count >= CacheCapacity)
                {
                    var last = _lru.Last;
                    if (last != null)
                    {
                        _lru.RemoveLast();
                        if (_cache.TryGetValue(last.Value, out var e))
                        {
                            e.Img.Dispose();
                            _cache.Remove(last.Value);
                        }
                    }
                }
                var node = _lru.AddFirst(key);
                _cache[key] = (copy, node);
            }
            return new Bitmap(copy);
        });
    }

    private static void SaveResized(Image src, string path, int maxSize)
    {
        int w = src.Width, h = src.Height;
        if (w > maxSize || h > maxSize)
        {
            if (w >= h)
            {
                h = (int)((double)h * maxSize / w);
                w = maxSize;
            }
            else
            {
                w = (int)((double)w * maxSize / h);
                h = maxSize;
            }
        }
        using var bmp = new Bitmap(w, h);
        using (var g = Graphics.FromImage(bmp))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.DrawImage(src, 0, 0, w, h);
        }
        var jpegCodec = ImageCodecInfo.GetImageEncoders().FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);
        var ep = new EncoderParameters(1);
        ep.Param[0] = new EncoderParameter(Encoder.Quality, 85L);
        bmp.Save(path, jpegCodec, ep);
    }
}
