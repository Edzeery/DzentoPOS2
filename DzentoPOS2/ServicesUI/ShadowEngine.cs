using System.Drawing.Drawing2D;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.ServicesUI
{
    /// <summary>
    /// Draws a real soft drop shadow for a control on its <b>parent</b>, so the
    /// control itself is never covered by an opaque gray block. The shadow is a
    /// blurred rounded rectangle rendered once into a cached bitmap (no per
    /// frame allocations) and is theme-aware: subtle in light mode, deeper in
    /// dark mode. The public surface (<see cref="Enable"/>) is unchanged.
    /// </summary>
    public static class ShadowEngine
    {
        private static readonly Dictionary<Control, ShadowEntry> Entries = new();
        private static readonly HashSet<Control> HookedParents = new();

        static ShadowEngine()
        {
            ThemeManager.ThemeChanged += (_, _) =>
            {
                foreach (var entry in Entries.Values.ToList())
                {
                    entry.Bitmap = null;
                    entry.Invalidate();
                }
            };
        }

        public static void Enable(Control control, int elevation = 5)
        {
            if (control == null) return;

            if (Entries.TryGetValue(control, out var existing))
            {
                if (existing.Elevation == elevation)
                    return;

                existing.Elevation = elevation;
                existing.Bitmap = null;
                existing.Invalidate();
                return;
            }

            var entry = new ShadowEntry(control, elevation);
            Entries[control] = entry;
            entry.Attach();
            entry.Invalidate();
        }

        internal static void Remove(Control control)
        {
            if (Entries.Remove(control, out var entry))
                entry.Detach();
        }

        internal static bool TryGet(Control control, out ShadowEntry entry)
            => Entries.TryGetValue(control, out entry!);

        internal static IEnumerable<ShadowEntry> ForParent(Control parent)
            => Entries.Values.Where(e => e.Control.Parent == parent && e.Control.Visible);

        private static void EnsureParentHooked(Control? parent)
        {
            if (parent == null || !HookedParents.Add(parent))
                return;

            parent.Paint += ParentPaint;
            parent.Disposed += (_, _) => HookedParents.Remove(parent);
        }

        private static void ParentPaint(object? sender, PaintEventArgs e)
        {
            if (sender is not Control parent)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            foreach (var entry in ForParent(parent))
                entry.Draw(e.Graphics);
        }

        internal sealed class ShadowEntry
        {
            private const int Radius = 16;

            private readonly Control _control;

            public int Elevation { get; set; }

            public Bitmap? Bitmap { get; set; }

            public Control Control => _control;

            public ShadowEntry(Control control, int elevation)
            {
                _control = control;
                Elevation = elevation;
            }

            public void Attach()
            {
                _control.Disposed += OnControlDisposed;
                _control.LocationChanged += OnControlMoved;
                _control.SizeChanged += OnControlMoved;
                _control.VisibleChanged += OnControlMoved;
                _control.ParentChanged += OnParentChanged;
                _control.HandleDestroyed += OnControlMoved;
                EnsureParentHooked(_control.Parent);
            }

            public void Detach()
            {
                _control.Disposed -= OnControlDisposed;
                _control.LocationChanged -= OnControlMoved;
                _control.SizeChanged -= OnControlMoved;
                _control.VisibleChanged -= OnControlMoved;
                _control.ParentChanged -= OnParentChanged;
                _control.HandleDestroyed -= OnControlMoved;
            }

            private void OnControlDisposed(object? sender, EventArgs e) => Remove(_control);

            private void OnParentChanged(object? sender, EventArgs e)
            {
                Bitmap = null;
                EnsureParentHooked(_control.Parent);
                Invalidate();
            }

            private void OnControlMoved(object? sender, EventArgs e) => Invalidate();

            private int ScaledElevation()
            {
                var dpi = _control.DeviceDpi <= 0 ? 96 : _control.DeviceDpi;
                return Math.Max(2, (int)Math.Round(Elevation * dpi / 96.0));
            }

            public void Invalidate()
            {
                var parent = _control.Parent;
                if (parent == null || parent.IsDisposed)
                    return;

                var elev = ScaledElevation();
                var b = _control.Bounds;
                var ring = new Rectangle(
                    b.X - elev,
                    b.Y - elev,
                    b.Width + elev * 2,
                    b.Height + elev * 2);

                parent.Invalidate(ring, false);
            }

            public void Draw(Graphics graphics)
            {
                var elev = ScaledElevation();
                var b = _control.Bounds;

                if (Bitmap == null || Bitmap.Width != b.Width + elev * 2 || Bitmap.Height != b.Height + elev * 2)
                    Bitmap = Render(b.Width, b.Height, elev);

                // Offset the shadow slightly down so it reads as light from above.
                var offset = Math.Max(1, elev / 2);
                graphics.DrawImage(Bitmap, b.X - elev, b.Y - elev + offset);
            }

            private static Bitmap Render(int width, int height, int elevation)
            {
                var w = Math.Max(1, width + elevation * 2);
                var h = Math.Max(1, height + elevation * 2);

                var bitmap = new Bitmap(w, h);
                using var g = Graphics.FromImage(bitmap);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                var baseAlpha = ThemeManager.IsDark ? 90 : 45;

                // Layered rounded rectangles: the outer ones are fainter, which
                // produces a soft blur without a gray fill.
                for (int i = elevation; i >= 1; i--)
                {
                    var alpha = (int)(baseAlpha * (1f - (float)i / (elevation + 1)) * 0.35f) + 2;
                    if (alpha <= 0)
                        continue;

                    alpha = Math.Min(alpha, 255);

                    var inset = elevation - i;
                    var rect = new Rectangle(
                        inset,
                        inset,
                        w - inset * 2 - 1,
                        h - inset * 2 - 1);

                    using var path = RoundedRect(rect, Radius);
                    using var brush = new SolidBrush(Color.FromArgb(alpha, 0, 0, 0));
                    g.FillPath(brush, path);
                }

                return bitmap;
            }

            private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
            {
                var diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
                var path = new GraphicsPath();

                if (diameter <= 0)
                {
                    path.AddRectangle(bounds);
                    return path;
                }

                path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
                path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
                path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
                path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
                path.CloseFigure();
                return path;
            }
        }
    }
}