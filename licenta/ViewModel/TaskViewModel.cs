using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace licenta.ViewModel
{
    public class TaskViewModel : ViewModelBase
    {
        private string _newTaskTitle;
        private string _newTaskDescription;
        private string _newTaskStatus;
        private string _employee;
        private bool _canCreateTasks = true;  // Set based on user role
        public string _currentUsername = LoginViewModel.UsernameForUse.Username;
        
        public List<UsersNamesDTO> _usersList = new List<UsersNamesDTO>();

        public ObservableCollection<TaskItem> Tasks { get; } = new ObservableCollection<TaskItem>();

        public ObservableCollection<string> Employees { get; } = new ObservableCollection<string>();

        public Collection<string> StatusValues { get; } = new Collection<string>
        {
            "Asignat",
            "In Progres",
            "Completat"
        };
        
        public ICommand CreateTaskCmd { get; }

        public string NewTaskTitle
        {
            get => _newTaskTitle;
            set { _newTaskTitle = value; OnPropertyChanged(nameof(NewTaskTitle)); }
        }

        public string NewTaskDescription
        {
            get => _newTaskDescription;
            set { _newTaskDescription = value; OnPropertyChanged(nameof(NewTaskDescription)); }
        }

        public string NewTaskStatus
        {
            get => _newTaskStatus;
            set { _newTaskStatus = value; OnPropertyChanged(nameof(NewTaskStatus)); }
        }
        
        public string Employee
        {
            get => _employee;
            set { _employee = value; OnPropertyChanged(nameof(Employee)); }
        }

        public bool CanCreateTasks
        {
            get => _canCreateTasks;
            set { _canCreateTasks = value; OnPropertyChanged(nameof(CanCreateTasks)); }
        }

        public TaskViewModel()
        {
            CreateTaskCmd = new RelayCommand(AddNewTask);
            InitializeAsync();
           
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
                Employees.Clear();

                // Pas 2: Dacă s-au primit date, populează colecția
                if (_usersList != null)
                {
                    foreach (var user in _usersList)
                    {
                        string fullName = $"{user.Name} {user.LastName}";
                        Employees.Add(fullName);
                    }
                }
            });
        }


        private async void AddNewTask()
        {
            // Validare de bază în UI
            if (string.IsNullOrWhiteSpace(NewTaskTitle) || string.IsNullOrWhiteSpace(Employee))
            {
                // Afișează un mesaj de eroare utilizatorului, ex: folosind un MessageBox
                Console.WriteLine("Titlul și angajatul sunt obligatorii.");
                return;
            }

            Console.WriteLine($"[DEBUG] Căutare utilizator pentru Employee: '{Employee}'");

            // Afișează lista de nume complete așa cum sunt ele în _usersList
            Console.WriteLine("[DEBUG] Lista de utilizatori disponibili în _usersList:");
            foreach(var u in _usersList)
            {
                Console.WriteLine($" -> '{u.Name} {u.LastName}'");
            }
            // --- END DEBUGGING ---

            // Folosim o comparație robustă, care ignoră majusculele/minusculele și spațiile extra.
            var foundUser = _usersList.FirstOrDefault(user => 
                string.Equals($"{user.Name} {user.LastName}".Trim(), Employee.Trim(), StringComparison.OrdinalIgnoreCase));

            if (foundUser == null)
            {
                Console.WriteLine($"Eroare: Utilizatorul '{Employee}' nu a fost găsit în lista internă. Verifică log-urile de debug de mai sus.");
                return;
            }
        
            // Extrage username-ul utilizatorului găsit
            string assignedUsername = foundUser.Username;
            
            // 1. Creează obiectul DTO pentru a-l trimite la server
            var taskRequest = new TaskCreateRequest
            {
                Title = NewTaskTitle,
                Description = NewTaskDescription,
                Status = NewTaskStatus ?? "Asignat", // Default status
                CreatedByUsername = _currentUsername, // Username-ul utilizatorului logat
                AssignedToUsername = assignedUsername // Numele selectat din ComboBox
            };

            // 2. Trimite request-ul HTTP POST
            HttpClient client = new HttpClient();
            try
            {
                // PostAsJsonAsync serializează automat obiectul taskRequest în JSON
                // și setează header-ul Content-Type: application/json
                var response = await client.PostAsJsonAsync("https://localhost:7088/api/Task/create", taskRequest);

                if (response.IsSuccessStatusCode)
                {
                    // Dacă a avut succes, poți actualiza lista de task-uri local
                    // Fie adaugi manual (cum făceai înainte), fie reîncarci lista de la server.

                    // Momentan, adăugăm local pentru un răspuns rapid al UI-ului.
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Tasks.Add(new TaskItem
                        {
                            Title = NewTaskTitle,
                            Description = NewTaskDescription,
                            Status = NewTaskStatus ?? "Asignat",
                            AssignedToUsername = Employee,
                            CanChangeStatus = true
                        });

                        // Golește câmpurile de input după succes
                        NewTaskTitle = string.Empty;
                        NewTaskDescription = string.Empty;
                        NewTaskStatus = null;
                        Employee = null;
                    });

                    Console.WriteLine("Task creat cu succes pe server!");
                }
                else
                {
                    // Gestionează eroarea venită de la server
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Eroare de la server: {response.StatusCode} - {errorContent}");
                    // Aici ai putea afișa un mesaj de eroare mai prietenos utilizatorului
                }
            }
            catch (Exception ex)
            {
                // Gestionează erori de rețea etc.
                Console.WriteLine($"A apărut o excepție: {ex.Message}");
            }
        }

    }

    public class TaskItem : ViewModelBase
    {
        private string _status;
        
        public string Title { get; set; }
        public string AssignedToUsername { get; set; }
        public string Description { get; set; }
        
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }

        public bool CanChangeStatus { get; set; }
        
    }

    public class UsersNamesDTO
    {
        [JsonPropertyName("username")] 
        public string Username { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }
    }
    
    public class TaskCreateRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string CreatedByUsername { get; set; }
        public string AssignedToUsername { get; set; }
    }
    
  
}