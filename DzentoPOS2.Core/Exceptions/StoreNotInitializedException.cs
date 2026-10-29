namespace DzentoPOS2.Core.Exceptions;

/// <summary>
/// Raised when an operation needs the store (branch / warehouse / cashbox)
/// to exist but the store has not been initialised yet. The store is created
/// once by the bootstrap step at startup; it is never created lazily in the
/// middle of a sale.
/// </summary>
public class StoreNotInitializedException : DomainException
{
    public StoreNotInitializedException()
        : base("The store is not initialized. No active branch, warehouse or cashbox is available.")
    {
    }

    public StoreNotInitializedException(string message)
        : base(message)
    {
    }
}
