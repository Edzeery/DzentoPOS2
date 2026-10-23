using EZControls.Core.Interfaces;
using EZControls.Core.Themes;

namespace EZControls.Core.Helpers;

// ══════════════════════════════════════════════════════════════════════════════
//  ThemeManager
//
//  Improvements over the original implementation:
//
//  1. Memory leak fixed:
//     The original stored a strong lambda (closure) per registered control in
//     a static event.  Controls were never unregistered, so they stayed alive
//     even after being disposed/GC'd.  This version uses WeakReference<T> so
//     the manager never prevents controls from being collected.  Dead references
//     are pruned on every theme change.
//
//  2. Thread safety:
//     SetTheme() and RegisterControl() are typically called from the UI thread,
//     but the lock guard handles accidental cross-thread calls gracefully.
//
//  3. Apply-on-register:
//     When a control registers after the initial theme has been set it
//     immediately receives the current theme rather than waiting for the next
//     SetTheme() call.
//
//  4. ThemeChanged event:
//     Consumers (e.g. a settings panel) can subscribe without also registering
//     a control – the event and the registration list are now independent.
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Application-wide theme coordinator.
/// Maintains a list of registered controls and broadcasts
/// theme changes to them via <see cref="IEZThemeable"/>.
/// </summary>
public static class ThemeManager
{
    // Protects _controls and _currentTheme during concurrent access.
    private static readonly object s_lock = new();

    // WeakReference list so registered controls can be GC'd freely.
    private static readonly List<WeakReference<IEZThemeable>> s_controls = new(32);

    private static EZTheme s_currentTheme = new EZLightTheme();

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// The currently active theme.  Read from any thread; set via
    /// <see cref="SetTheme"/> on the UI thread.
    /// </summary>
    public static EZTheme CurrentTheme
    {
        get { lock (s_lock) return s_currentTheme; }
    }

    /// <summary>
    /// Raised on the UI thread after a new theme has been applied to all
    /// registered controls.  Subscribers can use this to update non-control
    /// UI elements (e.g. custom painted panels).
    /// </summary>
    public static event EventHandler<EZTheme>? ThemeChanged;

    /// <summary>
    /// Switches the active theme and immediately applies it to all live
    /// registered controls.  Must be called from the UI thread.
    /// </summary>
    /// <param name="theme">The new theme to apply.  Must not be null.</param>
    /// <exception cref="ArgumentNullException"/>
    public static void SetTheme(EZTheme theme)
    {
        ArgumentNullException.ThrowIfNull(theme);

        lock (s_lock)
        {
            s_currentTheme = theme;
            ApplyToAll(theme);
        }

        ThemeChanged?.Invoke(null, theme);
    }

    /// <summary>
    /// Registers a control to receive future theme changes.
    /// If a theme is already active the control is styled immediately.
    /// </summary>
    /// <param name="control">
    /// The control to register.  Must implement <see cref="IEZThemeable"/>.
    /// The manager holds only a weak reference; no manual unregistration is needed.
    /// </param>
    public static void RegisterControl(IEZThemeable control)
    {
        ArgumentNullException.ThrowIfNull(control);

        EZTheme snapshot;

        lock (s_lock)
        {
            // Prune dead references while we already hold the lock.
            PruneDead();
            s_controls.Add(new WeakReference<IEZThemeable>(control));
            snapshot = s_currentTheme;
        }

        // Apply outside the lock to avoid potential deadlocks if ApplyTheme
        // triggers reentrant ThemeManager calls.
        control.ApplyTheme(snapshot);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Applies <paramref name="theme"/> to every live registered control.
    /// Dead references are removed.  Caller must hold <see cref="s_lock"/>.
    /// </summary>
    private static void ApplyToAll(EZTheme theme)
    {
        // Walk backwards so removal doesn't invalidate the loop index.
        for (int i = s_controls.Count - 1; i >= 0; i--)
        {
            if (s_controls[i].TryGetTarget(out IEZThemeable? target))
            {
                target.ApplyTheme(theme);
            }
            else
            {
                s_controls.RemoveAt(i);
            }
        }
    }

    /// <summary>Removes entries whose target has been collected.</summary>
    private static void PruneDead()
    {
        for (int i = s_controls.Count - 1; i >= 0; i--)
        {
            if (!s_controls[i].TryGetTarget(out _))
                s_controls.RemoveAt(i);
        }
    }
}