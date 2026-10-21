namespace DzentoPOS2.WinForms.Modules.Dashboard
{
    internal static class ColorHelper
    {
        public static Color[] GenerateHarmoniousPalette(int count, int saturation = 55, int lightness = 55)
        {
            var colors = new Color[count];
            int step = 360 / count;
            for (int i = 0; i < count; i++)
            {
                int hue = (i * step + 10) % 360;
                colors[i] = HslToRgb(hue, saturation, lightness);
            }
            return colors;
        }

        public static Color HslToRgb(int h, int s, int l)
        {
            double hue = h / 360.0;
            double sat = s / 100.0;
            double lit = l / 100.0;

            double c = (1 - Math.Abs(2 * lit - 1)) * sat;
            double x = c * (1 - Math.Abs((hue * 6) % 2 - 1));
            double m = lit - c / 2;

            double r, g, b;
            int segment = (int)(hue * 6) % 6;
            switch (segment)
            {
                case 0: r = c; g = x; b = 0; break;
                case 1: r = x; g = c; b = 0; break;
                case 2: r = 0; g = c; b = x; break;
                case 3: r = 0; g = x; b = c; break;
                case 4: r = x; g = 0; b = c; break;
                default: r = c; g = 0; b = x; break;
            }

            return Color.FromArgb(
                (int)((r + m) * 255),
                (int)((g + m) * 255),
                (int)((b + m) * 255));
        }

        public static Color Tint(Color color, float factor)
        {
            return Color.FromArgb(
                (int)(color.R + (255 - color.R) * factor),
                (int)(color.G + (255 - color.G) * factor),
                (int)(color.B + (255 - color.B) * factor));
        }
    }
}
