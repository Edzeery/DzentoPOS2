using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Purchases
{
    public class PurchaseReturn : AuditableEntity
    {
        public string ReturnNumber { get; set; } = string.Empty;

        public Guid PurchaseInvoiceId { get; set; }

        public Guid SupplierId { get; set; }

        public decimal TotalAmount { get; set; }

        public string Notes { get; set; } = string.Empty;

        public DateTime ReturnDate { get; set; } = DateTime.UtcNow;

        public virtual ICollection<PurchaseReturnItem> Items { get; set; } = new List<PurchaseReturnItem>();  
    }
}