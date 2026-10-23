using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
using DzentoPOS2.Core.Common;
 
namespace DzentoPOS2.Core.Entities.Inventory
{
    public class ProductCategory : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public Guid? ParentCategoryId { get; set; }

        public virtual ProductCategory? ParentCategory { get; set; } = null!;

        public virtual ICollection<ProductCategory> Children { get; set; } = new List<ProductCategory>();

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();

        public bool IsActive { get; set; } = true;
    }
}