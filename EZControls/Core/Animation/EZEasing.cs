using EZControls.Core.Enums; 

namespace EZControls.Core.Animation
{

    // ══════════════════════════════════════════════════════════════════════════════
    //  EZEasing
    // ══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Mathematical easing functions that map a normalized progress value
    /// (0 → 1) to an eased output value (also 0 → 1).
    /// </summary>
    public static class EZEasing
    {
        /// <summary>
        /// Applies the specified easing curve to a raw linear progress value.
        /// </summary>
        /// <param name="type">Which curve to apply.</param>
        /// <param name="progress">Linear progress in [0, 1].  Values outside this range are clamped.</param>
        /// <returns>Eased progress in [0, 1] (may briefly exceed 1.0 for <c>EaseOutBack</c>).</returns>
        public static float Apply(EZAnimationType type, float progress)
        {
            progress = Math.Clamp(progress, 0f, 1f);

            return type switch
            {
                EZAnimationType.Linear => progress,
                EZAnimationType.EaseIn => progress * progress,
                EZAnimationType.EaseOut => 1f - (1f - progress) * (1f - progress),
                EZAnimationType.EaseInOut => EaseInOut(progress),
                EZAnimationType.EaseOutBack => EaseOutBack(progress),
                EZAnimationType.EaseOutBounce => EaseOutBounce(progress),
                _ => progress
            };
        }

        // ── Private curve implementations ─────────────────────────────────────────

        private static float EaseInOut(float t) =>
            t < 0.5f
                ? 2f * t * t
                : 1f - MathF.Pow(-2f * t + 2f, 2f) / 2f;

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * MathF.Pow(t - 1f, 3f) + c1 * MathF.Pow(t - 1f, 2f);
        }

        private static float EaseOutBounce(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            if (t < 1f / d1) return n1 * t * t;
            if (t < 2f / d1) return n1 * (t -= 1.5f / d1) * t + 0.75f;
            if (t < 2.5f / d1) return n1 * (t -= 2.25f / d1) * t + 0.9375f;
            return n1 * (t -= 2.625f / d1) * t + 0.984375f;
        }
    }
}
