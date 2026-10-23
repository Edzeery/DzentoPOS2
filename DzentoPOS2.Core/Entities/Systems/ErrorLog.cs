using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Systems
{
    public class ErrorLog : AuditableEntity
    {
        public string Message { get; set; } = string.Empty;

        public string StackTrace { get; set; } = string.Empty;

        public string Source { get; set; } = string.Empty;
    }
}