using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.ServicesUI;
using DzentoPOS2.WinForms.Themes;
using FontAwesome.Sharp;
using Microsoft.Extensions.DependencyInjection;

namespace DzentoPOS2.WinForms.Navigation
{
    public class NavigationService : INavigationService
    {
        // Pages must not share one DbContext (or one scope) for the whole
        // session: every navigation gets its own scope, which is disposed
        // together with the previous page.
        private readonly IServiceScopeFactory _scopeFactory;
        private Panel? _container;
        private Label? _titleLabel;
        private Form? _activeForm;
        private IServiceScope? _activeScope;

        public Form? CurrentPage => _activeForm;
        private IconButton? _activeButton;

        // Colors — sourced from the central design tokens.
        private static Color ActiveBg => AppColors.Primary;
        private static Color InactiveBg => AppColors.SidebarSurface;
        private static Color ActiveFg => Color.White;
        private static Color InactiveFg => Color.White;

        public NavigationService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            ThemeManager.ThemeChanged += OnThemeChanged;
        }

        private void OnThemeChanged(object? sender, EventArgs e)
        {
            try
            {
                // Recolor sidebar buttons
                if (_activeButton != null)
                    SetActiveButton(_activeButton);

                // Reapply theme to active hosted form
                if (_activeForm != null && !_activeForm.IsDisposed)
                {
                    _activeForm.SuspendLayout();
                    ThemeManager.ApplyToControl(_activeForm);
                    _activeForm.ResumeLayout(true);
                    _activeForm.Invalidate(true);
                }
            }
            catch (Exception ex)
            {
                // A theme repaint must never take the shell down; record it.
                DzentoPOS2.App.Diagnostics.ExceptionLogger.Write(
                    ex,
                    "NavigationService theme reapply");
            }
        }


        public void SetHost(Panel container, Label titleLabel)
        {
            _container = container;
            _titleLabel = titleLabel;
        }

        public void SetActiveButton(IconButton button)
        {
            // Reset previous
            if (_activeButton != null)
            {
                _activeButton.BackColor = InactiveBg;
                _activeButton.ForeColor = InactiveFg;
                _activeButton.IconColor = InactiveFg;
            }

            // Highlight new
            _activeButton = button;
            _activeButton.BackColor = ActiveBg;
            _activeButton.ForeColor = ActiveFg;
            _activeButton.IconColor = ActiveFg;
        }

public void Navigate<TForm>() where TForm : Form
        {
            if (_container == null)
                throw new InvalidOperationException("Navigation host not set.");

            // Resolve the new page before touching the current one so a failure
            // (missing DI registration) never leaves the shell with a blank page.
            IServiceScope scope;
            TForm form;
            try
            {
                scope = _scopeFactory.CreateScope();
                form = scope.ServiceProvider.GetRequiredService<TForm>();
            }
            catch (Exception ex)
            {
                AppMessageBox.Error(ex.Message);
                return;
            }

            try
            {
                // Dispose old form (stop its fade first so nothing outlives it)
                if (_activeForm != null)
                {
                    FormTransition.Cancel(_activeForm);
                    _activeForm.Hide();
                    _activeForm.Dispose();
                    _activeForm = null;
                }

                // The previous page's scope owns its DbContext / unit of work;
                // dispose it only after the page it belongs to is gone.
                _activeScope?.Dispose();
                _activeScope = scope;

                // Clear container
                _container.SuspendLayout();
                foreach (Control ctrl in _container.Controls
                             .OfType<Form>().ToList())
                {
                    _container.Controls.Remove(ctrl);
                    ctrl.Dispose();
                }

                // Host new form
                form.TopLevel = false;
                form.FormBorderStyle = FormBorderStyle.None;
                form.Dock = DockStyle.Fill;

                _container.Controls.Add(form);
                _container.ResumeLayout();

                form.Show();
                form.BringToFront();

                // Update title
                if (_titleLabel != null && !form.IsDisposed)
                    _titleLabel.Text = form.Text;

                // Apply theme & animate
                form.SuspendLayout();
                ThemeManager.ApplyToControl(form);
                form.ResumeLayout(true);
                FormTransition.FadeIn(form);

                _activeForm = form;
            }
            catch (Exception ex)
            {
                if (_container != null && _container.IsHandleCreated)
                    _container.ResumeLayout();
                AppMessageBox.Error(ex.Message);
            }
        }
    }
}