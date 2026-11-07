namespace DzentoPOS2.Core.Services
{
    /// <summary>
    /// Ambient holder for the signed-in user's id. Stored as a simple static
    /// value because the application is single-user per process; this lets
    /// infrastructure code (EF Core interceptors) read the current user without
    /// depending on the application layer or the DbContext's service provider.
    /// </summary>
    public static class CurrentUserContext
    {
        public static Guid? UserId { get; set; }
    }
}