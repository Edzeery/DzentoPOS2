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
    public class StockTransfer : AuditableEntity
    {
        public string TransferNumber { get; set; } = string.Empty;

        public Guid FromWarehouseId { get; set; }

        public Guid ToWarehouseId { get; set; }

        public Guid CreatedByUserId { get; set; }

        public DateTime TransferDate { get; set; } = DateTime.UtcNow;

        public string? Notes { get; set; }

        public virtual Warehouse FromWarehouse { get; set; } = null!;

        public virtual Warehouse ToWarehouse { get; set; } = null!;

        public virtual User CreatedByUser { get; set; } = null!;

        public virtual ICollection<StockTransferItem> Items { get; set; }
            = new List<StockTransferItem>();
    }
}