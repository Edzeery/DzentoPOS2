using System.Windows.Forms.Design;

namespace EZControls.Design.Designers;

public class EZControlDesigner : ParentControlDesigner
{
    public override SelectionRules SelectionRules
    {
        get
        {
            return SelectionRules.AllSizeable;
        }
    }
}