namespace DzentoPOS2.WinForms.Models;

public sealed class UserInfoViewModel
{
    public Guid Id { get; init; }

    public string UserName { get; set; } = "";

    public string FullName { get; set; } = "";

    public string RoleName { get; set; } = "";

    public Image? Avatar { get; set; }
}