using DzentoPOS2.Core.Common;
using DzentoPOS2.Core.ValueObjects.Business;
using DzentoPOS2.Core.ValueObjects.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 

namespace DzentoPOS2.Core.Entities.Businesses
{
    public class Branch : AuditableEntity
    {
        public required string Name { get; set; }

        public Address? Address { get; set; }

        public PhoneNumber? PhoneNumber { get; set; }

        public bool IsMainBranch { get; set; }

        public bool IsActive { get; set; } = true;
        public virtual ICollection<Warehouse> Warehouses { get; set; }
    = new List<Warehouse>();

        public virtual ICollection<CashRegister> CashRegisters { get; set; }
            = new List<CashRegister>();
    }
}