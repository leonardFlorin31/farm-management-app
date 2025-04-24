using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace licenta.ViewModel;

public class HomeViewModel:ViewModelBase
{
    private string _name;
    private string _email;

    private List<string> _roles = new List<string> {"Manager", "Angajat", "Contabil"};
    private string _selectedRole;
    public string SelectedRole
    {
        get => _selectedRole;
        set
        {
            _selectedRole = value;
            OnPropertyChanged(nameof(SelectedRole));
        }
    }
    
    public List<string> Roles
    {
        get => _roles;
        set
        {
            _roles = value;
            OnPropertyChanged(nameof(Roles));
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            Console.WriteLine($"Name set to: {_name}");
            OnPropertyChanged(nameof(Name));
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            _email = value;
            Console.WriteLine($"Email set to: {_email}");
            OnPropertyChanged(nameof(Email));
        }
    }

    public ICommand TestCommand { get; set; }
    
    private readonly HttpClient _httpClient;
    private Guid _currentUserId;
    private string _currentUsername = LoginViewModel.UsernameForUse.Username;

    public HomeViewModel()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7088/")
        };

        InitializeUser();
        TestCommand = new RelayCommand(async () => await Test());
        Console.WriteLine("HomeViewModel initialized");
    }

    private async void  InitializeUser()
    {
        var client = new HttpClient();
        // Fetch the current user's ID using the username
        var response = await client.GetAsync($"http://localhost:5035/api/auth/{_currentUsername}");
        var userJson = await response.Content.ReadAsStringAsync();

        // Use case-insensitive deserialization for the user DTO
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var userDto = JsonSerializer.Deserialize<MapViewModel.UserDto>(userJson, options);
        if (userDto == null || userDto.Id == Guid.Empty)
        {
            MessageBox.Show("Failed to fetch current user information.");
            return;
        }

        _currentUserId = userDto.Id;
    }

    private async Task Test()
    {
        
        
        Console.WriteLine($"Testing assigning role '{SelectedRole}' to user '{Name}'...");

        // Prepare the request payload
        var payload = new
        {
            Username = Name,
            RoleName = SelectedRole,
            CreatedBy = _currentUserId
        };

        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync("api/UserRole/assign", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Success: {responseBody}");
            }
            else
            {
                Console.WriteLine($"Error ({response.StatusCode}): {responseBody}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception calling API: {ex.Message}");
        }
    }
}