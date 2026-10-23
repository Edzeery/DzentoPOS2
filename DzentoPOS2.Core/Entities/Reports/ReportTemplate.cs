using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Reports
{
    public class ReportTemplate : AuditableEntity
    {
        public required string Name { get; set; }

        public string TemplateContent { get; set; } = string.Empty;

        public string ReportType { get; set; } = string.Empty;

        public bool IsDefault { get; set; }
    }
}