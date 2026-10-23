using DzentoPOS2.Localization.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DzentoPOS2.WinForms.Forms.Auth
{
    public partial class SplashForm : Form
    {
        public event EventHandler? InitializationCompleted;
        public SplashForm()
        {
            InitializeComponent();
            timerSplash.Enabled = true; 
        }
        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);

            await InitializeApplicationAsync();
        }
        private async Task InitializeApplicationAsync()
        {
            await UpdateProgressAsync(20, AppStrings.Loadingconfiguration);

            await Task.Delay(300);

            await UpdateProgressAsync(50, AppStrings.Connectingdatabase);

            // 🔥 تحقق حقيقي
            // مثلا DbContext.Database.CanConnectAsync()

            await Task.Delay(300);

            await UpdateProgressAsync(80, AppStrings.PreparingServices);

            await Task.Delay(300);

            await UpdateProgressAsync(100, AppStrings.Ready);

            await Task.Delay(500);

            InitializationCompleted?.Invoke(this, EventArgs.Empty);

            Close();
        }
        private async Task UpdateProgressAsync(
            int value,
            string message)
        {
            progressBarSplash.Value = value;

            labelSplashLoading.Text = message;

            await Task.Delay(200);
        }

        //private void timerSplash_Tick(object sender, EventArgs e)
        //{

           
        //    progressBarSplash.Increment(2);
        //    labelSplashLoading.Text = "Loading..." + progressBarSplash.Value + "%";
        //    if (progressBarSplash.Value >= 100)
        //    {
        //        this.Close();
        //    }

        //}
    }
}
