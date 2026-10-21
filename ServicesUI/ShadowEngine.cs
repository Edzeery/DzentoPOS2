using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzentoPOS2.WinForms.ServicesUI
{
    public static class ShadowEngine
    {
        private static readonly Dictionary<Control, ShadowCache> cache =
            new Dictionary<Control, ShadowCache>();

        public static void Enable(Control control, int elevation = 5)
        {
            if (control == null) return;

            if (cache.TryGetValue(control, out var existing))
            {
                existing.Elevation = elevation;
                existing.Bitmap = null;
                control.Invalidate();
                return;
            }

            cache[control] = new ShadowCache
            {
                Elevation = elevation
            };

            control.Paint += Control_Paint;
            control.Resize += Control_Resize;
            control.Disposed += Control_Disposed;

            control.Invalidate();
        }

        private static void Control_Disposed(object? sender, EventArgs e)
        {
            Control? c = sender as Control;

            if (c != null && cache.ContainsKey(c))
            {
                cache.Remove(c);
            }
        }

        private static void Control_Resize(object? sender, EventArgs e)
        {
            Control? c = sender as Control;

            if (c == null) return;

            if (cache.ContainsKey(c))
            {
                cache[c].Bitmap = null; // force rebuild
            }

            c.Invalidate();
        }

        private static void Control_Paint(object? sender, PaintEventArgs e)
        {
            var c = sender as Control;
            if (c == null || !cache.ContainsKey(c)) return;

            var entry = cache[c];

            if (entry.Bitmap == null ||
                entry.Bitmap.Width != c.Width ||
                entry.Bitmap.Height != c.Height)
            {
                entry.Bitmap = CreateShadow(c, entry.Elevation);
            }

            e.Graphics.DrawImage(entry.Bitmap, 0, 0);
        }

        private static Bitmap CreateShadow(Control control, int elevation)
        {
            int size = elevation * 2;

            Bitmap bmp = new Bitmap(
                control.Width + size,
                control.Height + size);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle rect = new Rectangle(
                    elevation,
                    elevation,
                    control.Width - elevation,
                    control.Height - elevation);

                for (int i = elevation; i > 0; i--)
                {
                    int alpha = 20 / i;

                    using (SolidBrush brush =
                        new SolidBrush(Color.FromArgb(alpha, 0, 0, 0)))
                    {
                        Rectangle r = new Rectangle(
                            rect.X - i,
                            rect.Y - i,
                            rect.Width + i * 2,
                            rect.Height + i * 2);

                        g.FillRectangle(brush, r);
                    }
                }
            }

            return bmp;
        }
    }

    internal class ShadowCache
    {
        public int Elevation;
        public Bitmap? Bitmap;
    }
}
