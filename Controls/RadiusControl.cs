using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzentoPOS2.WinForms.Controls
{
    public static class RadiusControl
    {
        public static void Bind(Control control, int radius)
        {
            control.Resize += (s, e) =>
            {
                Apply(control, radius);
            };

            Apply(control, radius);
        }

        public static void Apply(Control control, int radius)
        {
            if (control.Width == 0 || control.Height == 0)
                return;

            GraphicsPath path = new GraphicsPath();

            int d = radius * 2;

            Rectangle r = new Rectangle(
                0,
                0,
                control.Width,
                control.Height);

            path.StartFigure();

            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);

            path.CloseFigure();

            control.Region = new Region(path);
        }
    }
}
