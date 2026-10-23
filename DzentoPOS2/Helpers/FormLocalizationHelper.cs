using DzentoPOS2.Localization.Services.Interfaces;

namespace DzentoPOS2.WinForms.Helpers;

public static class FormLocalizationHelper
{
    public static void ApplyDirection(
        Form form,
        ILanguageService languageService)
    {
        bool isRtl = languageService.IsRightToLeft();

        form.SuspendLayout();

        form.RightToLeftLayout = isRtl;
        form.RightToLeft =
            isRtl
                ? RightToLeft.Yes
                : RightToLeft.No;

        ApplyToControls(form.Controls, isRtl);

        form.ResumeLayout(true);
    }

    private static void ApplyToControls(
        Control.ControlCollection controls,
        bool isRtl)
    {
        foreach (Control control in controls)
        {
            control.SuspendLayout();

            control.RightToLeft =
                isRtl
                    ? RightToLeft.Yes
                    : RightToLeft.No;

            if (control is FlowLayoutPanel flowPanel)
            {
                flowPanel.FlowDirection =
                    isRtl
                        ? FlowDirection.RightToLeft
                        : FlowDirection.LeftToRight;
            }

            if (control is SplitContainer splitter)
            {
                splitter.Panel1.RightToLeft =
                    isRtl
                        ? RightToLeft.Yes
                        : RightToLeft.No;
                splitter.Panel2.RightToLeft =
                    isRtl
                        ? RightToLeft.Yes
                        : RightToLeft.No;
            }

            if (control.HasChildren)
            {
                ApplyToControls(control.Controls, isRtl);
            }

            control.ResumeLayout(true);
        }
    }
}