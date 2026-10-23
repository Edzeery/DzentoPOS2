using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Identity
{
    public class RolePermission : AuditableEntity
    {
        public Guid RoleId { get; set; }

        public Guid PermissionId { get; set; }

        public virtual Role Role { get; set; } = null!;

        public virtual Permission Permission { get; set; } = null!;
    }
}