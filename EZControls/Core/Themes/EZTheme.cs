using EZControls.Core.Constants;

namespace EZControls.Core.Themes;

// ══════════════════════════════════════════════════════════════════════════════
//  Theme System
//
//  EZTheme is an immutable-by-convention value object that carries the full
//  visual specification of a UI theme.  Callers create theme instances once,
//  then pass them to ThemeManager.SetTheme().
//
//  Design notes:
//  • Properties use init-only setters so that "with" expressions produce
//    modified copies without mutating the original.
//  • All Color properties are non-nullable.  A theme is always fully specified.
//  • The shared DefaultFont is owned by the theme; controls must NOT dispose it.
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Defines the complete visual specification for the EZControls framework.
/// Pass an instance to <see cref="Helpers.ThemeManager.SetTheme"/> to apply
/// it globally across all registered controls.
/// </summary>
public class EZTheme
{
    // ── Identity ──────────────────────────────────────────────────────────────

    /// <summary>Human-readable theme name used for display and serialisation.</summary>
    public virtual string Name { get; init; } = "Custom";

    // ── Brand colors ──────────────────────────────────────────────────────────

    /// <summary>Primary accent color (buttons, focus rings, checked states).</summary>
    public virtual Color PrimaryColor { get; init; } = EZDefaultColors.Primary;

    /// <summary>Secondary accent for gradients and highlights.</summary>
    public virtual Color SecondaryColor { get; init; } = EZDefaultColors.PrimaryFocus;

    /// <summary>Color used for hover states on primary elements.</summary>
    public virtual Color HoverColor { get; init; } = Color.FromArgb(0, 115, 115);

    /// <summary>Color used for pressed / active states.</summary>
    public virtual Color PressedColor { get; init; } = Color.FromArgb(0, 95, 95);

    /// <summary>Color used for the focus ring / outline.</summary>
    public virtual Color FocusedColor { get; init; } = EZDefaultColors.PrimaryFocus;

    // ── Surface colors ────────────────────────────────────────────────────────

    /// <summary>Main control / window background color.</summary>
    public virtual Color BackgroundColor { get; init; } = EZDefaultColors.Surface;

    /// <summary>Slightly offset surface for inset areas (panels, list items).</summary>
    public virtual Color SurfaceColor { get; init; } = EZDefaultColors.SurfaceVariant;

    /// <summary>Surface color when the element is hovered.</summary>
    public virtual Color SurfaceHoverColor { get; init; } = Color.FromArgb(240, 240, 240);

    // ── Text colors ────────────────────────────────────────────────────────────

    /// <summary>Primary text / foreground color.</summary>
    public virtual Color TextColor { get; init; } = EZDefaultColors.Text;

    /// <summary>Placeholder / hint text color for empty input controls.</summary>
    public virtual Color PlaceholderColor { get; init; } = EZDefaultColors.Placeholder;

    // ── Border ────────────────────────────────────────────────────────────────

    /// <summary>Default border color for idle controls.</summary>
    public virtual Color BorderColor { get; init; } = EZDefaultColors.Border;

    // ── Status ────────────────────────────────────────────────────────────────

    /// <summary>Color indicating an error or validation failure.</summary>
    public virtual Color ErrorColor { get; init; } = EZDefaultColors.Error;

    /// <summary>Color applied to disabled controls.</summary>
    public virtual Color DisabledColor { get; init; } = EZDefaultColors.Disabled;

    // ── Typography ────────────────────────────────────────────────────────────

    /// <summary>
    /// Default font applied to all EZ controls by this theme.
    /// The theme owns this font; controls must NOT dispose it.
    /// </summary>
    public virtual Font DefaultFont { get; init; } = EZDefaults.DefaultFont;
}
