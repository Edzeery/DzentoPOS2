namespace EZControls.Core.Helpers;

public static class TextBoxHelper
{
    public static void UpdateControlHeight(
        TextBox textBox,
        UserControl owner)
    {
        if (textBox.Multiline)
            return;

        int textHeight =
            TextRenderer.MeasureText(
                "Text",
                owner.Font).Height + 1;

        textBox.Multiline = true;

        textBox.MinimumSize =
            new Size(0, textHeight);

        textBox.Multiline = false;

        owner.Height =
            textBox.Height +
            owner.Padding.Top +
            owner.Padding.Bottom;
    }
}