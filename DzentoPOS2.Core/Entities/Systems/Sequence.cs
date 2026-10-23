using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Systems
{
    public class Sequence : AuditableEntity
    {
        public required string Name { get; set; }

        public string Prefix { get; set; } = string.Empty;

        public long CurrentNumber { get; set; }

        public int Padding { get; set; } = 5;
    }
}