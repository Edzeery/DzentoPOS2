using System;
using DzentoPOS2.Core.Common;
using DzentoPOS2.Core.ValueObjects.Common;
using DzentoPOS2.Core.ValueObjects.Inventory;

namespace DzentoPOS2.Core.Entities.Inventory
{
    public class ProductVariant : AuditableEntity
    {
        public Guid ProductId { get; set; }

        // Legacy display name (kept for compatibility; new code uses attribute values).
        public string Name { get; set; } = string.Empty;

        public string? SKU { get; set; }

        public decimal AdditionalPrice { get; set; }

        public Money CostPrice { get; set; } = null!;

        public Money SalePrice { get; set; } = null!;

        public Quantity MinimumStock { get; set; } = null!;

        public bool TrackStock { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public bool IsDefault { get; set; } = false;

        /// <summary>Warranty period in months (null = no warranty).</summary>
        public int? WarrantyMonths { get; set; }

        // Weight support
        public bool IsWeighed { get; set; } = false;
        public Guid? WeightUnitId { get; set; }

        // Batch/Expiry support
        public bool TrackBatch { get; set; } = false;
        public bool TrackExpiry { get; set; } = false;

        public string? ImagePath { get; set; }

        public string? ThumbnailPath { get; set; }

        public virtual Product Product { get; set; } = null!;
    }
}