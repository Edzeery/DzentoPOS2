using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Runtime.InteropServices;

namespace EZControls.Core.Helpers;

// ══════════════════════════════════════════════════════════════════════════════
//  AlignmentHelper
//
//  Maps ContentAlignment to pixel offsets using a pre-built lookup table.
//  Zero allocations on the hot (per-paint) path.
//
//  Previous implementation used ContentAlignment.ToString().Contains(…) which:
//    1. Allocates a string on every call.
//    2. Was wrong for MiddleLeft/Right (both axes must be decoded independently).
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Converts <see cref="ContentAlignment"/> values to pixel offsets using a
/// compile-time lookup table.  Zero allocation on the hot path.
/// </summary>
public static class AlignmentHelper
{
    // (h, v) where h: 0=left 1=center 2=right;  v: 0=top 1=middle 2=bottom
    private static readonly Dictionary<ContentAlignment, (int h, int v)> s_map =
        new(9)
        {
            { ContentAlignment.TopLeft,      (0, 0) },
            { ContentAlignment.TopCenter,    (1, 0) },
            { ContentAlignment.TopRight,     (2, 0) },
            { ContentAlignment.MiddleLeft,   (0, 1) },
            { ContentAlignment.MiddleCenter, (1, 1) },
            { ContentAlignment.MiddleRight,  (2, 1) },
            { ContentAlignment.BottomLeft,   (0, 2) },
            { ContentAlignment.BottomCenter, (1, 2) },
            { ContentAlignment.BottomRight,  (2, 2) },
        };

    /// <summary>
    /// Returns the (dx, dy) pixel offset to position content of size
    /// (<paramref name="contentW"/>, <paramref name="contentH"/>) inside a
    /// container of size (<paramref name="containerW"/>, <paramref name="containerH"/>)
    /// according to <paramref name="align"/>.
    /// </summary>
    public static (int dx, int dy) GetOffset(
        ContentAlignment align,
        int containerW, int containerH,
        int contentW, int contentH)
    {
        if (!s_map.TryGetValue(align, out (int h, int v) f))
            f = (1, 1); // safe default: centre

        int dx = f.h switch
        {
            0 => 0,
            1 => (containerW - contentW) / 2,
            _ => containerW - contentW
        };

        int dy = f.v switch
        {
            0 => 0,
            1 => (containerH - contentH) / 2,
            _ => containerH - contentH
        };

        return (dx, dy);
    }
}
