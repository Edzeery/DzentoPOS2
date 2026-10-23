using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZControls.Core.Constants
{
    // ══════════════════════════════════════════════════════════════════════════════
    //  EZDefaultColors.cs  –  Semantic color palette
    //
    //  Colors are expressed as ARGB to avoid allocating Color.FromArgb on every
    //  property read.  They are static readonly so they participate in the normal
    //  Color value-type semantics while being allocated only once.
    // ══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Semantic color palette shared across all EZ controls.
    /// Override individual colors via the theming system
    /// (<see cref="Helpers.ThemeManager"/>) rather than editing these defaults.
    /// </summary>
    public static class EZDefaultColors
    {
        // ── Brand ─────────────────────────────────────────────────────────────────

        /// <summary>Primary accent – teal.</summary>
        public static readonly Color Primary = Color.FromArgb(0, 139, 139);

        /// <summary>Lighter primary used for focus rings and highlights.</summary>
        public static readonly Color PrimaryFocus = Color.FromArgb(0, 192, 192);

        // ── Surfaces ──────────────────────────────────────────────────────────────

        /// <summary>Standard control background.</summary>
        public static readonly Color Surface = Color.White;

        /// <summary>Slightly off-white – used for inset / secondary surfaces.</summary>
        public static readonly Color SurfaceVariant = Color.WhiteSmoke;

        // ── Text ──────────────────────────────────────────────────────────────────

        /// <summary>Primary text color.</summary>
        public static readonly Color Text = Color.DimGray;

        /// <summary>Placeholder / hint text color.</summary>
        public static readonly Color Placeholder = Color.DarkGray;

        // ── Borders ───────────────────────────────────────────────────────────────

        /// <summary>Default border color for idle controls.</summary>
        public static readonly Color Border = Color.Gainsboro;

        // ── Status ────────────────────────────────────────────────────────────────

        /// <summary>Error / validation failure.</summary>
        public static readonly Color Error = Color.Firebrick;

        /// <summary>Disabled / inactive elements.</summary>
        public static readonly Color Disabled = Color.LightGray;
    }
}
