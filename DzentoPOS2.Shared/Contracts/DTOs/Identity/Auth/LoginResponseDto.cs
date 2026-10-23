namespace DzentoPOS2.Shared.Contracts.DTOs.Identity.Auth
{
    public class LoginResponseDto
    {
        public Guid UserId { get; set; }

        public string UserName { get; set; }
            = string.Empty;

        public string FullName { get; set; }
            = string.Empty;

        public string RoleName { get; set; }
            = string.Empty;

        public Guid? BranchId { get; set; }

        public string BranchName { get; set; }
            = string.Empty;

        public bool IsAdmin { get; set; }

        public DateTime LoginTime { get; set; }
    }
}