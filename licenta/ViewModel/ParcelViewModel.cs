using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace licenta.ViewModel;

public class ParcelViewModel : ViewModelBase, IDisposable
{
    public Guid _currentUserId; // Will be set after login
    public string _currentUsername = LoginViewModel.UsernameForUse.Username;
    
    private string _field1;
    private string _field2;
    private string _field3;
    private string _field4;
    private string _field5;
    private string _field6;
    private string _field7;
    private string _field8;
    private string _field9;
    
    private string _selectedOption;
    private string _label1 = "Label A1";
    private string _label2 = "Label A2";
    private string _label3 = "Label A3";
    private string _label4 = "Label A4";
    private string _label5 = "Label A5";
    private string _label6 = "Label A6";
    private string _label7 = "Label A7";
    private string _label8 = "Label A8";
    private string _label9 = "Label A9";
    
    private string _searchOption;
    private string _searchField1;
    private string _searchField2;
    private string _searchField3;
    private string _searchField4;
    private string _searchField5;
    private string _searchField6;
    private string _searchField7;
    private string _searchField8;
    private string _searchField9;
    
    public ObservableCollection<string> Options { get; } = new ObservableCollection<string> { "Animale", "Grane" };
    private List<ParcelData> _allParcels = new List<ParcelData>();
    private ObservableCollection<ParcelData> _savedParcels = new ObservableCollection<ParcelData>();
    
    private readonly MapViewModel _mapViewModel;
    
    public void Dispose()
    {
        _mapViewModel.PolygonsUpdated -= RefreshParcels;
    }

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
        set { _savedParcels = value; 
            OnPropertyChanged(nameof(SavedParcels)); }
    }

    #region SearchField
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
    
    public string SearchField5
    {
        get => _searchField5;
        set
        {
            _searchField5 = value;
            OnPropertyChanged(nameof(SearchField5));
            FilterParcels();
        }
    }

    public string SearchField6
    {
        get => _searchField6;
        set
        {
            _searchField6 = value;
            OnPropertyChanged(nameof(SearchField6));
            FilterParcels();
        }
    }

    public string SearchField7
    {
        get => _searchField7;
        set
        {
            _searchField7 = value;
            OnPropertyChanged(nameof(SearchField7));
            FilterParcels();
        }
    }

    public string SearchField8
    {
        get => _searchField8;
        set
        {
            _searchField8 = value;
            OnPropertyChanged(nameof(SearchField8));
            FilterParcels();
        }
    }
    public string SearchField9
    {
        get => _searchField9;
        set
        {
            _searchField9 = value;
            OnPropertyChanged(nameof(SearchField9));
            FilterParcels();
        }
    }
    #endregion
    
    #region Label
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
    
    public string Label5
    {
        get => _label5;
        set { _label5 = value; OnPropertyChanged(nameof(Label5)); }
    }

    public string Label6
    {
        get => _label6;
        set { _label6 = value; OnPropertyChanged(nameof(Label6)); }
    }

    public string Label7
    {
        get => _label7;
        set { _label7 = value; OnPropertyChanged(nameof(Label7)); }
    }
    
    public string Label8
    {
        get => _label8;
        set { _label8 = value; OnPropertyChanged(nameof(Label8)); }
    }
    public string Label9
    {
        get => _label9;
        set { _label9 = value; OnPropertyChanged(nameof(Label9)); }
    }
    #endregion
    
    #region Field
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
    
    public string Field5
    {
        get => _field5;
        set { _field5 = value; OnPropertyChanged(nameof(Field1)); }
    }

    public string Field6
    {
        get => _field6;
        set { _field6 = value; OnPropertyChanged(nameof(Field2)); }
    }

    public string Field7
    {
        get => _field7;
        set { _field7 = value; OnPropertyChanged(nameof(Field3)); }
    }

    public string Field8
    {
        get => _field8;
        set { _field8 = value; OnPropertyChanged(nameof(Field4)); }
    }
    
    public string Field9
    {
        get => _field9;
        set { _field9 = value; OnPropertyChanged(nameof(Field4)); }
    }
