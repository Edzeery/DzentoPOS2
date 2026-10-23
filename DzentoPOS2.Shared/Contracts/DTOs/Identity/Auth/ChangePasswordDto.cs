namespace DzentoPOS2.Shared.Contracts.DTOs.Identity.Auth
{
    public class ChangePasswordDto
    {
        public Guid UserId { get; set; }

        public string CurrentPassword { get; set; }
            = string.Empty;

        public string NewPassword { get; set; }
            = string.Empty;

        public string ConfirmPassword { get; set; }
            = string.Empty;
    }
}