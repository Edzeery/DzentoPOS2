using DzentoPOS2.Core.Common;
using DzentoPOS2.Core.Exceptions;
using DzentoPOS2.Core.Services.Sales;
using DzentoPOS2.Core.ValueObjects.Common;
using DzentoPOS2.Core.ValueObjects.Sales;

namespace DzentoPOS2.Core.Entities.Sales
{
    public class SalesInvoice : AuditableEntity
    {
        public InvoiceNumber InvoiceNumber { get; private set; } = null!;

        public Guid CustomerId { get; set; }

        public Guid BranchId { get; set; }

        public Guid? CashSessionId { get; private set; }

        public Money Subtotal { get; private set; } = null!;

        public Money DiscountAmount { get; private set; } = null!;

        public Money TaxAmount { get; private set; } = null!;

        public Money Total { get; private set; } = null!;

        public Percentage Discount { get; private set; } = null!;

        public Tax Tax { get; private set; } = null!;

        public Money PaidAmount { get; private set; } = null!;

        public Money RemainingAmount { get; private set; } = null!;

        public bool IsVoided { get; private set; }

        public DateTime? VoidedAtUtc { get; private set; }

        public Guid? VoidedByUserId { get; private set; }

        public string? VoidReason { get; private set; }

        public virtual ICollection<SalesInvoiceItem> Items { get; set; } = new HashSet<SalesInvoiceItem>();

        private SalesInvoice() { }

        public static SalesInvoice Create(
            string invoiceNumber,
            Guid customerId,
            Guid branchId,
            IReadOnlyCollection<InvoiceLine> lines,
            decimal discountPercent,
            decimal taxRate,
            decimal paidAmount,
            string currency,
            int currencyDecimalDigits)
        {
            var totals = InvoiceCalculator.Calculate(
                lines,
                discountPercent,
                taxRate,
                paidAmount,
                currencyDecimalDigits);

            return new SalesInvoice
            {
                InvoiceNumber = new InvoiceNumber(invoiceNumber),
                CustomerId = customerId,
                BranchId = branchId,
                Subtotal = new Money(totals.Subtotal, currency),
                DiscountAmount = new Money(totals.DiscountAmount, currency),
                TaxAmount = new Money(totals.TaxAmount, currency),
                Total = new Money(totals.Total, currency),
                Discount = new Percentage(discountPercent),
                Tax = new Tax(taxRate),
                PaidAmount = new Money(totals.PaidAmount, currency),
                RemainingAmount = new Money(totals.RemainingAmount, currency)
            };
        }

        public void AssignCashSession(Guid cashSessionId)
        {
            CashSessionId = cashSessionId;
        }

        public void Void(Guid? userId, string reason, DateTime utcNow)
        {
            if (IsVoided)
                return;

            if (string.IsNullOrWhiteSpace(reason))
                throw new DomainException("A void reason is required");

            IsVoided = true;
            VoidedAtUtc = utcNow;
            VoidedByUserId = userId;
            VoidReason = reason.Trim();
        }
    }
}
