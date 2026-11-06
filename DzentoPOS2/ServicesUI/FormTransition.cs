using DzentoPOS2.WinForms.Themes;
using System;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace DzentoPOS2.WinForms.ServicesUI
{
    /// <summary>
    /// Fade-in helper for forms.
    /// <para>
    /// A form hosted inside a panel (<see cref="Form.TopLevel"/> is
    /// <c>false</c>, which is how every navigation page is shown) is a child
    /// window: WinForms does not apply <see cref="Form.Opacity"/> to it, so the
    /// animation had no visual effect while still running a timer and forcing
    /// work on every navigation. Hosted pages are now shown immediately; the
    /// fade is kept for real top-level windows.
    /// </para>
    /// <para>
    /// Animations are bound to the form's lifetime: one timer per form, always
    /// stopped and disposed when the form is disposed, so a tick can never run
    /// against a released form.
    /// </para>
    /// </summary>
    public static class FormTransition
    {
        // One running animation per form. Weak keys: a form that is never
        // re-animated does not keep the entry alive.
        private static readonly ConditionalWeakTable<Form, System.Windows.Forms.Timer> Animations = new();

        /// <summary>
        /// Stops any running fade and makes <paramref name="form"/> fully
        /// visible. Safe to call on a disposed form.
        /// </summary>
        public static void Cancel(Form? form)
        {
            if (form is null)
                return;

            StopAnimation(form);

            if (!form.IsDisposed && !form.Disposing)
                form.Opacity = 1;
        }

        /// <summary>Fades <paramref name="form"/> in (top-level windows only).</summary>
        public static void FadeIn(Form form)
        {
            ArgumentNullException.ThrowIfNull(form);

            if (form.IsDisposed || form.Disposing)
                return;

            StopAnimation(form);

            // Hosted pages cannot fade - show them instantly instead of running
            // a no-op animation on every navigation.
            if (!form.TopLevel)
            {
                form.Opacity = 1;
                return;
            }

            form.Opacity = 0;

            var timer = new System.Windows.Forms.Timer { Interval = AppStyle.AnimationInterval };

            // Fires when the window is disposed (the user navigated again).
            void OnDisposed(object? sender, EventArgs e)
            {
                form.Disposed -= OnDisposed;
                StopAnimation(form);
            }

            timer.Tick += (_, _) =>
            {
                if (form.IsDisposed || form.Disposing)
                {
                    StopAnimation(form);
                    return;
                }

                var next = form.Opacity + 0.1;
                if (next >= 1.0)
                {
                    form.Opacity = 1;
                    StopAnimation(form);
                }
                else
                {
                    form.Opacity = next;
                }
            };

            form.Disposed += OnDisposed;
            Animations.Add(form, timer);
            timer.Start();
        }

        private static void StopAnimation(Form form)
        {
            if (Animations.TryGetValue(form, out var timer))
            {
                Animations.Remove(form);
                timer.Stop();
                timer.Dispose();
            }
        }
    }
}
