using DzentoPOS2.Localization.Resources;
using DzentoPOS2.WinForms.Controls;
using DzentoPOS2.WinForms.Themes;
using System.Drawing.Drawing2D;

namespace DzentoPOS2.WinForms.Helpers
{
    public static class AppMessageBox
    {
        public static void Error(string message, string? title = null)
        {
            Show(message, title ?? AppStrings.ErrorMsg, MessageBoxIcon.Error);
        }

        public static void Success(string message, string? title = null)
        {
            Show(message, title ?? AppStrings.DataSaved, MessageBoxIcon.Information);
        }

        public static void Warning(string message, string? title = null)
        {
            Show(message, title ?? AppStrings.Warning, MessageBoxIcon.Warning);
        }

        public static bool Confirm(string message, string? title = null)
        {
            return Show(message, title ?? AppStrings.ConfirmDelete, MessageBoxIcon.Question,
                MessageBoxButtons.YesNo) == DialogResult.Yes;
        }

        private static DialogResult Show(
            string message, string title,
            MessageBoxIcon icon,
            MessageBoxButtons buttons = MessageBoxButtons.OK)
        {
            using var form = new Form
            {
                Size = new Size(400, 180),
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.CenterScreen,
                BackColor = AppColors.Surface,
                Text = title,
                Padding = new Padding(20)
            };

            using var path = new GraphicsPath();
            int r = 12;
            path.AddArc(0, 0, r * 2, r * 2, 180, 90);
            path.AddArc(form.Width - r * 2, 0, r * 2, r * 2, 270, 90);
            path.AddArc(form.Width - r * 2, form.Height - r * 2, r * 2, r * 2, 0, 90);
            path.AddArc(0, form.Height - r * 2, r * 2, r * 2, 90, 90);
            path.CloseFigure();
            form.Region = new Region(path);

            Color accent = icon switch
            {
                MessageBoxIcon.Error => AppColors.Danger,
                MessageBoxIcon.Warning => AppColors.Warning,
                MessageBoxIcon.Information => AppColors.Info,
                MessageBoxIcon.Question => AppColors.Success,
                _ => AppColors.Neutral
            };

            var accentBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 4,
                BackColor = accent
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI Semibold", 12F),
                ForeColor = accent,
                Location = new Point(20, 20),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            var lblMessage = new Label
            {
                Text = message,
                Font = new Font("Segoe UI", 10F),
                ForeColor = AppColors.Ink,
                Location = new Point(20, 55),
                AutoSize = true,
                MaximumSize = new Size(360, 0),
                BackColor = Color.Transparent
            };

            var btnOk = new ThemedButton
            {
                Text = AppStrings.Save,
                Size = new Size(100, 34),
                Location = new Point(form.Width - 140, form.Height - 60),
                Tone = icon switch
                {
                    MessageBoxIcon.Error => ButtonTone.Danger,
                    MessageBoxIcon.Warning => ButtonTone.Warning,
                    MessageBoxIcon.Question => ButtonTone.Success,
                    _ => ButtonTone.Primary
                },
                Cursor = Cursors.Hand,
                DialogResult = buttons == MessageBoxButtons.OK ? DialogResult.OK : DialogResult.Yes
            };
            btnOk.ApplyTheme();

            form.Controls.AddRange([accentBar, lblTitle, lblMessage, btnOk]);

            if (buttons == MessageBoxButtons.YesNo)
            {
                var btnNo = new ThemedButton
                {
                    Text = AppStrings.Cancel,
                    Size = new Size(100, 34),
                    Location = new Point(form.Width - 250, form.Height - 60),
                    Tone = ButtonTone.Neutral,
                    Cursor = Cursors.Hand,
                    DialogResult = DialogResult.No
                };
                btnNo.ApplyTheme();
                form.Controls.Add(btnNo);
                form.AcceptButton = btnOk;
                form.CancelButton = btnNo;
            }
            else
            {
                form.AcceptButton = btnOk;
            }

            return form.ShowDialog();
        }
    }
}
