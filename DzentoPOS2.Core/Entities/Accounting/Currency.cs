using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Accounting
{
    public class Currency : AuditableEntity
    {
        public required string Name { get; set; }

        public required string Code { get; set; }

        public required string Symbol { get; set; }

        public decimal ExchangeRate { get; set; }

        public bool IsDefault { get; set; }
    }
}