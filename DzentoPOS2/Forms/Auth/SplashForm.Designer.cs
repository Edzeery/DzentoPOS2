namespace DzentoPOS2.WinForms.Forms.Auth
{
    partial class SplashForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashForm));
            labelSplashLoading = new Label();
            pictureBox1 = new PictureBox();
            timerSplash = new System.Windows.Forms.Timer(components);
            progressBarSplash = new EZControls.Controls.Progress.EZProgressBar();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // labelSplashLoading
            // 
            labelSplashLoading.BackColor = Color.Transparent;
            labelSplashLoading.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            labelSplashLoading.ForeColor = Color.White;
            labelSplashLoading.Location = new Point(13, 119);
            labelSplashLoading.Margin = new Padding(4, 0, 4, 0);
            labelSplashLoading.Name = "labelSplashLoading";
            labelSplashLoading.Size = new Size(465, 21);
            labelSplashLoading.TabIndex = 0;
            labelSplashLoading.Text = "Loading ...";
            labelSplashLoading.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(13, 10);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(465, 100);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            // 
            // progressBarSplash
            // 
            progressBarSplash.BackColor = Color.Transparent;
            progressBarSplash.BorderColor = Color.Transparent;
            progressBarSplash.Font = new Font("Segoe UI", 10F);
            progressBarSplash.Location = new Point(89, 149);
            progressBarSplash.Name = "progressBarSplash";
            progressBarSplash.ProgressColor = Color.FromArgb(0, 139, 139);
            progressBarSplash.ProgressColor2 = Color.FromArgb(0, 192, 192);
            progressBarSplash.Size = new Size(312, 15);
            progressBarSplash.TabIndex = 3;
            progressBarSplash.Text = "ezProgressBar1";
            progressBarSplash.TextColor = Color.White;
            progressBarSplash.TrackColor = Color.FromArgb(240, 240, 240);
            // 
            // SplashForm
            // 
            AutoScaleDimensions = new SizeF(9F, 21F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(1, 39, 49);
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Center;
            ClientSize = new Size(491, 174);
            Controls.Add(progressBarSplash);
            Controls.Add(pictureBox1);
            Controls.Add(labelSplashLoading);
            DoubleBuffered = true;
            Font = new Font("Segoe UI", 12F);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(4);
            Name = "SplashForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SplashForm";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Label labelSplashLoading;
        private PictureBox pictureBox1;
        private System.Windows.Forms.Timer timerSplash;
        private EZControls.Controls.Progress.EZProgressBar progressBarSplash;
    }
}