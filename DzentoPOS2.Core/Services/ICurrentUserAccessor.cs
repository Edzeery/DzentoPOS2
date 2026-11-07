namespace DzentoPOS2.Core.Services
{
    /// <summary>
    /// Exposes the identity of the signed-in user to infrastructure code that
    /// cannot depend on the application layer (e.g. EF Core interceptors).
    /// </summary>
    public interface ICurrentUserAccessor
    {
        Guid? UserId { get; }
    }
}