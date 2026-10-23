using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Accounting
{
    public class Cashbox : AuditableEntity
    {
        public required string Name { get; set; }

        public Guid BranchId { get; set; }

        public decimal CurrentBalance { get; set; }

        public bool IsActive { get; set; } = true;
    }
}