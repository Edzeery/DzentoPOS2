using DzentoPOS2.Core.Services.Sales;

namespace DzentoPOS2.Shared.Contracts.DTOs.Sales
{
    public class CreateSaleDto
    {
        public Guid CustomerId { get; set; }

        /// <summary>Percent or fixed amount; see <see cref="DiscountMode"/>.</summary>
        public DiscountMode DiscountMode { get; set; } = DiscountMode.Percent;

        /// <summary>Raw discount input (percent when <see cref="DiscountMode"/> is Percent).</summary>
        public decimal DiscountValue { get; set; }

        public decimal TaxRate { get; set; }

        /// <summary>Cash handed over by the customer (may exceed the total).</summary>
        public decimal PaidAmount { get; set; }

        public string PaymentMethod { get; set; } = "Cash";

        public List<CreateSaleItemDto> Items { get; set; } = new();
    }
}