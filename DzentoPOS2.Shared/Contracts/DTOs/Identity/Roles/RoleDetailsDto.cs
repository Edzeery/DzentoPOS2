namespace DzentoPOS2.Shared.Contracts.DTOs.Identity.Roles
{
    public class RoleDetailsDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
            = string.Empty;

        public string? Description { get; set; }

        public List<Guid> PermissionIds { get; set; }
            = new();

        public List<string> Permissions { get; set; }
            = new();
    }
}