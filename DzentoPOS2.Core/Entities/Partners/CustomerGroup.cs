using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Partners
{
    public class CustomerGroup : AuditableEntity
    {
        public required string Name { get; set; }

        public decimal DiscountPercentage { get; set; }

        public string Description { get; set; } = string.Empty;
    }
}