using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZControls.Core.Helpers;

// ══════════════════════════════════════════════════════════════════════════════
//  PlaceholderCache
//
//  Generates the default circular placeholder image once and shares the
//  singleton across all EZCircularPictureBox instances.
//
//  The instance is never disposed: it lives for the process lifetime, the same
//  as System.Drawing.SystemIcons or framework-provided bitmaps.
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Provides a lazily-initialized, process-lifetime placeholder <see cref="Bitmap"/>
/// for controls that have no image assigned.
/// </summary>
public static class PlaceholderCache
{
    private static readonly Lazy<Bitmap> s_instance =
        new(Build, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// The shared placeholder bitmap.
    /// Callers must NOT dispose this instance.
    /// </summary>
    public static Bitmap Instance => s_instance.Value;

    private static Bitmap Build()
    {
        Bitmap bmp = new(128, 128);
        using Graphics g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.FromArgb(45, 45, 48));

        // Silhouette: outer circle (body) + inner circle (head)
        using SolidBrush body = new(Color.FromArgb(90, 90, 95));
        g.FillEllipse(body, 20, 20, 88, 88);

        using SolidBrush head = new(Color.FromArgb(60, 60, 65));
        g.FillEllipse(head, 44, 32, 40, 40);

        return bmp;
    }
}

