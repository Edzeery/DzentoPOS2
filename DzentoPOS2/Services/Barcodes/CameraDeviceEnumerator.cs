using AForge.Video.DirectShow;

namespace DzentoPOS2.WinForms.Services.Barcodes;

/// <summary>
/// Lists the video capture devices installed on the machine.
/// Uses AForge's FilterInfoCollection which reliably enumerates DirectShow
/// video input devices including virtual cameras like DroidCam and OBS.
/// </summary>
public static class CameraDeviceEnumerator
{
    /// <summary>All cameras known to DirectShow (empty when none is installed).</summary>
    public static IReadOnlyList<CameraDeviceInfo> GetDevices()
    {
        var devices = new List<CameraDeviceInfo>();

        try
        {
            var collection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            for (int i = 0; i < collection.Count; i++)
            {
                try
                {
                    var filter = collection[i];
                    if (filter is null) continue;

                    string moniker = filter.MonikerString ?? string.Empty;
                    string name = filter.Name ?? string.Empty;
                    string description = filter.Name ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(moniker)) continue;

                    devices.Add(new CameraDeviceInfo(
                        moniker,
                        string.IsNullOrWhiteSpace(name) ? moniker : name,
                        description));
                }
                catch
                {
                    // A single broken device must not hide the other ones.
                }
            }
        }
        catch
        {
            // DirectShow is not available (no camera subsystem) - the caller
            // shows the "no camera" message instead of failing.
        }

        return devices;
    }
}
