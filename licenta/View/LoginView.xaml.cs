using System.Windows;
using System.Windows.Input;
using licenta.ViewModel;

namespace licenta.View;

public partial class LoginView : Window
{
    public LoginView()
    {
        InitializeComponent();
        
        
        if (this.DataContext is LoginViewModel loginViewModel)
        {
            loginViewModel.LoginSuccess += () =>
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            };

            loginViewModel.RegisterWindow += () =>
            {
                var registerView = new RegisterView();

                if (registerView.DataContext is RegisterViewModel registerViewModel)
                {
                    registerViewModel.BackToLogin += () =>
                    {
                        var newLoginView = new LoginView();
                        newLoginView.Show();
                        registerView.Close();
                    };
                }

                registerView.Show();
                this.Close();
            };
        }
    }

    private void Windows_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void BtnMinimize_OnClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void BtnClose_OnClick(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void BtnLogin_OnClickClick(object sender, RoutedEventArgs e)
    {
        
    }
    
}