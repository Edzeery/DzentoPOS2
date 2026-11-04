using DzentoPOS2.WinForms.Configurations;
using DzentoPOS2.WinForms.Forms.Sales;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DzentoPOS2.WinForms.Services.Barcodes
{
    public sealed class BarcodeScanService : IBarcodeScanService
    {
        private readonly ICurrentSettingsService _settings;
        private readonly SemaphoreSlim _gate = new(1, 1);

        public BarcodeScanService(ICurrentSettingsService settings)
        {
            _settings = settings;
        }

        public bool IsEnabled => _settings.Current.CameraBarcodeEnabled;

        public async Task<string?> ScanOnceAsync(IWin32Window owner, CancellationToken ct = default)
        {
            if (!IsEnabled) return null;

            await _gate.WaitAsync(ct);
            try
            {
                using var overlay = new BarcodeScannerOverlay(_settings.Current.BarcodeCameraDevice);
                // apply saved source/url
                if (_settings.Current.BarcodeCameraSource == "Ip")
                {
                    // overlay handles source via its own controls; set after load
                }
                var tcs = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
                overlay.BarcodeDetected += v => tcs.TrySetResult(v);
                overlay.FormClosed += (_, _) => tcs.TrySetResult(null);

                overlay.ShowDialog(owner);
                ct.ThrowIfCancellationRequested();
                return await tcs.Task;
            }
            finally
            {
                _gate.Release();
            }
        }
    }
}