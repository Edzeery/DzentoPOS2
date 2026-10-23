using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Purchases
{
    public class PurchaseReturnItem : AuditableEntity
    {
        public Guid PurchaseReturnId { get; set; }

        public Guid ProductId { get; set; }

        public decimal Quantity { get; set; }

        public decimal CostPrice { get; set; }

        public decimal Total { get; set; }

        public virtual PurchaseReturn PurchaseReturn { get; set; } = null!;
    }
}