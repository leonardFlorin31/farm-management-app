

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
    public ICommand ShowMapViewCommand { get; }
    
    public ICommand ShowParcelViewCommand { get; } 
        
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
        if (!_viewModelCache.TryGetValue(typeof(ParcelViewModel), out var viewModel))
        {
            Console.WriteLine("Creating new ParcelViewModel");
            viewModel = new ParcelViewModel();
            _viewModelCache[typeof(ParcelViewModel)] = viewModel;
        }
        CurrentChildView = viewModel;
        Console.WriteLine($"CurrentChildView set to: {CurrentChildView.GetType().Name}");
        Caption = "Parcel";
        Icon = IconChar.LocationCrosshairs;
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
    
