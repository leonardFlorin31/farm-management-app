

using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using FontAwesome.Sharp;

namespace licenta.ViewModel;
using Model;
using Repositories;

public class MainViewModel : ViewModelBase
{
    //Fields
    private UserAccountModel _currentUserAccount;
    private IUserRepository _userRepository;
    private ViewModelBase _currentChildView;
    private string _caption;
    private IconChar _icon;
    private readonly Dictionary<Type, ViewModelBase> _viewModelCache = new();
    private Guid _currentUserId;
    private string _currentUsername = LoginViewModel.UsernameForUse.Username;
    public string _currentRole = "test";


    public ViewModelBase CurrentChildView
    {
        get => _currentChildView;
        set
        {
            _currentChildView = value;
            OnPropertyChanged(nameof(CurrentChildView));
        }
    }

    public string Caption
    {
        get => _caption;
        set
        {
            _caption = value;
            OnPropertyChanged(nameof(Caption));
        }
    }
    
    //Commands
    public ICommand ShowHomeViewCommand { get; }
    public ICommand ShowMapViewCommand { get; }
    
    public ICommand ShowParcelViewCommand { get; } 
    
    public ICommand ShowExpensesViewCommand { get; }
    
    public ICommand ShowTaskViewCommand { get; }
        
    private void ExecuteShowHomeViewCommand(object obj)
    {
        Console.WriteLine("Executing ShowHomeViewCommand");

        // Check if the HomeViewModel is already in the cache
        if (!_viewModelCache.TryGetValue(typeof(HomeViewModel), out var viewModel))
        {
            Console.WriteLine("Creating new HomeViewModel");
            viewModel = new HomeViewModel();
            _viewModelCache[typeof(HomeViewModel)] = viewModel;
        }
        else
        {
            Console.WriteLine("Reusing cached HomeViewModel");
        }

        CurrentChildView = viewModel; // Use cached instance
        Caption = "Home";
        Icon = IconChar.Home;
        Console.WriteLine($"CurrentChildView set to: {CurrentChildView.GetType().Name}");
    }

    private void ExecuteShowMapViewCommand(object obj)
    {
        Console.WriteLine("Executing ShowMapViewCommand");
        if (!_viewModelCache.TryGetValue(typeof(MapViewModel), out var viewModel))
        {
            Console.WriteLine("Creating new MapViewModel");
            viewModel = new MapViewModel();
            _viewModelCache[typeof(MapViewModel)] = viewModel;
        }
        CurrentChildView = viewModel;
        Console.WriteLine($"CurrentChildView set to: {CurrentChildView.GetType().Name}");
        Caption = "Harta";
        Icon = IconChar.Map;
    }

    private void ExecuteShowParcelViewCommand(object obj)
    {
        Console.WriteLine("Executing ShowParcelViewCommand");
        if (_currentRole != "Contabil")
        {
            // Obține instanța de MapViewModel din cache sau creează una nouă
            if (!_viewModelCache.TryGetValue(typeof(MapViewModel), out var mapViewModel))
            {
                Console.WriteLine("Creating new MapViewModel");
                mapViewModel = new MapViewModel();
                _viewModelCache[typeof(MapViewModel)] = mapViewModel;
            }

            // Verifică dacă ParcelViewModel este în cache, altfel creează-l cu MapViewModel-ul existent
            if (!_viewModelCache.TryGetValue(typeof(ParcelViewModel), out var parcelViewModel))
            {
                Console.WriteLine("Creating new ParcelViewModel");
                parcelViewModel = new ParcelViewModel((MapViewModel)mapViewModel);
                _viewModelCache[typeof(ParcelViewModel)] = parcelViewModel;
            }

            CurrentChildView = parcelViewModel;
            Console.WriteLine($"CurrentChildView set to: {CurrentChildView.GetType().Name}");
            Caption = "Parcele";
            Icon = IconChar.LocationCrosshairs;
        }
        else
        {
            if (!_viewModelCache.TryGetValue(typeof(AccesDeniedViewModel), out var deniedVm))
            {
                deniedVm = new AccesDeniedViewModel();
                _viewModelCache[typeof(AccesDeniedViewModel)] = deniedVm;
            }

            // Swap of current view to the AccessDeniedView
            CurrentChildView = deniedVm;
            Caption = "Acces Refuzat";
            Icon    = IconChar.Lock; 
        }
    }
    
