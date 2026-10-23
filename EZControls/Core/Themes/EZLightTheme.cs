namespace EZControls.Core.Themes;


// ══════════════════════════════════════════════════════════════════════════════
//  Built-in theme implementations
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// A clean light theme suitable for standard Windows desktop applications.
/// </summary>
public sealed class EZLightTheme : EZTheme
{
    /// <summary>Initialises the light theme with its default palette.</summary>
    public EZLightTheme()
    {
        // All properties are set via the base class init-only setters through
        // object initialiser syntax in the record hierarchy – C# allows init
        // setters to be set in derived constructors via this workaround using
        // the "with" idiom on a temp. For a sealed class we simply re-assign.
    }

    // Override read-only properties by shadowing with new init – cleaner than
    // a constructor with 15 arguments.
    /// <inheritdoc/>
    public override string Name => "Light";

    /// <inheritdoc/>
    public override Color PrimaryColor => Color.FromArgb(0, 139, 129);

    /// <inheritdoc/>
    public override Color SecondaryColor => Color.FromArgb(0, 192, 192);

    /// <inheritdoc/>
    public override Color HoverColor => Color.FromArgb(0, 115, 107);

    /// <inheritdoc/>
    public override Color PressedColor => Color.FromArgb(0, 95, 88);

    /// <inheritdoc/>
    public override Color FocusedColor => Color.FromArgb(0, 192, 192);

    /// <inheritdoc/>
    public override Color BackgroundColor => Color.White;

    /// <inheritdoc/>
    public override Color SurfaceColor => Color.WhiteSmoke;

    /// <inheritdoc/>
    public override Color SurfaceHoverColor => Color.FromArgb(240, 240, 240);

    /// <inheritdoc/>
    public override Color TextColor => Color.FromArgb(45, 45, 45);

    /// <inheritdoc/>
    public override Color PlaceholderColor => Color.DarkGray;

    /// <inheritdoc/>
    public override Color BorderColor => Color.Gainsboro;

    /// <inheritdoc/>
    public override Color ErrorColor => Color.Firebrick;

    /// <inheritdoc/>
    public override Color DisabledColor => Color.LightGray;
}
