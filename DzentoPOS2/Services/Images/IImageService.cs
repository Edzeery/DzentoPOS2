using System.Drawing;

namespace DzentoPOS2.WinForms.Services.Images;

public interface IImageService
{
    /// <summary>Saves an image file, resizes to 1024px + 256px thumbnail, returns relative paths.</summary>
    Task<(string ImagePath, string ThumbnailPath)> SaveAsync(string sourcePath, string ownerFolder);

    /// <summary>Deletes an image and its thumbnail by relative path.</summary>
    Task DeleteAsync(string? relativePath);

    /// <summary>Loads an image from relative path (caller disposes). Cached LRU.</summary>
    Task<Image?> LoadAsync(string? relativePath, bool thumbnail = false);
}
