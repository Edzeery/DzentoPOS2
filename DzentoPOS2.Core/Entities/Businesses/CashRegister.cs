using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Businesses
{
    public class CashRegister : AuditableEntity
    {
        public required string Name { get; set; }

        public string Code { get; set; } = string.Empty;

        public Guid BranchId { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual Branch Branch { get; set; } = null!;
    }
}