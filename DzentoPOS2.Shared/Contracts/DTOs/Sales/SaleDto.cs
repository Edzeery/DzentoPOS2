using DzentoPOS2.Core.Services.Sales;

namespace DzentoPOS2.Shared.Contracts.DTOs.Sales
{
    public class SaleDto
    {
        public Guid Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public DiscountMode DiscountMode { get; set; }
        public decimal DiscountValue { get; set; }

        /// <summary>Percent value for display (0 when the mode is a fixed amount).</summary>
        public decimal Discount { get; set; }

        /// <summary>Human readable discount, e.g. "10% (12.50)" or "12.50".</summary>
        public string DiscountDisplay { get; set; } = string.Empty;

        public decimal DiscountAmount { get; set; }
        public decimal AfterDiscount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }

        /// <summary>Net amount applied to the invoice (never more than the total).</summary>
        public decimal PaidAmount { get; set; }

        public decimal RemainingAmount { get; set; }

        /// <summary>Cash handed over by the customer.</summary>
        public decimal TenderedAmount { get; set; }

        /// <summary>Change given back to the customer.</summary>
        public decimal ChangeAmount { get; set; }

        public bool IsVoided { get; set; }
        public string? VoidReason { get; set; }
        public DateTime? VoidedAtUtc { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<SaleItemDto> Items { get; set; } = new();
    }
}