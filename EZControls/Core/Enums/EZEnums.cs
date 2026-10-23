namespace EZControls.Core.Enums
{
  

    // ══════════════════════════════════════════════════════════════════════════════
    //  EZEnums.cs  –  All framework-wide enumerations in one file.
    //
    //  Rationale for consolidation:
    //    Scattering one-to-three-member enums across separate files adds noise to
    //    the file tree and makes cross-referencing harder.  A single, well-sectioned
    //    file makes the whole domain vocabulary immediately visible.
    // ══════════════════════════════════════════════════════════════════════════════

    // ── Control interaction state ─────────────────────────────────────────────────

    /// <summary>
    /// Represents the visual/interaction state of any EZ control.
    /// Controls transition between states in response to mouse, keyboard, and
    /// focus events; renderers read this to select the correct brush/pen/color.
    /// </summary>
    public enum EZControlState
    {
        /// <summary>Default idle state.</summary>
        Normal,


        /// <summary>Mouse cursor is over the control.</summary>
        Hover,

        /// <summary>Control holds keyboard focus.</summary>
        Focused,

        /// <summary>Mouse button is held down.</summary>
        Pressed,

        /// <summary>Control is non-interactive (<see cref="Control.Enabled"/> = false).</summary>
        Disabled
    }

    // ── Animation ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Easing curve applied to <see cref="EZAnimation"/> progress (0 → 1).
    /// </summary>
    public enum EZAnimationType
    {
        /// <summary>Constant speed from start to finish.</summary>
        Linear,

        /// <summary>Starts slow, accelerates toward the end.</summary>
        EaseIn,

        /// <summary>Starts fast, decelerates toward the end.</summary>
        EaseOut,

        /// <summary>Slow start, fast middle, slow end.  Most natural for UI.</summary>
        EaseInOut,

        /// <summary>Overshoots the target then settles back (spring-like).</summary>
        EaseOutBack,

        /// <summary>Bounces at the end of the transition.</summary>
        EaseOutBounce
    }

    // ── Border ────────────────────────────────────────────────────────────────────

    /// <summary>
    /// High-level border presentation modes used by input controls.
    /// The underlying <see cref="System.Drawing.Drawing2D.DashStyle"/> is
    /// configured separately on <see cref="Drawing.BorderStyleSettings"/>.
    /// </summary>
    public enum EZBorderStyle
    {
        /// <summary>Full rectangular or rounded border around the control.</summary>
        Solid,

        /// <summary>Only a bottom line is drawn, giving an underline / Material-style look.</summary>
        Underline
    }

    // ── Text ──────────────────────────────────────────────────────────────────────

    /// <summary>Semantic text-input modes for EZ input controls.</summary>
    public enum EZTextStyle
    {
        /// <summary>Plain text with no masking or filtering.</summary>
        Normal,

        /// <summary>Characters are masked; suitable for passwords.</summary>
        Password,

        /// <summary>Only numeric characters are accepted.</summary>
        Numeric,

        /// <summary>Email-address format; triggers simple validation.</summary>
        Email
    }
}
