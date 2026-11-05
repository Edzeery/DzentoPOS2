using DzentoPOS2.WinForms.Services.Barcodes;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DzentoPOS2.WinForms.Controls
{
    [ToolboxItem(true)]
    public class BarcodeScanButton : ThemedButton
    {
        private Control? _target;
        private IBarcodeScanService? _scanService;

        public BarcodeScanButton()
        {
            Text = "📷";
            Width = 32;
            Height = 28;
        }

        [Category("Behavior")]
        [Description("The text box filled with the scanned barcode.")]
        public Control? TargetTextBox
        {
            get => _target;
            set => _target = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IBarcodeScanService? ScanService
        {
            get => _scanService;
            set => _scanService = value;
        }

        public event EventHandler<string>? BarcodeScanned;

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            _ = ScanAsync();
        }

        private async Task ScanAsync()
        {
            if (_scanService == null || !_scanService.IsEnabled)
                return;

            try
            {
                IWin32Window owner = (IWin32Window?)FindForm() ?? this;
                string? value = await _scanService.ScanOnceAsync(owner, CancellationToken.None);
                if (string.IsNullOrWhiteSpace(value)) return;

                if (_target != null)
                {
                    _target.Text = value;
                    _target.Focus();
                }

                BarcodeScanned?.Invoke(this, value);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }
}