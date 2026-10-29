using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Sales
{
    public class SalesPayment : AuditableEntity
    {
        public Guid SalesInvoiceId { get; set; }

        /// <summary>Net amount applied to the invoice (never more than the total).</summary>
        public decimal Amount { get; set; }

        /// <summary>Cash handed over by the customer (may exceed <see cref="Amount"/>).</summary>
        public decimal TenderedAmount { get; set; }

        /// <summary>Change given back to the customer.</summary>
        public decimal ChangeAmount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        public string PaymentMethod { get; set; } = string.Empty;

        public string ReferenceNumber { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        public virtual SalesInvoice SalesInvoice { get; set; } = null!;
    }
}