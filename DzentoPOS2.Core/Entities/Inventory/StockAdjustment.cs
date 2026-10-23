using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Common;
using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Core.Entities.Identity;

namespace DzentoPOS2.Core.Entities.Inventory
{
    public class StockAdjustment : AuditableEntity
    {
        public string ReferenceNumber { get; set; } = string.Empty;

        public Guid WarehouseId { get; set; }

        public Guid CreatedByUserId { get; set; }

        public string? Reason { get; set; }

        public DateTime AdjustmentDate { get; set; } = DateTime.UtcNow;

        public virtual Warehouse Warehouse { get; set; } = null!;

        public virtual User CreatedByUser { get; set; } = null!;

        public virtual ICollection<InventoryTransaction> Transactions { get; set; }
            = new List<InventoryTransaction>();
    }
}