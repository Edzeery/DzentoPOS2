using System.Reflection;

namespace EZControls.Core.Extensions;



// ══════════════════════════════════════════════════════════════════════════════
//  ControlExtensions
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Extension methods for <see cref="Control"/> instances.
/// </summary>
public static class ControlExtensions
{
    /// <summary>
    /// Enables double-buffering on <paramref name="control"/> via reflection.
    /// Useful for third-party controls or controls that do not expose
    /// <c>DoubleBuffered</c> as a public setter.
    /// </summary>
    public static void EnableDoubleBuffer(this Control control)
    {
        typeof(Control)
            .GetProperty(
                "DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(control, true);
    }

    /// <summary>Suspends visual redrawing of <paramref name="control"/>.</summary>
    public static void SuspendDrawing(this Control control) =>
        NativeMethods.SuspendDrawing(control);

    /// <summary>Resumes visual redrawing and forces a repaint of <paramref name="control"/>.</summary>
    public static void ResumeDrawing(this Control control) =>
        NativeMethods.ResumeDrawing(control);
}