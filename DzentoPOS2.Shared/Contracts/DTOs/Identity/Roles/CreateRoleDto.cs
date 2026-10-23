namespace DzentoPOS2.Shared.Contracts.DTOs.Identity.Roles
{
    public class CreateRoleDto
    {
        public string Name { get; set; }
            = string.Empty;

        public string? Description { get; set; }

        public List<Guid> PermissionIds { get; set; }
            = new();
    }
}