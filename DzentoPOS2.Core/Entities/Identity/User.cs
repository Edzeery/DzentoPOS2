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

        /// <summary>
        /// Set when an account still uses a seeded/administrator-assigned
        /// password. The shell must force a password change before the user can
        /// work, and a successful change clears the flag.
        /// </summary>
        public bool MustChangePassword { get; set; }

        /// <summary>
        /// BCrypt hash of the recovery key. The key itself is shown once at
        /// generation time and is never stored in clear text. Null means no
        /// recovery key has been issued yet.
        /// </summary>
        public string? RecoveryKeyHash { get; set; }

        public Guid? BranchId { get; set; }

        public virtual Branch? Branch { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }
            = new List<UserRole>();
    }
}