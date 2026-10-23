namespace EZControls.Core.Drawing;


// ══════════════════════════════════════════════════════════════════════════════
//  TextRendererHelper
//
//  Thin wrappers over TextRenderer to keep the measurement API discoverable
//  from a single namespace.
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Utility methods for measuring text using GDI <see cref="TextRenderer"/>.
/// Prefer these over <see cref="Graphics.MeasureString"/> in WinForms;
/// <c>TextRenderer</c> produces values that match actual GDI rendering.
/// </summary>
public static class TextRendererHelper
{
    /// <summary>Measures the pixel size of <paramref name="text"/> rendered with <paramref name="font"/>.</summary>
    public static Size MeasureText(string text, Font font) =>
        TextRenderer.MeasureText(text, font);

    /// <summary>
    /// Returns the pixel height of a single line of text rendered with
    /// <paramref name="font"/>.  Uses a fixed probe string to ensure
    /// consistent results regardless of the actual content.
    /// </summary>
    public static int MeasureLineHeight(Font font) =>
        TextRenderer.MeasureText("Wy", font).Height;
}