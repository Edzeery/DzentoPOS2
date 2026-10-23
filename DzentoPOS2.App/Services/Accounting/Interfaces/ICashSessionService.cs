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

        Task<List<CashSessionDto>> GetRecentSessionsAsync(Guid cashboxId, int take = 10);
    }
}
