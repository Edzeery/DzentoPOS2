namespace DzentoPOS2.Shared.Contracts.DTOs.Identity.Roles
{
    public class RoleDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
            = string.Empty;

        public string? Description { get; set; }

        public int PermissionsCount { get; set; }
    }
}