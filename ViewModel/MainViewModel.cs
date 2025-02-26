

using System.ComponentModel;
using System.Net.Http;
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
    public ICommand ShowAnimalViewCommand { get; }
    
    public ICommand ShowGrainViewCommand { get; } 
        
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

    private void ExecuteShowAnimalViewCommand(object obj)
    {
        Console.WriteLine("Executing ShowAnimalViewCommand");
        if (!_viewModelCache.TryGetValue(typeof(AnimalsViewModel), out var viewModel))
        {
            Console.WriteLine("Creating new AnimalsViewModel");
            viewModel = new AnimalsViewModel();
            _viewModelCache[typeof(AnimalsViewModel)] = viewModel;
        }
        CurrentChildView = viewModel;
        Console.WriteLine($"CurrentChildView set to: {CurrentChildView.GetType().Name}");
        Caption = "Animals";
        Icon = IconChar.UserGroup;
    }

    private void ExecuteShowGrainViewCommand(object obj)
    {
        Console.WriteLine("Executing ShowGrainViewCommand");
        if (!_viewModelCache.TryGetValue(typeof(GrainViewModel), out var viewModel))
        {
            Console.WriteLine("Creating new GrainViewModel");
            viewModel = new GrainViewModel();
            _viewModelCache[typeof(GrainViewModel)] = viewModel;
        }
        CurrentChildView = viewModel;
        Console.WriteLine($"CurrentChildView set to: {CurrentChildView.GetType().Name}");
        Caption = "Grain";
        Icon = IconChar.WheatAwn;
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
        ShowAnimalViewCommand = new ViewModelCommand(ExecuteShowAnimalViewCommand);
        ShowGrainViewCommand = new ViewModelCommand(ExecuteShowGrainViewCommand);
        
        //Default view
        ExecuteShowHomeViewCommand(null);
        
        LoadCurrentUserData();
    }

    private async void LoadCurrentUserData()
    {
        try
        {
            var user = await _userRepository.GetUserByUsernameAsync(Thread.CurrentPrincipal.Identity.Name);
            if (user != null)
            {
                CurrentUserAccount.Username = user.Username;
                CurrentUserAccount.DisplayName = $" {user.Name} {user.LastName}";
                CurrentUserAccount.ProfilePicture = null;
            }
            else
            {
                CurrentUserAccount.DisplayName = "User not found";
            }
            OnPropertyChanged(nameof(CurrentUserAccount));
        }
        catch (HttpRequestException ex)
        {
            CurrentUserAccount.DisplayName = "Error connecting to server";
            OnPropertyChanged(nameof(CurrentUserAccount));
        }
    }
}
    
