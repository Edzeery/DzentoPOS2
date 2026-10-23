namespace DzentoPOS2.Shared.Contracts.DTOs.Sales
{
    public class CreateSaleItemDto
    {
        public Guid ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }

        /// <summary>Free text shown on the invoice line (used for custom lines).</summary>
        public string Description { get; set; } = string.Empty;
    }
}
