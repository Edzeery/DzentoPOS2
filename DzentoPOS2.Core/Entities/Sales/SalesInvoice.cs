using DzentoPOS2.Core.Common;
using DzentoPOS2.Core.ValueObjects.Common;
using DzentoPOS2.Core.ValueObjects.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzentoPOS2.Core.Entities.Sales
{
    public class SalesInvoice : AuditableEntity
    {
        public InvoiceNumber InvoiceNumber { get; private set; } = null!;

        public Guid CustomerId { get; set; }

        public Guid BranchId { get; set; }

        public Money Total { get; private set; } = null!;

        public Percentage Discount { get; private set; } = null!;

        public Tax Tax { get; private set; } = null!;

        public Money PaidAmount { get; private set; } = null!;

        public Money RemainingAmount { get; private set; } = null!;

        public virtual ICollection<SalesInvoiceItem> Items { get; set; } = new HashSet<SalesInvoiceItem>();

        private SalesInvoice() { }

        public static SalesInvoice Create(
            string invoiceNumber,
            Guid customerId,
            Guid branchId,
            decimal total,
            decimal discount,
            decimal taxRate,
            decimal paidAmount,
            string currency = "DZD")
        {
            var taxAmount = total * taxRate / 100;
            var totalWithTax = total + taxAmount;
            var remaining = totalWithTax - paidAmount;
            if (remaining < 0) remaining = 0;

            return new SalesInvoice
            {
                InvoiceNumber = new InvoiceNumber(invoiceNumber),
                CustomerId = customerId,
                BranchId = branchId,
                Total = new Money(totalWithTax, currency),
                Discount = new Percentage(discount),
                Tax = new Tax(taxRate),
                PaidAmount = new Money(paidAmount, currency),
                RemainingAmount = new Money(remaining, currency)
            };
        }
    }
}