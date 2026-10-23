using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Identity
{
    public class Permission : AuditableEntity
    {
        public required string Name { get; set; }

        public required string Code { get; set; }

        public string? Module { get; set; }

        public string? Description { get; set; }

        public virtual ICollection<RolePermission> RolePermissions { get; set; }
            = new List<RolePermission>();
    }
}