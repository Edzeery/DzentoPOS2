using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzentoPOS2.WinForms.Controls
{

    // ══════════════════════════════════════════════════════════════════════════════
    //  LayoutMetrics
    //
    //  Issue #4 addressed here: negative width/height when the control is tiny.
    //
    //  GDI+ throws ArgumentException ("Parameter is not valid") if you pass a
    //  Rectangle with Width <= 0 or Height <= 0 to AddEllipse, DrawPath, etc.
    //  That crash is especially embarrassing in the designer when a user drags a
    //  control to a very small size.
    //
    //  All three rectangle-producing methods clamp their output so Width and
    //  Height are always >= 1.  Callers still need to check IsValid() before
    //  using the result for actual drawing – a 1×1 rect is valid for GDI+ but
    //  may not be meaningful for the control.
    // ══════════════════════════════════════════════════════════════════════════════
    internal readonly struct LayoutMetrics
    {
        public Rectangle Shadow { get; }
        public Rectangle Border { get; }
        public Rectangle Image { get; }

        /// <summary>
        /// True when all three rectangles have positive dimensions and are
        /// worth painting.
        /// </summary>
        public bool IsUsable =>
            Shadow.Width > 0 && Shadow.Height > 0 &&
            Border.Width > 0 && Border.Height > 0 &&
            Image.Width > 0 && Image.Height > 0;

        public LayoutMetrics(int controlWidth, int controlHeight,
                             int shadowSize, float currentBorderSize)
        {
            Shadow = SafeRect(
                shadowSize / 2,
                shadowSize / 2,
                controlWidth - shadowSize,
                controlHeight - shadowSize);

            Border = SafeRect(
                shadowSize,
                shadowSize,
                controlWidth - shadowSize * 2 - 1,
                controlHeight - shadowSize * 2 - 1);

            int imageMargin = shadowSize + (int)currentBorderSize + 2;
            Image = SafeRect(
                imageMargin,
                imageMargin,
                controlWidth - imageMargin * 2,
                controlHeight - imageMargin * 2);
        }

        private static Rectangle SafeRect(int x, int y, int w, int h) =>
            new(x, y, Math.Max(1, w), Math.Max(1, h));
    }
}
