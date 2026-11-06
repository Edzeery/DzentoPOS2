using System;
using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Inventory
{
    public class VariantAttribute : AuditableEntity
    {
        public Guid VariantId { get; set; }

        public Guid DefinitionId { get; set; }

        public Guid ValueId { get; set; }

        public virtual ProductVariant Variant { get; set; } = null!;

        public virtual AttributeDefinition Definition { get; set; } = null!;

        public virtual AttributeValue Value { get; set; } = null!;
    }
}