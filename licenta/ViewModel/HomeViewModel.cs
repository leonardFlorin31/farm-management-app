namespace licenta.ViewModel;

public class HomeViewModel:ViewModelBase
{
    private string _name;
    private string _email;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            _email = value;
            OnPropertyChanged(nameof(Email));
        }
    }
}