namespace EZControls.Core.Drawing;

// ══════════════════════════════════════════════════════════════════════════════
//  LayoutMetrics
//
//  Issue #4 – negative width/height when the control is very small:
//
//    GDI+ throws ArgumentException ("Parameter is not valid") when a Rectangle
//    passed to AddEllipse, DrawPath, etc. has Width <= 0 or Height <= 0.
//    This is especially visible in the designer when the user drags a control
//    to a tiny size, which crashes the designer surface.
//
//    SafeRect() clamps Width and Height to a minimum of 1.  Callers still check
//    IsUsable before painting, so a 1×1 rect does not cause incorrect rendering –
//    it just prevents the GDI+ exception.
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Calculates the three layout rectangles used by <see cref="Controls.Media.EZCircularPictureBox"/>:
/// <list type="bullet">
///   <item><see cref="Shadow"/> – the bounding box for the shadow gradient halo.</item>
///   <item><see cref="Border"/> – the circle border ring.</item>
///   <item><see cref="Image"/>  – the inner disc that clips the image.</item>
/// </list>
/// All rectangles are guaranteed to have positive dimensions (minimum 1×1)
/// to prevent GDI+ exceptions on very small controls.
/// </summary>
internal readonly struct LayoutMetrics
{
    /// <summary>Full bounding box for the shadow halo.</summary>
    public Rectangle Shadow { get; }

    /// <summary>Bounding box for the border ring.</summary>
    public Rectangle Border { get; }

    /// <summary>Inner image disc, inset from the border by its pen width.</summary>
    public Rectangle Image { get; }

    /// <summary>
    /// <see langword="true"/> when all three rectangles have genuinely useful
    /// positive dimensions and are worth painting.
    /// A rect with Width=1 or Height=1 satisfies GDI+ but is visually meaningless.
    /// </summary>
    public bool IsUsable =>
        Shadow.Width > 4 && Shadow.Height > 4 &&
        Border.Width > 2 && Border.Height > 2 &&
        Image.Width > 0 && Image.Height > 0;

    /// <param name="controlWidth">Client width of the owning control.</param>
    /// <param name="controlHeight">Client height of the owning control.</param>
    /// <param name="shadowSize">Current shadow spread in logical pixels.</param>
    /// <param name="currentBorderSize">Current (animated) border width.</param>
    public LayoutMetrics(
        int controlWidth,
        int controlHeight,
        int shadowSize,
        float currentBorderSize)
    {
        int s = shadowSize;

        Shadow = SafeRect(
            s / 2, s / 2,
            controlWidth - s,
            controlHeight - s);

        //Border = SafeRect(
        //    s, s,
        //    controlWidth - s * 2 - 1,
        //    controlHeight - s * 2 - 1);
        Border = SafeRect(
                s,
                s,
                controlWidth - s * 2,
                controlHeight - s * 2);

        int imageMargin = Math.Max(
    2,
    s + (int)Math.Ceiling(currentBorderSize));

        int imageSize = Math.Max(
            10,
            Math.Min(
                controlWidth - imageMargin * 2,
                controlHeight - imageMargin * 2));

        Image = new Rectangle(
            imageMargin,
            imageMargin,
            imageSize,
            imageSize);
    }

    private static Rectangle SafeRect(int x, int y, int w, int h) =>
        new(x, y, Math.Max(1, w), Math.Max(1, h));
}