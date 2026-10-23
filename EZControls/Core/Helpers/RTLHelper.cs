namespace EZControls.Core.Helpers;

public static class RTLHelper
{
    public static void ApplyRTL(Control control)
    {
        control.RightToLeft = RightToLeft.Yes;

        if (control is UserControl uc)
        {
            foreach (Control child in uc.Controls)
            {
                child.RightToLeft = RightToLeft.Yes;
            }
        }
    }

    public static void ApplyLTR(Control control)
    {
        control.RightToLeft = RightToLeft.No;

        if (control is UserControl uc)
        {
            foreach (Control child in uc.Controls)
            {
                child.RightToLeft = RightToLeft.No;
            }
        }
    }
}