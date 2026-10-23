using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZControls.Core.Animation
{


    // ══════════════════════════════════════════════════════════════════════════════
    //  AnimationState  –  time-based lerp for a single float value
    //
    //  Key improvement over the original 0.2f-per-tick approach:
    //    Speed is expressed as "units per second", derived from the requested
    //    duration.  The actual step per Tick() call is proportional to the
    //    real elapsed wall-clock time, so animation speed is decoupled from the
    //    timer interval.  Changing the timer from 15 ms to 8 ms no longer doubles
    //    animation speed.
    // ══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Manages a single floating-point value that animates toward a target.
    /// Uses wall-clock time so animation speed is independent of the timer interval.
    /// </summary>
    internal sealed class AnimationState
    {
        private const float Epsilon = 0.05f;

        private float _value;
        private float _target;
        private float _speed;        // units per second
        private long _lastTickTs;
        private bool _running;

        /// <summary>Current animated value.</summary>
        public float Value => _value;

        /// <summary><see langword="true"/> while the value is still moving toward the target.</summary>
        public bool IsActive => _running;

        /// <param name="initial">Starting value.</param>
        public AnimationState(float initial)
        {
            _value = initial;
            _target = initial;
        }

        /// <summary>
        /// Sets a new target value and starts animating toward it.
        /// </summary>
        /// <param name="target">Desired end value.</param>
        /// <param name="durationSeconds">
        /// Approximate time to complete the full transition.
        /// Defaults to the framework default animation duration.
        /// </param>
        public void SetTarget(float target, float durationSeconds = EZDefaults.DefaultAnimationDurationMs / 1000f)
        {
            _target = target;
            _lastTickTs = System.Diagnostics.Stopwatch.GetTimestamp();
            float delta = Math.Abs(_target - _value);
            _speed = delta > Epsilon && durationSeconds > 0f
                                ? delta / durationSeconds
                                : float.MaxValue;
            _running = delta > Epsilon;
        }

        /// <summary>Force the value to <paramref name="value"/> immediately, stopping any active animation.</summary>
        public void SnapTo(float value)
        {
            _value = value;
            _target = value;
            _running = false;
        }

        /// <summary>
        /// Advances the animation by the real elapsed time since the last call.
        /// </summary>
        /// <returns><see langword="true"/> while still animating; <see langword="false"/> when settled.</returns>
        public bool Tick()
        {
            if (!_running) return false;

            long now = System.Diagnostics.Stopwatch.GetTimestamp();
            float elapsed = (float)(now - _lastTickTs) / System.Diagnostics.Stopwatch.Frequency;
            _lastTickTs = now;

            float maxStep = _speed * elapsed;
            float delta = _target - _value;

            if (Math.Abs(delta) <= maxStep)
            {
                _value = _target;
                _running = false;
                return false;
            }

            _value += Math.Sign(delta) * maxStep;
            return true;
        }
    } 
}
