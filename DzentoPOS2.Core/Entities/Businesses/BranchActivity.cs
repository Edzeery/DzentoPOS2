using System;
using DzentoPOS2.Core.Common;
using DzentoPOS2.Core.Enums;

namespace DzentoPOS2.Core.Entities.Businesses
{
    public class BranchActivity : AuditableEntity
    {
        public Guid BranchId { get; set; }
        public ActivityType ActivityType { get; set; }
        public bool IsEnabled { get; set; } = true;

        public virtual Branch Branch { get; set; } = null!;
    }
}
