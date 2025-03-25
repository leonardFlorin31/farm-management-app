using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Windows.Input;

namespace licenta.ViewModel;

public class RegisterViewModel : ViewModelBase
{
    private string _username;
    private SecureString _password;
    private SecureString _passwordConfirm;
    private string _errorMessage;
    
    public event Action BackToLogin;
    
    public ICommand BackCommand { get; }
    
    public ICommand RegisterCommand { get; }
    
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
    
    public SecureString PasswordConfirm
    {
        get => _passwordConfirm;
        set
        {
            _passwordConfirm = value;
            OnPropertyChanged(nameof(PasswordConfirm));
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
    RegisterViewModel()
    {
        BackCommand = new RelayCommand(BackAction);
        RegisterCommand = new RelayCommand(Register);
    }

    private void Register()
    {
        // try
        // {
        //     // Convertim SecureString în string
        //     string password = SecureStringToString(Password);
        //     
        //     
        //
        //     // Trimitem cererea către server
        //     using (var client = new HttpClient())
        //     {
        //         var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
        //         var response = await client.PostAsync("http://localhost:5035/api/auth/login", content);
        //
        //         if (response.IsSuccessStatusCode)
        //         {
        //          Console.WriteLine("Registration successful");   
        //         }
        //         else
        //         {
        //             // Autentificare eșuată
        //             ErrorMessage = "Nume de utilizator sau parolă incorectă!";
        //         }
        //     }
        // }
        // catch (Exception ex)
        // {
        //     // Tratăm erorile de rețea sau alte excepții
        //     ErrorMessage = "Eroare la conectarea la server: " + ex.Message;
        // }
        BackAction();
    }

    private void BackAction()
    {
        BackToLogin?.Invoke();
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