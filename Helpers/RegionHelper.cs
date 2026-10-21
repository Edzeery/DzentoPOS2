using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzentoPOS2.WinForms.Helpers
{
    public static class RegionHelper
    {
        public static GraphicsPath GetRoundPath(
            Rectangle rect,
            int radius)
        {
            GraphicsPath path = new GraphicsPath();

            int diameter = radius * 2;

            path.StartFigure();

            path.AddArc(rect.X, rect.Y,
                diameter, diameter, 180, 90);

            path.AddArc(rect.Right - diameter,
                rect.Y,
                diameter,
                diameter,
                270,
                90);

            path.AddArc(rect.Right - diameter,
                rect.Bottom - diameter,
                diameter,
                diameter,
                0,
                90);

            path.AddArc(rect.X,
                rect.Bottom - diameter,
                diameter,
                diameter,
                90,
                90);

            path.CloseFigure();

            return path;
        }
    }
}
