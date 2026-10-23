namespace DzentoPOS2.Shared.Contracts.DTOs.Purchases
{
    public class CreatePurchaseInvoiceDto
    {
        public Guid SupplierId { get; set; }
        public decimal PaidAmount { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public List<CreatePurchaseItemDto> Items { get; set; } = new();
    }
}
