using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Data;
using System.Windows.Input;

namespace licenta.ViewModel
{
    public class TaskViewModel : ViewModelBase
    {
        private string _newTaskTitle;
        private string _newTaskDescription;
        private string _newTaskStatus;
        private string _employee;
        public string _currentUsername = LoginViewModel.UsernameForUse.Username;
        private string _filterTitle;
        private string _filterEmployee;
        private string _filterStatus;
        public static string _currentRole = MainViewModel.CurrentRole.RoleName;
        private string _currentUserFullName;
        
        
        
        public List<UsersNamesDTO> _usersList = new List<UsersNamesDTO>();

        public ObservableCollection<TaskItem> Tasks { get; } = new ObservableCollection<TaskItem>();

        public ObservableCollection<string> Employees { get; } = new ObservableCollection<string>();
        
        public ICollectionView FilteredTasks { get; }

        public Collection<string> StatusValues { get; } = new Collection<string>
        {
            "Asignat",
            "In Progres",
            "Completat"
        };
        
        public ICommand CreateTaskCmd { get; }
        public ICommand DeleteTaskCmd { get; }
        
        public ICommand ResetFiltersCmd { get; }

        public bool CanCreateTasks => _currentRole != null && 
                                      (_currentRole.Equals("admin", StringComparison.OrdinalIgnoreCase) || 
                                       _currentRole.Equals("manager", StringComparison.OrdinalIgnoreCase));

        public string FilterTitle
        {
            get => _filterTitle;
            set { _filterTitle = value; OnPropertyChanged(nameof(FilterTitle)); ApplyFilter(); }
        }

        public string FilterEmployee
        {
            get => _filterEmployee;
            set { _filterEmployee = value; OnPropertyChanged(nameof(FilterEmployee)); ApplyFilter(); }
        }

        public string FilterStatus
        {
            get => _filterStatus;
            set { _filterStatus = value; OnPropertyChanged(nameof(FilterStatus)); ApplyFilter(); }
        }
        
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

        public TaskViewModel()
        {
            
            FilteredTasks = CollectionViewSource.GetDefaultView(Tasks);
            FilteredTasks.Filter = FilterPredicate;
            
            Tasks.CollectionChanged += Tasks_CollectionChanged;
            CreateTaskCmd = new RelayCommand(AddNewTask);
            DeleteTaskCmd = new RelayCommand(DeleteTask);
            ResetFiltersCmd = new RelayCommand(ResetFilters);
           
            InitializeAsync();
           
        }
        
        private void ResetFilters()
        {
            FilterTitle = string.Empty;
            FilterEmployee = null; 
            FilterStatus = null;
        }
        
        private bool FilterPredicate(object item)
        {
            if (item is TaskItem task)
            {
                // Verifică fiecare filtru. Dacă un filtru este gol, se consideră că nu se aplică.
                bool titleMatch = string.IsNullOrWhiteSpace(FilterTitle) ||
                                  task.Title.Contains(FilterTitle, StringComparison.OrdinalIgnoreCase);

                bool employeeMatch = string.IsNullOrWhiteSpace(FilterEmployee) ||
                                     task.AssignedToUsername.Equals(FilterEmployee, StringComparison.OrdinalIgnoreCase);

                bool statusMatch = string.IsNullOrWhiteSpace(FilterStatus) ||
                                   task.Status.Equals(FilterStatus, StringComparison.OrdinalIgnoreCase);

                // Task-ul este vizibil doar dacă trece de toate filtrele active.
                return titleMatch && employeeMatch && statusMatch;
            }
            return false;
        }
        
        private void ApplyFilter()
        {
            FilteredTasks?.Refresh();
        }

         private async void DeleteTask()
        {
            
            if (!CanCreateTasks)
            {
                Console.WriteLine("Acțiune interzisă: Doar un manager sau admin poate șterge task-uri.");
                return;
            }
            
            // Validare pentru câmpurile necesare
            if (string.IsNullOrWhiteSpace(NewTaskTitle) || string.IsNullOrWhiteSpace(Employee))
            {
                Console.WriteLine("Pentru a șterge, trebuie să specificați titlul task-ului și angajatul.");
                // Aici poți afișa un mesaj de eroare utilizatorului.
                return;
            }

            // Găsește username-ul corespunzător numelui complet selectat
            var foundUser = _usersList.FirstOrDefault(user =>
                string.Equals($"{user.Name} {user.LastName}".Trim(), Employee.Trim(), StringComparison.OrdinalIgnoreCase));

            if (foundUser == null)
            {
                Console.WriteLine($"Eroare: Utilizatorul '{Employee}' nu a fost găsit.");
                return;
            }
            
            string assignedUsername = foundUser.Username;
            string titleToDelete = NewTaskTitle;

            // Construiește URL-ul pentru request, encodând parametrii pentru siguranță
            string requestUrl = $"https://localhost:7088/api/Task/delete?title={Uri.EscapeDataString(titleToDelete)}&username={Uri.EscapeDataString(assignedUsername)}";

            HttpClient client = new HttpClient();
            try
            {
                var response = await client.DeleteAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Task șters cu succes de pe server.");
                    // Reîncarcă lista de task-uri pentru a reflecta schimbarea în UI
                    await LoadTasksAsync();
                    
                    // Golește câmpurile de input
                    NewTaskTitle = string.Empty;
                    Employee = null;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Eroare la ștergerea task-ului: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepție la ștergerea task-ului: {ex.Message}");
            }
        }

