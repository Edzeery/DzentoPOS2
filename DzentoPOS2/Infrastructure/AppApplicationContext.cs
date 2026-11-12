using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.App.Session;
using DzentoPOS2.WinForms.Forms.Auth;
using DzentoPOS2.WinForms.Forms.Layout;
using Microsoft.Extensions.DependencyInjection;

namespace DzentoPOS2.WinForms.Infrastructure;

public class AppApplicationContext : ApplicationContext
{
    private Form? _currentForm;

    // A top-level window (login, shell) owns the DI scope it was resolved
    // from; the scope is disposed when the window is replaced or the login
    // fails, so no scoped service (DbContext, unit of work, ...) is kept
    // alive for the whole application lifetime.
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ICurrentUserService _currentUser;
    private readonly IActiveBranchContext _activeBranch;
    private IServiceScope? _currentScope;
    private bool _loginSucceeded;

    public AppApplicationContext(
        IServiceScopeFactory scopeFactory,
        ICurrentUserService currentUser,
        IActiveBranchContext activeBranch)
    {
        _scopeFactory = scopeFactory;
        _currentUser = currentUser;
        _activeBranch = activeBranch;

        ShowSplash();
    }

    private void ShowSplash()
    {
        var splash = new SplashForm();

        splash.FormClosed += (_, _) => ShowLogin();

        _currentForm = splash;
        splash.Show();
    }

    private void ShowLogin()
    {
        _currentScope = _scopeFactory.CreateScope();

        var login = _currentScope.ServiceProvider.GetRequiredService<LoginForm>();

        login.LoginSuccess += OnLoginSuccess;
        login.FormClosed += Login_FormClosed;

        _currentForm = login;
        login.Show();
    }

    private void Login_FormClosed(
        object? sender,
        FormClosedEventArgs e)
    {
        if (!_loginSucceeded)
        {
            DisposeCurrentScope();
            ExitThread();
        }
    }

    private void OnLoginSuccess(
        object? sender,
        UserSession session)
    {
        _loginSucceeded = true;

        _currentUser.SetUser(session);

        // Initialize active branch context before showing MainForm.
        // If no branch can be resolved the context stays initialized with
        // an empty branch so the shell still opens: the admin is warned but
        // not locked out.
        bool noActiveBranch;
        try
        {
            _activeBranch.InitializeAsync(_currentUser).GetAwaiter().GetResult();
            noActiveBranch = _activeBranch.ActiveBranchId == Guid.Empty;
        }
        catch (Exception ex)
        {
            DzentoPOS2.App.Diagnostics.ExceptionLogger.Write(ex, "AppApplicationContext.InitializeActiveBranch");
            // Leave the context initialized with an empty branch so getters do
            // not throw while the shell opens with its warning.
            _activeBranch.InitializeEmpty();
            noActiveBranch = true;
        }

        var loginForm = _currentForm;
        var loginScope = _currentScope;

        _currentScope = _scopeFactory.CreateScope();

        var main = _currentScope.ServiceProvider.GetRequiredService<MainForm>();

        main.FormClosed += (_, _) =>
        {
            DisposeCurrentScope();
            ExitThread();
        };

        loginForm?.Hide();

        _currentForm = main;

        main.Show();

        // The login form raised this event, so it (and its scope) must not be
        // disposed while the event is still unwinding. Queue the teardown
        // until the current message is processed.
        main.BeginInvoke(new Action(async () =>
        {
            loginForm?.Dispose();
            if (loginScope is not null)
            {
                // UnitOfWork (and other scoped services) may only implement
                // IAsyncDisposable; dispose the scope asynchronously to avoid
                // "Use DisposeAsync to dispose the container."
                if (loginScope is IAsyncDisposable asyncDisposable)
                    await asyncDisposable.DisposeAsync();
                else
                    loginScope.Dispose();
            }

            if (noActiveBranch)
            {
                DzentoPOS2.WinForms.Helpers.AppMessageBox.Warning(
                    DzentoPOS2.Localization.Resources.AppStrings.NoActiveBranchMessage,
                    DzentoPOS2.Localization.Resources.AppStrings.Warning);
            }
        }));
    }

    private void DisposeCurrentScope()
    {
        _currentScope?.Dispose();
        _currentScope = null;
    }
}
