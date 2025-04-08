namespace licenta.ViewModel;

public class ExpensesViewModel : ViewModelBase
{
    private readonly MapViewModel _mapViewModel;
    public ExpensesViewModel(MapViewModel mapViewModel)
    {
        _mapViewModel = mapViewModel ?? throw new ArgumentNullException(nameof(mapViewModel));
        
    }
}