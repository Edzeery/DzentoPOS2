namespace DzentoPOS2.Shared.Contracts.DTOs.Sales
{
    public class CreateSaleDto
    {
        public Guid CustomerId { get; set; }
        public decimal Discount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal PaidAmount { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public List<CreateSaleItemDto> Items { get; set; } = new();
    }
}
