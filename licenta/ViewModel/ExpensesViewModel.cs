using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace licenta.ViewModel;

public class ExpensesViewModel : ViewModelBase
{
    private readonly MapViewModel _mapViewModel;
    public Guid _currentUserId;
    public string _currentUsername = LoginViewModel.UsernameForUse.Username;
    public SeriesCollection MonthlyProfitLoss { get; set; }
    public SeriesCollection ExpensePercentages { get; set; }
    public string[] Months { get; } = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames;
    public Func<double, string> AmountFormatter { get; } = value => value.ToString("N0") + " RON";
    
    private List<ParcelData> _allParcels = new List<ParcelData>();
    private ObservableCollection<ParcelData> _savedParcels = new ObservableCollection<ParcelData>();
    private List<ParcelNameAndID> _parcelNameAndIDs = new List<ParcelNameAndID>();
    
    
    
    private ObservableCollection<string> _parcels = new ObservableCollection<string>();
    public ObservableCollection<string> Parcels
    {
        get => _parcels;
        set
        {
           _parcels = value;
           OnPropertyChanged(nameof(Parcels));
        }
    }

    private string _selectedParcel;
    public string SelectedParcel
    {
        get => _selectedParcel;
        set  {
        _selectedParcel = value;
        OnPropertyChanged(nameof(SelectedParcel));
    }
    }

    private string _newCategory;
    public string NewCategory
    {
        get => _newCategory;
        set
        {
            _newCategory = value;
            OnPropertyChanged(nameof(NewCategory));
        }
    }

    private decimal _value;
    public decimal Value
    {
        get => _value;
        set
        {
            _value = value;
            OnPropertyChanged(nameof(Value));
        }
    }

    private ObservableCollection<string> _existingCategories = new ObservableCollection<string>();
    public ObservableCollection<string> ExistingCategories
    {
        get => _existingCategories;
        set
        {
            _existingCategories = value;
            OnPropertyChanged(nameof(ExistingCategories));
        }
    }

    private ObservableCollection<Entry> _entries = new ObservableCollection<Entry>();
    public ObservableCollection<Entry> Entries
    {
        get => _entries;
        set
        {
            _entries = value;
            OnPropertyChanged(nameof(Entries));
        }
    }
    
    public ICommand AddEntryCommand { get; }

    public ExpensesViewModel(MapViewModel mapViewModel)
    {
        _mapViewModel = mapViewModel ?? throw new ArgumentNullException(nameof(mapViewModel));
        InitializeUserAndData();
        
       
        LoadDemoData();
        AddEntryCommand = new RelayCommand(AddEntry);
        
        _mapViewModel.PolygonsUpdated += RefreshParcels;
    
    }
    
    private async void InitializeUserAndData()
    {
        await InitializeUser(); // Așteptăm finalizarea inițializării utilizatorului

        if (_currentUserId != Guid.Empty)
        {
            GetParcelNamesAndIDs(); // Acum _currentUserId este setat
            GetEntriesFromParcels();
        }
        else
        {
            MessageBox.Show("User ID invalid.");
        }
    }

    private async void GetEntriesFromParcels()
    {
        var client = new HttpClient(); 

        Console.WriteLine(_currentUserId);
        var response = await client
            .GetAsync($"https://localhost:7088/api/PolygonEntries?userId={_currentUserId.ToString()}")
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to fetch expenses data. Status code: {response.StatusCode}");
            return;
        }

        var entriesJson = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Răspuns server: {entriesJson}"); // Log răspunsul serverului

        // Use case-insensitive options to avoid casing issues
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var entries = JsonSerializer.Deserialize<List<Entry>>(entriesJson, options);

