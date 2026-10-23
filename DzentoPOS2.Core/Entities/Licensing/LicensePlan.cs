using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Licensing
{
    public class LicensePlan : AuditableEntity
    {
        public required string Name { get; set; }

        public decimal Price { get; set; }

        public int DurationInDays { get; set; }

        public int MaxUsers { get; set; }

        public int MaxBranches { get; set; }

        public bool IsActive { get; set; } = true;
    }
}