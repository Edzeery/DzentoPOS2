using DzentoPOS2.WinForms.Themes;
using System; 
namespace DzentoPOS2.WinForms.ServicesUI
{
    public static class FormTransition
    {
        public static void FadeIn(Form form)
        {
            System.Windows.Forms. Timer timer = new System.Windows.Forms.Timer();

            form.Opacity = 0;

            timer.Interval = AppStyle.AnimationInterval;


            timer.Tick += (s, e) =>
            {
                if (form.Opacity >= 1)
                {
                    timer.Stop();
                    timer.Dispose();
                }
                else
                {
                    form.Opacity += 0.08;
                }
            };

            timer.Start();
        }
    }
}
