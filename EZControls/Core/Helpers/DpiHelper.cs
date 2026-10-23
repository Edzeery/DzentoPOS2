namespace EZControls.Core.Helpers;


// ══════════════════════════════════════════════════════════════════════════════
//  DpiHelper
//
//  Provides DPI-aware scaling helpers.
//  ScaleFactor creates a temporary Graphics each call – callers that need the
//  value repeatedly should cache it locally.
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Utilities for DPI-aware layout calculations.
/// </summary>
public static class DpiHelper
{
    /// <summary>
    /// The current display scale factor relative to 96 DPI.
    /// On a 96 DPI screen this returns 1.0; on a 192 DPI (200 %) screen it
    /// returns 2.0.
    /// </summary>
    /// <remarks>
    /// This method creates a temporary <see cref="Graphics"/> context on every
    /// call.  Cache the result if you need it in a tight loop.
    /// </remarks>
    public static float ScaleFactor
    {
        get
        {
            using Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            return g.DpiX / 96f;
        }
    }

    /// <summary>Scales <paramref name="value"/> by the current DPI factor.</summary>
    public static int Scale(int value) => (int)(value * ScaleFactor);

    /// <summary>Scales both dimensions of <paramref name="size"/> by the current DPI factor.</summary>
    public static Size Scale(Size size) => new(Scale(size.Width), Scale(size.Height));
}
