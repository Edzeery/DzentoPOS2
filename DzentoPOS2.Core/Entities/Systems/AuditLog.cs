using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Systems
{
    public class AuditLog : AuditableEntity
    {
        public Guid UserId { get; set; }

        public string Action { get; set; } = string.Empty;

        public string EntityName { get; set; } = string.Empty;

        public string OldValues { get; set; } = string.Empty;   

        public string NewValues { get; set; } = string.Empty;

        public string IpAddress { get; set; } = string.Empty;
    }
}