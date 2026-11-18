using System.ComponentModel;

namespace DzentoPOS2.WinForms.Controls;

/// <summary>
/// Design-time detection for the custom controls. The Visual Studio designer
/// creates controls outside a running application, so anything that needs DI,
/// the database or the file system must be skipped there. A control that is
/// only dropped on a design surface must also never start timers or
/// background work.
/// </summary>
internal static class DesignTime
{
    public static bool IsActive(Control? control = null)
    {
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            return true;

        return control is not null && IsOnDesignSurface(control);
    }

    public static bool IsRuntime(Control? control = null) => !IsActive(control);

    private static bool IsOnDesignSurface(Control control)
    {
        for (Control? current = control; current is not null; current = current.Parent)
        {
            if (current.Site?.DesignMode == true)
                return true;
        }

        return false;
    }
}
