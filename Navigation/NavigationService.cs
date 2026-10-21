using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.ServicesUI;
using DzentoPOS2.WinForms.Themes;
using FontAwesome.Sharp;
using Microsoft.Extensions.DependencyInjection;

namespace DzentoPOS2.WinForms.Navigation
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private Panel? _container;
        private Label? _titleLabel;
        private Form? _activeForm;
        private IconButton? _activeButton;

        // Colors — sourced from the central design tokens.
        private static Color ActiveBg => AppColors.Primary;
        private static Color InactiveBg => AppColors.SidebarSurface;
        private static Color ActiveFg => Color.White;
        private static Color InactiveFg => Color.White;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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
            catch
            {
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

            // Dispose old form
            if (_activeForm != null)
            {
                _activeForm.Hide();
                _activeForm.Dispose();
                _activeForm = null;
            }

            // Clear container
            _container.SuspendLayout();
            foreach (Control ctrl in _container.Controls
                         .OfType<Form>().ToList())
            {
                _container.Controls.Remove(ctrl);
                ctrl.Dispose();
            }

            // Create & host new form
            var form = _serviceProvider.GetRequiredService<TForm>();
            if (form == null)
                throw new InvalidOperationException($"Unable to resolve {typeof(TForm).Name}.");

            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            form.Opacity = 0;

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
    }
}