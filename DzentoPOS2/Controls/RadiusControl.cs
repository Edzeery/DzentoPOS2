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
            if (control.IsDisposed || control.Width <= 0 || control.Height <= 0)
                return;

            // The radius never exceeds half of the smallest side, otherwise the
            // arcs would have a zero size and GDI+ would reject the path.
            int r = Math.Min(radius, Math.Min(control.Width, control.Height) / 2);
            if (r <= 0)
            {
                ClearRegion(control);
                return;
            }

            int d = r * 2;

            GraphicsPath path = new GraphicsPath();

            Rectangle bounds = new Rectangle(
                0,
                0,
                control.Width,
                control.Height);

            path.StartFigure();

            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);

            path.CloseFigure();

            var oldRegion = control.Region;
            control.Region = new Region(path);
            oldRegion?.Dispose();
        }

        private static void ClearRegion(Control control)
        {
            if (control.IsDisposed) return;
            var oldRegion = control.Region;
            control.Region = null;
            oldRegion?.Dispose();
        }
    }
}
