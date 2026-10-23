using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Purchases
{
    public class PurchaseInvoice : AuditableEntity
    {
        public string InvoiceNumber { get; set; } = string.Empty;

        public Guid SupplierId { get; set; }

        public Guid BranchId { get; set; }

        public decimal Total { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal RemainingAmount { get; set; }

        public virtual ICollection<PurchaseInvoiceItem> Items { get; set; }
            = new List<PurchaseInvoiceItem>();
    }
}