        App.Current.Dispatcher.Invoke(() =>
        {
            if (entries != null)
            {
                foreach (var entry in entries)
                {
                    string polygonName= null;

                    try
                    {
                        foreach (var parcel in _parcelNameAndIDs)
                        {
                            if (parcel.Id == entry.ParcelId)
                            {
                                polygonName = parcel.Name;
                            }
                        }
                        Console.WriteLine($"Polygon Name: {polygonName}");

                        Entry entryModel = new Entry()
                        {
                            ParcelName = polygonName,
                            Category = entry.Category,
                            Value = entry.Value,
                            Date = entry.Date
                        };
                        Console.WriteLine($"Entry value: {entry.Value}");
                        
                        Entries.Add(entryModel);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        });
    }

    private async Task InitializeUser()
    {
        try
        {
            // Folosiți HttpClient cu handler care ignoră erorile SSL (doar pentru mediu de dezvoltare!)
            // var handler = new HttpClientHandler
            // {
            //     ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            // };

            var client = new HttpClient(); //handler)
            // {
            //     Timeout = TimeSpan.FromSeconds(30) // Timeout crescut la 30 secunde
            // };

            var response = await client.GetAsync($"http://localhost:5035/api/auth/{_currentUsername}")
                .ConfigureAwait(false); // Evită blocarea contextului UI

            response.EnsureSuccessStatusCode(); // Aruncă excepție dacă răspunsul nu e succes

            var userJson = await response.Content.ReadAsStringAsync();

            // Use case-insensitive deserialization for the user DTO
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var userDto = JsonSerializer.Deserialize<MapViewModel.UserDto>(userJson, options);
            if (userDto == null || userDto.Id == Guid.Empty)
            {
                MessageBox.Show("Failed to fetch current user information.");
                return;
            }

            _currentUserId = userDto.Id;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine(e.InnerException.Message);
        }
    }
    
     private async void GetParcelNamesAndIDs()
    {
        //https://localhost:7088/api/Polygons/names?userId=4cff5da4-c2e5-4125-9a63-997e7d040565
        try
        {
            // var handler = new HttpClientHandler
            // {
            //     ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            // };

            var client = new HttpClient(); //handler)
            // {
            //     Timeout = TimeSpan.FromSeconds(30) // Timeout crescut la 30 secunde
            // };
            // Fetch polygons for the current user
            Console.WriteLine(_currentUserId);
            var response = await client
                .GetAsync($"https://localhost:7088/api/Polygons/names?userId={_currentUserId.ToString()}")
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to fetch polygons. Status code: {response.StatusCode}");
                return;
            }

            var entriesJson = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Răspuns server: {entriesJson}"); // Log răspunsul serverului

            // Use case-insensitive options to avoid casing issues
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var polygons = JsonSerializer.Deserialize<List<ParcelNameAndID>>(entriesJson, options);

            App.Current.Dispatcher.Invoke(() =>
            {
                Parcels.Clear();
                if (polygons != null)
                {
                    Parcels.Add("");
                    foreach (var polygon in polygons)
                    {
                        Parcels.Add(polygon.Name);
                        Console.WriteLine($"Name: {polygon.Name}");
                    }
                }
            });

            foreach (var polygon in polygons)
            {
                try
                {
                    ParcelNameAndID parcelData = new ParcelNameAndID();
                    parcelData.Name = polygon.Name;
                    parcelData.Id = polygon.Id;
                    _parcelNameAndIDs.Add(parcelData);
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Eroare la procesarea poligonului (GrainParcel): {ex.Message}");
                }
            }
        }
        catch
        {
            MessageBox.Show("Failed to fetch polygons");
        }
    }
    
    private void InitializeParcels()
    {
        // Add empty option
        //Parcels.Insert(0, new string { Name = "(Nicio parcelă)" });
        
        
    }

    private void RefreshParcels()
    {
        _parcels.Clear();
        _savedParcels.Clear();
        GetParcelNamesAndIDs();
        Console.WriteLine("refresh parcels");
    }

    private void AddEntry()
    {
        Entries.Add(new Entry
        {
            ParcelName = SelectedParcel ?? "Nicio parcelă",
            Category = NewCategory,
            Value = Value,
            Date = DateTime.Now
        });

        // Add to existing categories if new
        if (!string.IsNullOrEmpty(NewCategory) && !ExistingCategories.Contains(NewCategory))
            ExistingCategories.Add(NewCategory);

        // Clear inputs
        NewCategory = string.Empty;
        Value = 0;
    }

    private void LoadDemoData()
    {
        // Date Demo - Profit/Pierdere pe Lună
        var monthlyValues = new List<double>
        {
            15000, -10000, 20000, 3000, -2000, 18000,
            9000, -3000, 12000, 7000, -4000, 25000
        };

        var positiveValues = new ChartValues<double>();
        var negativeValues = new ChartValues<double>();

        for (int i = 0; i < monthlyValues.Count; i++)
        {
            if (monthlyValues[i] >= 0)
            {
                positiveValues.Add(monthlyValues[i]);
            }
            else
            {
                negativeValues.Add(monthlyValues[i]);
            }
        }

        MonthlyProfitLoss = new SeriesCollection
        {
            new ColumnSeries
            {
                Title = "Profit",
                Values = positiveValues,
                Fill = new SolidColorBrush(Color.FromRgb(76, 175, 80)), // Verde
                StrokeThickness = 2,
                DataLabels = true,
                LabelPoint = (chartPoint) => chartPoint.Y > 0 ? AmountFormatter(chartPoint.Y) : "" // Am schimbat chartPoint.Value în chartPoint.Y
            },
            new ColumnSeries
            {
                Title = "Pierdere",
                Values = negativeValues,
                Fill = new SolidColorBrush(Color.FromRgb(244, 67, 54)), // Roșu
                StrokeThickness = 2,
                DataLabels = true,
                LabelPoint = (chartPoint) => chartPoint.Y < 0 ? AmountFormatter(chartPoint.Y) : "" // Am schimbat chartPoint.Value în chartPoint.Y
            }
        };

        // Date Demo - Procente Cheltuieli (rămâne neschimbat)
        ExpensePercentages = new SeriesCollection
        {
            new PieSeries
            {
                Title = "Semințe",
                Values = new ChartValues<double> { 40 },
                Fill = Brushes.DodgerBlue,
                DataLabels = true,
                LabelPoint = chartPoint => string.Format("{0} ({1:P})", chartPoint.SeriesView.Title, chartPoint.Participation)
            },
            new PieSeries
            {
                Title = "Îngrășăminte",
                Values = new ChartValues<double> { 25 },
                Fill = Brushes.Orange,
                DataLabels = true,
                LabelPoint = chartPoint => string.Format("{0} ({1:P})", chartPoint.SeriesView.Title, chartPoint.Participation)
            },
            new PieSeries
            {
                Title = "Lucrări",
                Values = new ChartValues<double> { 20 },
                Fill = Brushes.LightGreen,
                DataLabels = true,
                LabelPoint = chartPoint => string.Format("{0} ({1:P})", chartPoint.SeriesView.Title, chartPoint.Participation)
            },
            new PieSeries
            {
                Title = "Altele",
                Values = new ChartValues<double> { 15 },
                Fill = Brushes.Violet,
                DataLabels = true,
                LabelPoint = chartPoint => string.Format("{0} ({1:P})", chartPoint.SeriesView.Title, chartPoint.Participation)
            }
        };
    }
    
    public class Entry
    {
        [JsonPropertyName("id")]
        public string ParcelName { get; set; }
        
        [JsonPropertyName("PolygonID")]
        public Guid ParcelId { get; set; }
        
        [JsonPropertyName("CreatedByUserID")]
        public Guid CreatedByUserId { get; set; }
        
        [JsonPropertyName("Categorie")]
        public string Category { get; set; }
        
        [JsonPropertyName("Valoare")]
        public decimal Value { get; set; }
        
        [JsonPropertyName("DataCreare")]
        public DateTime Date { get; set; }

    }
    
    
}