    private void ExecuteShowExpensesViewCommand(object obj)
    {
        Console.WriteLine("Executing ShowParcelViewCommand");

        // Obține instanța de MapViewModel din cache sau creează una nouă
        if (!_viewModelCache.TryGetValue(typeof(MapViewModel), out var mapViewModel))
        {
            Console.WriteLine("Creating new MapViewModel");
            mapViewModel = new MapViewModel();
            _viewModelCache[typeof(MapViewModel)] = mapViewModel;
        }

        // Verifică dacă ExpensesViewModel este în cache, altfel creează-l cu MapViewModel-ul existent
        if (!_viewModelCache.TryGetValue(typeof(ExpensesViewModel), out var expensesViewModel))
        {
            Console.WriteLine("Creating new ParcelViewModel");
            expensesViewModel = new ExpensesViewModel((MapViewModel)mapViewModel);
            _viewModelCache[typeof(ExpensesViewModel)] = expensesViewModel;
        }

        CurrentChildView = expensesViewModel;
        Console.WriteLine($"CurrentChildView set to: {CurrentChildView.GetType().Name}");
        Caption = "Cheltuieli";
        Icon = IconChar.MoneyBills;
    }
    
    private void ExecuteShowTaskViewCommand(object obj)
    {
        Console.WriteLine("Executing ShowTaskViewCommand");

        // Verifică dacă ExpensesViewModel este în cache, altfel creează-l cu MapViewModel-ul existent
        if (!_viewModelCache.TryGetValue(typeof(TaskViewModel), out var taskViewModel))
        {
            Console.WriteLine("Creating new ParcelViewModel");
            taskViewModel = new TaskViewModel();
            _viewModelCache[typeof(TaskViewModel)] = taskViewModel;
        }

        CurrentChildView = taskViewModel;
        Console.WriteLine($"CurrentChildView set to: {CurrentChildView.GetType().Name}");
    }
    
    public IconChar Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            OnPropertyChanged(nameof(Icon));
        }
    }


    public UserAccountModel CurrentUserAccount
    {
        get => _currentUserAccount;
        set
        {
            _currentUserAccount = value;
            OnPropertyChanged(nameof(CurrentUserAccount));
        }
    }
    
    public MainViewModel()
    {
        _userRepository = new UserRepository();
        CurrentUserAccount = new UserAccountModel();
        
        //Initialize Commands
        ShowHomeViewCommand = new ViewModelCommand(ExecuteShowHomeViewCommand);
        ShowMapViewCommand = new ViewModelCommand(ExecuteShowMapViewCommand);
        ShowParcelViewCommand = new ViewModelCommand(ExecuteShowParcelViewCommand);
        ShowExpensesViewCommand = new ViewModelCommand(ExecuteShowExpensesViewCommand);
        ShowTaskViewCommand = new ViewModelCommand(ExecuteShowTaskViewCommand);
        
        //Default view
        ExecuteShowHomeViewCommand(null);
        
       
        LoadUserRoleData();
    }

    private async void LoadUserRoleData()
    {
        await LoadCurrentUserData();
        
        if (_currentUserId == Guid.Empty)
            return;

        try
        {
            using var client = new HttpClient();
            var url = $"http://localhost:5035/api/UserRole/{_currentUserId}";
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to load role: {response.StatusCode}");
                return;
            }

            var json    = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var roleDto = JsonSerializer.Deserialize<UserRoleDto>(json, options);

            if (roleDto != null)
            {
                // e.g. expose this through a bindable property
                _currentRole = roleDto.RoleName;
                CurrentRole.RoleName = _currentRole;
                Console.WriteLine($"Loaded role: {roleDto.RoleName} (created by {roleDto.CreatedBy})");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error connecting to server: {ex.Message}");
        }
    }

    private async Task LoadCurrentUserData()
    {
        try
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
            
            
            // var user = await _userRepository.GetUserByUsernameAsync(Thread.CurrentPrincipal.Identity.Name);
            // if (user != null)
            // {
            //     CurrentUserAccount.Username = user.Username;
            //     CurrentUserAccount.DisplayName = $" {user.Name} {user.LastName}";
            //     CurrentUserAccount.ProfilePicture = null;
            // }
            // else
            // {
            //     CurrentUserAccount.DisplayName = "User not found";
            // }
            // OnPropertyChanged(nameof(CurrentUserAccount));
        }
        catch (HttpRequestException ex)
        {
            CurrentUserAccount.DisplayName = "Error connecting to server";
            OnPropertyChanged(nameof(CurrentUserAccount));
        }
    }
    
    public class UserRoleDto
    {
        public Guid UserId      { get; set; }
        public Guid RoleId      { get; set; }
        public string RoleName  { get; set; }
        public Guid? CreatedBy  { get; set; }
    }

    public static class CurrentRole
    {
        public static string RoleName  { get; set; }
    }
}
    
