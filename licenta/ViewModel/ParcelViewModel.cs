using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace licenta.ViewModel;

public class ParcelViewModel : ViewModelBase
{
    private string _field1;
    private string _field2;
    private string _field3;
    private string _field4;
    
    private string _selectedOption;
    private string _label1 = "Label A1";
    private string _label2 = "Label A2";
    private string _label3 = "Label A3";
    private string _label4 = "Label A4";
    
    private string _searchOption;
    private string _searchField1;
    private string _searchField2;
    private string _searchField3;
    private string _searchField4;
    
    public ObservableCollection<string> Options { get; } = new ObservableCollection<string> { "Option A", "Option B" };
    private List<ParcelData> _allParcels = new List<ParcelData>();
    private ObservableCollection<ParcelData> _savedParcels = new ObservableCollection<ParcelData>();

    public string SelectedOption
    {
        get => _selectedOption;
        set
        {
            _selectedOption = value;
            UpdateLabels();
            OnPropertyChanged(nameof(SelectedOption));
        }
    }

    public ObservableCollection<ParcelData> SavedParcels
    {
        get => _savedParcels;
        set { _savedParcels = value; OnPropertyChanged(nameof(SavedParcels)); }
    }

    public string SearchOption
    {
        get => _searchOption;
        set
        {
            _searchOption = value;
            OnPropertyChanged(nameof(SearchOption));
            FilterParcels();
        }
    }

    public string SearchField1
    {
        get => _searchField1;
        set
        {
            _searchField1 = value;
            OnPropertyChanged(nameof(SearchField1));
            FilterParcels();
        }
    }

    public string SearchField2
    {
        get => _searchField2;
        set
        {
            _searchField2 = value;
            OnPropertyChanged(nameof(SearchField2));
            FilterParcels();
        }
    }

    public string SearchField3
    {
        get => _searchField3;
        set
        {
            _searchField3 = value;
            OnPropertyChanged(nameof(SearchField3));
            FilterParcels();
        }
    }

    public string SearchField4
    {
        get => _searchField4;
        set
        {
            _searchField4 = value;
            OnPropertyChanged(nameof(SearchField4));
            FilterParcels();
        }
    }

    public string Label1
    {
        get => _label1;
        set { _label1 = value; OnPropertyChanged(nameof(Label1)); }
    }

    public string Label2
    {
        get => _label2;
        set { _label2 = value; OnPropertyChanged(nameof(Label2)); }
    }

    public string Label3
    {
        get => _label3;
        set { _label3 = value; OnPropertyChanged(nameof(Label3)); }
    }
    
    public string Label4
    {
        get => _label4;
        set { _label4 = value; OnPropertyChanged(nameof(Label4)); }
    }

    public string Field1
    {
        get => _field1;
        set { _field1 = value; OnPropertyChanged(nameof(Field1)); }
    }

    public string Field2
    {
        get => _field2;
        set { _field2 = value; OnPropertyChanged(nameof(Field2)); }
    }

    public string Field3
    {
        get => _field3;
        set { _field3 = value; OnPropertyChanged(nameof(Field3)); }
    }

    public string Field4
    {
        get => _field4;
        set { _field4 = value; OnPropertyChanged(nameof(Field4)); }
    }

    public ICommand SaveCommand { get; }

    public ParcelViewModel()
    {
        SelectedOption = Options.First(); // Selectează automat prima opțiune
        SaveCommand = new RelayCommand(SaveData);
    }

    private void SaveData()
    {
        var newParcel = new ParcelData
        {
            Option = SelectedOption,
            Field1 = Field1,
            Field2 = Field2,
            Field3 = Field3,
            Field4 = Field4
        };

        _allParcels.Add(newParcel);
        SavedParcels.Add(newParcel);
    }
    
    private void UpdateLabels()
    {
        if (SelectedOption == "Option A")
        {
            Label1 = "Label A1";
            Label2 = "Label A2";
            Label3 = "Label A3";
            Label4 = "Label A4";
        }
        else if (SelectedOption == "Option B")
        {
            Label1 = "Label B1";
            Label2 = "Label B2";
            Label3 = "Label B3";
            Label4 = "Label B4";
        }
    }
    
    private void FilterParcels()
    {
        var filteredParcels = _allParcels.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchOption))
        {
            filteredParcels = filteredParcels.Where(p => p.Option.Contains(SearchOption, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(SearchField1))
        {
            filteredParcels = filteredParcels.Where(p => p.Field1.Contains(SearchField1, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(SearchField2))
        {
            filteredParcels = filteredParcels.Where(p => p.Field2.Contains(SearchField2, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(SearchField3))
        {
            filteredParcels = filteredParcels.Where(p => p.Field3.Contains(SearchField3, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(SearchField4))
        {
            filteredParcels = filteredParcels.Where(p => p.Field4.Contains(SearchField4, StringComparison.OrdinalIgnoreCase));
        }

        SavedParcels = new ObservableCollection<ParcelData>(filteredParcels);
    }


    public class ParcelData
    {
        public string Option { get; set; }
        public string Field1 { get; set; }
        public string Field2 { get; set; }
        public string Field3 { get; set; }
        public string Field4 { get; set; }
    }
}