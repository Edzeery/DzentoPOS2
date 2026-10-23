using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Systems
{
    public class ActivityLog : AuditableEntity
    {
        public Guid? UserId { get; set; }

        public string Activity { get; set; } = string.Empty;

        public string Module { get; set; } = string.Empty;

        public string IpAddress { get; set; } = string.Empty;
    }
}