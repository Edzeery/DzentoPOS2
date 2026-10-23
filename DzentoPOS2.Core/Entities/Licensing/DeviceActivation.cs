using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Licensing
{
    public class DeviceActivation : AuditableEntity
    {
        public Guid LicenseId { get; set; }

        public required string DeviceName { get; set; }

        public required string DeviceIdentifier { get; set; }

        public DateTime ActivatedAt { get; set; } = DateTime.UtcNow;

        public virtual License License { get; set; } = null!;
    }
}