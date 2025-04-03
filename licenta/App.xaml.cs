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
        // if (loginView.DataContext is LoginViewModel loginViewModel)
        // {
        //     loginViewModel.LoginSuccess += () =>
        //     {
        //         var mainWindow = new MainWindow();
        //         mainWindow.Show();
        //         loginView.Close();
        //     };
        //
        //     loginViewModel.RegisterWindow += () =>
        //     {
        //         var registerView = new RegisterView();
        //
        //         // Subscribe to the BackToLogin event of RegisterViewModel.
        //         if (registerView.DataContext is RegisterViewModel registerViewModel)
        //         {
        //             registerViewModel.BackToLogin += () =>
        //             {
        //                 var newLoginView = new LoginView();
        //                 newLoginView.Show();
        //                 registerView.Close();
        //             };
        //         }
        //
        //         registerView.Show();
        //         loginView.Close();
        //     };
        // }

        
        GMapProvider.WebProxy = System.Net.WebRequest.DefaultWebProxy;
        GMapProvider.WebProxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
        GMaps.Instance.Mode = AccessMode.ServerAndCache;
    }
}