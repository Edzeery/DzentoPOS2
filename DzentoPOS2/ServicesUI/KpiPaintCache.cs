using System.Drawing;

namespace DzentoPOS2.WinForms.ServicesUI;

/// <summary>
/// Caches the GDI objects a KPI card paints with so no <see cref="Font"/>,
/// <see cref="SolidBrush"/> or <see cref="Pen"/> is created inside a paint
/// pass. Keys include the DPI-scaled pixel size, so a monitor change rebuilds
/// the entries once; <see cref="Reset"/> releases everything.
/// </summary>
internal sealed class KpiPaintCache : IDisposable
{
    private readonly Dictionary<FontKey, Font> _fonts = new();
    private readonly Dictionary<uint, SolidBrush> _brushes = new();
    private readonly Dictionary<PenKey, Pen> _pens = new();

    private int _dpi = 96;

    public int Dpi => _dpi;

    /// <summary>
    /// Updates the cached DPI. Returns true when the DPI changed and the
    /// caller must re-measure the layout.
    /// </summary>
    public bool EnsureDpi(int dpi)
    {
        if (dpi <= 0) dpi = 96;
        if (dpi == _dpi) return false;

        _dpi = dpi;
        DisposeFonts();
        DisposePens();
        return true;
    }

    /// <summary>Releases every cached GDI object (theme / DPI / close).</summary>
    public void Reset()
    {
        DisposeFonts();
        DisposeBrushes();
        DisposePens();
        _dpi = 96;
    }

    public void ClearBrushes() => DisposeBrushes();

    /// <summary>Font described in points, converted to device pixels for the current DPI.</summary>
    public Font GetTextFont(string family, float points, FontStyle style)
    {
        var pixels = points * _dpi / 72f;
        return GetFont(family, pixels, style);
    }

    /// <summary>Font described directly in device pixels (used by the glyph badge).</summary>
    public Font GetPixelFont(FontFamily family, float pixels, FontStyle style)
    {
        if (pixels <= 0) pixels = 1;
        return GetFont(family.Name, pixels, style, family);
    }

    public SolidBrush GetBrush(Color color)
    {
        var key = unchecked((uint)color.ToArgb());
        if (!_brushes.TryGetValue(key, out var brush))
        {
            brush = new SolidBrush(color);
            _brushes[key] = brush;
        }

        return brush;
    }

    public Pen GetPen(Color color, float width)
    {
        if (width <= 0) width = 1;
        var key = new PenKey(unchecked((uint)color.ToArgb()), width);
        if (!_pens.TryGetValue(key, out var pen))
        {
            pen = new Pen(color, width);
            _pens[key] = pen;
        }

        return pen;
    }

    private Font GetFont(string family, float pixels, FontStyle style, FontFamily? fontFamily = null)
    {
        if (pixels <= 0) pixels = 1;
        var key = new FontKey(family, pixels, style);
        if (_fonts.TryGetValue(key, out var font))
            return font;

        font = fontFamily == null
            ? new Font(family, pixels, style, GraphicsUnit.Pixel)
            : new Font(fontFamily, pixels, style, GraphicsUnit.Pixel);
        _fonts[key] = font;
        return font;
    }

    private void DisposeFonts()
    {
        foreach (var font in _fonts.Values)
            font.Dispose();
        _fonts.Clear();
    }

    private void DisposeBrushes()
    {
        foreach (var brush in _brushes.Values)
            brush.Dispose();
        _brushes.Clear();
    }

    private void DisposePens()
    {
        foreach (var pen in _pens.Values)
            pen.Dispose();
        _pens.Clear();
    }

    public void Dispose() => Reset();

    private readonly record struct FontKey(string Family, float Pixels, FontStyle Style);

    private readonly record struct PenKey(uint Argb, float Width);
}