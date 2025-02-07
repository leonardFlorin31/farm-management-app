using System;
using System.Net;
using System.Security;
using System.Security.Principal;
using System.Windows.Input;
using licenta.Model;
using licenta.Repositories;

namespace licenta.ViewModel;

public class LoginViewModel : ViewModelBase
{
    // Fields
    private string _username;
    private SecureString _password;
    private string _errorMessage;
    private bool _isViewVisible = true;

    private IUserRepository _userRepository;

    // Events
    public event Action LoginSuccess;

    // Properties
    public string Username
    {
        get => _username;
        set
        {
            _username = value;
            OnPropertyChanged(nameof(Username));
        }
    }

    public SecureString Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged(nameof(Password));
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged(nameof(ErrorMessage));
        }
    }

    public bool IsViewVisible
    {
        get => _isViewVisible;
        set
        {
            _isViewVisible = value;
            OnPropertyChanged(nameof(IsViewVisible));
        }
    }

    // Commands
    public ICommand LoginCommand { get; }
    public ICommand RecoverPasswordCommand { get; }
    public ICommand ShowPasswordCommand { get; }
    public ICommand RememberPasswordCommand { get; }

    // Constructors
    public LoginViewModel()
    {
        _userRepository = new UserRepository();
        LoginCommand = new ViewModelCommand(ExecuteLoginCommand, CanExecuteLoginCommand);
        RecoverPasswordCommand = new ViewModelCommand(p => ExecuteRecoverPasswordCommand("", ""));
    }

    // Methods
    private void ExecuteLoginCommand(object obj)
    {
        var validUser = _userRepository.AutenticateUser(new NetworkCredential(Username, Password));
        if (validUser)
        {
            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(Username), null);
            IsViewVisible = false;

            // Notify that the login was successful
            LoginSuccess?.Invoke();
        }
        else
        {
            ErrorMessage = "Username sau parola incorecta!";
        }
    }

    private bool CanExecuteLoginCommand(object obj)
    {
        bool validData;
        if (string.IsNullOrEmpty(Username) || Username.Length < 3 || Password == null
            || Password.Length < 3)
        {
            //errorMessage in campurile acestea reseteaza errorMessage-ul din ExecuteLoginCommand
            //ErrorMessage = "Va rugam completati campurile!";
            validData = false;
        }
        else
        {
            //ErrorMessage = "";
            validData = true;
        }
        return validData;
    }

    private void ExecuteRecoverPasswordCommand(string username, string email)
    {
        // Implement password recovery logic here
    }
}