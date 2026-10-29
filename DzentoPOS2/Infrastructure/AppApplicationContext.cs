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
    private IServiceScope? _currentScope;
    private bool _loginSucceeded;

    public AppApplicationContext(
        IServiceScopeFactory scopeFactory,
        ICurrentUserService currentUser)
    {
        _scopeFactory = scopeFactory;
        _currentUser = currentUser;

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
        main.BeginInvoke(new Action(() =>
        {
            loginForm?.Dispose();
            loginScope?.Dispose();
        }));
    }

    private void DisposeCurrentScope()
    {
        _currentScope?.Dispose();
        _currentScope = null;
    }
}
