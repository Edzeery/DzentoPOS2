namespace DzentoPOS2.WinForms.Events;

public enum SettingsMenuAction
{
    OpenSettings,
    Subscription,
    Upgrade,
    Logout,
    Custom
}

public sealed class SettingsMenuEventArgs : EventArgs
{
    public SettingsMenuEventArgs(SettingsMenuAction action, string? customActionKey = null)
    {
        Action = action;
        CustomActionKey = customActionKey;
    }

    public SettingsMenuAction Action { get; }

    public string? CustomActionKey { get; }
}
