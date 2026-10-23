using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Partners
{
    public class SupplierPayment : AuditableEntity
    {
        public Guid SupplierId { get; set; }

        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        public string PaymentMethod { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        public virtual Supplier Supplier { get; set; } = null!;
    }
}