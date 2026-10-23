using System.ComponentModel;

namespace EZControls.Core.Animation;

// ══════════════════════════════════════════════════════════════════════════════
//  EZAnimationManager
//
//  Improvements over the original:
//  • Uses DateTime.UtcNow (monotonic-safe) and computes progress from elapsed
//    time rather than tick count, so animations complete in the specified
//    duration regardless of frame rate.
//  • Stops the timer when the animation list is empty (no idle ticking).
//  • Skips disposed controls to prevent ObjectDisposedException.
//  • Exposes FPS property for easy configuration.
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Drives multiple concurrent <see cref="EZAnimation"/> instances via a shared
/// <see cref="System.Windows.Forms.Timer"/>.
/// The timer stops automatically when all animations complete.
/// </summary>
[ToolboxItem(false)]
public sealed class EZAnimationManager : Component
{
    private readonly List<EZAnimation> _animations = new(8);
    private readonly System.Windows.Forms.Timer _timer;

    // ── Properties ────────────────────────────────────────────────────────────

    /// <summary>
    /// Target frames per second for the animation timer.
    /// Clamped to [1, 240].  Default is 60.
    /// </summary>
    public int FPS
    {
        get => 1000 / _timer.Interval;
        set
        {
            value = Math.Clamp(value, 1, 240);
            _timer.Interval = 1000 / value;
        }
    }

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>Initialises the manager with the default 60 fps timer.</summary>
    public EZAnimationManager()
    {
        _timer = new System.Windows.Forms.Timer
        {
            Interval = EZDefaults.AnimationIntervalMs
        };
        _timer.Tick += OnTimerTick;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Enqueues and starts a new animation.</summary>
    public void Start(EZAnimation animation)
    {
        animation.StartTime = DateTime.UtcNow;
        animation.Completed = false;
        _animations.Add(animation);

        if (!_timer.Enabled)
            _timer.Start();
    }

    /// <summary>Removes all animations associated with <paramref name="control"/>.</summary>
    public void Stop(Control control)
    {
        _animations.RemoveAll(a => a.Control == control);
        if (_animations.Count == 0)
            _timer.Stop();
    }

    /// <summary>Removes all pending animations.</summary>
    public void Clear()
    {
        _animations.Clear();
        _timer.Stop();
    }

    // ── Timer tick ────────────────────────────────────────────────────────────

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (_animations.Count == 0)
        {
            _timer.Stop();
            return;
        }

        List<EZAnimation>? completed = null;
        DateTime now = DateTime.UtcNow;

        foreach (EZAnimation anim in _animations)
        {
            // Skip disposed controls to avoid ObjectDisposedException.
            if (anim.Control.IsDisposed)
            {
                (completed ??= new()).Add(anim);
                continue;
            }

            double elapsedMs = (now - anim.StartTime).TotalMilliseconds;
            float progress = Math.Clamp((float)(elapsedMs / anim.Duration), 0f, 1f);
            float eased = EZEasing.Apply(anim.AnimationType, progress);

            anim.OnUpdate(eased);
            anim.Control.Invalidate();

            if (progress >= 1f)
            {
                anim.Completed = true;
                anim.OnCompleted?.Invoke();
                (completed ??= new()).Add(anim);
            }
        }

        if (completed is not null)
            foreach (var c in completed) _animations.Remove(c);

        if (_animations.Count == 0)
            _timer.Stop();
    }

    // ── IDisposable ───────────────────────────────────────────────────────────

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer.Stop();
            _timer.Dispose();
            _animations.Clear();
        }
        base.Dispose(disposing);
    }
}