using System.Drawing;

namespace DzentoPOS2.WinForms.Themes
{
    /// <summary>
    /// Single source of truth for semantic colors across the application.
    /// Colors are resolved dynamically from ThemeManager so light/dark modes
    /// stay consistent across every surface, card, field and control.
    /// </summary>
    public static class AppColors
    {
        // =========================
        // Brand / accent
        // =========================

        public static Color Primary => ThemeManager.IsDark ? ThemeManager.PrimaryColor : Color.FromArgb(0, 122, 116);
        public static Color PrimaryHover => ThemeManager.IsDark ? ThemeManager.ChangeBrightness(ThemeManager.PrimaryColor, -0.12) : Color.FromArgb(0, 104, 99);
        public static Color PrimaryPressed => ThemeManager.IsDark ? ThemeManager.ChangeBrightness(ThemeManager.PrimaryColor, -0.24) : Color.FromArgb(0, 86, 82);
        public static Color PrimarySoft => ThemeManager.IsDark ? ThemeManager.ChangeBrightness(ThemeManager.PrimaryColor, -0.72) : Color.FromArgb(229, 246, 245);

        // =========================
        // Status
        // =========================

        public static Color Success => Color.FromArgb(52, 168, 83);
        public static Color SuccessHover => Color.FromArgb(44, 145, 71);

        public static Color Warning => Color.FromArgb(240, 173, 41);
        public static Color WarningHover => Color.FromArgb(214, 152, 30);

        public static Color Danger => Color.FromArgb(220, 53, 69);
        public static Color DangerHover => Color.FromArgb(193, 44, 58);

        public static Color Info => ThemeManager.IsDark ? Color.FromArgb(90, 170, 255) : Color.FromArgb(0, 123, 255);
        public static Color InfoHover => ThemeManager.IsDark ? Color.FromArgb(70, 150, 235) : Color.FromArgb(0, 105, 217);

        // =========================
        // Neutrals
        // =========================

        public static Color Ink => ThemeManager.IsDark ? Color.FromArgb(230, 234, 240) : Color.FromArgb(29, 29, 31);
        public static Color InkMuted => ThemeManager.IsDark ? Color.FromArgb(160, 170, 188) : Color.FromArgb(110, 110, 115);
        public static Color OnDark => ThemeManager.IsDark ? Color.FromArgb(236, 240, 246) : Color.White;
        public static Color Neutral => ThemeManager.IsDark ? Color.FromArgb(150, 160, 178) : Color.FromArgb(108, 117, 125);
        public static Color NeutralHover => ThemeManager.IsDark ? Color.FromArgb(170, 180, 196) : Color.FromArgb(92, 100, 107);

        // =========================
        // Surfaces
        // =========================

        public static Color Background => ThemeManager.IsDark ? Color.FromArgb(9, 14, 22) : Color.FromArgb(245, 247, 250);
        public static Color Surface => ThemeManager.IsDark ? Color.FromArgb(17, 24, 39) : Color.White;
        public static Color SurfaceAlt => ThemeManager.IsDark ? Color.FromArgb(21, 30, 48) : Color.FromArgb(249, 250, 252);
        public static Color Border => ThemeManager.IsDark ? Color.FromArgb(39, 49, 69) : Color.FromArgb(224, 228, 234);

        // =========================
        // Inputs
        // =========================

        public static Color InputBackground => ThemeManager.IsDark ? Color.FromArgb(24, 33, 52) : Color.White;
        public static Color InputBorder => ThemeManager.IsDark ? Color.FromArgb(48, 59, 82) : Border;
        public static Color InputBorderFocus => Primary;

        // =========================
        // Shell (sidebar / topbar)
        // =========================

        public static Color SidebarSurface => ThemeManager.IsDark ? Color.FromArgb(9, 14, 22) : Color.FromArgb(0, 39, 49);
        public static Color SidebarHover => ThemeManager.IsDark ? Color.FromArgb(21, 30, 48) : Color.FromArgb(1, 56, 55);
        public static Color SidebarPressed => ThemeManager.IsDark ? Color.FromArgb(30, 42, 64) : Color.FromArgb(14, 94, 93);
        public static Color SidebarAccentSoft => ThemeManager.IsDark ? ThemeManager.ChangeBrightness(Primary, -0.72) : Color.FromArgb(229, 246, 245);
        public static Color SidebarMuted => ThemeManager.IsDark ? Color.FromArgb(163, 173, 189) : Color.FromArgb(186, 213, 213);

