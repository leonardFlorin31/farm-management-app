using System.Net;
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
    private string _email;
    private string _name;
    private string _lastName;
    private bool _isAdmin;

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

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    public string LastName
    {
        get => _lastName;
        set
        {
            _lastName = value;
            OnPropertyChanged(nameof(LastName));
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

    public string Email
    {
        get => _email;
        set
        {
            _email = value;
            OnPropertyChanged(nameof(Email));
        }
    }

    public bool IsAdmin
    {
        get => _isAdmin;
        set
        {
            _isAdmin = value;
            OnPropertyChanged(nameof(IsAdmin));
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
        RegisterCommand = new RelayCommand(async () => await RegisterAsync());
    }

    private async Task RegisterAsync()
    {
        ErrorMessage = string.Empty;

        // 1) Validări client‑side
        if (string.IsNullOrWhiteSpace(Username) ||
            string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Name) ||
            string.IsNullOrWhiteSpace(LastName) ||
            Password?.Length == 0)
        {
            ErrorMessage = "Toate câmpurile sunt obligatorii.";
            return;
        }

        // 2) Pregătește request‑ul
        var passwordPlain = SecureStringToString(Password);
        var registerDto = new
        {
            Name = Name.Trim(),
            LastName = LastName.Trim(),
            Username = Username.Trim(),
            Email = Email.Trim(),
            Password = passwordPlain,
            Role = IsAdmin
        };

        var json = JsonSerializer.Serialize(registerDto, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // 3) Trimite POST-ul
        try
        {
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync("https://localhost:7088/api/Auth/register", content);

                if (response.StatusCode == HttpStatusCode.Created)
                {
                    // Înregistrare OK -> te întorci la login
                    IsAdmin = false;
                    BackAction();
                }
                else if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    // 409 Conflict -> username sau email deja existent
                    var msg = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Eroare: {msg}";
                }
                else
                {
                    // alt cod de eroare
                    var msg = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Server error ({(int)response.StatusCode}): {msg}";
                }
            } ;
        }
        catch (Exception ex)
        {
            ErrorMessage = "Nu s-a putut conecta la server: " + ex.Message;
        }
    }

    private void Register()
    {
        // try
        // {
        //     // Convertim SecureString în string
        //     
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

        //string password = SecureStringToString(Password);

        // if(IsAdmin == true)
        // Console.WriteLine("true");
        // else
        // {
        //     Console.WriteLine("false");
        // }

        
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