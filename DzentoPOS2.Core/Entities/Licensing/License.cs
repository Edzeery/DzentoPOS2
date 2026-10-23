using DzentoPOS2.Core.Common;
using DzentoPOS2.Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 

namespace DzentoPOS2.Core.Entities.Licensing
{
    public class License : AuditableEntity
    {
        public Guid LicensePlanId { get; set; }

        public required string LicenseKey { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime ExpiryDate { get; set; }

        public bool IsActive { get; set; }

        public virtual LicensePlan LicensePlan { get; set; } = null!;

        public virtual ICollection<DeviceActivation> DeviceActivations { get; set; }
            = new List<DeviceActivation>();
    }
}