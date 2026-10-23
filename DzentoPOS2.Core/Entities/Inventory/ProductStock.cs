using DzentoPOS2.Core.Common;
using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Core.ValueObjects.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzentoPOS2.Core.Entities.Inventory
{
    public class ProductStock : AuditableEntity
    {
        public Guid ProductId { get; set; }

        public Product Product { get; set; } = null!;

        public Guid WarehouseId { get; set; }

        public Warehouse Warehouse { get; set; } = null!;

        public Quantity Quantity { get; set; } = null!;

        public int Version { get; set; }
    }
}