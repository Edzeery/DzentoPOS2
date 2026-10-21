using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace DzentoPOS2.WinForms.Services.Barcodes;

/// <summary>
/// Lists the video capture devices installed on the machine.
/// The enumeration goes through the DirectShow device enumerator, therefore it
/// only ever returns devices that can really be opened by <c>VideoCaptureDevice</c>.
/// </summary>
public static class CameraDeviceEnumerator
{
    private static readonly Guid ClsidDeviceEnumerator =
        new("29840822-5B84-11CF-BBD3-00AA00A1CE5C");

    private static readonly Guid IidDeviceEnumerator =
        new("B9A59933-B8F9-4F70-8C39-64F5D1976EA4");

    /// <summary>All cameras known to DirectShow (empty when none is installed).</summary>
    public static IReadOnlyList<CameraDeviceInfo> GetDevices()
    {
        var devices = new List<CameraDeviceInfo>();

        try
        {
            var type = Type.GetTypeFromCLSID(ClsidDeviceEnumerator);
            if (type is null) return devices;

            var instance = Activator.CreateInstance(type);
            if (instance is not IDirectShowDeviceEnumerator enumerator)
                return devices;

            Guid category = AForge.Video.DirectShow.FilterCategory.VideoInputDevice;
            if (enumerator.EnumDevices(ref category, out IEnumMoniker? monikers, 0) != 0 || monikers is null)
                return devices;

            var buffer = new IMoniker[1];
            IntPtr fetched = Marshal.AllocCoTaskMem(sizeof(int));

            try
            {
                while (monikers.Next(1, buffer, fetched) == 0)
                {
                    IMoniker? moniker = buffer[0];
                    if (moniker is null) continue;

                    try
                    {
                        string monikerPath = string.Empty;
                        moniker.GetDisplayName(null!, null!, out monikerPath!);
                        if (string.IsNullOrWhiteSpace(monikerPath)) continue;

                        string friendlyName = ReadProperty(moniker, "FriendlyName");
                        string description = ReadProperty(moniker, "Description");

                        devices.Add(new CameraDeviceInfo(
                            monikerPath,
                            string.IsNullOrWhiteSpace(friendlyName) ? monikerPath : friendlyName,
                            description));
                    }
                    catch
                    {
                        // A single broken device must not hide the other ones.
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(moniker);
                        buffer[0] = null!;
                    }
                }
            }
            finally
            {
                Marshal.FreeCoTaskMem(fetched);
                Marshal.ReleaseComObject(monikers);
            }
        }
        catch
        {
            // DirectShow is not available (no camera subsystem) - the caller
            // shows the "no camera" message instead of failing.
        }

        return devices;
    }

    private static string ReadProperty(IMoniker moniker, string propertyName)
    {
        try
        {
            Guid iid = typeof(IPropertyBag).GUID;
            moniker.BindToStorage(null!, null!, ref iid, out object? bag);

            if (bag is not IPropertyBag propertyBag) return string.Empty;

            try
            {
                if (propertyBag.Get(propertyName, out object? value) == 0 && value is string text)
                    return text;
            }
            finally
            {
                Marshal.ReleaseComObject(propertyBag);
            }
        }
        catch
        {
            // The property bag is optional.
        }

        return string.Empty;
    }

    [ComImport]
    [Guid("B9A59933-B8F9-4F70-8C39-64F5D1976EA4")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IDirectShowDeviceEnumerator
    {
        [PreserveSig]
        int EnumDevices(
            [In] ref Guid category,
            [Out, MarshalAs(UnmanagedType.Interface)] out IEnumMoniker? enumerator,
            [MarshalAs(UnmanagedType.U4)] uint flags);
    }

    /// <summary>
    /// Minimal IPropertyBag declaration - the runtime does not ship a public
    /// one, and it is only used to read the friendly name of a device.
    /// </summary>
    [ComImport]
    [Guid("0000000C-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IPropertyBag
    {
        [PreserveSig]
        int Get(
            [MarshalAs(UnmanagedType.LPWStr)] string pszPropName,
            [MarshalAs(UnmanagedType.Struct)] out object pPropVar);

        [PreserveSig]
        int GetProperty(
            [MarshalAs(UnmanagedType.LPWStr)] string pszPropName,
            [MarshalAs(UnmanagedType.Struct)] out object pPropVar);

        [PreserveSig]
        int Put(
            [MarshalAs(UnmanagedType.LPWStr)] string pszPropName,
            [MarshalAs(UnmanagedType.Struct)] ref object pPropVar);

        [PreserveSig]
        int PutProperty(
            [MarshalAs(UnmanagedType.LPWStr)] string pszPropName,
            [MarshalAs(UnmanagedType.Struct)] ref object pPropVar);
    }
}
