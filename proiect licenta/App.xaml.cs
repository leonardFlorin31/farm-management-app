using System.Windows;
using GMap.NET;
using GMap.NET.MapProviders;
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
        GMapProvider.WebProxy = System.Net.WebRequest.DefaultWebProxy;
        GMapProvider.WebProxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
        GMaps.Instance.Mode = AccessMode.ServerAndCache;
    }
}