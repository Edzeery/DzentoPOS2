namespace DzentoPOS2.WinForms.Services.Barcodes;

/// <summary>
/// Description of a camera that can be used to read barcodes.
/// </summary>
/// <param name="Moniker">DirectShow moniker (the technical identity of the device).</param>
/// <param name="Name">Friendly name shown in the user interface.</param>
/// <param name="Description">Optional device description.</param>
public sealed class CameraDeviceInfo
{
    public CameraDeviceInfo(string moniker, string name, string description)
    {
        Moniker = moniker ?? string.Empty;
        Name = name ?? string.Empty;
        Description = description ?? string.Empty;
    }

    public string Moniker { get; }

    public string Name { get; }

    public string Description { get; }

    public override string ToString() => Name;
}