        private async Task InitializeAsync()
        {
            await InitiateUsersNames();
            
            await LoadTasksAsync();
        }
        
        public async Task LoadTasksAsync()
        {
            HttpClient client = new HttpClient();
            try
            {
                var response = await client.GetAsync($"https://localhost:7088/api/Task/user/{_currentUsername}");
                if (response.IsSuccessStatusCode)
                {
                    var tasksFromServer = await response.Content.ReadFromJsonAsync<List<TaskItem>>();
                    
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Tasks.Clear();
                        if (tasksFromServer != null)
                        {
                            bool isManagerOrAdmin = CanCreateTasks;
                            foreach (var task in tasksFromServer)
                            {
                                // Verificăm dacă utilizatorul curent este cel căruia i s-a asignat task-ul
                                bool isAssignedUser = task.AssignedToUsername.Contains(_currentUsername, StringComparison.OrdinalIgnoreCase);
                                
                                // Setăm permisiunea de a schimba statusul pentru UI
                                task.CanChangeStatus = isManagerOrAdmin || isAssignedUser;

                                Tasks.Add(task);
                            }
                        }
                    });
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
        private void Tasks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Când se adaugă un task nou în listă, ne abonăm la modificările lui.
            if (e.NewItems != null)
            {
                foreach (TaskItem item in e.NewItems)
                {
                    item.PropertyChanged += TaskItem_PropertyChanged;
                }
            }
            // Când un task este șters, ne dezabonăm pentru a evita pierderi de memorie.
            if (e.OldItems != null)
            {
                foreach (TaskItem item in e.OldItems)
                {
                    item.PropertyChanged -= TaskItem_PropertyChanged;
                }
            }
        }

        private void TaskItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Verificăm dacă proprietatea care s-a schimbat este "Status".
            if (e.PropertyName == nameof(TaskItem.Status))
            {
                var taskItem = sender as TaskItem;
                if (taskItem != null)
                {
                    // --- LOGICA DE AUTORIZARE LA SCHIMBARE ---
                    // Se verifică din nou permisiunea în momentul schimbării, conform solicitării.
                    // Aceasta este o măsură de siguranță suplimentară, pe lângă dezactivarea controlului în UI.
                    bool isManagerOrAdmin = CanCreateTasks;
                    bool isAssignedUser = taskItem.AssignedToUsername.Contains(_currentUsername, StringComparison.OrdinalIgnoreCase);

                    if (isManagerOrAdmin || isAssignedUser)
                    {
                        // Doar dacă are permisiunea, trimite update-ul la server
                        Task.Run(() => UpdateTaskStatusOnServerAsync(taskItem.Id, taskItem.Status));
                    }
                    else
                    {
                        // Dacă utilizatorul nu are permisiunea, logăm o avertizare.
                        // Acest caz nu ar trebui să se întâmple dacă binding-ul 'IsEnabled' funcționează corect.
                        Console.WriteLine($"AVERTISMENT: Tentativă de modificare a statusului fără permisiune pentru task-ul '{taskItem.Title}'.");
                    }
                }
            }
        }
        
        private async Task UpdateTaskStatusOnServerAsync(Guid taskId, string newStatus)
        {
            HttpClient client = new HttpClient();
            var requestData = new { NewStatus = newStatus };

            try
            {
                var response = await client.PutAsJsonAsync($"https://localhost:7088/api/Task/update/{taskId}", requestData);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Status actualizat cu succes pentru task-ul {taskId}");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Eroare la actualizarea statusului: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepție la actualizarea statusului: {ex.Message}");
            }
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
            if (!CanCreateTasks)
            {
                Console.WriteLine("Acțiune interzisă: Doar un manager sau admin poate adăuga task-uri.");
                return;
            }
            
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
        public Guid Id { get; set; }
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