

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
    
    private void ExecuteShowHomeViewCommand(object obj)
    {
        // Reuse or create HomeViewModel
        if (!_viewModelCache.TryGetValue(typeof(HomeViewModel), out var viewModel))
        {
            viewModel = new HomeViewModel();
            _viewModelCache[typeof(HomeViewModel)] = viewModel;
        }
        CurrentChildView = viewModel; // Use cached instance
        Caption = "Home";
        Icon = IconChar.Home;
    }

    private void ExecuteShowAnimalViewCommand(object obj)
    {
        // Reuse or create AnimalsViewModel
        if (!_viewModelCache.TryGetValue(typeof(AnimalsViewModel), out var viewModel))
        {
            viewModel = new AnimalsViewModel();
            _viewModelCache[typeof(AnimalsViewModel)] = viewModel;
        }
        CurrentChildView = viewModel; // Use cached instance
        Caption = "Animals";
        Icon = IconChar.UserGroup;
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
    
