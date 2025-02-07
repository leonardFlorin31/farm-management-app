using System.Windows;
using licenta.View;
using licenta.ViewModel;

namespace licenta;

public partial class App : Application
{
    private void ApplicationStart(object sender, StartupEventArgs e)
    {
        var loginView = new LoginView();
        loginView.Show();

        // Subscribe to the login success event
        if (loginView.DataContext is LoginViewModel loginViewModel)
        {
            loginViewModel.LoginSuccess += () =>
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
                loginView.Close();
            };
        }
    }
}