namespace DzentoPOS2.WinForms.Events;

public enum UserMenuAction
{
    Profile,
    ChangePassword,
    Logout
}

public sealed class UserMenuEventArgs : EventArgs
{
    public UserMenuEventArgs(UserMenuAction action)
    {
        Action = action;
    }

    public UserMenuAction Action { get; }
}
