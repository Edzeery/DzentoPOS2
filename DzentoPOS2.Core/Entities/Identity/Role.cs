using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Identity
{
    public class Role : AuditableEntity
    {
        public required string Name { get; set; }

        public string? Description { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }
            = new List<UserRole>();

        public virtual ICollection<RolePermission> RolePermissions { get; set; }
            = new List<RolePermission>();
    }
}