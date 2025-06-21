using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
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

        public ICommand CreateTaskCmd => new RelayCommand(AddNewTask);

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
            InitializeAsync();
           
        }

        private async Task InitializeAsync()
        {
            await InitiateUsersNames();
        }

        public async Task InitiateUsersNames()
        {
            HttpClient client = new HttpClient();
            List<UsersNamesDTO> usersList = null; // Variabilă locală

            try
            {
                var response = await client.GetAsync($"https://localhost:7088/api/Auth/users-by-role-creator/{_currentUsername}")
                    .ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    usersList = JsonSerializer.Deserialize<List<UsersNamesDTO>>(json, options);
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
                if (usersList != null)
                {
                    foreach (var user in usersList)
                    {
                        string fullName = $"{user.Name} {user.LastName}";
                        Employees.Add(fullName);
                    }
                }
            });
        }


        private void AddNewTask()
        {
            if (!string.IsNullOrWhiteSpace(NewTaskTitle))
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                Tasks.Add(new TaskItem
                {
                    Title = NewTaskTitle,
                    Description = NewTaskDescription,
                    Status = NewTaskStatus ?? "Not Started",
                    AssignedToUsername = Employee, // You'll need to implement user assignment
                    CanChangeStatus = true
                });
                });

                // Clear input fields
                NewTaskTitle = string.Empty;
                NewTaskDescription = string.Empty;
                NewTaskStatus = null;
                
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
    
  
}