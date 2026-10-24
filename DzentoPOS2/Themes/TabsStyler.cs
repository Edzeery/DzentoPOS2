using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace DzentoPOS2.WinForms.Themes
{
    /// <summary>
    /// WinForms paints the tab strip with the Windows visual style and hard-codes
    /// <see cref="TabControl.BackColor"/> to the system control color, so tabs stay
    /// light in dark mode. Owner drawing is the only way to keep them in sync with
    /// the palette; colors are resolved from <see cref="AppColors"/> at paint time,
    /// which also makes runtime theme switches work without re-attaching handlers.
    /// </summary>
    public static class TabsStyler
    {
        private static readonly ConditionalWeakTable<TabControl, object> Styled = new();

        public static void Apply(TabControl tab)
        {
            if (tab == null || tab.IsDisposed) return;

            if (tab.DrawMode != TabDrawMode.OwnerDrawFixed)
                tab.DrawMode = TabDrawMode.OwnerDrawFixed;

            if (Styled.TryGetValue(tab, out _))
            {
                tab.Invalidate();
                return;
            }

            Styled.Add(tab, new object());
            tab.DrawItem += OnDrawItem;
            tab.Paint += OnPaint;
            tab.Invalidate();
        }

        /// <summary>
        /// The strip around the tabs is painted by the visual style renderer and
        /// shows through with a light color; repaint the parts that no tab covers.
        /// </summary>
        private static void OnPaint(object? sender, PaintEventArgs e)
        {
            if (sender is not TabControl tab) return;

            using var brush = new SolidBrush(AppColors.Background);
            int h = tab.ItemSize.Height + 14;
            if (tab.DisplayRectangle.Top > 0) h = tab.DisplayRectangle.Top;
            e.Graphics.FillRectangle(brush, new Rectangle(0, 0, tab.Width, Math.Max(h + 2, 1)));
        }

        private static void OnDrawItem(object? sender, DrawItemEventArgs e)
        {
            if (sender is not TabControl tab || e.Index < 0 || e.Index >= tab.TabCount) return;

            var g = e.Graphics;
            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            var bounds = e.Bounds;
            using (var back = new SolidBrush(selected ? AppColors.Surface : AppColors.Background))
                g.FillRectangle(back, bounds);

            if (selected)
            {
                using var accent = new SolidBrush(AppColors.Primary);
                g.FillRectangle(accent, bounds.Left + 4, Math.Max(bounds.Top, bounds.Bottom - 3), bounds.Width - 8, 3);
            }
            else
            {
                using var line = new Pen(AppColors.Border);
                g.DrawLine(line, bounds.Left, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
            }

            var color = selected ? AppColors.Ink : AppColors.InkMuted;
            var font = e.Font ?? tab.Font;
            var fontDraw = selected ? new Font(font, FontStyle.Regular) : font;
            const TextFormatFlags flags =
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;
            var textBounds = new Rectangle(bounds.X + 8, bounds.Y, Math.Max(0, bounds.Width - 16), bounds.Height);
            TextRenderer.DrawText(g, tab.TabPages[e.Index].Text, fontDraw, textBounds, color, flags);
            if (selected) fontDraw.Dispose();
        }

        private static int StripHeight(TabControl tab)
        {
            int height = tab.DisplayRectangle.Top;
            return height > 0 ? height : tab.ItemSize.Height + 4;
        }
    }
}

