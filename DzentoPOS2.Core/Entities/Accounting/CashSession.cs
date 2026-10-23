using DzentoPOS2.Core.Common;
using DzentoPOS2.Core.Enums;

namespace DzentoPOS2.Core.Entities.Accounting
{
    public class CashSession : AuditableEntity
    {
        public Guid CashboxId { get; set; }

        public DateTime OpenedAtUtc { get; set; } = DateTime.UtcNow;

        public Guid? OpenedByUserId { get; set; }

        public decimal OpeningBalance { get; set; }

        public DateTime? ClosedAtUtc { get; set; }

        public Guid? ClosedByUserId { get; set; }

        public decimal? ClosingBalance { get; set; }

        public decimal? ExpectedBalance { get; set; }

        public decimal? Difference { get; set; }

        public CashSessionStatus Status { get; set; } = CashSessionStatus.Open;

        public string? Notes { get; set; }
    }
}
