using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Inventory
{
    public class ProductUnit : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;

        public string Symbol { get; set; } = string.Empty;

        public bool IsBaseUnit { get; set; }

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}