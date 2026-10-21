using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzentoPOS2.WinForms.Themes
{
    public static class AppStyle
    {
        // =========================
        // Radius
        // =========================

        public const int RadiusSmall = 8;
        public const int RadiusMedium = 12;
        public const int RadiusLarge = 18;

        // =========================
        // Spacing
        // =========================

        public const int SpaceXS = 4;
        public const int SpaceSM = 8;
        public const int SpaceMD = 12;
        public const int SpaceLG = 16;
        public const int SpaceXL = 24;

        // =========================
        // Sidebar
        // =========================

        public const int SidebarExpandedWidth = 220;
        public const int SidebarCollapsedWidth = 100;

        // =========================
        // TitleBar
        // =========================

        public const int TitleBarHeight = 50;

        // =========================
        // Card
        // =========================

        public static Padding CardPadding
            = new Padding(16);

        // =========================
        // Fonts
        // =========================

        public static readonly Font TitleFont
            = new Font("Segoe UI Semibold", 16F);

        public static readonly Font SubtitleFont
            = new Font("Segoe UI Semibold", 12F);

        public static readonly Font BodyFont
            = new Font("Segoe UI", 10F);

        public static readonly Font ButtonFont
            = new Font("Segoe UI Semibold", 10F);

        // =========================
        // Animation
        // =========================

        public const int AnimationInterval = 10;
        public const int AnimationStep = 20;

        // =========================
        // Shadow
        // =========================

        public static readonly Color ShadowColor
            = Color.FromArgb(40, 0, 0, 0);

        // =========================
        // Border
        // =========================

        public const int BorderSize = 1;
    }
}
