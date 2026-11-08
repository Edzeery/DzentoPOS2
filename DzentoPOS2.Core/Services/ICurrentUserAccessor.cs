namespace DzentoPOS2.Core.Services
{
    public interface ICurrentUserAccessor
    {
        Guid? UserId { get; }
    }
}