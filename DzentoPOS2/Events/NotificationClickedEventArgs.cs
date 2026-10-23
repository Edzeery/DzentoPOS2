using DzentoPOS2.WinForms.Models;

namespace DzentoPOS2.WinForms.Events;

public sealed class NotificationClickedEventArgs : EventArgs
{
    public NotificationClickedEventArgs(NotificationItem? notification)
    {
        Notification = notification;
    }

    public NotificationItem? Notification { get; }
}
