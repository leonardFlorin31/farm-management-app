using System;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows.Input;
using licenta.Model;
using licenta.View;

namespace licenta.ViewModel;

public class LoginViewModel : ViewModelBase
{
    // Fields
    private string _username;
    private SecureString _password;
    private string _errorMessage;
    private bool _isViewVisible = true;

    // Events
    public event Action LoginSuccess;
    public event Action RegisterWindow;

    // Properties
    public string Username
    {
        get => _username;
        set
        {
            _username = value;
            OnPropertyChanged(nameof(Username));
        }
    }

    public SecureString Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged(nameof(Password));
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged(nameof(ErrorMessage));
        }
    }

    public bool IsViewVisible
    {
        get => _isViewVisible;
        set
        {
            _isViewVisible = value;
            OnPropertyChanged(nameof(IsViewVisible));
        }
    }

    // Commands
    public ICommand LoginCommand { get; }
    public ICommand RecoverPasswordCommand { get; }
    
    public ICommand RegisterCommand { get; }
    

    // Constructors
    public LoginViewModel()
    {
        LoginCommand = new ViewModelCommand(ExecuteLoginCommand, CanExecuteLoginCommand);
        RecoverPasswordCommand = new ViewModelCommand(p => ExecuteRecoverPasswordCommand("", ""));
        RegisterCommand = new RelayCommand(Register);
       
    }

    private void Register()
    {
    RegisterWindow?.Invoke();
    }

    // Methods
    private async void ExecuteLoginCommand(object obj)
    {
        try
        {
            // Convertim SecureString în string
            string password = SecureStringToString(Password);

            // Creăm obiectul pentru cererea de autentificare
            var loginRequest = new { Username = this.Username, Password = password };

            // Trimitem cererea către server
            using (var client = new HttpClient())
            {
                var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:5035/api/auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    // Autentificare reușită
                    Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(Username), null);
                    IsViewVisible = false;
                    
                    UsernameForUse.Username = Username;
                    // Notificăm că autentificarea a avut succes
                    LoginSuccess?.Invoke();
                }
                else
                {
                    // Autentificare eșuată
                    ErrorMessage = "Nume de utilizator sau parolă incorectă!";
                }
            }
        }
        catch (Exception ex)
        {
            // Tratăm erorile de rețea sau alte excepții
            ErrorMessage = "Eroare la conectarea la server: " + ex.Message;
        }
    }

    private bool CanExecuteLoginCommand(object obj)
    {
        bool validData;
        if (string.IsNullOrEmpty(Username) || Username.Length < 3 || Password == null
            || Password.Length < 3)
        {
            validData = false;
        }
        else
        {
            validData = true;
        }
        return validData;
    }

    private void ExecuteRecoverPasswordCommand(string username, string email)
    {
        // Logica pentru recuperarea parolei
    }

    private string SecureStringToString(SecureString secureString)
    {
        IntPtr ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.SecureStringToGlobalAllocUnicode(secureString);
            return Marshal.PtrToStringUni(ptr);
        }
        finally
        {
            Marshal.ZeroFreeGlobalAllocUnicode(ptr);
        }
    }

    public static class UsernameForUse
    {
        public static string Username { get; set; }
    }
    
}