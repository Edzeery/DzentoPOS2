using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ZXing;
using ZXing.Common;

namespace DzentoPOS2.WinForms.Services.Barcodes;

/// <summary>
/// Reads barcodes from a camera.
/// <para>
/// The capture runs on the DirectShow worker thread and the decoding runs on a
/// dedicated thread, so the user interface is never blocked. Only the newest
/// frame is decoded (older frames are dropped), which keeps the latency low
/// even on weak machines.
/// </para>
/// </summary>
public sealed class BarcodeCameraService : IDisposable
{
    private const int MaxFrameWidth = 800;
    private const int MaxFrameHeight = 600;
    private const int MinRepeatDelayMs = 1500;

    private readonly BarcodeReaderGeneric _reader;
    private readonly AutoResetEvent _frameAvailable = new(false);
    private readonly object _sync = new();

    private VideoCaptureDevice? _device;
    private Thread? _decodeThread;
    private Bitmap? _currentFrame;
    private volatile bool _running;
    private string _lastBarcode = string.Empty;
    private DateTime _lastBarcodeAt = DateTime.MinValue;
    private bool _disposed;

    /// <summary>Raised (on the decoding thread) when a barcode was recognised.</summary>
    public event Action<string>? BarcodeDetected;

    /// <summary>Raised (on the decoding thread) when the camera cannot be used.</summary>
    public event Action<string>? Failed;

    public BarcodeCameraService()
    {
        _reader = new BarcodeReaderGeneric
        {
            AutoRotate = false,
            Options = new DecodingOptions
            {
                TryHarder = false,
                PureBarcode = false,
                TryInverted = false,
                PossibleFormats = new List<BarcodeFormat>
                {
                    BarcodeFormat.EAN_13,
                    BarcodeFormat.EAN_8,
                    BarcodeFormat.UPC_A,
                    BarcodeFormat.UPC_E,
                    BarcodeFormat.CODE_128,
                    BarcodeFormat.CODE_39,
                    BarcodeFormat.ITF,
                    BarcodeFormat.QR_CODE,
                    BarcodeFormat.DATA_MATRIX
                }
            }
        };
    }

    /// <summary>True while the camera is streaming.</summary>
    public bool IsRunning => _running;

    /// <summary>All cameras installed on the machine.</summary>
    public static IReadOnlyList<CameraDeviceInfo> GetDevices() => CameraDeviceEnumerator.GetDevices();

    /// <summary>
    /// Opens the camera and starts capturing. Returns false with a message when
    /// the device cannot be opened.
    /// </summary>
    public bool Start(string moniker, out string error)
    {
        error = string.Empty;

        if (_running) return true;

        if (string.IsNullOrWhiteSpace(moniker))
        {
            error = "no-camera";
            return false;
        }

        try
        {
            var device = new VideoCaptureDevice(moniker);
            TrySetResolution(device, 640, 480);

            device.NewFrame += OnNewFrame;
            device.VideoSourceError += OnVideoSourceError;

            _device = device;
            device.Start();

            _running = true;
            _decodeThread = new Thread(DecodeLoop)
            {
                IsBackground = true,
                Name = "BarcodeDecoder"
            };
            _decodeThread.Start();

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            Stop();
            return false;
        }
    }

    /// <summary>Stops the capture and releases the camera.</summary>
    public void Stop()
    {
        _running = false;

        try
        {
            if (_device is not null)
            {
                _device.NewFrame -= OnNewFrame;
                _device.VideoSourceError -= OnVideoSourceError;
                _device.Stop();
                _device = null;
            }
        }
        catch
        {
            // The device may already be gone.
        }

        try
        {
            _frameAvailable.Set();
            _decodeThread?.Join(500);
        }
        catch
        {
            // Ignore: the thread is a background thread.
        }

        _decodeThread = null;

        lock (_sync)
        {
            _currentFrame?.Dispose();
            _currentFrame = null;
        }
    }

