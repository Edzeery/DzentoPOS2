using DzentoPOS2.Core.Common;
using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Core.ValueObjects.Inventory;

namespace DzentoPOS2.Core.Entities.Inventory
{
 

    public class InventoryTransaction : AuditableEntity
    {
        public Guid ProductId { get; set; }

        public Guid WarehouseId { get; set; }

        // 🔥 Value Object
        public Quantity Quantity { get; set; } = null!;

        public MovementType TransactionType { get; set; }

        public string? ReferenceNumber { get; set; }

        public string? Notes { get; set; }

        public virtual Product Product { get; set; } = null!;
        public virtual Warehouse Warehouse { get; set; } = null!;
    }
}