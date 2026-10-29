using DzentoPOS2.Localization.Resources;
using DzentoPOS2.WinForms.Controls;
using DzentoPOS2.WinForms.Modules.Dashboard;
using DzentoPOS2.WinForms.Services.Barcodes;
using DzentoPOS2.WinForms.Themes;
using FontAwesome.Sharp;

namespace DzentoPOS2.WinForms.Forms.Sales;

/// <summary>
/// Modal window that shows the live camera stream and reports the first
/// barcode that could be decoded. <para>
/// The capture and the decoding run on background threads (see
/// <see cref="BarcodeCameraService"/>); this window only copies the newest
/// frame into the picture box, so the user interface never freezes.
/// </para>
/// </summary>
public sealed class BarcodeScannerOverlay : Form
{
    private const int FrameIntervalMs = 33;

    private readonly BarcodeCameraService _camera = new();
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = FrameIntervalMs };
    private readonly List<CameraItem> _devices = new();

    private readonly PictureBox _preview;
    private readonly Panel _previewHost;
    private readonly Label _lblStatus;
    private readonly Label _lblHint;
    private readonly ThemedComboBox _cmbDevice;
    private readonly ThemedButton _btnRefresh;
    private readonly ThemedButton _btnClose;
    private readonly Button _btnCloseOverlay;

    private Bitmap? _frame;
    private bool _detected;

    /// <summary>Raised when a barcode was recognised.</summary>
    public event Action<string>? BarcodeDetected;

    public BarcodeScannerOverlay(string preferredDevice)
    {
        Text = AppStrings.BarcodeScanning;
        BackColor = AppColors.Background;
        Size = new Size(760, 620);
        MinimumSize = new Size(520, 460);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        KeyPreview = true;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = AppColors.Background,
            Padding = new Padding(16),
            Margin = Padding.Empty
        };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        // ── Preview ────────────────────────────────────────────────────────
        _previewHost = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AppColors.SurfaceAlt,
            Margin = new Padding(0, 0, 0, 12)
        };
        _previewHost.SetRoundedRegion(AppStyle.RadiusMedium);

        _preview = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = AppColors.SurfaceAlt
        };

        _previewHost.Controls.Add(_preview);
        root.Controls.Add(_previewHost, 0, 0);

        // ── Status ─────────────────────────────────────────────────────────
        _lblStatus = new Label
        {
            Text = AppStrings.ScanHint,
            Font = AppStyle.SubtitleFont,
            ForeColor = AppColors.Ink,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 4)
        };

        _lblHint = new Label
        {
            Text = AppStrings.ScanHint,
            Font = AppStyle.BodyFont,
            ForeColor = AppColors.InkMuted,
            AutoSize = true,
            Margin = Padding.Empty
        };

        var statusPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.Transparent,
            Margin = new Padding(0, 0, 0, 12)
        };
        statusPanel.Controls.Add(_lblStatus);
        statusPanel.Controls.Add(_lblHint);
        root.Controls.Add(statusPanel, 0, 1);

        // ── Footer ─────────────────────────────────────────────────────────
        _cmbDevice = new ThemedComboBox { Width = 260 };
        _cmbDevice.SelectedIndexChanged += (_, _) => SwitchDevice();

        _btnRefresh = ButtonFactory.Neutral(AppStrings.Refresh, ButtonFactory.ButtonSize.Medium);
        _btnRefresh.Click += (_, _) => RefreshDevices();

        _btnClose = ButtonFactory.Neutral(AppStrings.Close, ButtonFactory.ButtonSize.Medium);
        _btnClose.Click += (_, _) => Close();

        var footer = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.Transparent,
            Margin = Padding.Empty
        };
        footer.Controls.Add(_btnClose);
        footer.Controls.Add(_btnRefresh);

        var deviceLabel = new Label
        {
            Text = AppStrings.CameraDevice,
            Font = AppStyle.BodyFont,
            ForeColor = AppColors.Ink,
            AutoSize = true,
            Margin = new Padding(0, 8, 8, 0)
        };
        footer.Controls.Add(_cmbDevice);
        footer.Controls.Add(deviceLabel);
        root.Controls.Add(footer, 0, 2);

        Controls.Add(root);

        _btnCloseOverlay = new Button
        {
            Text = AppStrings.Close,
            Visible = false,
            Size = new Size(0, 0)
        };
        Controls.Add(_btnCloseOverlay);

        _camera.BarcodeDetected += OnBarcodeDetected;
        _camera.Failed += OnCameraFailed;

        _timer.Tick += (_, _) => RenderFrame();

        LoadDevices(preferredDevice);
    }

    /// <summary>Opens the camera and starts the preview.</summary>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        StartSelectedDevice();

        if (_camera.IsRunning)
        {
            _timer.Start();
            _lblStatus.Text = AppStrings.BarcodeScanning;
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _timer.Stop();
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
        catch
        {
            // The camera subsystem is not available.
        }

        if (_devices.Count == 0)
        {
            _cmbDevice.Items = new[] { AppStrings.NoCameraFound };
            int bitness = Environment.Is64BitProcess ? 64 : 32;
            _lblStatus.Text = AppStrings.NoCameraFound;
            _lblHint.Text = $"Start DroidCam Client first; the app must match its 32/64-bit filter ({bitness}-bit)";
            return;
        }

        _cmbDevice.Bind(_devices, "Display", "Moniker");

        int index = _devices.FindIndex(d =>
            string.Equals(d.Moniker, previousMoniker, StringComparison.OrdinalIgnoreCase));

        _cmbDevice.SelectedIndex = index >= 0 ? index : 0;
    }

    private void StartSelectedDevice()
    {
        _camera.Stop();

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

    private void RenderFrame()
    {
        if (!_camera.HasFrame) return;

        _frame ??= new Bitmap(1, 1);
        _camera.CopyLatestFrame(_frame);

        _preview.Image?.Dispose();
        _preview.Image = (Bitmap)_frame.Clone();
    }

    private void OnBarcodeDetected(string barcode)
    {
        if (_detected) return;
        _detected = true;

        try
        {
            System.Media.SystemSounds.Beep.Play();
        }
        catch
        {
            // The beep is cosmetic.
        }

        _lblStatus.Text = $"{AppStrings.BarcodeDetected}: {barcode}";

        BarcodeDetected?.Invoke(barcode);

        BeginInvoke(() => Close());
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
