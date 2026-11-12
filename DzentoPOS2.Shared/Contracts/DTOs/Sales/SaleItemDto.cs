namespace DzentoPOS2.Shared.Contracts.DTOs.Sales
{
    public class SaleItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }

        /// <summary>Variant sold. Empty for legacy invoices created before variants.</summary>
        public Guid VariantId { get; set; }

        /// <summary>Variant name at sale time. Product name for legacy invoices.</summary>
        public string VariantName { get; set; } = string.Empty;

        /// <summary>Serial number for serialized items.</summary>
        public string? SerialNumber { get; set; }

        /// <summary>IMEI for phones.</summary>
        public string? IMEI { get; set; }

        /// <summary>Warranty expiry date.</summary>
        public DateTime? WarrantyExpiry { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
        public decimal UnitCost { get; set; }
        public decimal ListPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public bool IsCostEstimated { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
