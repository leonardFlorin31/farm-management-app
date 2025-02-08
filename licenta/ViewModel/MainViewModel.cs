

using System.Windows;

namespace licenta.ViewModel;
using Model;
using Repositories;

public class MainViewModel : ViewModelBase
{
    //Fields
    private UserAccountModel _currentUserAccount;
    private IUserRepository _userRepository;
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
        LoadCurrentUserData();
    }
    
    private void LoadCurrentUserData()
    {
        var user = _userRepository.GetUserByUsername(Thread.CurrentPrincipal.Identity.Name);
        if (user != null)
        {
            _currentUserAccount.Username = user.Username;
            _currentUserAccount.DisplayName = $"Welcome {user.FirstName} {user.LastName}";
            _currentUserAccount.ProfilePicture = null;
        }
        else
        {
           CurrentUserAccount.DisplayName="User not found";
            //Application.Current.Shutdown();
        }
    }
}
    
