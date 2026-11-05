using System;
using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Inventory
{
    public class AttributeDefinition : AuditableEntity
    {
        public Guid ProductId { get; set; }

        public string Name { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public virtual Product Product { get; set; } = null!;

        public virtual ICollection<AttributeValue> Values { get; set; } = new List<AttributeValue>();
    }
}