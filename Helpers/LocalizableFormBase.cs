using DzentoPOS2.Localization.Services.Interfaces;

namespace DzentoPOS2.WinForms.Helpers
{
    public class LocalizableFormBase : Form
    {
        protected readonly ILanguageService _languageService;
        private bool _languageSubscribed;

        protected LocalizableFormBase()
        {
            _languageService = null!;
        }

        protected LocalizableFormBase(ILanguageService languageService)
        {
            _languageService = languageService;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!_languageSubscribed && _languageService != null)
            {
                _languageService.LanguageChanged += OnLanguageChanged;
                _languageSubscribed = true;
            }
            if (_languageService != null)
                FormLocalizationHelper.ApplyDirection(this, _languageService);
            LoadText();
            OnInitialized();
        }

        private void OnLanguageChanged(object? sender, string culture)
        {
            if (IsDisposed) return;
            if (InvokeRequired)
            {
                BeginInvoke(() =>
                {
                    if (!IsDisposed) ApplyLocalization();
                });
            }
            else
            {
                ApplyLocalization();
            }
        }

        private void ApplyLocalization()
        {
            if (IsDisposed) return;
            SuspendLayout();
            FormLocalizationHelper.ApplyDirection(this, _languageService);
            LoadText();
            OnLanguageChangedAfterLoadText();
            ResumeLayout(true);
        }

        /// <summary>
        /// Override to refresh data after text is reloaded on language switch.
        /// Called after LoadText() when language changes (not on initial load).
        /// </summary>
        protected virtual void OnLanguageChangedAfterLoadText() { }

        /// <summary>
        /// Called once after LoadText() on initial form load.
        /// </summary>
        protected virtual void OnInitialized() { }

        protected virtual void LoadText() { }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _languageSubscribed && _languageService != null)
            {
                _languageService.LanguageChanged -= OnLanguageChanged;
                _languageSubscribed = false;
            }
            base.Dispose(disposing);
        }
    }
}
