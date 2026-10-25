using DzentoPOS2.Core.Enums;

namespace DzentoPOS2.Shared.Contracts.DTOs.Accounting
{
    public class CashboxDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = "";

        public decimal CurrentBalance { get; set; }

        public bool IsActive { get; set; }
    }

    public class CashSessionDto
    {
        public Guid Id { get; set; }

        public Guid CashboxId { get; set; }

        public DateTime OpenedAtUtc { get; set; }

        public Guid? OpenedByUserId { get; set; }

        public decimal OpeningBalance { get; set; }

        public DateTime? ClosedAtUtc { get; set; }

        public Guid? ClosedByUserId { get; set; }

        public decimal? ClosingBalance { get; set; }

        public decimal? ExpectedBalance { get; set; }

        public decimal? Difference { get; set; }

        public CashSessionStatus Status { get; set; }

        public string? Notes { get; set; }
    }

    public class OpenCashSessionDto
    {
        public Guid CashboxId { get; set; }

        public decimal OpeningBalance { get; set; }

        public string? Notes { get; set; }
    }

    public class CloseCashSessionDto
    {
        public Guid SessionId { get; set; }

        public decimal ClosingBalance { get; set; }

        public string? Notes { get; set; }
    }

    public class CashSessionSummaryDto
    {
        public Guid SessionId { get; set; }

        public decimal OpeningBalance { get; set; }

        public decimal CashSales { get; set; }

        public decimal CashRefunds { get; set; }

        public decimal PayIns { get; set; }

        public decimal PayOuts { get; set; }

        public decimal Expenses { get; set; }

        public decimal ExpectedBalance { get; set; }
    }

    public class CashMovementDto
    {
        public Guid CashboxId { get; set; }

        public Guid? CashSessionId { get; set; }

        public decimal Amount { get; set; }

        public string ReferenceNumber { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;
    }
}
