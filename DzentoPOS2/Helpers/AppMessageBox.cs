using DzentoPOS2.App.Diagnostics;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.WinForms.Controls;
using DzentoPOS2.WinForms.Themes;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace DzentoPOS2.WinForms.Helpers
{
    public static class AppMessageBox
    {
        public static void Error(string message, string? title = null)
        {
            Show(message, title ?? AppStrings.ErrorMsg, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Shows a friendly, localized message for the exception's category,
        /// writes the full exception to the rolling file log and offers a
        /// collapsible "Details" view with a copy button. The raw provider
        /// message is never shown as the headline.
        /// </summary>
        public static void Error(Exception exception, string? context = null)
        {
            if (exception is null)
            {
                Error(AppStrings.ErrorUnknown);
                return;
            }

            var logPath = ExceptionLogger.Write(exception, context);
            ShowError(exception, logPath);
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

            ApplyRoundedRegion(form, 12);

            Color accent = ResolveAccent(icon);

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
                // The dialog's confirm button must read OK / موافق, never "Save".
                Text = AppStrings.Ok,
                Size = new Size(100, 34),
                Location = new Point(form.Width - 140, form.Height - 60),
                Tone = ResolveTone(icon),
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

        private static void ShowError(Exception exception, string? logPath)
        {
            const int collapsedHeight = 230;
            const int detailsHeight = 420;

            var friendly = ExceptionFormatter.FriendlyMessage(exception);
            var details = ExceptionFormatter.BuildDetails(exception);

            using var form = new Form
            {
                Size = new Size(560, collapsedHeight),
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.CenterScreen,
                BackColor = AppColors.Surface,
                Text = AppStrings.ErrorMsg,
                Padding = new Padding(20),
                RightToLeft = IsRightToLeft() ? RightToLeft.Yes : RightToLeft.No
            };

            ApplyRoundedRegion(form, 12);

            var accent = AppColors.Danger;

            var accentBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 4,
                BackColor = accent
            };

            var lblTitle = new Label
            {
                Text = AppStrings.ErrorMsg,
                Font = new Font("Segoe UI Semibold", 12F),
                ForeColor = accent,
                Location = new Point(20, 18),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            var lblMessage = new Label
            {
                Text = friendly,
                Font = new Font("Segoe UI", 10F),
                ForeColor = AppColors.Ink,
                Location = new Point(20, 50),
                AutoSize = true,
                MaximumSize = new Size(510, 0),
                BackColor = Color.Transparent
            };

            var lblLog = new Label
            {
                Text = logPath is null ? string.Empty : AppStrings.ErrorLoggedToFile,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = AppColors.InkMuted,
                Location = new Point(20, 110),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            var detailsBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Font = new Font("Consolas", 9F),
                BackColor = AppColors.SurfaceAlt,
                ForeColor = AppColors.Ink,
                BorderStyle = BorderStyle.FixedSingle,
                Text = details,
                Visible = false,
                Location = new Point(20, 140),
                Size = new Size(520, detailsHeight - 230),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            var btnOk = new ThemedButton
            {
                Text = AppStrings.Ok,
                Size = new Size(100, 34),
                Location = new Point(440, collapsedHeight - 60),
                Tone = ButtonTone.Danger,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.OK,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnOk.ApplyTheme();

            var btnDetails = new ThemedButton
            {
                Text = AppStrings.ErrorDetails,
                Size = new Size(120, 34),
                Location = new Point(20, collapsedHeight - 60),
                Tone = ButtonTone.Neutral,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnDetails.ApplyTheme();

            var btnCopy = new ThemedButton
            {
                Text = AppStrings.ErrorCopyDetails,
                Size = new Size(120, 34),
                Location = new Point(150, collapsedHeight - 60),
                Tone = ButtonTone.Neutral,
                Cursor = Cursors.Hand,
                Visible = false,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnCopy.ApplyTheme();

            btnDetails.Click += (_, _) =>
            {
                var expand = !detailsBox.Visible;
                detailsBox.Visible = expand;
                btnCopy.Visible = expand;
                form.Height = expand ? detailsHeight : collapsedHeight;
                btnDetails.Text = expand ? AppStrings.Close : AppStrings.ErrorDetails;
            };

            btnCopy.Click += (_, _) =>
            {
                try
                {
                    Clipboard.SetText(details);
                    btnCopy.Text = AppStrings.ErrorDetailsCopied;
                }
                catch
                {
                    // Clipboard can be busy; ignore.
                }
            };

            form.Controls.AddRange(
                [accentBar, lblTitle, lblMessage, lblLog, detailsBox, btnDetails, btnCopy, btnOk]);

            form.AcceptButton = btnOk;
            form.CancelButton = btnOk;

            form.ShowDialog();
        }

        private static Color ResolveAccent(MessageBoxIcon icon) => icon switch
        {
            MessageBoxIcon.Error => AppColors.Danger,
            MessageBoxIcon.Warning => AppColors.Warning,
            MessageBoxIcon.Information => AppColors.Info,
            MessageBoxIcon.Question => AppColors.Success,
            _ => AppColors.Neutral
        };

        private static ButtonTone ResolveTone(MessageBoxIcon icon) => icon switch
        {
            MessageBoxIcon.Error => ButtonTone.Danger,
            MessageBoxIcon.Warning => ButtonTone.Warning,
            MessageBoxIcon.Question => ButtonTone.Success,
            _ => ButtonTone.Primary
        };

        private static bool IsRightToLeft()
            => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
                .Equals("ar", StringComparison.OrdinalIgnoreCase);

        private static void ApplyRoundedRegion(Form form, int radius)
        {
            using var path = new GraphicsPath();
            path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
            path.AddArc(form.Width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
            path.AddArc(form.Width - radius * 2, form.Height - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(0, form.Height - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            form.Region = new Region(path);
        }
    }
}