using DzentoPOS2.Core.Common;
using DzentoPOS2.Core.ValueObjects.Common;
using DzentoPOS2.Core.ValueObjects.Inventory; 

namespace DzentoPOS2.Core.Entities.Inventory
{
    public class Product : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;

        public string? SKU { get; set; }

        public string? Barcode { get; set; }

        public string? Description { get; set; }

        public Guid CategoryId { get; set; }

        public Guid UnitId { get; set; }

        // 🔥 Value Object
        public Money CostPrice { get; set; } = null!;

        public Money SalePrice { get; set; } = null!;

        // 🔥 Value Object (بدل decimal)
        public Quantity MinimumStock { get; set; } = null!;

        public bool TrackStock { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public virtual ProductCategory Category { get; set; } = null!;
        public virtual ProductUnit Unit { get; set; } = null!;

        public virtual ICollection<ProductBarcode> Barcodes { get; set; } = new List<ProductBarcode>();
        public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public virtual ICollection<ProductPrice> Prices { get; set; } = new List<ProductPrice>();
        public virtual ICollection<ProductStock> Stocks { get; set; } = new List<ProductStock>();
        public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
    }
}