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
        private bool _canCreateTasks = true;  // Set based on user role

        public ObservableCollection<TaskItem> Tasks { get; } = new ObservableCollection<TaskItem>();
        public ObservableCollection<string> StatusValues { get; } = new ObservableCollection<string>
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

        public bool CanCreateTasks
        {
            get => _canCreateTasks;
            set { _canCreateTasks = value; OnPropertyChanged(nameof(CanCreateTasks)); }
        }

        private void AddNewTask()
        {
            if (!string.IsNullOrWhiteSpace(NewTaskTitle))
            {
                Tasks.Add(new TaskItem
                {
                    Title = NewTaskTitle,
                    Description = NewTaskDescription,
                    Status = NewTaskStatus ?? "Not Started",
                    AssignedToUsername = "Unassigned", // You'll need to implement user assignment
                    CanChangeStatus = true
                });

                // Clear input fields
                NewTaskTitle = string.Empty;
                NewTaskDescription = string.Empty;
                NewTaskStatus = null;
            }
        }
        
    }

    public class TaskItem : INotifyPropertyChanged
    {
        private string _status;
        
        public string Title { get; set; }
        public string AssignedToUsername { get; set; }
        public string Description { get; set; }
        
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        public bool CanChangeStatus { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Basic RelayCommand implementation
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object parameter) => _execute();

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}