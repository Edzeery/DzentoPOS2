using DzentoPOS2.WinForms.Controls;
using FontAwesome.Sharp;

namespace DzentoPOS2.WinForms.Themes
{
    /// <summary>
    /// Centralized factory for application buttons. Guarantees consistent
    /// sizing, radius, typography, colors and hover/pressed behavior so every
    /// screen reads as one product.
    /// </summary>
    public static class ButtonFactory
    {
        public enum ButtonSize
        {
            Small,
            Medium,
            Large
        }

        public static Size GetSize(ButtonSize size) => size switch
        {
            ButtonSize.Small => new Size(96, 32),
            ButtonSize.Large => new Size(160, 44),
            _ => new Size(120, 38)
        };

        // =========================
        // Solid buttons (primary / status)
        // =========================

        public static ThemedButton Primary(string text, ButtonSize size = ButtonSize.Medium)
            => Solid(text, ButtonTone.Primary, size);

        public static ThemedButton Success(string text, ButtonSize size = ButtonSize.Medium)
            => Solid(text, ButtonTone.Success, size);

        public static ThemedButton Danger(string text, ButtonSize size = ButtonSize.Medium)
            => Solid(text, ButtonTone.Danger, size);

        public static ThemedButton Neutral(string text, ButtonSize size = ButtonSize.Medium)
            => Solid(text, ButtonTone.Neutral, size);

        private static ThemedButton Solid(string text, ButtonTone tone, ButtonSize size)
        {
            var button = new ThemedButton
            {
                Text = text,
                Size = GetSize(size),
                Tone = tone
            };

            button.ApplyTheme();

            RadiusControl.Bind(button, AppStyle.RadiusSmall);
            return button;
        }

        // =========================
        // Outlined toolbar buttons (icon + label)
        // =========================

        public static ThemedIconButton Tool(IconChar icon, string text, Color accent, ButtonSize size = ButtonSize.Small)
        {
            var button = new ThemedIconButton
            {
                IconChar = icon,
                IconSize = 18,
                Text = $"  {text}",
                Size = GetSize(size)
            };

            button.ApplyTheme();
            return button;
        }

        public static ThemedIconButton ToolAdd(string text, ButtonSize size = ButtonSize.Small)
            => Tool(IconChar.Plus, text, AppColors.Primary, size);

        public static ThemedIconButton ToolEdit(string text, ButtonSize size = ButtonSize.Small)
            => Tool(IconChar.Edit, text, AppColors.Info, size);

        public static ThemedIconButton ToolDelete(string text, ButtonSize size = ButtonSize.Small)
            => Tool(IconChar.Trash, text, AppColors.Danger, size);
    }
}
