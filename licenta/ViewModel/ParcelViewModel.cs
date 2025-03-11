using System.Collections.ObjectModel;
using System.Windows;
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
    private string _label4 = "Label A3";
    
    public ObservableCollection<string> Options { get; } = new ObservableCollection<string> { "Option A", "Option B" };

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
        set { _field1 = value;
            OnPropertyChanged(nameof(Field1)); }
    }

    public string Field2
    {
        get => _field2;
        set { _field2 = value;
            OnPropertyChanged(nameof(Field2)); }
    }

    public string Field3
    {
        get => _field3;
        set { _field3 = value; 
            OnPropertyChanged(nameof(Field3)); }
    }

    public string Field4
    {
        get => _field4;
        set { _field4 = value; 
            OnPropertyChanged(nameof(Field4)); }
    }

    public ICommand SaveCommand { get; }

    public ParcelViewModel()
    {
        Options = new ObservableCollection<string> { "Option A", "Option B" };
        SelectedOption = Options.First(); // Selectează automat prima opțiune
        SaveCommand = new RelayCommand(SaveData);
    }

    private void SaveData()
    {
        // Implementarea logicii de salvare
        MessageBox.Show($"Saving: {Field1}, {Field2}, {Field3}, {Field4}");
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

}