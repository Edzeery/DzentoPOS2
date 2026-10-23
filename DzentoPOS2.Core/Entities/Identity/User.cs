using DzentoPOS2.Core.Common;
using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Core.ValueObjects.Identity;

namespace DzentoPOS2.Core.Entities.Identity
{
    public class User : AuditableEntity
    {
        public required string FullName { get; set; }

        public required string UserName { get; set; }

        public Email? Email { get; set; }

        public PhoneNumber? PhoneNumber { get; set; }

        public required string PasswordHash { get; set; }

        public bool IsActive { get; set; } = true;

        public Guid? BranchId { get; set; }

        public virtual Branch? Branch { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }
            = new List<UserRole>();
    }
}