using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Systems
{
    public class AppSetting : AuditableEntity
    {
        public required string Key { get; set; } 

        public string Value { get; set; } = string.Empty;

        public string Group { get; set; } = string.Empty;
    }
}