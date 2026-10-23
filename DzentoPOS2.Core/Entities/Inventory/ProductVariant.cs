using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Inventory
{
    public class ProductVariant : AuditableEntity
    {
        public Guid ProductId { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal AdditionalPrice { get; set; }

        public virtual Product Product { get; set; } = null!;
    }
}