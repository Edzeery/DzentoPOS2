namespace DzentoPOS2.Shared.Contracts.DTOs.Purchases
{
    public class CreatePurchaseItemDto
    {
        public Guid ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal CostPrice { get; set; }
    }
}
