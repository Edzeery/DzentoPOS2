using DzentoPOS2.Shared.Helpers;

namespace DzentoPOS2.WinForms.Configurations;

/// <summary>
/// Runtime access point for the user preferences. It keeps the in-memory copy
/// of the settings in sync with the persisted file, mirrors the regional
/// values into the shared <see cref="RegionalContext"/> and notifies the UI
/// when anything changed.
/// </summary>
public interface ICurrentSettingsService
{
    /// <summary>The settings currently in use.</summary>
    AppSettings Current { get; }

    /// <summary>Currency / country / time zone currently in use.</summary>
    RegionalContext Regional { get; }

    /// <summary>Raised after the settings were saved.</summary>
    event EventHandler? Changed;

    /// <summary>Persists the given settings and refreshes the runtime state.</summary>
    void Save(AppSettings settings);

    /// <summary>Mutates a copy of the current settings and persists the result.</summary>
    void Update(Action<AppSettings> mutate);
}
