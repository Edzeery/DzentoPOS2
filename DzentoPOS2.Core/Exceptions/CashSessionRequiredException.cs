namespace DzentoPOS2.Core.Exceptions;

public sealed class CashSessionRequiredException : DomainException
{
    public CashSessionRequiredException()
        : base("An open cash session is required before completing a sale.")
    {
    }
}
