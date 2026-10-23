namespace DzentoPOS2.Shared.Contracts.DTOs.Purchases
{
    public class PurchaseItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal CostPrice { get; set; }
        public decimal Total { get; set; }
    }
}