#endregion
    public ICommand SaveCommand { get; }
    
    public ParcelViewModel(MapViewModel mapViewModel)
    {
        _mapViewModel = mapViewModel ?? throw new ArgumentNullException(nameof(mapViewModel));
        
        SelectedOption = Options.First(); // Selectează automat prima opțiune
        SaveCommand = new RelayCommand(SaveData);
        
        InitializeUserAndData();
        
        _mapViewModel.PolygonsUpdated += RefreshParcels;
    }
    
    private void RefreshParcels()
    {
        _allParcels.Clear();
        _savedParcels.Clear();
        GetParcelNamesAndIDs();
        Console.WriteLine("refresh parcels");
    }
    
    private async void InitializeUserAndData()
    {
        await InitializeUser(); // Așteptăm finalizarea inițializării utilizatorului
    
        if (_currentUserId != Guid.Empty)
        {
            GetParcelNamesAndIDs(); // Acum _currentUserId este setat
        }
        else
        {
            MessageBox.Show("User ID invalid.");
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

            var client = new HttpClient();//handler)
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

            var polygonsJson = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Răspuns server: {polygonsJson}"); // Log răspunsul serverului

            // Use case-insensitive options to avoid casing issues
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var polygons = JsonSerializer.Deserialize<List<ParcelNameAndID>>(polygonsJson, options);

            foreach (var polygon in polygons)
            {
                try
                {
                    ParcelData parcel = await DataTest(polygon.Name, polygon.Id.ToString());

                    App.Current.Dispatcher.Invoke((Action)delegate()
                    {
                        _savedParcels.Add(parcel);
                        _allParcels.Add(parcel);
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Eroare la procesarea poligonului: {ex.Message}");
                }
            }
        }
        catch
        {
            MessageBox.Show("Failed to fetch polygons");
        }
    }

    private async Task<ParcelData> DataTest(string polygonName, string parcelId)
    {
        //https://localhost:7088/api/ParcelData/polygon/302836ff-eac0-4efa-a0fe-03124b578fb2
                    
        ParcelData parcelData = new ParcelData();
        
        HttpClient client = new HttpClient();
                    
        var response  = await client.GetAsync($"https://localhost:7088/api/ParcelData/polygon/{parcelId}").ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to fetch data. Status code: {response.StatusCode}");
            
        }
                    
        var json = await response.Content.ReadAsStringAsync();
        
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull // Opțiune corectă
        };
        var GrainParcelDataList = JsonSerializer.Deserialize<List<GrainParcelDataDto>>(json, options);

        
        if (GrainParcelDataList != null && GrainParcelDataList.Count > 0)
        {
            var firstParcel = GrainParcelDataList[0]; // Ia primul element dacă e nevoie
            parcelData.Option = "Grane";
            parcelData.Field1 = polygonName;
            parcelData.Field2 = firstParcel.ParcelArea.ToString();
            parcelData.Field3 = firstParcel.Season;
            parcelData.Field4 = firstParcel.CropType;
            parcelData.Field5 = firstParcel.IrrigationType;
            parcelData.Field6 = firstParcel.Yield.ToString();
            parcelData.Field7 = firstParcel.FertilizerUsed.ToString();
            parcelData.Field8 = firstParcel.PesticideUsed.ToString();
            parcelData.Field9 = firstParcel.WaterUsage.ToString();

        }
        else
        {
            Console.WriteLine("No data found.");
        }

        return parcelData;
    }
    
    private async Task  InitializeUser()
    {
        try
        {
            // Folosiți HttpClient cu handler care ignoră erorile SSL (doar pentru mediu de dezvoltare!)
            // var handler = new HttpClientHandler
            // {
            //     ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            // };

            var client = new HttpClient();//handler)
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

    private void SaveData()
    {
        var newParcel = new ParcelData
        {
            Option = SelectedOption,
            Field1 = Field1,
            Field2 = Field2,
            Field3 = Field3,
            Field4 = Field4,
            Field5 = Field5,
            Field6 = Field6,
            Field7 = Field7,
            Field8 = Field8,
            Field9 = Field9
        };

        _allParcels.Add(newParcel);
        SavedParcels.Add(newParcel);
    }
    
    private void UpdateLabels()
    {
        if (SelectedOption == "Animale")
        {
            Label1 = "Denumire";
            Label2 = "Label A2";
            Label3 = "Label A3";
            Label4 = "Label A4";
            Label5 = "Label A5";
            Label6 = "Label A6";
            Label7 = "Label A7";
            Label8 = "Label A8";
            Label9 = "Label A9";
        }
        else if (SelectedOption == "Grane")
        {
            Label1 = "Denumire";
            Label2 = "Label B2";
            Label3 = "Label B3";
            Label4 = "Label B4";
            Label5 = "Label B5";
            Label6 = "Label B6";
            Label7 = "Label B7";
            Label8 = "Label B8";
            Label9 = "Label B9";
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
        
        if (!string.IsNullOrWhiteSpace(SearchField5))
        {
            filteredParcels = filteredParcels.Where(p => p.Field5.Contains(SearchField5, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(SearchField6))
        {
            filteredParcels = filteredParcels.Where(p => p.Field6.Contains(SearchField6, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(SearchField7))
        {
            filteredParcels = filteredParcels.Where(p => p.Field7.Contains(SearchField7, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(SearchField8))
        {
            filteredParcels = filteredParcels.Where(p => p.Field8.Contains(SearchField8, StringComparison.OrdinalIgnoreCase));
        }
        
        if (!string.IsNullOrWhiteSpace(SearchField9))
        {
            filteredParcels = filteredParcels.Where(p => p.Field9.Contains(SearchField9, StringComparison.OrdinalIgnoreCase));
        }

        SavedParcels = new ObservableCollection<ParcelData>(filteredParcels);
    }
    
}

public class ParcelData
{
    public string Option { get; set; }
    public string Field1 { get; set; }
    public string Field2 { get; set; }
    public string Field3 { get; set; }
    public string Field4 { get; set; }
    public string Field5 { get; set; }
    public string Field6 { get; set; }
    public string Field7 { get; set; }
    public string Field8 { get; set; }
    public string Field9 { get; set; }
}

public class ParcelNameAndID()
{
    [JsonPropertyName("Id")]
    public Guid Id { get; set; }
    [JsonPropertyName("Name")]
    public string Name { get; set; }
}

public class GrainParcelDataDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("polygonId")]
    public Guid PolygonId { get; set; }

    [JsonPropertyName("cropType")]
    public string CropType { get; set; }

    [JsonPropertyName("parcelArea")]
    public double ParcelArea { get; set; }

    [JsonPropertyName("irrigationType")]
    public string IrrigationType { get; set; }

    [JsonPropertyName("fertilizerUsed")]
    public double FertilizerUsed { get; set; }

    [JsonPropertyName("pesticideUsed")]
    public double PesticideUsed { get; set; }

    [JsonPropertyName("yield")]
    public double Yield { get; set; }

    [JsonPropertyName("soilType")]
    public string SoilType { get; set; }

    [JsonPropertyName("season")]
    public string Season { get; set; }

    [JsonPropertyName("waterUsage")]
    public double WaterUsage { get; set; }

    [JsonPropertyName("createdDate")]
    public DateTime CreatedDate { get; set; }
    
    [JsonPropertyName("polygon")]
    public Polygon Polygon { get; set; }
}

public class Polygon
{
    [JsonPropertyName("polygonId")]
    public Guid PolygonId { get; set; }

    [JsonPropertyName("polygonName")]
    public string PolygonName { get; set; }

    [JsonPropertyName("createdByUserId")]
    public Guid CreatedByUserId { get; set; }

    [JsonPropertyName("createdDate")]
    public DateTime CreatedDate { get; set; }

    [JsonPropertyName("points")]
    public PolygonPoint[] Points { get; set; }
}

public class PolygonPoint
{
    [JsonPropertyName("pointId")]
    public Guid PointId { get; set; }

    [JsonPropertyName("polygonId")]
    public Guid PolygonId { get; set; }

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("order")]
    public int Order { get; set; }

    [JsonPropertyName("polygon")]
    public string Polygon { get; set; }
}