using EZControls.Core.Constants;
using EZControls.Core.Enums;

namespace EZControls.Core.Animation;


// ══════════════════════════════════════════════════════════════════════════════
//  EZAnimation  –  data object for the general-purpose animation manager
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Describes a single animation task managed by <see cref="EZAnimationManager"/>.
/// Create with <c>required</c> properties and pass to
/// <see cref="EZAnimationManager.Start"/>.
/// </summary>
public sealed class EZAnimation
{
    /// <summary>The control to invalidate on each tick.</summary>
    public required Control Control { get; init; }

    /// <summary>Callback invoked each tick with the current eased progress (0 → 1).</summary>
    public required Action<float> OnUpdate { get; init; }

    /// <summary>Optional callback invoked once when progress reaches 1.</summary>
    public Action? OnCompleted { get; init; }

    /// <summary>Total animation duration in milliseconds.  Defaults to 180 ms.</summary>
    public int Duration { get; init; } = EZDefaults.DefaultAnimationDurationMs;

    /// <summary>Easing curve applied to the raw progress.</summary>
    public EZAnimationType AnimationType { get; init; } = EZAnimationType.EaseInOut;

    // ── Internal state (managed by EZAnimationManager) ────────────────────────
    internal DateTime StartTime { get; set; }
    internal bool Completed { get; set; }
}
