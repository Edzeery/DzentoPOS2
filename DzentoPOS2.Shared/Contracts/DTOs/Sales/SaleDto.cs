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
        public decimal Discount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public bool IsVoided { get; set; }
        public string? VoidReason { get; set; }
        public DateTime? VoidedAtUtc { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<SaleItemDto> Items { get; set; } = new();
    }
}
