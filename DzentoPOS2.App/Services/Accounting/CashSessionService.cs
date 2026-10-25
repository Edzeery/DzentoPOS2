using DzentoPOS2.App.Services.Accounting;
using DzentoPOS2.App.Session;
using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Accounting.Interfaces;
using DzentoPOS2.Data.Repositories.Common.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Accounting;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.App.Services.Accounting
{
    public class CashSessionService : ICashSessionService
    {
        private readonly ICashSessionRepository _sessionRepo;
        private readonly ICashboxRepository _cashboxRepo;
        private readonly ICashTransactionRepository _transactionRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly AppDbContext _context;

        public CashSessionService(
            ICashSessionRepository sessionRepo,
            ICashboxRepository cashboxRepo,
            ICashTransactionRepository transactionRepo,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            AppDbContext context)
        {
            _sessionRepo = sessionRepo;
            _cashboxRepo = cashboxRepo;
            _transactionRepo = transactionRepo;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _context = context;
        }

        public async Task<List<CashboxDto>> GetCashboxesAsync()
        {
            var boxes = await _cashboxRepo.GetActiveAsync();
            return boxes
                .Select(c => new CashboxDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    CurrentBalance = c.CurrentBalance,
                    IsActive = c.IsActive
                })
                .ToList();
        }

        public async Task<CashSessionDto?> GetOpenSessionAsync()
        {
            var session = await _sessionRepo.GetOpenAsync();
            if (session == null) return null;

            return Map(session);
        }

        public async Task<CashSessionDto?> GetOpenSessionByCashboxAsync(Guid cashboxId)
        {
            var session = await _sessionRepo.GetOpenByCashboxAsync(cashboxId);
            if (session == null) return null;

            return Map(session);
        }

        public async Task<CashSessionDto> OpenSessionAsync(OpenCashSessionDto dto)
        {
            var existing = await _sessionRepo.GetOpenByCashboxAsync(dto.CashboxId);
            if (existing != null)
                throw new InvalidOperationException("A cash session is already open for this cashbox.");

            var cashbox = await _cashboxRepo.GetByIdAsync(dto.CashboxId);
            if (cashbox == null)
                throw new KeyNotFoundException($"Cashbox {dto.CashboxId} not found.");

            var session = new CashSession
            {
                CashboxId = dto.CashboxId,
                OpenedAtUtc = DateTime.UtcNow,
                OpenedByUserId = _currentUser.UserId,
                OpeningBalance = dto.OpeningBalance,
                Status = CashSessionStatus.Open,
                Notes = dto.Notes?.Trim()
            };

            await _sessionRepo.AddAsync(session);
            await _unitOfWork.SaveChangesAsync();

            return Map(session);
        }

        public async Task<CashSessionDto> CloseSessionAsync(CloseCashSessionDto dto)
        {
            var session = await _context.CashSessions
                .FirstOrDefaultAsync(s => s.Id == dto.SessionId && !s.IsDeleted && s.Status == CashSessionStatus.Open);

            if (session == null)
                throw new KeyNotFoundException($"Open cash session {dto.SessionId} not found.");

            var cashbox = await _context.Cashboxes
                .FirstOrDefaultAsync(c => c.Id == session.CashboxId && !c.IsDeleted);

            var summary = await GetSessionSummaryAsync(session.Id, default);
            var expected = summary.ExpectedBalance;

            decimal closing = dto.ClosingBalance;
            decimal difference = closing - expected;

            session.ClosedAtUtc = DateTime.UtcNow;
            session.ClosedByUserId = _currentUser.UserId;
            session.ClosingBalance = closing;
            session.ExpectedBalance = expected;
            session.Difference = difference;
            session.Status = CashSessionStatus.Closed;
            session.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? session.Notes : dto.Notes.Trim();

            if (cashbox != null)
            {
                cashbox.CurrentBalance = closing;
            }

            _sessionRepo.Update(session);
            await _unitOfWork.SaveChangesAsync();

            return Map(session);
        }

        public async Task<CashSessionSummaryDto> GetSessionSummaryAsync(
            Guid sessionId,
            CancellationToken cancellationToken = default)
        {
            var session = await _context.CashSessions
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == sessionId && !x.IsDeleted,
                    cancellationToken)
                ?? throw new KeyNotFoundException($"Cash session {sessionId} not found.");

            var transactions = await _context.CashTransactions
                .AsNoTracking()
                .Where(x =>
                    x.CashSessionId == sessionId &&
                    !x.IsDeleted)
                .ToListAsync(cancellationToken);

            decimal cashSales = 0m;
            decimal cashRefunds = 0m;
            decimal payIns = 0m;
            decimal payOuts = 0m;
            decimal expenses = 0m;

            foreach (var tx in transactions)
            {
                if (tx.TransactionType == MovementType.SaleCashIn)
                    cashSales += tx.Amount;
                else if (tx.TransactionType == MovementType.SaleVoidCashOut)
                    cashRefunds += tx.Amount;
                else if (tx.TransactionType == MovementType.SessionPayIn)
                    payIns += tx.Amount;
                else if (tx.TransactionType == MovementType.SessionPayOut)
                    payOuts += tx.Amount;
                else if (tx.TransactionType == MovementType.Purchase)
                    expenses += tx.Amount;
            }

            var expected = session.OpeningBalance + cashSales - cashRefunds + payIns - payOuts - expenses;

            return new CashSessionSummaryDto
            {
                SessionId = session.Id,
                OpeningBalance = session.OpeningBalance,
                CashSales = cashSales,
                CashRefunds = cashRefunds,
                PayIns = payIns,
                PayOuts = payOuts,
                Expenses = expenses,
                ExpectedBalance = expected
            };
        }

        public async Task<CashTransaction> RecordPayInAsync(
            CashMovementDto dto,
            CancellationToken cancellationToken = default)
        {
            var session = await GetOpenSessionForCashboxAsync(dto.CashboxId, cancellationToken);

            var transaction = new CashTransaction
            {
                CashboxId = dto.CashboxId,
                CashSessionId = session.Id,
                Amount = dto.Amount,
                TransactionType = MovementType.SessionPayIn,
                SourceType = "CashSession",
                SourceId = session.Id,
                ReferenceNumber = dto.ReferenceNumber,
                Notes = dto.Reason,
                TransactionDate = DateTime.UtcNow
            };

            await _context.CashTransactions.AddAsync(transaction, cancellationToken);
            await _unitOfWork.SaveChangesAsync();
            return transaction;
        }

        public async Task<CashTransaction> RecordPayOutAsync(
            CashMovementDto dto,
            CancellationToken cancellationToken = default)
        {
            var session = await GetOpenSessionForCashboxAsync(dto.CashboxId, cancellationToken);

            var transaction = new CashTransaction
            {
                CashboxId = dto.CashboxId,
                CashSessionId = session.Id,
                Amount = dto.Amount,
                TransactionType = MovementType.SessionPayOut,
                SourceType = "CashSession",
                SourceId = session.Id,
                ReferenceNumber = dto.ReferenceNumber,
                Notes = dto.Reason,
                TransactionDate = DateTime.UtcNow
            };

            await _context.CashTransactions.AddAsync(transaction, cancellationToken);
            await _unitOfWork.SaveChangesAsync();
            return transaction;
        }

        private async Task<CashSession> GetOpenSessionForCashboxAsync(
            Guid cashboxId,
            CancellationToken cancellationToken)
        {
            var session = await _context.CashSessions
                .FirstOrDefaultAsync(
                    x => x.CashboxId == cashboxId &&
                         x.Status == CashSessionStatus.Open &&
                         !x.IsDeleted,
                    cancellationToken)
                ?? throw new KeyNotFoundException("No open cash session for this cashbox.");

            return session;
        }

        public async Task<List<CashSessionDto>> GetRecentSessionsAsync(Guid cashboxId, int take = 10)
        {
            var sessions = await _sessionRepo.GetRecentAsync(cashboxId, take);
            var list = new List<CashSessionDto>(sessions.Count());
            foreach (var s in sessions)
                list.Add(Map(s));
            return list;
        }

        private static CashSessionDto Map(CashSession s) => new()
        {
            Id = s.Id,
            CashboxId = s.CashboxId,
            OpenedAtUtc = s.OpenedAtUtc,
            OpenedByUserId = s.OpenedByUserId,
            OpeningBalance = s.OpeningBalance,
            ClosedAtUtc = s.ClosedAtUtc,
            ClosedByUserId = s.ClosedByUserId,
            ClosingBalance = s.ClosingBalance,
            ExpectedBalance = s.ExpectedBalance,
            Difference = s.Difference,
            Status = s.Status,
            Notes = s.Notes
        };
    }
}
