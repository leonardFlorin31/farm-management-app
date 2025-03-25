using System.Windows.Input;

namespace licenta.ViewModel;

public class RegisterViewModel : ViewModelBase
{
    public event Action BackToLogin;
    
    public ICommand BackCommand { get; }
    RegisterViewModel()
    {
        BackCommand = new RelayCommand(BackAction);
    }

    private void BackAction()
    {
        BackToLogin?.Invoke();
    }
}