using System;
using DzentoPOS2.Core.Common;
using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Core.Enums;

namespace DzentoPOS2.Core.Entities.Inventory
{
    public class SerialUnit : AuditableEntity
    {
        public Guid VariantId { get; set; }
        public Guid WarehouseId { get; set; }

        public string? SerialNumber { get; set; }
        public string? Imei1 { get; set; }
        public string? Imei2 { get; set; }

        public SerialUnitStatus Status { get; set; } = SerialUnitStatus.Available;

        public Guid? PurchaseInvoiceItemId { get; set; }
        public Guid? SalesInvoiceItemId { get; set; }

        public decimal UnitCost { get; set; }

        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SoldAt { get; set; }
        public DateTime? WarrantyStartsAt { get; set; }
        public DateTime? WarrantyExpiresAt { get; set; }

        public virtual ProductVariant Variant { get; set; } = null!;
        public virtual Warehouse Warehouse { get; set; } = null!;
    }
}
