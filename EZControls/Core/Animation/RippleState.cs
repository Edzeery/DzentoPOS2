using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZControls.Core.Animation
{
    // ══════════════════════════════════════════════════════════════════════════════
    //  RippleState
    //
    //  Encapsulates the lifecycle of a single click-ripple effect.
    //  The control's OnAnimationTick queries IsActive and Tick() each frame.
    // ══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Tracks the state of a click-ripple animation.
    /// The ripple expands from the click point at a fixed pixel-per-second rate.
    /// </summary>
    internal sealed class RippleState
    {
        private const float ExpandPerSecond = 400f; // px/s

        /// <summary><see langword="true"/> while the ripple is still expanding.</summary>
        public bool IsActive { get; private set; }

        /// <summary>Current radius of the ripple disc in logical pixels.</summary>
        public float Radius { get; private set; }

        /// <summary>Click origin in control-client coordinates.</summary>
        public Point Origin { get; private set; }

        private long _lastTickTs;
        private float _maxRadius;

        /// <summary>Starts a new ripple from <paramref name="origin"/>.</summary>
        /// <param name="origin">Click point in control-client coordinates.</param>
        /// <param name="maxRadius">
        /// Radius at which the ripple stops.  Typically the diagonal of the
        /// control (so the disc covers the entire surface at its largest).
        /// </param>
        public void Start(Point origin, float maxRadius)
        {
            Origin = origin;
            Radius = 0f;
            _maxRadius = maxRadius;
            IsActive = true;
            _lastTickTs = System.Diagnostics.Stopwatch.GetTimestamp();
        }

        /// <summary>
        /// Advances the ripple.
        /// </summary>
        /// <returns><see langword="true"/> while still expanding.</returns>
        public bool Tick()
        {
            if (!IsActive) return false;

            long now = System.Diagnostics.Stopwatch.GetTimestamp();
            float elapsed = (float)(now - _lastTickTs) / System.Diagnostics.Stopwatch.Frequency;
            _lastTickTs = now;

            Radius += ExpandPerSecond * elapsed;

            if (Radius >= _maxRadius)
            {
                Radius = _maxRadius;
                IsActive = false;
                return false;
            }

            return true;
        }

        /// <summary>Immediately stops and resets the ripple.</summary>
        public void Cancel()
        {
            IsActive = false;
            Radius = 0f;
        }
    }

}
