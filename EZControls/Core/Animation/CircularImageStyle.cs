using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZControls.Core.Animation
{
    // ══════════════════════════════════════════════════════════════════════════════
    //  CircularImageStyle
    //  Immutable value-object that carries every visual knob for the control.
    //  Pass it between instances or serialise it for theming / persistence.
    //  Null colours are intentional: null means "use the default", which keeps
    //  serialised JSON compact (only non-default values need to appear).
    // ══════════════════════════════════════════════════════════════════════════════
    public sealed record CircularImageStyle
    {
        // ── border ───────────────────────────────────────────────────────────────
        public int BorderSize { get; init; } = 3;
        public Color? BorderColor { get; init; }          // null → RoyalBlue
        public Color? BorderColor2 { get; init; }          // null → DeepPink
        public bool UseGradient { get; init; } = true;
        public float GradientAngle { get; init; } = 45f;
        public DashStyle BorderDashStyle { get; init; } = DashStyle.Solid;
        public float HoverBorderSize { get; init; } = 5f;

        // ── hover ────────────────────────────────────────────────────────────────
        public Color? HoverGlowColor { get; init; }              // null → 80,White

        // ── shadow ───────────────────────────────────────────────────────────────
        public int ShadowSize { get; init; } = 8;
        public int ShadowOpacity { get; init; } = 60;
        public Color? ShadowColor { get; init; }               // null → Black

        // ── sentinel ─────────────────────────────────────────────────────────────
        public static CircularImageStyle Default { get; } = new();
    }
}
