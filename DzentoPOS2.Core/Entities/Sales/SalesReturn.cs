using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Common; 

namespace DzentoPOS2.Core.Entities.Sales
{
    public class SalesReturn : AuditableEntity
    {
        public string ReturnNumber { get; set; } = string.Empty;

        public Guid SalesInvoiceId { get; set; }

        public Guid CustomerId { get; set; }

        public decimal TotalAmount { get; set; }

        public string Notes { get; set; } = string.Empty;

        public DateTime ReturnDate { get; set; } = DateTime.UtcNow;

        public virtual ICollection<SalesReturnItem> Items { get; set; } = new HashSet<SalesReturnItem>();
    }
}