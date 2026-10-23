  namespace EZControls.Core.Helpers;


// ══════════════════════════════════════════════════════════════════════════════
//  NativeMethods
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Minimal P/Invoke declarations for WinForms drawing optimisation.
/// </summary>
internal static class NativeMethods
{
    private const int WM_SETREDRAW = 11;

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, bool wParam, int lParam);

    /// <summary>Suspends redrawing for <paramref name="control"/> to batch layout updates.</summary>
    internal static void SuspendDrawing(Control control) =>
        SendMessage(control.Handle, WM_SETREDRAW, false, 0);

    /// <summary>Resumes redrawing and forces a refresh of <paramref name="control"/>.</summary>
    internal static void ResumeDrawing(Control control)
    {
        SendMessage(control.Handle, WM_SETREDRAW, true, 0);
        control.Refresh();
    }
}
