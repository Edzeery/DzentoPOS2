using EZControls.Controls.Inputs;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace EZControls.Design.SmartTags;

public class EZTextBoxSmartTagActionList : DesignerActionList
{
    private readonly EZTextBox textBox;

    public EZTextBoxSmartTagActionList(
        IComponent component)
        : base(component)
    {
        textBox = (EZTextBox)component;
    }

    public Color BorderColor
    {
        get => textBox.BorderColor;
        set => TypeDescriptor
            .GetProperties(textBox)
            ["BorderColor"]
            ?.SetValue(textBox, value);
    }

    public int BorderRadius
    {
        get => textBox.BorderRadius;
        set => TypeDescriptor
            .GetProperties(textBox)
            ["BorderRadius"]
            ?.SetValue(textBox, value);
    }

    public string PlaceholderText
    {
        get => textBox.PlaceholderText;
        set => TypeDescriptor
            .GetProperties(textBox)
            ["PlaceholderText"]
            ?.SetValue(textBox, value);
    }

    public override DesignerActionItemCollection
        GetSortedActionItems()
    {
        DesignerActionItemCollection items = new();

        items.Add(
            new DesignerActionHeaderItem(
                "Appearance"));

        items.Add(
            new DesignerActionPropertyItem(
                nameof(BorderColor),
                "Border Color"));

        items.Add(
            new DesignerActionPropertyItem(
                nameof(BorderRadius),
                "Border Radius"));

        items.Add(
            new DesignerActionPropertyItem(
                nameof(PlaceholderText),
                "Placeholder"));

        return items;
    }
}