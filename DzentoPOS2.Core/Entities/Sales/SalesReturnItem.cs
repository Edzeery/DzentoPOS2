using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Sales
{
    public class SalesReturnItem : AuditableEntity
    {
        public Guid SalesReturnId { get; set; }

        public Guid ProductId { get; set; }

        public decimal Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal Total { get; set; }

        public virtual SalesReturn SalesReturn { get; set; } = null!;
    }
}