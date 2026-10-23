namespace EZControls.Core.Extensions;


// ══════════════════════════════════════════════════════════════════════════════
//  ColorExtensions
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Extension methods for <see cref="Color"/> manipulation.
/// All methods return new <see cref="Color"/> values; the original is unchanged.
/// </summary>
public static class ColorExtensions
{
    /// <summary>
    /// Returns a new <see cref="Color"/> with each RGB channel multiplied by
    /// <paramref name="factor"/>.  Values are clamped to [0, 255].
    /// Alpha is preserved.
    /// </summary>
    /// <param name="color">Source color.</param>
    /// <param name="factor">
    /// Multiplier.  Values &lt; 1 darken; values &gt; 1 lighten.
    /// </param>
    public static Color ChangeBrightness(this Color color, float factor)
    {
        return Color.FromArgb(
            color.A,
            Math.Clamp((int)(color.R * factor), 0, 255),
            Math.Clamp((int)(color.G * factor), 0, 255),
            Math.Clamp((int)(color.B * factor), 0, 255));
    }

    /// <summary>
    /// Returns <see langword="true"/> when the color has a perceptual brightness
    /// above 0.8 (i.e. it is a "light" color that benefits from dark text).
    /// </summary>
    public static bool IsLight(this Color color) =>
        color.GetBrightness() >= 0.8f;

    /// <summary>
    /// Returns a new <see cref="Color"/> with the specified alpha channel.
    /// RGB channels are preserved.
    /// </summary>
    public static Color WithAlpha(this Color color, int alpha) =>
        Color.FromArgb(Math.Clamp(alpha, 0, 255), color);

    /// <summary>
    /// Blends <paramref name="color"/> toward <paramref name="target"/> by
    /// <paramref name="amount"/> (0 = no change, 1 = fully <paramref name="target"/>).
    /// </summary>
    public static Color Blend(this Color color, Color target, float amount)
    {
        amount = Math.Clamp(amount, 0f, 1f);
        return Color.FromArgb(
            color.A,
            (int)(color.R + (target.R - color.R) * amount),
            (int)(color.G + (target.G - color.G) * amount),
            (int)(color.B + (target.B - color.B) * amount));
    }
}