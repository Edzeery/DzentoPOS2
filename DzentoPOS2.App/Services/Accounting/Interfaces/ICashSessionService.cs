using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Shared.Contracts.DTOs.Accounting;

namespace DzentoPOS2.App.Services.Accounting
{
    public interface ICashSessionService
    {
        Task<List<CashboxDto>> GetCashboxesAsync();

        Task<CashSessionDto?> GetOpenSessionAsync();

        Task<CashSessionDto?> GetOpenSessionByCashboxAsync(Guid cashboxId);

        Task<CashSessionDto> OpenSessionAsync(OpenCashSessionDto dto);

        Task<CashSessionDto> CloseSessionAsync(CloseCashSessionDto dto);

        Task<CashSessionSummaryDto> GetSessionSummaryAsync(Guid sessionId, CancellationToken cancellationToken = default);

        Task<CashTransaction> RecordPayInAsync(CashMovementDto dto, CancellationToken cancellationToken = default);

        Task<CashTransaction> RecordPayOutAsync(CashMovementDto dto, CancellationToken cancellationToken = default);

        Task<List<CashSessionDto>> GetRecentSessionsAsync(Guid cashboxId, int take = 10);
    }
}
