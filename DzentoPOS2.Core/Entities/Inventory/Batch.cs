using System;
using DzentoPOS2.Core.Common;
using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Core.ValueObjects.Inventory;

namespace DzentoPOS2.Core.Entities.Inventory
{
    public class Batch : AuditableEntity
    {
        public Guid VariantId { get; set; }
        public Guid WarehouseId { get; set; }

        public string BatchNumber { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

        public virtual ProductVariant Variant { get; set; } = null!;
        public virtual Warehouse Warehouse { get; set; } = null!;
    }
}
