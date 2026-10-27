using DzentoPOS2.App.Services.Accounting;
using DzentoPOS2.App.Services.Events;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.App.Session;
using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Core.Exceptions;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Accounting.Interfaces;
using DzentoPOS2.Data.Repositories.Common.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Accounting;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.App.Services.Accounting
{
    /// <summary>
    /// Cash-drawer lifecycle. <see cref="GetSessionSummaryAsync"/> is the single
    /// source of truth for the expected drawer balance: the dashboard reads it
    /// and <see cref="CloseSessionAsync"/> stores exactly the same number, so the
    /// two can never disagree.
    /// </summary>
    public class CashSessionService : ICashSessionService
    {
        private const string CashPaymentMethod = "Cash";
        private const string ExpenseSourceType = "Expense";

        private readonly ICashSessionRepository _sessionRepo;
        private readonly ICashboxRepository _cashboxRepo;
        private readonly ICashTransactionRepository _transactionRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly AppDbContext _context;
        private readonly IBusinessClock _clock;
        private readonly ISalesEventPublisher _events;

        public CashSessionService(
            ICashSessionRepository sessionRepo,
            ICashboxRepository cashboxRepo,
            ICashTransactionRepository transactionRepo,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            AppDbContext context,
            IBusinessClock clock,
            ISalesEventPublisher events)
        {
            _sessionRepo = sessionRepo;
            _cashboxRepo = cashboxRepo;
            _transactionRepo = transactionRepo;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _context = context;
            _clock = clock;
            _events = events;
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
            // The filtered unique index is the last line of defence, but the
            // service still rejects a second open session with a stable code so
            // the UI can show a localized message instead of a raw UNIQUE error.
            var existing = await _sessionRepo.GetOpenByCashboxAsync(dto.CashboxId);
            if (existing != null)
                throw new CodedDomainException(
                    DomainErrorCodes.SessionAlreadyOpen,
                    "A cash session is already open for this cashbox.");

            var cashbox = await _cashboxRepo.GetByIdAsync(dto.CashboxId)
                ?? throw new CodedDomainException(
                    DomainErrorCodes.CashboxNotFound,
                    $"Cashbox {dto.CashboxId} not found.");

            var session = new CashSession
            {
                CashboxId = dto.CashboxId,
                OpenedAtUtc = _clock.UtcNow,
                OpenedByUserId = _currentUser.UserId,
                OpeningBalance = dto.OpeningBalance,
                Status = CashSessionStatus.Open,
                Notes = dto.Notes?.Trim()
            };

            await _sessionRepo.AddAsync(session);
            await _unitOfWork.SaveChangesAsync();

            return Map(session);
        }

        /// <summary>
        /// Closes the session using <see cref="GetSessionSummaryAsync"/> as the
        /// expected balance. A session can be closed only once: the second call
        /// is rejected instead of overwriting the stored closing figures.
        /// </summary>
        public async Task<CashSessionDto> CloseSessionAsync(CloseCashSessionDto dto)
        {
            var session = await _context.CashSessions
                .FirstOrDefaultAsync(s => s.Id == dto.SessionId && !s.IsDeleted)
                ?? throw new CodedDomainException(
                    DomainErrorCodes.SessionNotFound,
                    $"Cash session {dto.SessionId} not found.");

            if (session.Status == CashSessionStatus.Closed)
                throw new CodedDomainException(
                    DomainErrorCodes.SessionAlreadyClosed,
                    "This cash session is already closed.");

            var cashbox = await _context.Cashboxes
                .FirstOrDefaultAsync(c => c.Id == session.CashboxId && !c.IsDeleted);

            // Single source of truth: exactly what the dashboard shows.
            var summary = await GetSessionSummaryAsync(session.Id);
            var expected = summary.ExpectedBalance;

            decimal closing = dto.ClosingBalance;
            decimal difference = closing - expected;

            session.ClosedAtUtc = _clock.UtcNow;
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
                ?? throw new CodedDomainException(
                    DomainErrorCodes.SessionNotFound,
                    $"Cash session {sessionId} not found.");

            // Sales are read from the invoices, not from the cash rows, so a
            // card sale shows up in NonCashSalesByMethod and never in CashSales.
            var invoices = await _context.SalesInvoices
                .AsNoTracking()
                .Where(x =>
                    x.CashSessionId == sessionId &&
                    !x.IsDeleted)
                .Select(x => new { x.Id, x.IsVoided, Total = x.Total.Amount })
                .ToListAsync(cancellationToken);

            var liveInvoiceIds = invoices
                .Where(x => !x.IsVoided)
                .Select(x => x.Id)
                .ToList();

            var payments = liveInvoiceIds.Count == 0
                ? new List<PaymentRow>()
                : await _context.SalesPayments
                    .AsNoTracking()
                    .Where(p =>
                        liveInvoiceIds.Contains(p.SalesInvoiceId) &&
                        !p.IsDeleted)
                    .Select(p => new PaymentRow(p.PaymentMethod, p.Amount))
                    .ToListAsync(cancellationToken);

            var nonCash = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            foreach (var payment in payments)
            {
                // Cash is read from the cash movements below; only the
                // non-cash split needs the payment rows.
                if (IsCash(payment.Method))
                    continue;

                var method = string.IsNullOrWhiteSpace(payment.Method)
                    ? "Other"
                    : payment.Method.Trim();

                nonCash[method] = nonCash.GetValueOrDefault(method) + payment.Amount;
            }

            // Movements of the session: cash refunds, pay in / pay out and
            // expenses. Cash refunds are kept separate from CashSales so a void
            // reverses the original cash-in without double counting.
            var transactions = await _context.CashTransactions
                .AsNoTracking()
                .Where(x => x.CashSessionId == sessionId && !x.IsDeleted)
                .Select(x => new { x.TransactionType, x.Amount, x.SourceType })
                .ToListAsync(cancellationToken);

            decimal cashSales = 0m;
            decimal cashRefunds = 0m;
            decimal payIns = 0m;
            decimal payOuts = 0m;
            decimal expenses = 0m;

            foreach (var tx in transactions)
            {
                switch (tx.TransactionType)
                {
                    // Cash actually taken for sales, read from the cash
                    // movements (not the payment rows) and summed in memory:
                    // SQLite cannot aggregate a decimal server-side. A voided
                    // sale keeps its cash-in (gross) and gains an equal
                    // cash-out in CashRefunds, so the drawer still nets to
                    // zero, while payment rows only drive the card split.
                    case MovementType.SaleCashIn:
                        cashSales += tx.Amount;
                        break;
                    case MovementType.SaleVoidCashOut:
                        cashRefunds += tx.Amount;
                        break;
                    case MovementType.SessionPayIn:
                        payIns += tx.Amount;
                        break;
                    case MovementType.SessionPayOut:
                        payOuts += tx.Amount;
                        break;
                    case MovementType.Purchase:
                        if (string.Equals(tx.SourceType, ExpenseSourceType, StringComparison.OrdinalIgnoreCase))
                            expenses += tx.Amount;
                        break;
                }
            }

            var totalSales = invoices.Where(x => !x.IsVoided).Sum(x => x.Total);
            var invoiceCount = invoices.Count(x => !x.IsVoided);
            var voidCount = invoices.Count(x => x.IsVoided);

            var expected = session.OpeningBalance
                + cashSales
                - cashRefunds
                + payIns
                - payOuts
                - expenses;

            return new CashSessionSummaryDto
            {
                SessionId = session.Id,
                CashboxId = session.CashboxId,
                OpeningBalance = session.OpeningBalance,
                TotalSales = totalSales,
                CashSales = cashSales,
                NonCashSalesByMethod = nonCash,
                CashRefunds = cashRefunds,
                PayIns = payIns,
                PayOuts = payOuts,
                Expenses = expenses,
                InvoiceCount = invoiceCount,
                VoidCount = voidCount,
                ExpectedBalance = expected
            };
        }

        public async Task<CashTransaction> RecordPayInAsync(
            CashMovementDto dto,
            CancellationToken cancellationToken = default)
        {
            return await RecordMovementAsync(dto, MovementType.SessionPayIn, cancellationToken);
        }

        public async Task<CashTransaction> RecordPayOutAsync(
            CashMovementDto dto,
            CancellationToken cancellationToken = default)
        {
            return await RecordMovementAsync(dto, MovementType.SessionPayOut, cancellationToken);
        }

        private async Task<CashTransaction> RecordMovementAsync(
            CashMovementDto dto,
            MovementType movementType,
            CancellationToken cancellationToken)
        {
            var session = await GetOpenSessionForCashboxAsync(dto.CashboxId, cancellationToken);

            var transaction = new CashTransaction
            {
                CashboxId = dto.CashboxId,
                CashSessionId = session.Id,
                Amount = dto.Amount,
                TransactionType = movementType,
                SourceType = "CashSession",
                SourceId = session.Id,
                ReferenceNumber = dto.ReferenceNumber,
                Notes = dto.Reason,
                TransactionDate = _clock.UtcNow
            };

            await _context.CashTransactions.AddAsync(transaction, cancellationToken);
            await _unitOfWork.SaveChangesAsync();

            // Raised after the row is committed so a subscriber that reads the
            // database observes it.
            _events.PublishCashMovement(new CashMovementEventArgs(
                session.Id,
                transaction.CashboxId,
                transaction.Amount,
                transaction.TransactionType,
                transaction.SourceType ?? "CashSession",
                _clock.UtcNow));

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
                ?? throw new CodedDomainException(
                    DomainErrorCodes.NoOpenSession,
                    "No open cash session for this cashbox.");

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

        private static bool IsCash(string? method)
            => string.IsNullOrWhiteSpace(method) ||
               string.Equals(method.Trim(), CashPaymentMethod, StringComparison.OrdinalIgnoreCase);

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

        private sealed record PaymentRow(string Method, decimal Amount);
    }
}