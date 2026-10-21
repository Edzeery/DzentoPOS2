using DzentoPOS2.WinForms.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzentoPOS2.WinForms.Themes
{
    public static class ThemeManager
    {
        private static readonly string ThemeFile =
            Path.Combine(
             Application.StartupPath,
            "theme.config");
        // =========================
        // Mode
        // =========================

        public static ThemeMode Mode { get; private set; } = ThemeMode.Light;
        public static bool IsDark => Mode == ThemeMode.Dark;
        // =========================
        // Theme Changed Event
        // =========================
        public static event EventHandler? ThemeChanged;

        // =========================
        // Colors
        // =========================
        public static Color PrimaryColor { get; private set; }
        public static Color SecondaryColor { get; private set; }

        public static Color BackgroundColor { get; private set; }
        public static Color SurfaceColor { get; private set; }
        public static Color TextColor { get; private set; }

        public static Color MutedText => ChangeBrightness(TextColor, 0.35);
        // =========================
        // INIT DEFAULT THEME
        // =========================
        static ThemeManager()
        {
            // NOTE: the default mode must not be persisted here, otherwise it
            // would overwrite the saved preference before LoadTheme() reads it.
            SetMode(ThemeMode.Light, save: false);
        }


        // =========================
        // TOGGLE LIGHT / DARK
        // =========================
        public static void ToggleMode()
        {
            SetMode(
                Mode == ThemeMode.Dark
                    ? ThemeMode.Light
                    : ThemeMode.Dark
            );
        }

        // =========================
        // SET MODE
        // ========================= 
        public static void SetMode(ThemeMode mode)
        {
            SetMode(mode, save: true);
        }

        private static void SetMode(ThemeMode mode, bool save)
        {
            Mode = mode;

            if (mode == ThemeMode.Dark)
            {
                // ACCENT
                PrimaryColor = Color.FromArgb(0, 120, 212);

                SecondaryColor =
                    ChangeBrightness(PrimaryColor, -0.25);

                // UI
                BackgroundColor = Color.FromArgb(9, 14, 22);

                SurfaceColor = Color.FromArgb(17, 24, 39);

                TextColor = Color.Gainsboro;
            }
            else
            {
                // ACCENT

                PrimaryColor = Color.FromArgb(0, 99, 109);
                SecondaryColor =
                    ChangeBrightness(PrimaryColor, -0.20);

                // UI  

                BackgroundColor = Color.FromArgb(240, 242, 245);
                SurfaceColor = Color.White;
                TextColor = Color.FromArgb(20, 20, 20);
            }

            RaiseThemeChanged();

            if (save)
                SaveTheme();
        }
        public static void SaveTheme()
        {
            try
            {
                File.WriteAllText(
                    ThemeFile,
                    Mode.ToString());
            }
            catch
            {
                // A read-only deployment folder must not break the application.
            }
        }

        public static void LoadTheme()
        {
            if (!File.Exists(ThemeFile))
                return;

            string mode;

            try
            {
                mode = File.ReadAllText(ThemeFile).Trim();
            }
            catch
            {
                return;
            }

            SetMode(
                string.Equals(mode, "Dark", StringComparison.OrdinalIgnoreCase)
                    ? ThemeMode.Dark
                    : ThemeMode.Light);
        }

        // =========================
        // BRIGHTNESS
        // =========================
        public static Color ChangeBrightness(Color color, double factor)
        {
            double r = color.R;
            double g = color.G;
            double b = color.B;

            if (factor < 0)
            {
                factor = 1 + factor;
                r *= factor;
                g *= factor;
                b *= factor;
            }
            else
            {
                r = (255 - r) * factor + r;
                g = (255 - g) * factor + g;
                b = (255 - b) * factor + b;
            }

            return Color.FromArgb(color.A, (byte)r, (byte)g, (byte)b);
        }

        // =========================
        // APPLY TO CONTROLS
        // =========================
        public static void ApplyToControl(Control control)
        {
            if (control == null || control.IsDisposed) return;

            control.SuspendLayout();
            try
            {
                // Controls that own their identity (cards, themed buttons...)
                // are refreshed by themselves and never overwritten.
                if (control is Controls.IThemeAware aware)
                {
                    aware.ApplyTheme();
                    return;
                }

                // =========================
                // FORM
                // =========================
                if (control is Form)
                {
                    control.BackColor = AppColors.Background;
                }
                // =========================
                // PANEL
                // =========================
                else if (control is Panel panel)
                {
                    string? tag = panel.Tag?.ToString();
                    if (string.Equals(tag, "background", StringComparison.OrdinalIgnoreCase))
                        panel.BackColor = AppColors.Background;
                    else if (string.Equals(tag, "surfacealt", StringComparison.OrdinalIgnoreCase))
                        panel.BackColor = AppColors.SurfaceAlt;
                    else if (string.Equals(tag, "sidebar", StringComparison.OrdinalIgnoreCase))
                        panel.BackColor = AppColors.SidebarSurface;
                    else
                        panel.BackColor = AppColors.Surface;
                }
                // =========================
                // BUTTON
                // =========================
                else if (control is Button btn)
                {
                    // Skip factory buttons that manage their own colors
                    if (btn.Name?.StartsWith("btnFactory", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        // keep as is
                    }
                    else if (btn.FlatStyle == FlatStyle.Flat)
                    {
                        // Let ButtonFactory/custom styles remain
                    }
                    else
                    {
                        btn.BackColor = AppColors.Primary;
                        btn.ForeColor = Color.White;
                        try { RadiusControl.Bind(btn, AppStyle.RadiusSmall); } catch { }
                    }
                }
                // =========================
                // LABEL
                // =========================
                else if (control is Label lbl)
                {
                    lbl.BackColor = Color.Transparent;
                    lbl.ForeColor = ThemeRules.ResolveLabelColor(lbl);
                }
                // =========================
                // TEXTBOX
                // =========================
                else if (control is TextBox txt)
                {
                    txt.BackColor = AppColors.InputBackground;
                    txt.ForeColor = AppColors.Ink;
                    txt.BorderStyle = BorderStyle.FixedSingle;
                }
                // =========================
                // COMBOBOX
                // =========================
                else if (control is ComboBox cbo)
                {
                    cbo.BackColor = AppColors.InputBackground;
                    cbo.ForeColor = AppColors.Ink;
                }
                // =========================
                // NUMERICUPDOWN
                // =========================
                else if (control is NumericUpDown nud)
                {
                    nud.BackColor = AppColors.InputBackground;
                    nud.ForeColor = AppColors.Ink;
                }
                // =========================
                // CHECKBOX / RADIOBUTTON
                // =========================
                else if (control is CheckBox chk)
                {
                    chk.ForeColor = AppColors.Ink;
                    chk.BackColor = Color.Transparent;
                }
                else if (control is RadioButton rb)
                {
                    rb.ForeColor = AppColors.Ink;
                    rb.BackColor = Color.Transparent;
                }
                // =========================
                // DATEPICKER
                // =========================
                else if (control is DateTimePicker dtp)
                {
                    dtp.BackColor = AppColors.InputBackground;
                    dtp.ForeColor = AppColors.Ink;
                }
                // =========================
                // LISTS / TABS
                // =========================
                else if (control is ListView lv)
                {
                    lv.BackColor = AppColors.Surface;
                    lv.ForeColor = AppColors.Ink;
                }
                else if (control is ListBox lb)
                {
                    lb.BackColor = AppColors.InputBackground;
                    lb.ForeColor = AppColors.Ink;
                }
                else if (control is TreeView tv)
                {
                    tv.BackColor = AppColors.Surface;
                    tv.ForeColor = AppColors.Ink;
                }
                else if (control is TabControl tc)
                {
                    tc.BackColor = AppColors.Surface;
                    tc.ForeColor = AppColors.Ink;
                }
                else if (control is TabPage tp)
                {
                    tp.BackColor = AppColors.Surface;
                    tp.ForeColor = AppColors.Ink;
                }
                // =========================
                // DATAGRIDVIEW
                // =========================
                else if (control is DataGridView dgv)
                {
                    Helpers.DataGridViewStyleHelper.ApplyModernStyle(dgv, true);
                }

                // =========================
                // CHILDREN
                // =========================
                foreach (Control child in control.Controls)
                {
                    ApplyToControl(child);
                }
            }
            finally
            {
                control.ResumeLayout(true);
            }
        }
        public static void AttachTheme(Control control)
        {
            ApplyToControl(control);

            control.ControlAdded += (s, e) =>
            {
                if (e.Control != null)
                    ApplyToControl(e.Control);
            };
        }
        // =========================
        // EVENT TRIGGER
        // =========================
        private static void RaiseThemeChanged()
        {
            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}
