namespace DzentoPOS2.Shared.Contracts.DTOs.Identity.Users
{
    public class UpdateUserDto
    {
        public Guid Id { get; set; }

        public string FullName { get; set; }
            = string.Empty;

        public string UserName { get; set; }
            = string.Empty;

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; }

        public Guid? BranchId { get; set; }

        public List<Guid> RoleIds { get; set; }
            = new();
    }
}