        // =========================
        // Grid / list
        // =========================

        public static Color GridHeaderBackground => ThemeManager.IsDark ? Color.FromArgb(26, 36, 56) : Color.FromArgb(245, 247, 250);
        public static Color GridHeaderForeground => Ink;
        public static Color GridRowAlt => ThemeManager.IsDark ? Color.FromArgb(22, 31, 49) : Color.FromArgb(249, 250, 252);
        public static Color GridHover => ThemeManager.IsDark ? Color.FromArgb(34, 46, 70) : Color.FromArgb(229, 246, 245);
        public static Color GridSelectionBackground => ThemeManager.IsDark ? ThemeManager.ChangeBrightness(Primary, -0.55) : Color.FromArgb(229, 246, 245);
        public static Color GridSelectionForeground => Ink;
        public static Color GridLine => ThemeManager.IsDark ? Color.FromArgb(44, 56, 78) : Color.FromArgb(240, 243, 245);

        // =========================
        // Disabled
        // =========================

        public static Color DisabledBack => ThemeManager.IsDark ? Color.FromArgb(33, 44, 65) : Color.FromArgb(233, 236, 239);
        public static Color DisabledFore => ThemeManager.IsDark ? Color.FromArgb(118, 128, 148) : Color.FromArgb(160, 166, 172);

        // =========================
        // KPI card accents
        // =========================

        public static readonly Color KpiTeal = Color.FromArgb(0, 122, 116);
        public static readonly Color KpiBlue = Color.FromArgb(0, 113, 227);
        public static readonly Color KpiIndigo = Color.FromArgb(88, 86, 214);
        public static readonly Color KpiGreen = Color.FromArgb(52, 168, 83);
        public static readonly Color KpiAmber = Color.FromArgb(240, 173, 41);
        public static readonly Color KpiOrange = Color.FromArgb(240, 124, 32);
        public static readonly Color KpiRed = Color.FromArgb(220, 53, 69);
        public static readonly Color KpiPink = Color.FromArgb(214, 51, 132);
        public static readonly Color KpiPurple = Color.FromArgb(155, 81, 224);
        public static readonly Color KpiCyan = Color.FromArgb(0, 158, 178);

        /// <summary>
        /// Curated, brand-aligned accent sequence used for KPI cards so both the
        /// Dashboard and Reports present an identical, restrained color system.
        /// In dark mode the accents are lifted so they stay vivid on dark cards.
        /// </summary>
        public static Color[] KpiPalette => ThemeManager.IsDark ? KpiPaletteDark : KpiPaletteLight;

        private static readonly Color[] KpiPaletteLight =
        {
            KpiTeal, KpiBlue, KpiIndigo, KpiGreen,
            KpiAmber, KpiOrange, KpiPurple, KpiCyan,
            KpiPink, KpiRed, KpiIndigo, KpiGreen
        };

        private static readonly Color[] KpiPaletteDark = BuildDarkPalette();

        private static Color[] BuildDarkPalette()
        {
            var palette = new Color[KpiPaletteLight.Length];
            for (int i = 0; i < KpiPaletteLight.Length; i++)
                palette[i] = AdaptAccent(KpiPaletteLight[i]);
            return palette;
        }

        /// <summary>
        /// Lifts an accent color so it keeps its contrast on dark surfaces.
        /// </summary>
        public static Color AdaptAccent(Color color)
        {
            if (!ThemeManager.IsDark) return color;

            return Color.FromArgb(
                color.A,
                (byte)Math.Min(255, color.R + (255 - color.R) * 0.22f),
                (byte)Math.Min(255, color.G + (255 - color.G) * 0.22f),
                (byte)Math.Min(255, color.B + (255 - color.B) * 0.22f));
        }

        public static Color Hover(Color baseColor)
            => ThemeManager.ChangeBrightness(baseColor, ThemeManager.IsDark ? -0.08 : -0.12);

        public static Color Pressed(Color baseColor)
            => ThemeManager.ChangeBrightness(baseColor, ThemeManager.IsDark ? -0.16 : -0.24);

        public static Color Tint(Color color, float factor)
            => Color.FromArgb(
                (int)(color.R + (255 - color.R) * factor),
                (int)(color.G + (255 - color.G) * factor),
                (int)(color.B + (255 - color.B) * factor));
    }
}
