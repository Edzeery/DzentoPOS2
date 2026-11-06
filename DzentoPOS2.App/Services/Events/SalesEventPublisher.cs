namespace DzentoPOS2.App.Services.Events;

/// <summary>
/// In-process, singleton event bus for the sales / cash-drawer flow. Services
/// raise an event <b>after</b> their database transaction committed, so any
/// subscriber that reads the database observes the committed state.
///
/// <para>
/// Subscribers own their lifetime: they must unsubscribe in <c>Dispose</c>.
/// WinForms subscribers marshal to the UI thread themselves (the publisher
/// never touches a control), so the publisher stays UI-agnostic and testable.
/// </para>
/// </summary>
public interface ISalesEventPublisher
{
    /// <summary>A cash or credit sale was committed.</summary>
    event EventHandler<SaleCompletedEventArgs>? SaleCompleted;

    /// <summary>A sale was voided (stock and cash reversed) and committed.</summary>
    event EventHandler<SaleVoidedEventArgs>? SaleVoided;

    /// <summary>A cash movement (pay in / pay out / linked cash row) was committed.</summary>
    event EventHandler<CashMovementEventArgs>? CashMovementAdded;

    /// <summary>Raises <see cref="SaleCompleted"/>. Call only after the commit.</summary>
    void PublishSaleCompleted(SaleCompletedEventArgs args);

    /// <summary>Raises <see cref="SaleVoided"/>. Call only after the commit.</summary>
    void PublishSaleVoided(SaleVoidedEventArgs args);

    /// <summary>Raises <see cref="CashMovementAdded"/>. Call only after the commit.</summary>
    void PublishCashMovement(CashMovementEventArgs args);
}

/// <inheritdoc cref="ISalesEventPublisher" />
public sealed class SalesEventPublisher : ISalesEventPublisher
{
    public event EventHandler<SaleCompletedEventArgs>? SaleCompleted;

    public event EventHandler<SaleVoidedEventArgs>? SaleVoided;

    public event EventHandler<CashMovementEventArgs>? CashMovementAdded;

    /// <summary>
    /// Raises <see cref="SaleCompleted"/>. Each subscriber is isolated: a
    /// throwing handler cannot stop the others (or the caller).
    /// </summary>
    public void PublishSaleCompleted(SaleCompletedEventArgs args) => Raise(SaleCompleted, args);

    /// <summary>Raises <see cref="SaleVoided"/>.</summary>
    public void PublishSaleVoided(SaleVoidedEventArgs args) => Raise(SaleVoided, args);

    /// <summary>Raises <see cref="CashMovementAdded"/>.</summary>
    public void PublishCashMovement(CashMovementEventArgs args) => Raise(CashMovementAdded, args);

    private static void Raise<T>(EventHandler<T>? handler, T args)
    {
        if (handler is null)
            return;

        // Copy the invocation list so a handler that unsubscribes while the
        // event is being raised cannot corrupt the iteration.
        foreach (EventHandler<T> subscriber in handler.GetInvocationList())
        {
            try
            {
                subscriber.Invoke(null, args);
            }
            catch
            {
                // A bad subscriber must never break the sale that was already
                // committed; it is intentionally swallowed here.
            }
        }
    }
}
