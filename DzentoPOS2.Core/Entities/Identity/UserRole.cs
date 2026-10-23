using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Identity
{
    public class UserRole : AuditableEntity
    {
        public Guid UserId { get; set; }

        public Guid RoleId { get; set; }

        public virtual User User { get; set; } = null!;

        public virtual Role Role { get; set; } = null!;
    }
}