using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Sales
{
    public class SalesInvoiceItem : AuditableEntity
    {
        public Guid SalesInvoiceId { get; set; }

        public Guid ProductId { get; set; }

        public Guid VariantId { get; set; }

        public decimal Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal Total { get; set; }

        /// <summary>Cost per unit frozen at sale time (moving weighted average).</summary>
        public decimal UnitCost { get; set; }

        /// <summary>Product list (sale) price frozen at sale time.</summary>
        public decimal ListPrice { get; set; }

        /// <summary>This line's share of the invoice-level discount.</summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>True when UnitCost/ListPrice were backfilled from current data (legacy invoices).</summary>
        public bool IsCostEstimated { get; set; }

        /// <summary>Variant name frozen at sale time. Null/empty for legacy invoices
        /// (they display the product name instead).</summary>
        public string? VariantName { get; set; }

        /// <summary>Serial number for serialized items (legacy untracked stock).</summary>
        public string? SerialNumber { get; set; }

        /// <summary>IMEI for phones.</summary>
        public string? IMEI { get; set; }

        /// <summary>Warranty expiry date (computed from sale date + variant.WarrantyMonths).</summary>
        public DateTime? WarrantyExpiry { get; set; }

        /// <summary>Digital code delivered on this line (snapshot for the receipt).</summary>
        public string? DigitalCode { get; set; }

        /// <summary>Free text of the line (empty for regular product lines).</summary>
        public string Description { get; set; } = string.Empty;
    }
}