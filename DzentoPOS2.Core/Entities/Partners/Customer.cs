using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Common;
using DzentoPOS2.Core.ValueObjects.Business;
using DzentoPOS2.Core.ValueObjects.Identity;

namespace DzentoPOS2.Core.Entities.Partners
{
    public class Customer : AuditableEntity
    {
        public required string FullName { get; set; }

        public PhoneNumber? PhoneNumber { get; set; }

        public Address? Address { get; set; }

        public decimal Balance { get; set; }

        public bool IsActive { get; set; } = true;
    }
}