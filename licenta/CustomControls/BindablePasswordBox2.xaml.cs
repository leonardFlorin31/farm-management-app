using System.Security;
using System.Windows;
using System.Windows.Controls;

namespace licenta.CustomControls;

public partial class BindablePasswordBox2 : UserControl
{
    public static readonly DependencyProperty PasswordProperty= 
        DependencyProperty.Register("Password", typeof(SecureString), typeof(BindablePasswordBox));
    
    public SecureString Password
    {
        get => (SecureString)GetValue(PasswordProperty);
        set => SetValue(PasswordProperty, value);
    }
    
    public BindablePasswordBox2()
    {
        InitializeComponent();
        txtPassword.PasswordChanged += OnPasswordChanged;
    }
    
    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        Password = txtPassword.SecurePassword;
    }
}