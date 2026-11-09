namespace DzentoPOS2.Shared.Contracts.DTOs.Sales
{
    public class CreateSaleItemDto
    {
        public Guid ProductId { get; set; }

        /// <summary>Variant sold. Empty means the product's default variant.</summary>
        public Guid VariantId { get; set; }

        /// <summary>Serial unit sold (for serialized products). Empty means non-serialized.</summary>
        public Guid SerialUnitId { get; set; }

        /// <summary>Variant name at sale time (for cart display).</summary>
        public string VariantName { get; set; } = string.Empty;

        public decimal Quantity { get; set; }
        public decimal Price { get; set; }

        /// <summary>Free text shown on the invoice line (used for custom lines).</summary>
        public string Description { get; set; } = string.Empty;
    }
}