    /// <summary>
    /// Copies the newest frame into the given bitmap. Safe to call from the
    /// user interface thread.
    /// </summary>
    public void CopyLatestFrame(Bitmap target)
    {
        lock (_sync)
        {
            if (_currentFrame is null) return;

            using var graphics = Graphics.FromImage(target);
            graphics.DrawImageUnscaled(_currentFrame, 0, 0);
        }
    }

    /// <summary>True when a frame is available.</summary>
    public bool HasFrame
    {
        get
        {
            lock (_sync)
            {
                return _currentFrame is not null;
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Stop();
        _frameAvailable.Dispose();
    }

    private void OnNewFrame(object? sender, NewFrameEventArgs e)
    {
        try
        {
            var frame = Scale(e.Frame);

            lock (_sync)
            {
                var previous = _currentFrame;
                _currentFrame = frame;
                previous?.Dispose();
            }
        }
        catch
        {
            // A dropped frame is not a problem.
        }
    }

    private void OnVideoSourceError(object? sender, VideoSourceErrorEventArgs e)
    {
        _running = false;
        Failed?.Invoke(e.Description ?? "camera-error");
    }

    private void DecodeLoop()
    {
        while (_running)
        {
            try
            {
                _frameAvailable.WaitOne(200);
            }
            catch
            {
                break;
            }

            if (!_running) break;

            Bitmap? snapshot = null;

            lock (_sync)
            {
                if (_currentFrame is not null)
                    snapshot = (Bitmap)_currentFrame.Clone();
            }

            if (snapshot is null) continue;

            try
            {
                string? text = Decode(snapshot);
                if (!string.IsNullOrWhiteSpace(text) && ShouldReport(text))
                    BarcodeDetected?.Invoke(text);
            }
            catch
            {
                // Decoding errors are expected (no barcode in the frame).
            }
            finally
            {
                snapshot.Dispose();
            }
        }
    }

    private string? Decode(Bitmap bitmap)
    {
        var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

        try
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            int stride = data.Stride;
            byte[] buffer = new byte[Math.Abs(stride) * height];

            for (int y = 0; y < height; y++)
            {
                IntPtr row = IntPtr.Add(data.Scan0, y * stride);
                Marshal.Copy(row, buffer, y * Math.Abs(stride), Math.Abs(stride));
            }

            var source = new RGBLuminanceSource(
                buffer,
                width,
                height,
                RGBLuminanceSource.BitmapFormat.BGR24);

            var result = _reader.Decode(source);
            return result?.Text;
        }
        finally
        {
            bitmap.UnlockBits(data);
        }
    }

    private bool ShouldReport(string text)
    {
        var now = DateTime.UtcNow;

        if (string.Equals(text, _lastBarcode, StringComparison.Ordinal))
        {
            if ((now - _lastBarcodeAt).TotalMilliseconds < MinRepeatDelayMs)
                return false;
        }

        _lastBarcode = text;
        _lastBarcodeAt = now;
        return true;
    }

    private static void TrySetResolution(VideoCaptureDevice device, int width, int height)
    {
        try
        {
            foreach (var capability in device.VideoCapabilities)
            {
                if (capability.FrameSize.Width == width && capability.FrameSize.Height == height)
                {
                    device.VideoResolution = capability;
                    return;
                }
            }
        }
        catch
        {
            // The camera may not expose its capabilities.
        }
    }

    private static Bitmap Scale(Bitmap source)
    {
        int width = source.Width;
        int height = source.Height;

        if (width > MaxFrameWidth || height > MaxFrameHeight)
        {
            double factor = Math.Min(
                (double)MaxFrameWidth / width,
                (double)MaxFrameHeight / height);

            width = Math.Max(1, (int)(width * factor));
            height = Math.Max(1, (int)(height * factor));
        }

        var target = new Bitmap(width, height, PixelFormat.Format24bppRgb);

        using (var graphics = Graphics.FromImage(target))
        {
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
            graphics.DrawImage(source, new Rectangle(0, 0, width, height));
        }

        return target;
    }
}
