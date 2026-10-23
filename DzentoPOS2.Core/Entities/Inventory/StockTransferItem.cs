using DzentoPOS2.Core.Common;
using DzentoPOS2.Core.ValueObjects.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzentoPOS2.Core.Entities.Inventory
{
    public class StockTransferItem : AuditableEntity
    {
        public Guid StockTransferId { get; set; }

        public Guid ProductId { get; set; }

        public Quantity Quantity { get; set; } = null!;

        public virtual StockTransfer StockTransfer { get; set; } = null!;

        public virtual Product Product { get; set; } = null!;
    }
}