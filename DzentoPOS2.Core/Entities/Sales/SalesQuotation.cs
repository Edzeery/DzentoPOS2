using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Sales
{
    public class SalesQuotation : AuditableEntity
    {
        public string QuotationNumber { get; set; } = null!;

        public Guid CustomerId { get; set; }

        public Guid BranchId { get; set; }

        public decimal Total { get; set; }

        public decimal Discount { get; set; }

        public DateTime ExpiryDate { get; set; }

        public string Notes { get; set; } = string.Empty;
    }
}