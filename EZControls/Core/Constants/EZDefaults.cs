namespace EZControls.Core.Constants;

// ══════════════════════════════════════════════════════════════════════════════
//  EZDefaults.cs  –  Framework-wide default values
//
//  All "magic numbers" used across the library are defined here so that a
//  single change propagates everywhere.  Controls read these in their field
//  initialisers; the designer therefore sees the correct default in the
//  Properties window without any extra [DefaultValue] ceremony.
//
//  Convention:
//    • Primitive constants (int, float, string) are compile-time const.
//    • Reference-type defaults (Font) are static readonly to ensure a single
//      shared instance.  Callers must NOT dispose the shared font.
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Compile-time constants and shared defaults for the EZControls framework.
/// </summary>
public static class EZDefaults
{
    // ── Control geometry ──────────────────────────────────────────────────────

    /// <summary>Default control width used by most EZ controls (150 px at 96 DPI).</summary>
    public const int DefaultControlWidth = 150;

    /// <summary>Default control height used by most EZ controls (38 px at 96 DPI).</summary>
    public const int DefaultControlHeight = 38;

    // ── Border ────────────────────────────────────────────────────────────────

    /// <summary>Border width in logical pixels.</summary>
    public const int DefaultBorderSize = 2;

    /// <summary>Corner radius for rounded borders, in logical pixels.</summary>
    public const int DefaultBorderRadius = 10;

    // ── Animation ─────────────────────────────────────────────────────────────

    /// <summary>Target frame-rate for the animation timer (frames per second).</summary>
    public const int AnimationFps = 60;

    /// <summary>Derived timer interval in milliseconds (≈16 ms at 60 fps).</summary>
    public const int AnimationIntervalMs = 1000 / AnimationFps;

    /// <summary>Default animation duration in milliseconds.</summary>
    public const int DefaultAnimationDurationMs = 180;

    // ── Typography ────────────────────────────────────────────────────────────

    /// <summary>GDI+ font family name used by all EZ controls.</summary>
    public const string DefaultFontFamily = "Segoe UI";

    /// <summary>GDI+ font size in points.</summary>
    public const float DefaultFontSize = 10F;

    /// <summary>
    /// Shared framework font.  Callers must NOT dispose this instance;
    /// it is owned by the framework and lives for the process lifetime.
    /// </summary>
    public static Font DefaultFont { get; } = new(DefaultFontFamily, DefaultFontSize);
}

