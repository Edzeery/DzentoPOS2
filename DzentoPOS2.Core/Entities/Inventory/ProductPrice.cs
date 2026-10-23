using DzentoPOS2.Core.Common;
using DzentoPOS2.Core.ValueObjects.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzentoPOS2.Core.Entities.Inventory
{
    public class ProductPrice : AuditableEntity
    {
        public Guid ProductId { get; set; }

        // ✔ ValueObject
        public Money Price { get; set; } = null!;

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public virtual Product Product { get; set; } = null!;
    }
}