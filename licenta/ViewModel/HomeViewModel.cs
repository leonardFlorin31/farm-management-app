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
            Console.WriteLine($"Name set to: {_name}");
            OnPropertyChanged(nameof(Name));
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            _email = value;
            Console.WriteLine($"Email set to: {_email}");
            OnPropertyChanged(nameof(Email));
        }
    }

    public HomeViewModel()
    {
        Console.WriteLine("HomeViewModel initialized");
    }
}