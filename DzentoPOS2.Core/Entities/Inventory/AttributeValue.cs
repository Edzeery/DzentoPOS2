using System;
using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Inventory
{
    public class AttributeValue : AuditableEntity
    {
        public Guid DefinitionId { get; set; }

        public string Value { get; set; } = string.Empty;

        public virtual AttributeDefinition Definition { get; set; } = null!;
    }
}