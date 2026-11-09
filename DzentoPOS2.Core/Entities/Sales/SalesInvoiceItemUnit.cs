using System;
using DzentoPOS2.Core.Common;
using DzentoPOS2.Core.Entities.Inventory;

namespace DzentoPOS2.Core.Entities.Sales
{
    public class SalesInvoiceItemUnit : AuditableEntity
    {
        public Guid SalesInvoiceItemId { get; set; }
        public Guid SerialUnitId { get; set; }

        public virtual SalesInvoiceItem SalesInvoiceItem { get; set; } = null!;
        public virtual Inventory.SerialUnit SerialUnit { get; set; } = null!;
    }
}
