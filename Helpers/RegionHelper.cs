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

            // The radius is clamped to half of the smallest side: a zero sized
            // arc is rejected by GDI+ with "Parameter is not valid.".
            int r = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2);
            if (r <= 0)
            {
                path.StartFigure();
                path.AddRectangle(rect);
                path.CloseFigure();
                return path;
            }

            int diameter = r * 2;

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
