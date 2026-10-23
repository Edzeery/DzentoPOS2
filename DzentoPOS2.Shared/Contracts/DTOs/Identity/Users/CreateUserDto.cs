namespace DzentoPOS2.Shared.Contracts.DTOs.Identity.Users
{
    public class CreateUserDto
    {
        public string FullName { get; set; }
            = string.Empty;

        public string UserName { get; set; }
            = string.Empty;

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string Password { get; set; }
            = string.Empty;

        public bool IsActive { get; set; } = true;

        public Guid? BranchId { get; set; }

        public List<Guid> RoleIds { get; set; }
            = new();
    }
}