using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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

        public ObservableCollection<TaskItem> Tasks { get; } = new ObservableCollection<TaskItem>();

        public Collection<string> Employees { get; } = new Collection<string>
        {
            "gica",
            "relu"
        };
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
    
  
}