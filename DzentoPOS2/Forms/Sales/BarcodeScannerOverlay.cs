using DzentoPOS2.App.Services.Barcodes;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.WinForms.Controls;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Modules.Dashboard;
using DzentoPOS2.WinForms.Services;
using DzentoPOS2.WinForms.Services.Barcodes;
using DzentoPOS2.WinForms.Themes;
using FontAwesome.Sharp;
using System.Drawing.Imaging;

namespace DzentoPOS2.WinForms.Forms.Sales;

/// <summary>
/// Modal window that shows the live camera stream and reports the first
/// barcode that could be decoded. <para>
/// The capture and the decoding run on background threads (see
/// <see cref="BarcodeCameraService"/>); this window only copies the newest
/// frame into the picture box, so the user interface never freezes.
/// </para>
/// </summary>
public sealed partial class BarcodeScannerOverlay : Form
{
    private const int FrameIntervalMs = 33;

    private readonly BarcodeCameraService _camera = new();
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = FrameIntervalMs };
    private readonly List<CameraItem> _devices = new();

    private PictureBox _preview;
    private Panel _previewHost;
    private Label _lblStatus;
    private Label _lblHint;
    private ThemedComboBox _cmbDevice;
    private ThemedComboBox _cmbSource;
    private TextBox _txtCameraUrl;
    private ThemedButton _btnRefresh;
    private ThemedButton _btnTestConnection;
    private ThemedButton _btnClose;
    private Button _btnCloseOverlay;

    private Bitmap? _frame;
    private int _detected;
    private Panel? _aimFrame;
    private TextBox? _txtManual;
    private ThemedButton? _btnManualOk;
    private CheckBox? _chkContinuous;
    private System.Windows.Forms.Timer? _animTimer;
    private int _scanLineY;
    private bool _isIpSource;
    private readonly System.Diagnostics.Stopwatch _sw = System.Diagnostics.Stopwatch.StartNew();
    private int _closeRequested;
    private bool _continuousScan;

    [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    /// <summary>Raised when a barcode was recognised.</summary>
    public event Action<string>? BarcodeDetected;

    public BarcodeScannerOverlay(string preferredDevice)
    {
        InitializeComponent();

        Text = AppStrings.BarcodeScanning;
        BackColor = AppColors.Background;

        // Aim frame overlay on preview
        _aimFrame = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        _aimFrame.Paint += AimFrame_Paint;
        _preview.Controls.Add(_aimFrame);
        _aimFrame.BringToFront();

        // Manual input + continuous scan
        _txtManual = new TextBox { Width = 200, Font = AppStyle.BodyFont };
        _btnManualOk = ButtonFactory.Neutral(AppStrings.Ok, ButtonFactory.ButtonSize.Small);
        _btnManualOk.Click += (_, _) =>
        {
            if (!string.IsNullOrWhiteSpace(_txtManual.Text))
            {
                OnBarcodeDetected(_txtManual.Text.Trim());
                _txtManual.Clear();
            }
        };
        _chkContinuous = new CheckBox { Text = AppStrings.ContinuousScan, AutoSize = true, ForeColor = AppColors.Ink };

        // Add to status panel
        var manualPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, BackColor = Color.Transparent };
        manualPanel.Controls.Add(new Label { Text = AppStrings.ManualEntry, AutoSize = true, ForeColor = AppColors.Ink, Margin = new Padding(0, 6, 6, 0) });
        manualPanel.Controls.Add(_txtManual);
        manualPanel.Controls.Add(_btnManualOk);
        manualPanel.Controls.Add(_chkContinuous);
        // find statusPanel
        var sp = Controls.Find("statusPanel", true).OfType<FlowLayoutPanel>().FirstOrDefault();
        sp?.Controls.Add(manualPanel);

        _animTimer = new System.Windows.Forms.Timer { Interval = 30 };
        _animTimer.Tick += (_, _) => { _scanLineY = (_scanLineY + 4) % 200; _aimFrame?.Invalidate(); };

        // Root fix: ensure designer-created buttons exist (VS designer may drop
        // factory-created controls). Create them here if missing.
        var footer = Controls.Find("footer", true).OfType<FlowLayoutPanel>().FirstOrDefault();
        if (_btnTestConnection == null)
        {
            _btnTestConnection = ButtonFactory.Neutral(AppStrings.TestConnection, ButtonFactory.ButtonSize.Small);
            footer?.Controls.Add(_btnTestConnection);
        }
        if (_btnRefresh == null)
        {
            _btnRefresh = ButtonFactory.Neutral(AppStrings.Refresh, ButtonFactory.ButtonSize.Medium);
            footer?.Controls.Add(_btnRefresh);
        }
        if (_btnClose == null)
        {
            _btnClose = ButtonFactory.Neutral(AppStrings.Close, ButtonFactory.ButtonSize.Medium);
            footer?.Controls.Add(_btnClose);
        }

        // Wire events after designer creation.
        _cmbSource.SelectedIndexChanged += (_, _) => UpdateSourceVisibility();
        _cmbDevice.SelectedIndexChanged += (_, _) => SwitchDevice();
        _txtCameraUrl.TextChanged += (_, _) => ValidateCameraUrl();
        _btnTestConnection.Click += async (_, _) => await TestCameraConnectionAsync();
        _btnRefresh.Click += (_, _) => RefreshDevices();

        _cmbSource.ClearItems();
        _cmbSource.Add(AppStrings.CameraSourceLocal, AppStrings.CameraSourceIp);
        _cmbSource.SelectedIndex = 0;

        _camera.BarcodeDetected += OnBarcodeDetected;
        _camera.Failed += OnCameraFailed;

        _timer.Tick += (_, _) => RenderFrame();

        LoadDevices(preferredDevice);
        UpdateSourceVisibility();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        try
        {
            int dark = 1;
            DwmSetWindowAttribute(Handle, 20, ref dark, sizeof(int));
        }
        catch
        {
            // Dark title bar is cosmetic.
        }
    }

    /// <summary>Opens the camera and starts the preview.</summary>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        StartSelectedDevice();

        _timer.Start();
        _animTimer?.Start();
        if (_camera.IsRunning)
            _lblStatus.Text = AppStrings.BarcodeScanning;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _timer.Stop();
        _animTimer?.Stop();
        _animTimer?.Dispose();
        _camera.BarcodeDetected -= OnBarcodeDetected;
        _camera.Failed -= OnCameraFailed;
        _camera.Dispose();

        _preview.Image?.Dispose();
        _preview.Image = null;
        _frame?.Dispose();
        _frame = null;

        base.OnFormClosed(e);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape)
        {
            Close();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    // ── Devices ─────────────────────────────────────────────────────────────

    private void LoadDevices(string preferredDevice)
    {
        string? previousMoniker = _cmbDevice.SelectedItem is CameraItem current && !string.IsNullOrWhiteSpace(current.Moniker) ? current.Moniker : preferredDevice;
        _devices.Clear();

        try
        {
            _devices.AddRange(BarcodeCameraService.GetDevices()
                .Select(d => new CameraItem(d.Moniker, d.Name)));
        }
        catch (Exception ex)
        {
            // The camera subsystem is not available.
            System.Diagnostics.Debug.WriteLine($"LoadDevices: {ex.Message}");
        }

        if (_devices.Count == 0)
        {
            _cmbDevice.ClearItems();
            _cmbDevice.Add(AppStrings.NoCameraFound);
            int bitness = Environment.Is64BitProcess ? 64 : 32;
            _lblStatus.Text = AppStrings.NoCameraFound;
            _lblHint.Text = string.Format(AppStrings.CameraDriverBitnessHint, bitness);
            return;
        }

        _cmbDevice.Bind(_devices, "Display", "Moniker");

        int index = _devices.FindIndex(d =>
            string.Equals(d.Moniker, previousMoniker, StringComparison.OrdinalIgnoreCase));

        _cmbDevice.SelectedIndex = index >= 0 ? index : 0;
    }

    private void UpdateSourceVisibility()
    {
        _isIpSource = _cmbSource.SelectedIndex == 1;
        _txtCameraUrl.Visible = _isIpSource;
        _btnTestConnection.Visible = _isIpSource;
        _cmbDevice.Enabled = !_isIpSource;
        _btnRefresh.Enabled = !_isIpSource;
        ValidateCameraUrl();
    }

    private void ValidateCameraUrl()
    {
        if (_txtCameraUrl.Visible)
        {
            bool valid = CameraUrlValidator.IsValid(_txtCameraUrl.Text);
            _txtCameraUrl.BackColor = valid ? Color.White : Color.MistyRose;
        }
    }

    private async Task TestCameraConnectionAsync()
    {
        string url = _txtCameraUrl.Text.Trim();
        if (!CameraUrlValidator.IsValid(url))
        {
            AppMessageBox.Error(AppStrings.CameraUrlInvalid);
            return;
        }

        _btnTestConnection.Enabled = false;
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var result = await Task.Run(() =>
            {
                try
                {
                    using var client = new System.Net.Http.HttpClient
                    {
                        Timeout = TimeSpan.FromSeconds(5)
                    };
                    var response = client.GetAsync(url, cts.Token).GetAwaiter().GetResult();
                    return response.IsSuccessStatusCode;
                }
                catch
                {
                    return false;
                }
            }, cts.Token);

            if (result)
                AppMessageBox.Success(AppStrings.ConnectionSuccess);
            else
                AppMessageBox.Error(AppStrings.ConnectionFailed);
        }
        catch (Exception ex)
        {
            AppMessageBox.Error(ex.Message);
        }
        finally
        {
            if (!IsDisposed)
                _btnTestConnection.Enabled = true;
        }
    }

    private void StartSelectedDevice()
    {
        _camera.Stop();

        if (_isIpSource)
        {
            string url = _txtCameraUrl.Text.Trim();
            if (string.IsNullOrWhiteSpace(url) || !CameraUrlValidator.IsValid(url))
            {
                _lblStatus.Text = AppStrings.CameraUrlInvalid;
                return;
            }

            if (!_camera.Start(url, out string ipError))
            {
                _lblStatus.Text = string.IsNullOrWhiteSpace(ipError)
                    ? AppStrings.NoCameraFound
                    : ipError;
            }
            return;
        }

        if (_cmbDevice.SelectedItem is not CameraItem device) return;
        if (string.IsNullOrWhiteSpace(device.Moniker)) return;

        if (!_camera.Start(device.Moniker, out string error))
        {
            _lblStatus.Text = string.IsNullOrWhiteSpace(error)
                ? AppStrings.NoCameraFound
                : error;
        }
    }

    private void SwitchDevice()
    {
        if (!IsHandleCreated) return;
        StartSelectedDevice();
    }

    private void RefreshDevices()
    {
        string? preferred = _cmbDevice.SelectedItem is CameraItem c ? c.Moniker : null;
        LoadDevices(preferred ?? string.Empty);
        StartSelectedDevice();
        if (_camera.IsRunning)
        {
            _timer.Start();
            _lblStatus.Text = AppStrings.BarcodeScanning;
        }
    }

    // ── Rendering ───────────────────────────────────────────────────────────

    private void AimFrame_Paint(object? sender, PaintEventArgs e)
    {
        if (_aimFrame == null) return;
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        int w = _aimFrame.Width;
        int h = _aimFrame.Height;
        int rw = (int)(w * 0.7);
        int rh = (int)(h * 0.5);
        int x = (w - rw) / 2;
        int y = (h - rh) / 2;
        var rect = new Rectangle(x, y, rw, rh);
        using var pen = new Pen(AppColors.Primary, 3);
        int corner = 24;
        // corners
        g.DrawLine(pen, x, y, x + corner, y);
        g.DrawLine(pen, x, y, x, y + corner);
        g.DrawLine(pen, x + rw, y, x + rw - corner, y);
        g.DrawLine(pen, x + rw, y, x + rw, y + corner);
        g.DrawLine(pen, x, y + rh, x + corner, y + rh);
        g.DrawLine(pen, x, y + rh, x, y + rh - corner);
        g.DrawLine(pen, x + rw, y + rh, x + rw - corner, y + rh);
        g.DrawLine(pen, x + rw, y + rh, x + rw, y + rh - corner);
        // scan line
        int ly = y + (_scanLineY % rh);
        using var linePen = new Pen(Color.FromArgb(180, AppColors.Primary), 2);
        g.DrawLine(linePen, x + 4, ly, x + rw - 4, ly);
    }

    private void RenderFrame()
    {
        if (!_camera.HasFrame) return;

        var size = _camera.GetLatestFrameSize();
        if (size.IsEmpty) return;

        if (_frame == null || _frame.Size != size)
        {
            _frame?.Dispose();
            _frame = new Bitmap(size.Width, size.Height, PixelFormat.Format24bppRgb);
        }

        _camera.CopyLatestFrame(_frame);

        var img = (Bitmap)_frame.Clone();
        var previous = _preview.Image;
        _preview.Image = img;
        previous?.Dispose();
    }

    private void OnBarcodeDetected(string barcode)
    {
        if (System.Threading.Interlocked.Exchange(ref _detected, 1) == 1)
        {
            return;
        }


        BeginInvoke(() =>
        {
            if (IsDisposed) return;

            try { System.Media.SystemSounds.Beep.Play(); }
            catch { /* cosmetic */ }

            _continuousScan = _chkContinuous?.Checked == true;
            _lblStatus.Text = $"{AppStrings.BarcodeDetected}: {barcode}";
            _timer.Stop();


            BarcodeDetected?.Invoke(barcode);

            if (!_continuousScan)
            {
                _ = System.Threading.Tasks.Task.Run(() =>
                {
                    try { _camera.Stop(); } catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
                });
                BeginInvoke(() => { if (!IsDisposed) Close(); });
            }
        });
    }

    private void OnCameraFailed(string error)
    {
        if (IsDisposed) return;

        try
        {
            BeginInvoke(() =>
            {
                if (IsDisposed) return;
                _lblStatus.Text = string.IsNullOrWhiteSpace(error)
                    ? AppStrings.NoCameraFound
                    : error;
                _lblHint.Text = AppStrings.CameraDevice;
            });
        }
        catch
        {
            // The window may already be closing.
        }
    }

    private sealed record CameraItem(string Moniker, string Display);
}
