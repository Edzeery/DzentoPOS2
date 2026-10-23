using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Reports
{
    public class DashboardWidget : AuditableEntity
    {
        public string Title { get; set; } = string.Empty;

        public string WidgetType { get; set; } = string.Empty;

        public string Configuration { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }
}