using DzentoPOS2.Core.Common;
using DzentoPOS2.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzentoPOS2.Core.Entities.Accounting
{
    public class CashTransaction : AuditableEntity
    {
        public Guid CashboxId { get; set; }

        public Guid? CashSessionId { get; set; }

        public decimal Amount { get; set; }

        public MovementType TransactionType { get; set; }

        public string? SourceType { get; set; }

        public Guid? SourceId { get; set; }

        public required string ReferenceNumber { get; set; }

        public string? Notes { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    }
}