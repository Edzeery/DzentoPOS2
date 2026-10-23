namespace EZControls.Core.Themes;


/// <summary>
/// A dark theme suitable for low-light environments and modern dark-mode UIs.
/// </summary>
public sealed class EZDarkTheme : EZTheme
{
    /// <summary>Initialises the dark theme with its default palette.</summary>
    public EZDarkTheme() { }

    /// <inheritdoc/>
    public override string Name => "Dark";

    /// <inheritdoc/>
    public override Color PrimaryColor => Color.FromArgb(0, 192, 192);

    /// <inheritdoc/>
    public override Color SecondaryColor => Color.MediumTurquoise;

    /// <inheritdoc/>
    public override Color HoverColor => Color.FromArgb(0, 160, 160);

    /// <inheritdoc/>
    public override Color PressedColor => Color.FromArgb(0, 130, 130);

    /// <inheritdoc/>
    public override Color FocusedColor => Color.MediumTurquoise;

    /// <inheritdoc/>
    public override Color BackgroundColor => Color.FromArgb(32, 32, 32);

    /// <inheritdoc/>
    public override Color SurfaceColor => Color.FromArgb(45, 45, 45);

    /// <inheritdoc/>
    public override Color SurfaceHoverColor => Color.FromArgb(55, 55, 55);

    /// <inheritdoc/>
    public override Color TextColor => Color.FromArgb(220, 220, 220);

    /// <inheritdoc/>
    public override Color PlaceholderColor => Color.FromArgb(120, 120, 120);

    /// <inheritdoc/>
    public override Color BorderColor => Color.FromArgb(60, 60, 60);

    /// <inheritdoc/>
    public override Color ErrorColor => Color.FromArgb(220, 80, 80);

    /// <inheritdoc/>
    public override Color DisabledColor => Color.FromArgb(80, 80, 80);
}