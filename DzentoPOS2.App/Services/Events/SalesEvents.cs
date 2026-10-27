using DzentoPOS2.Core.Enums;

namespace DzentoPOS2.App.Services.Events;

/// <summary>
/// Minimal data carried by <see cref="ISalesEventPublisher.SaleCompleted"/>.
/// Every value is read from the data that was committed, so a subscriber can
/// trust it without re-querying (the publisher raises after the commit).
/// </summary>
public sealed record SaleCompletedEventArgs(
    Guid InvoiceId,
    string InvoiceNumber,
    decimal Total,
    decimal AppliedCash,
    string PaymentMethod,
    Guid CashboxId,
    Guid? CashSessionId,
    DateTime OccurredAtUtc);

/// <summary>Data carried by <see cref="ISalesEventPublisher.SaleVoided"/>.</summary>
public sealed record SaleVoidedEventArgs(
    Guid InvoiceId,
    string InvoiceNumber,
    decimal ReversedCash,
    Guid? CashSessionId,
    DateTime OccurredAtUtc);

/// <summary>Data carried by <see cref="ISalesEventPublisher.CashMovementAdded"/>.</summary>
public sealed record CashMovementEventArgs(
    Guid CashSessionId,
    Guid CashboxId,
    decimal Amount,
    MovementType MovementType,
    string SourceType,
    DateTime OccurredAtUtc);
