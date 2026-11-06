using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Helpers
{
    /// <summary>
    /// Base class for modal dialogs that must follow the active light/dark theme.
    /// It applies the central design tokens on load and repaints itself when the
    /// theme changes (modal dialogs are not hosted by <c>NavigationService</c>,
    /// so they would otherwise keep their construction-time colors).
    /// </summary>
    public class ThemedDialog : Form
    {
        protected ThemedDialog()
        {
            BackColor = AppColors.Surface;
            ForeColor = AppColors.Ink;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ThemeManager.ThemeChanged += OnThemeChanged;
            ApplyTheme();
        }

        /// <summary>
        /// Re-applies all theme tokens to this dialog and its children. Derived
        /// dialogs can override to restore colors that are intentionally
        /// different from the generic defaults, then call <c>base.ApplyTheme()</c>.
        /// </summary>
        protected virtual void ApplyTheme()
        {
            if (IsDisposed) return;

            SuspendLayout();
            try
            {
                ThemeManager.ApplyToControl(this);
                BackColor = AppColors.Surface;
                ForeColor = AppColors.Ink;
            }
            finally
            {
                ResumeLayout(true);
            }

            Invalidate(true);
        }

        private void OnThemeChanged(object? sender, EventArgs e)
        {
            if (IsDisposed) return;
            if (InvokeRequired)
            {
                BeginInvoke(ApplyTheme);
                return;
            }
            ApplyTheme();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                ThemeManager.ThemeChanged -= OnThemeChanged;

            base.Dispose(disposing);
        }
    }
}
