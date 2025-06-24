using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace licenta.ViewModel;

 public class HomeViewModel : ViewModelBase
    {
        // Proprietate pentru câmpul "username" din formularul de test
        private string _name;
        // Proprietate pentru numele complet afișat în mesajul de bun venit
        private string _userFullName;
        // Proprietate pentru rolul curent al utilizatorului
        public static string _currentUserRole = MainViewModel.CurrentRole.RoleName;
        
        public static string _currentRole = MainViewModel.CurrentRole.RoleName;
        // Lista de roluri pentru ComboBox
        private List<string> _roles = new List<string> { "Manager", "Angajat", "Contabil" };
        private string _selectedRole;
        // Lista de colegi
        private ObservableCollection<string> _colleagues = new ObservableCollection<string>();
        
        public List<UsersNamesDTO> _usersList = new List<UsersNamesDTO>();

        public string UserFullName
        {
            get => _userFullName;
            set
            {
                _userFullName = value;
                OnPropertyChanged(nameof(UserFullName));
            }
        }

        public string CurrentUserRole
        {
            get => _currentUserRole;
            set
            {
                _currentUserRole = value;
                OnPropertyChanged(nameof(CurrentUserRole));
            }
        }

        public ObservableCollection<string> Colleagues
        {
            get => _colleagues;
            set
            {
                _colleagues = value;
                OnPropertyChanged(nameof(Colleagues));
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
        
        public List<string> Roles
        {
            get => _roles;
            set
            {
                _roles = value;
                OnPropertyChanged(nameof(Roles));
            }
        }
        
        public string SelectedRole
        {
            get => _selectedRole;
            set
            {
                _selectedRole = value;
                OnPropertyChanged(nameof(SelectedRole));
            }
        }

        public ICommand TestCommand { get; set; }

        private readonly HttpClient _httpClient;
        private Guid _currentUserId;
        private string _currentUsername = LoginViewModel.UsernameForUse.Username;

        public HomeViewModel()
        {
            CurrentUserRole = _currentUserRole ?? "Neinitializat";

            InitializeAsync();
            
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7088/")
            };

            Colleagues = new ObservableCollection<string>();
            InitializeUser();
            TestCommand = new RelayCommand(async () => await Test());
        }

        private async Task InitializeAsync()
        {
            await InitiateUsersNames();
        }

        public async Task InitiateUsersNames()
        {
            HttpClient client = new HttpClient();

            try
            {
                var response = await client.GetAsync($"https://localhost:7088/api/Auth/users-by-role-creator/{_currentUsername}")
                    .ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    _usersList = JsonSerializer.Deserialize<List<UsersNamesDTO>>(json, options);
                }
                else
                {
                    Console.WriteLine($"Eroare la preluarea datelor. Status: {response.StatusCode}");
                }
            }
            catch (Exception ex) // Prinde orice excepție (rețea, JSON, etc.)
            {
                Console.WriteLine($"A apărut o eroare: {ex.Message}");
            }

            // Acum, indiferent de rezultat, actualizează UI-ul în siguranță
            App.Current.Dispatcher.Invoke(() =>
            {
                // Pas 1: Golește colecția existentă (pe firul de UI)
                Colleagues.Clear();

                // Pas 2: Dacă s-au primit date, populează colecția
                if (_usersList != null)
                {
                    foreach (var user in _usersList)
                    {
                        string fullName = $"{user.Name} {user.LastName}";
                        Colleagues.Add(fullName);
                    }
                }
            });
        }

        private async void InitializeUser()
        {
            var client = new HttpClient();
            try
            {
                var response = await client.GetAsync($"http://localhost:5035/api/auth/{_currentUsername}");
                response.EnsureSuccessStatusCode();

                var userJson = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var userDto = JsonSerializer.Deserialize<MapViewModel.UserDto>(userJson, options);

                if (userDto == null || userDto.Id == Guid.Empty)
                {
                    MessageBox.Show("Failed to fetch current user information.");
                    return;
                }

                UserFullName = userDto.Name;
                _currentUserId = userDto.Id;
                
                
            }
            catch (Exception ex)
            {
                UserFullName = "Utilizator Indisponibil";
                CurrentUserRole = "N/A";
                Console.WriteLine($"API call failed: {ex.Message}");
            }
        }
        
        private async Task Test()
        {
            Console.WriteLine($"Testing assigning role '{SelectedRole}' to user '{Name}'...");
            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(SelectedRole))
            {
                MessageBox.Show("Vă rugăm introduceți numele de utilizator și selectați un rol.", "Date incomplete", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
                    MessageBox.Show($"Rolul '{SelectedRole}' a fost atribuit cu succes utilizatorului '{Name}'.", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                    Console.WriteLine($"Success: {responseBody}");
                }
                else
                {
                    MessageBox.Show($"A apărut o eroare: {responseBody}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                    Console.WriteLine($"Error ({response.StatusCode}): {responseBody}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception calling API: {ex.Message}");
                MessageBox.Show($"A apărut o excepție: {ex.Message}", "Eroare API", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
    }