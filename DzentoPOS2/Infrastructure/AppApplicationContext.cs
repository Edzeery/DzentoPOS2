using DzentoPOS2.App.Session;
using DzentoPOS2.WinForms.Forms.Auth;
using DzentoPOS2.WinForms.Forms.Layout;
using Microsoft.Extensions.DependencyInjection;

namespace DzentoPOS2.WinForms.Infrastructure;

public class AppApplicationContext : ApplicationContext
{
    private Form? _currentForm;
     
    private readonly IServiceProvider _serviceProvider;
    private readonly ICurrentUserService _currentUser;
    private bool _loginSucceeded;
    public AppApplicationContext(
        IServiceProvider serviceProvider,
        ICurrentUserService currentUser)
    {
        _serviceProvider = serviceProvider;
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
        var login = _serviceProvider.GetRequiredService<LoginForm>();

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
            ExitThread();
        }
    }

    private void OnLoginSuccess(
        object? sender,
        UserSession session)
    {
        _loginSucceeded = true;

        _currentUser.SetUser(session);

        var main = _serviceProvider.GetRequiredService<MainForm>();

        main.FormClosed += (_, _) => ExitThread();

        _currentForm?.Hide();

        _currentForm = main;

        main.Show();
    }
}