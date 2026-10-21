namespace DzentoPOS2.WinForms.Models;

public sealed class NotificationItem
{
    public Guid Id { get; set; }

    public string Title { get; set; } = "";

    public string Message { get; set; } = "";

    public DateTime Date { get; set; }

    public bool IsRead { get; set; }
}