using EZControls.Core.Enums;
using EZControls.Core.Themes;

namespace EZControls.Core.Interfaces;

// ══════════════════════════════════════════════════════════════════════════════
//  Framework Interfaces
//
//  Keeping interfaces minimal and single-purpose follows ISP (Interface
//  Segregation Principle).  Controls implement only what they actually support,
//  and the theming / animation systems can target the interface rather than a
//  concrete type.
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Implemented by any EZ control that can receive a visual theme at runtime.
/// </summary>
/// <remarks>
/// The <see cref="Helpers.ThemeManager"/> broadcasts theme changes to all
/// registered controls via this interface.  Implementations should update
/// their colors, font, and any cached drawing resources, then call
/// <see cref="Control.Invalidate()"/> to schedule a repaint.
/// </remarks>
public interface IEZThemeable
{
    /// <summary>
    /// Applies <paramref name="theme"/> to the control's visual appearance.
    /// Called automatically by <see cref="Helpers.ThemeManager"/> when the
    /// active theme changes.
    /// </summary>
    /// <param name="theme">The theme to apply.  Never null.</param>
    void ApplyTheme(EZTheme theme);
}

/// <summary>
/// Implemented by EZ controls that expose a discrete interaction state.
/// </summary>
/// <remarks>
/// Renderers and painters can query this interface to select the correct
/// brush/pen without needing a reference to a concrete control type.
/// </remarks>
public interface IEZStateful
{
    /// <summary>Current interaction state of the control.</summary>
    EZControlState State { get; }
}

/// <summary>
/// Implemented by EZ input controls that support validation.
/// </summary>
public interface IEZValidatable
{
    /// <summary><see langword="true"/> when the control is in an error state.</summary>
    bool HasError { get; }

    /// <summary>Descriptive error message, or <see cref="string.Empty"/> when valid.</summary>
    string ErrorMessage { get; }

    /// <summary>Puts the control into an error state and displays <paramref name="message"/>.</summary>
    void SetError(string message);

    /// <summary>Clears any active error state.</summary>
    void ClearError();
}

/// <summary>
/// Implemented by controls that support a visual style preset applied atomically.
/// </summary>
/// <typeparam name="TStyle">The style record type specific to the control.</typeparam>
public interface IEZStylable<in TStyle> where TStyle : class
{
    /// <summary>Applies all properties in <paramref name="style"/> in a single layout pass.</summary>
    void ApplyStyle(TStyle style);
}