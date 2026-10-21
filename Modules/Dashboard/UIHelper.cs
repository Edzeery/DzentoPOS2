using System.Drawing.Drawing2D;
using DzentoPOS2.WinForms.ServicesUI;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Modules.Dashboard
{
    internal static class UIHelper
    {
        public static void SetRoundedRegion(this Control ctrl, int radius)
        {
            if (ctrl.Width <= 0 || ctrl.Height <= 0) return;
            ApplyRoundedRegion(ctrl, radius);
            ctrl.Resize += (_, _) => ApplyRoundedRegion(ctrl, radius);
        }

        private static void ApplyRoundedRegion(Control ctrl, int radius)
        {
            if (ctrl.Width <= 0 || ctrl.Height <= 0) return;
            using var path = new GraphicsPath();
            var r = Math.Min(radius, Math.Min(ctrl.Width, ctrl.Height) / 2);
            path.AddArc(0, 0, r, r, 180, 90);
            path.AddArc(ctrl.Width - r, 0, r, r, 270, 90);
            path.AddArc(ctrl.Width - r, ctrl.Height - r, r, r, 0, 90);
            path.AddArc(0, ctrl.Height - r, r, r, 90, 90);
            path.CloseFigure();
            var oldRegion = ctrl.Region;
            ctrl.Region = new Region(path);
            oldRegion?.Dispose();
        }

        public static Panel WrapInCard(Control inner, string title,
            int innerTop = 40, int innerPadX = 32, int innerPadY = 56,
            int innerLeft = 0, int margin = 10, int titleLeft = 16, int titleTop = 12)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(margin),
                BackColor = AppColors.Surface,
                Padding = new Padding(16)
            };
            card.SetRoundedRegion(16);

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI Semibold", 12F),
                ForeColor = AppColors.Ink,
                Location = new Point(titleLeft, titleTop),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            inner.Location = new Point(innerLeft, innerTop);
            card.Resize += (_, _) =>
            {
                var w = Math.Max(0, card.Width - innerPadX);
                var h = Math.Max(1, card.Height - innerPadY);
                inner.Size = new Size(w, h);
            };

            ShadowEngine.Enable(card, 3);

            // Only the elevation reacts to hover. Moving the card would break
            // the surrounding TableLayoutPanel, so the position is left alone.
            card.MouseEnter += (_, _) => ShadowEngine.Enable(card, 6);
            card.MouseLeave += (_, _) => ShadowEngine.Enable(card, 3);

            card.Controls.Add(lblTitle);
            card.Controls.Add(inner);

            void ApplyCardTheme()
            {
                if (card.IsDisposed) return;
                if (card.InvokeRequired)
                {
                    card.BeginInvoke(new Action(ApplyCardTheme));
                    return;
                }
                card.BackColor = AppColors.Surface;
                lblTitle.ForeColor = AppColors.Ink;
                card.Invalidate(true);
            }

            ThemeManager.ThemeChanged += OnThemeChanged;
            card.Disposed += (_, _) => ThemeManager.ThemeChanged -= OnThemeChanged;

            void OnThemeChanged(object? sender, EventArgs e) => ApplyCardTheme();

            return card;
        }
    }
}
