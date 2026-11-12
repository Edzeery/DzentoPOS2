using System;
using DzentoPOS2.Core.Common;
using DzentoPOS2.Core.Enums;

namespace DzentoPOS2.Core.Entities.Inventory
{
    /// <summary>
    /// A single digital code (prepaid card, recharge voucher or activation key)
    /// belonging to one variant. Codes are imported in bulk while
    /// <see cref="Status"/> is <see cref="DigitalCodeStatus.Available"/>, handed
    /// to the customer when the variant is sold and released back to the pool
    /// when that sale is voided.
    /// </summary>
    public class DigitalCode : AuditableEntity
    {
        /// <summary>The code itself (what the customer receives).</summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>Optional secret (PIN) that accompanies the code.</summary>
        public string? Pin { get; set; }

        public Guid VariantId { get; set; }

        public DigitalCodeStatus Status { get; set; } = DigitalCodeStatus.Available;

        /// <summary>Sale that delivered this code (null while available).</summary>
        public Guid? SalesInvoiceId { get; set; }

        /// <summary>Invoice line that delivered this code.</summary>
        public Guid? SalesInvoiceItemId { get; set; }

        /// <summary>When the code was delivered to the customer.</summary>
        public DateTime? DeliveredAt { get; set; }

        public virtual ProductVariant Variant { get; set; } = null!;
    }
}
