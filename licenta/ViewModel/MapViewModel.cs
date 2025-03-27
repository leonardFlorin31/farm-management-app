using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Clipper2Lib;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using licenta.Repositories;
using FillRule = Clipper2Lib.FillRule;

namespace licenta.ViewModel
{
    public class MapViewModel : ViewModelBase
    {
        private static MapViewModel _instance;
        public static MapViewModel Instance => _instance ??= new MapViewModel();

        private PointLatLng _mapCenter;
        private int _zoomLevel = 13; // Initial zoom level
        private GMapProvider _mapProvider = GoogleSatelliteMapProvider.Instance;
        public int _mapTypeCounter = 0;

        private ObservableCollection<CenterPointsAndName> _centerPoints =
            new ObservableCollection<CenterPointsAndName>();

        private CenterPointsAndName _selectedCenterPoint;
        private List<ParcelNameAndID> _parcelNameAndIDs = new List<ParcelNameAndID>();
        private List<PolygonDto> _polygons = new List<PolygonDto>();
        
        private Visibility _parcelDetailsVisibility = Visibility.Collapsed;
        private double _borderWidth = 300;
        private double _borderHeight = 350; //dimensiunea inițială

        public ObservableCollection<string> CenterPointNames { get; } = new ObservableCollection<string>
        {
        };

        private ParcelData _selectedParcel;
        private ParcelData _selectedParcel2;

        // List to store marker coordinates
        private List<PointLatLng> _markerCoordinates = new List<PointLatLng>();
        private PointLatLng _polygonCentroid;

        // Event for zoom change notifications
        public event Action<int> ZoomChanged;

        // Fields for server interaction
        private readonly HttpClient _httpClient = new HttpClient();
        private string _apiBaseUrl = "https://localhost:7088/api";
        private Guid _currentUserId; // Will be set after login
        private string _currentUsername = LoginViewModel.UsernameForUse.Username;
        private List<string> _polygonNames = new List<string> { };

        public GMapControl MapControl { get; set; }

        public event Action? PolygonsUpdated;

        // Bindable properties

        public GMapProvider MapProvider
        {
            get => _mapProvider;
            set => Set(ref _mapProvider, value);
        }

        public PointLatLng MapCenter
        {
            get => _mapCenter;
            set => Set(ref _mapCenter, value);
        }


        public PointLatLng PolygonCentroid
        {
            get => _polygonCentroid;
            set => Set(ref _polygonCentroid, value);
        }

        public ObservableCollection<CenterPointsAndName> CenterPoints
        {
            get { return _centerPoints; }
            set
            {
                _centerPoints = value;
                OnPropertyChanged(nameof(CenterPoints));
            }
        }

        public CenterPointsAndName SelectedCenterPoint
        {
            get { return _selectedCenterPoint; }
            set
            {
                _selectedCenterPoint = value;
                OnPropertyChanged(nameof(SelectedCenterPoint));

                // Aici poți adăuga logica pentru selecție
                // if (value != null)
                // {
                //     Console.WriteLine($"Ai selectat: {value.Name}");
                // }
            }
        }
        
        public Visibility ParcelDetailsVisibility
        {
            get => _parcelDetailsVisibility;
            set
            {
                _parcelDetailsVisibility = value;
                OnPropertyChanged(nameof(ParcelDetailsVisibility));
            }
        }

        public double BorderWidth
        {
            get => _borderWidth;
            set
            {
                _borderWidth = value;
                OnPropertyChanged(nameof(BorderWidth));
            }
        }

        public double BorderHeight
        {
            get => _borderHeight;
            set
            {
                _borderHeight = value;
                OnPropertyChanged(nameof(BorderHeight));
            }
        }

        private string _selectedCenterPointName;

        public string SelectedCenterPointName
        {
            get => _selectedCenterPointName;
            set
            {
                if (_selectedCenterPointName != value)
                {
                    _selectedCenterPointName = value;
                    OnPropertyChanged(nameof(SelectedCenterPointName));
                }
            }
        }


        private string _PolygonName;

        public string PolygonName
        {
            get => _PolygonName;
            set
            {
                _PolygonName = value;
                OnPropertyChanged(nameof(PolygonName));
                OnPropertyChanged(nameof(IsPlaceholderVisible));
            }
        }

        public bool IsPlaceholderVisible => string.IsNullOrEmpty(PolygonName);

        public int ZoomLevel
        {
            get => _zoomLevel;
            set
            {
                if (Set(ref _zoomLevel, value))
                {
                    // Update the map's zoom if it’s out of sync
                    if (MapControl.Zoom != value)
                    {
                        MapControl.Zoom = value;
                    }

                    Console.WriteLine(ZoomLevel);
                    ZoomChanged?.Invoke(value);
                    UpdateLabelVisibility();
                }
            }
        }

        // Commands for zooming
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }

        // Command for map click
        public ICommand MapClickedCommand { get; }

        // Command for creating a polygon
        public ICommand CreatePolygonCommand { get; }

        public ICommand DeleteLastMarkerCommand { get; }

        public ICommand DeletePolygonCommand { get; }

        public ICommand ChangeMapTypeCommand { get; }

        public ICommand SelectionChangedCommand { get; }
        
        public ICommand ToggleParcelDetailsCommand { get; }
        public ICommand ResizeHorizontalCommand { get; }
        public ICommand ResizeVerticalCommand { get; }
        public ICommand ResizeCornerCommand { get; }

        public MapViewModel()
        {
            MapCenter = new PointLatLng(44.4268, 26.1025); // București
            MapClickedCommand = new RelayCommand<PointLatLng>(OnMapClicked);

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_apiBaseUrl) // Set the base address
            };

            // Initialize zoom commands
            ZoomInCommand = new RelayCommand(ZoomIn);
            ZoomOutCommand = new RelayCommand(ZoomOut);

            // Command to create a polygon
            CreatePolygonCommand = new RelayCommand(CreatePolygon);

            DeleteLastMarkerCommand = new RelayCommand(DeleteLastMarker);

            DeletePolygonCommand = new RelayCommand(DeletePolygon);

            ChangeMapTypeCommand = new RelayCommand(ChangeMapType);

            SelectionChangedCommand = new RelayCommand(SelectionChanged);
            
            ToggleParcelDetailsCommand = new RelayCommand(ToggleParcelDetails);
            
            ResizeHorizontalCommand = new RelayCommand<DragDeltaEventArgs>(OnResizeHorizontal);
            ResizeVerticalCommand = new RelayCommand<DragDeltaEventArgs>(OnResizeVertical);
            ResizeCornerCommand = new RelayCommand<DragDeltaEventArgs>(OnResizeCorner);

            MapControl = new GMapControl
            {
                MapProvider = GMap.NET.MapProviders.GoogleSatelliteMapProvider.Instance,
                Position = new PointLatLng(44.4268, 26.1025), // București
                MinZoom = 2,
                MaxZoom = 18,
                Zoom = 13,
                MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter,
                CanDragMap = true,
                DragButton = MouseButton.Left
            };

            // Fetch the current user's ID and load polygons
            InitializeUserAndPolygons();

            // Subscribe to the OnPositionChanged event
            MapControl.OnPositionChanged += MapControl_OnPositionChanged;

            // Handle right-click events
            MapControl.MouseRightButtonDown += MapControl_MouseRightButtonDown;
        }

        private void OnResizeCorner(DragDeltaEventArgs e)
        {
            // Actualizează ambele dimensiuni
            OnResizeHorizontal(e);
            OnResizeVertical(e);
        }

        private void OnResizeVertical(DragDeltaEventArgs e)
        {
            double newHeight = BorderHeight + e.VerticalChange;
            if (newHeight > 100)
                BorderHeight = newHeight;
        }

        private void OnResizeHorizontal(DragDeltaEventArgs e)
        {
            double newWidth = BorderWidth + e.HorizontalChange;
            if (newWidth > 150)
                BorderWidth = newWidth;
        }

        private void ToggleParcelDetails()
        {
            ParcelDetailsVisibility = ParcelDetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void SelectionChanged()
        {
            if (string.IsNullOrEmpty(SelectedCenterPointName))
            {
                //MessageBox.Show("Niciun element selectat sau elementul este null."); // astea ruleaza pe thread secundar si fac ca aplicatia sa dea crash
                return;
            }
            // MessageBox.Show($"Ai selectat: {SelectedCenterPointName}");

            foreach (var item in CenterPoints)
            {
                if (item.Name == SelectedCenterPointName)
                {
                    MapControl.Position = item.Points;
                    Console.WriteLine(item.Points.ToString());
                }
            }

            ;
            //MapControl.Position = new PointLatLng();
        }

        private void ChangeMapType()
        {
            switch (_mapTypeCounter)
            {
                case 0:
                    MapControl.MapProvider = GoogleMapProvider.Instance;
                    _mapTypeCounter++;
                    break;

                case 1:
                    MapControl.MapProvider = GoogleSatelliteMapProvider.Instance;
                    _mapTypeCounter--;
                    break;
            }
        }

        private async void InitializeUserAndPolygons()
        {
            try
            {
                var client = new HttpClient();
                // Fetch the current user's ID using the username
                var response = await client.GetAsync($"http://localhost:5035/api/auth/{_currentUsername}");
                var userJson = await response.Content.ReadAsStringAsync();

                // Use case-insensitive deserialization for the user DTO
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var userDto = JsonSerializer.Deserialize<UserDto>(userJson, options);
                if (userDto == null || userDto.Id == Guid.Empty)
                {
                    MessageBox.Show("Failed to fetch current user information.");
                    return;
                }

                _currentUserId = userDto.Id;

                // Load polygons for the current user
                LoadPolygonsFromServer();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing user and polygons: {ex.Message}");
            }
        }

        public class UserDto
        {
            public Guid Id { get; set; }
            public string Username { get; set; }
            public string Name { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
        }

        private async void LoadPolygonsFromServer()
        {
            try
            {
                // Ensure _currentUserId is set
                if (_currentUserId == Guid.Empty)
                {
                    MessageBox.Show("User ID is not available.");
                    return;
                }

                var client = new HttpClient();
                // Fetch polygons for the current user
                var response = await client.GetAsync($"http://localhost:5035/api/Polygons?userId={_currentUserId}");

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Failed to fetch polygons. Status code: {response.StatusCode}");
                    return;
                }

                var polygonsJson = await response.Content.ReadAsStringAsync();

                // Use case-insensitive options to avoid casing issues
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var polygons = JsonSerializer.Deserialize<List<PolygonDto>>(polygonsJson, options);

                if (polygons == null || polygons.Count == 0)
                {
                    Console.WriteLine("No polygons found for the current user.");
                    return;
                }

                // Add each polygon to the map
                foreach (var polygon in polygons)
                {
                    _polygons.Add(polygon);
                    _polygonNames.Add(polygon.Name);
                    AddPolygonToMap(polygon);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading polygons: {ex.Message}");
            }
        }

        // Updated DTO classes with JSON property mapping
        public class PolygonDto
        {
            [JsonPropertyName("polygonId")] public Guid Id { get; set; }

            [JsonPropertyName("name")] public string Name { get; set; }

            [JsonPropertyName("createdByUserId")] public Guid CreatedByUserId { get; set; }

            [JsonPropertyName("createdDate")] public DateTime CreatedDate { get; set; }

            [JsonPropertyName("points")] public List<PointDto> Points { get; set; }
        }

        public class PointDto
        {
            [JsonPropertyName("pointId")] public Guid PointId { get; set; }

            [JsonPropertyName("latitude")] public decimal Latitude { get; set; }

            [JsonPropertyName("longitude")] public decimal Longitude { get; set; }

            [JsonPropertyName("order")] public int Order { get; set; }
        }
        

        public class CreatePolygonRequest
        {
            public Guid UserId { get; set; }
            public string Name { get; set; }
            public List<PointRequest> Points { get; set; }
        }

        public class PointRequest
        {
            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
        }

        private void MapControl_OnPositionChanged(PointLatLng point)
        {
            // Sync the ViewModel's ZoomLevel with the map's current zoom
            ZoomLevel = (int)MapControl.Zoom;
        }

        private void MapControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Get the position of the right-click
            var point = MapControl.FromLocalToLatLng((int)e.GetPosition(MapControl).X,
                (int)e.GetPosition(MapControl).Y);

            // Add a marker at the clicked position
            AddMarker(point);

            // Add the coordinate to the list
            _markerCoordinates.Add(point);
        }

        private void AddMarker(PointLatLng point)
        {
            GMapMarker marker = new GMapMarker(point)
            {
                Shape = new System.Windows.Shapes.Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = Brushes.Red,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                }
            };

            MapControl.Markers.Add(marker);
        }

        // Method to add a polygon to the map from a DTO
        private void AddPolygonToMap(PolygonDto polygon)
        {
            try
            {
                if (polygon == null)
                {
                    Console.WriteLine("Polygon is null.");
                    return;
                }

                if (polygon.Points == null || polygon.Points.Count == 0)
                {
                    Console.WriteLine("Polygon points are null or empty.");
                    return;
                }

                var points = polygon.Points
                    .OrderBy(p => p.Order)
                    .Select(p => new PointLatLng((double)p.Latitude, (double)p.Longitude))
                    .ToList();

                if (MapControl == null)
                {
                    Console.WriteLine("MapControl is null.");
                    return;
                }

                if (MapControl.Markers == null)
                {
                    Console.WriteLine("MapControl.Markers is null.");
                    return;
                }

                var gmapPolygon = new GMapPolygon(points)
                {
                    Shape = new System.Windows.Shapes.Polygon
                    {
                        Stroke = Brushes.Red,
                        Fill = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)),
                        StrokeThickness = 2
                    },
                    Tag = polygon
                        .Name // Store polygon Name for reference (!!!Daca crapa ceva schimba in polygon.Id si nu o sa mai mearga RemovePolygonFromMap)
                };

                MapControl.Markers.Add(gmapPolygon);

                // Add text to the polygon
                AddTextToPolygon(gmapPolygon, polygon.Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding polygon to map: {ex.Message}");
            }
        }

        // Modified CreatePolygon method to save to the server
       private async void CreatePolygon()
{
    if (_markerCoordinates.Count < 3)
    {
        Console.WriteLine("Trebuie să ai cel puțin 3 puncte pentru a crea un poligon.");
        return;
    }

    foreach (var polygonName in _polygonNames)
    {
        if (polygonName == PolygonName)
        {
            MessageBox.Show("Numele poligonului există deja.");
            return;
        }
    }

    try
    {
        if (_currentUserId == Guid.Empty)
        {
            Console.WriteLine("User ID nu este disponibil.");
            return;
        }

        if (string.IsNullOrWhiteSpace(PolygonName))
        {
            MessageBox.Show("Te rog să introduci un nume pentru poligon.");
            return;
        }

        // Factor de scalare pentru a mări precizia coordonatelor
        double scale = 1000000.0;

        // Convertim markerCoordinates într-un PathD pentru noul poligon (aplicăm scalare)
        var newPolygonPathD = new PathD();
        foreach (var marker in _markerCoordinates)
        {
            newPolygonPathD.Add(new PointD(marker.Lng * scale, marker.Lat * scale));
        }
        // Convertim noul poligon în Path64 (valorile scalate)
        var newPolygonPath64 = Clipper.Path64(newPolygonPathD);

        // Verificăm suprapunerea cu fiecare poligon existent din listă
        foreach (var existingPolygonDto in _polygons)
        {
            // Convertim poligonul existent (PolygonDto) într-un PathD (aplicăm scalare)
            var existingPolygonPathD = new PathD();
            foreach (var pt in existingPolygonDto.Points.OrderBy(p => p.Order))
            {
                // Folosim Longitude pentru x și Latitude pentru y
                existingPolygonPathD.Add(new PointD((double)pt.Longitude * scale, (double)pt.Latitude * scale));
            }
            // Convertim în Path64
            var existingPolygonPath64 = Clipper.Path64(existingPolygonPathD);

            // Efectuăm operația de intersecție între noul poligon și cel existent
            Paths64 intersectionResult = Clipper.Intersect(
                new Paths64 { newPolygonPath64 },
                new Paths64 { existingPolygonPath64 },
                FillRule.NonZero
            );

            // Dacă rezultatul intersecției nu este gol, există suprapunere
            if (intersectionResult != null && intersectionResult.Any())
            {
                MessageBox.Show("Noul poligon se suprapune peste un poligon existent!");
                return;
            }
        }

        // Nu a fost detectată nicio suprapunere, deci continuăm cu trimiterea către server
        var client = new HttpClient();
        var request = new CreatePolygonRequest
        {
            UserId = _currentUserId,
            Name = PolygonName,
            Points = _markerCoordinates.ConvertAll(p => new PointRequest
            {
                Latitude = (decimal)p.Lat,
                Longitude = (decimal)p.Lng
            })
        };

        _polygonNames.Add(request.Name);

        var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"https://localhost:7088/api/Polygons?userId={_currentUserId}", jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Crearea poligonului a eșuat. Status code: {response.StatusCode}");
            return;
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var createdPolygon = JsonSerializer.Deserialize<PolygonDto>(responseContent, options);

        if (createdPolygon == null || createdPolygon.Id == Guid.Empty)
        {
            Console.WriteLine("Crearea poligonului pe server a eșuat.");
            return;
        }

        // Dacă dorești să afișezi poligonul la coordonate reale, poți scala invers (opțional)
        // Aici folosim newPolygonPathD, dar fără scalare inversă arată coordonatele scalate
        // Pentru afișare, este recomandat să refaci conversia la coordonate reale:
        var newPolygonPathDForDisplay = new PathD();
        foreach (var pt in newPolygonPathD)
        {
            newPolygonPathDForDisplay.Add(new PointD(pt.x / scale, pt.y / scale));
        }

        // Adăugăm poligonul nou pe hartă
        AddClipperPolygonToMap(newPolygonPathDForDisplay, createdPolygon.Name);
        PolygonsUpdated?.Invoke();
        ClearMarkers();
        PolygonName = string.Empty;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Eroare la crearea poligonului: {ex.Message}");
    }
}


        private void AddClipperPolygonToMap(PathD clipperPolygon, string polygonName)
        {
            // Convertește PathD la o listă de PointLatLng (asigură-te că ordinea coordonatelor e corectă)
            var points = clipperPolygon
                .Select(p => new PointLatLng(p.y, p.x)) // reține: în Clipper2, de obicei se folosește (X, Y)
                .ToList();

            var gmapPolygon = new GMapPolygon(points)
            {
                Shape = new System.Windows.Shapes.Polygon
                {
                    Stroke = Brushes.Red,
                    Fill = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)),
                    StrokeThickness = 2
                },
                Tag = polygonName
            };

            MapControl.Markers.Add(gmapPolygon);
            AddTextToPolygon(gmapPolygon, polygonName);
        }


        // Method to delete a polygon from the server
        public async void DeletePolygon()
        {
            try
            {
                var pName = PolygonName;
                var client = new HttpClient();

                // var encodedName = Uri.EscapeDataString(PolygonName);
                var response =
                    await client.DeleteAsync($"https://localhost:7088/api/Polygons/{pName}?userId={_currentUserId}");

                if (response.IsSuccessStatusCode)
                {
                    PolygonsUpdated?.Invoke();
                    RemovePolygonFromMap(pName);
                }
                else
                {
                    Console.WriteLine("Failed to delete polygon from the server.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting polygon: {ex.Message}");
            }
        }

        // Method to remove a polygon from the map
        private void RemovePolygonFromMap(string polygonName)
        {
            //remove based on the polygonName
            var toRemove = MapControl.Markers
                .FirstOrDefault(m =>
                    m is GMapPolygon polygon && polygon.Tag != null && polygon.Tag.ToString() == polygonName);

            if (toRemove != null)
            {
                MapControl.Markers.Remove(toRemove);
            }

            var textMarkers = MapControl.Markers
                .Where(m => m.Shape is Button tb && tb.Content?.ToString() == polygonName)
                .ToList();

            foreach (var marker in textMarkers)
            {
                MapControl.Markers.Remove(marker);
            }

            CenterPointNames.Remove(polygonName);

            _polygonNames.Remove(polygonName);
        }

        private void AddTextToPolygon(GMapPolygon polygon, string text)
        {
            PointLatLng centroid = CalculateCentroid(polygon.Points, text);

            CenterPointsAndName _centroidPointsAndName = new CenterPointsAndName();
            _centroidPointsAndName.Points = centroid;
            _centroidPointsAndName.Name = text;
            _centerPoints.Add(_centroidPointsAndName);
            CenterPointNames.Add(text);

            Button textBlock = new Button()
            {
                Content = text,
                Foreground = Brushes.Black,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Background = Brushes.Transparent,
                Padding = new Thickness(2),
                BorderThickness = new Thickness(0)
            };

            textBlock.Click += (sender, args) =>
            {
                if (sender is Button clickedButton && clickedButton.Content is string buttonText)
                {
                    InitiateDataForPolygon(buttonText);
                }
            };

            // Setăm ZIndex-ul pentru a ne asigura că textul este afișat deasupra poligonului
            Panel.SetZIndex(textBlock, 999);

            ScaleTransform scaleTransform = new ScaleTransform();
            textBlock.RenderTransform = scaleTransform;

            GMapMarker textMarker = new GMapMarker(centroid)
            {
                Shape = textBlock
            };

            MapControl.Markers.Add(textMarker);

            ZoomChanged += (zoom) => UpdateTextScale(textBlock, scaleTransform, zoom);

            UpdateTextScale(textBlock, scaleTransform, ZoomLevel);
        }

        public ParcelData SelectedParcel
        {
            get => _selectedParcel;
            set
            {
                _selectedParcel = value;
                OnPropertyChanged(nameof(SelectedParcel));
            }
        }

        public ParcelData SelectedParcel2
        {
            get => _selectedParcel2;
            set
            {
                _selectedParcel2 = value;
                OnPropertyChanged(nameof(SelectedParcel2));
            }
        }

        private async void InitiateDataForPolygon(string polygonName)
        {
            Console.WriteLine($"Ai apăsat pe butonul: {polygonName}");

            await InitiatePolygonsNameAndId();

            Guid _id = new Guid();
            foreach (var item in _parcelNameAndIDs)
            {
                if (item.Name == polygonName)
                    _id = item.Id;
            }

            Console.WriteLine(_id);

            InitiateDataForPolygonAnimals(polygonName, _id);

            try
            {
                ParcelData parcelData = new ParcelData();

                HttpClient client = new HttpClient();

                var response = await client.GetAsync($"https://localhost:7088/api/ParcelData/polygon/{_id}")
                    .ConfigureAwait(false);

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
                    parcelData.Field2 = firstParcel.CropType;
                    parcelData.Field3 = firstParcel.ParcelArea.ToString();
                    parcelData.Field4 = firstParcel.IrrigationType;
                    parcelData.Field5 = firstParcel.FertilizerUsed.ToString();
                    parcelData.Field6 = firstParcel.PesticideUsed.ToString();
                    parcelData.Field7 = firstParcel.Yield.ToString();
                    parcelData.Field8 = firstParcel.SoilType;
                    parcelData.Field9 = firstParcel.WaterUsage.ToString();

                    SelectedParcel = parcelData;
                }
                else
                {
                    Console.WriteLine("No data found.");
                }
            }
            catch
            {
                Console.WriteLine("Failed to initiate data for a Polygon.");
            }
        }

        private async void InitiateDataForPolygonAnimals(string polygonName, Guid _id)
        {
            try
            {
                ParcelData parcelData = new ParcelData();

                HttpClient client = new HttpClient();

                var response = await client.GetAsync($"https://localhost:7088/api/AnimalParcelData/polygon/{_id}")
                    .ConfigureAwait(false);

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
                var AnimalParcelDataList = JsonSerializer.Deserialize<List<AnimalParcelDataDto>>(json, options);


                if (AnimalParcelDataList != null && AnimalParcelDataList.Count > 0)
                {
                    var firstParcel = AnimalParcelDataList[0]; // Ia primul element dacă e nevoie
                    parcelData.Option = "Animale";
                    parcelData.Field1 = polygonName;
                    parcelData.Field2 = firstParcel.AnimalType;
                    parcelData.Field3 = firstParcel.NumberOfAnimale.ToString();
                    parcelData.Field4 = firstParcel.FeedType;
                    parcelData.Field5 = firstParcel.WaterConsumption.ToString();
                    parcelData.Field6 = firstParcel.VeterinaryVisits.ToString();
                    parcelData.Field7 = firstParcel.WasteManagement;
                    parcelData.Field8 = "";
                    parcelData.Field9 = "";

                    SelectedParcel2 = parcelData;
                }
                else
                {
                    Console.WriteLine("No data found.");
                }
            }
            catch
            {
                Console.WriteLine("Failed to initiate data for a Polygon.");
            }
        }

        private async Task InitiatePolygonsNameAndId()
        {
            _parcelNameAndIDs.Clear();

            HttpClient client = new HttpClient();
            Console.WriteLine($"Current user ID: {_currentUserId}");

            var response = await client.GetAsync($"https://localhost:7088/api/Polygons/names?userId={_currentUserId}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to fetch polygons. Status code: {response.StatusCode}");
                return;
            }

            var polygonsJson = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Răspuns server: {polygonsJson}"); // Verifică ce primești

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var polygons = JsonSerializer.Deserialize<List<ParcelNameAndID>>(polygonsJson, options);

            if (polygons == null || polygons.Count == 0)
            {
                Console.WriteLine("Nu s-au găsit poligoane pentru acest utilizator.");
                return;
            }

            foreach (var polygon in polygons)
            {
                try
                {
                    ParcelNameAndID parcelData = new ParcelNameAndID
                    {
                        Name = polygon.Name,
                        Id = polygon.Id
                    };
                    _parcelNameAndIDs.Add(parcelData);
                    Console.WriteLine($"Adăugat poligon: {parcelData.Name} cu Id: {parcelData.Id}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Eroare la procesarea poligonului: {ex.Message}");
                }
            }
        }


        // Update text size based on zoom level
        private void UpdateTextScale(Button textBlock, ScaleTransform scaleTransform, int zoomLevel)
        {
            double scale = Math.Max(0.1, zoomLevel / 18.0);
            scaleTransform.ScaleX = scale;
            scaleTransform.ScaleY = scale;
        }

        private void UpdateLabelVisibility()
        {
            foreach (var marker in MapControl.Markers)
            {
                if (marker.Shape is Button textBlock)
                {
                    if (ZoomLevel < 13)
                    {
                        textBlock.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        textBlock.Visibility = Visibility.Visible;
                        double scale = Math.Max(0.3, ZoomLevel / 18.0);
                        ((ScaleTransform)textBlock.RenderTransform).ScaleX = scale;
                        ((ScaleTransform)textBlock.RenderTransform).ScaleY = scale;
                    }
                }
            }
        }

        private void ClearMarkers()
        {
            var markersToRemove = new List<GMapMarker>();

            foreach (var marker in MapControl.Markers)
            {
                if (marker.Shape is System.Windows.Shapes.Ellipse)
                {
                    markersToRemove.Add(marker);
                }
            }

            foreach (var marker in markersToRemove)
            {
                MapControl.Markers.Remove(marker);
            }

            _markerCoordinates.Clear();
        }

        private void SaveCoordinates(List<PointLatLng> coordinates)
        {
            foreach (var point in coordinates)
            {
                // Example: Log the coordinates to the console
                // Console.WriteLine($"Saved Coordinate: Lat = {point.Lat}, Lng = {point.Lng}");
            }
            // TODO: Add logic to save to a database or file
        }

        private void OnMapClicked(PointLatLng point)
        {
            Console.WriteLine($"Clicked at: {point.Lat}, {point.Lng}");
        }

        private PointLatLng CalculateCentroid(List<PointLatLng> points, string text)
        {
            double latitudeSum = 0;
            double longitudeSum = 0;

            foreach (var point in points)
            {
                latitudeSum += point.Lat;
                longitudeSum += point.Lng;
            }

            double centroidLat = latitudeSum / points.Count;
            double centroidLng = longitudeSum / points.Count;

            Console.WriteLine($"Centroid: Lat = {centroidLat}, Lng = {centroidLng}");
            return new PointLatLng(centroidLat, centroidLng);
        }

        private void DeleteLastMarker()
        {
            if (_markerCoordinates.Count == 0)
            {
                Console.WriteLine("No markers to remove.");
                return;
            }

            // Get the last coordinate added
            var lastCoordinate = _markerCoordinates.Last();
            _markerCoordinates.RemoveAt(_markerCoordinates.Count - 1);

            // Find the corresponding marker in the MapControl.Markers
            // We look for a marker with an Ellipse shape whose position matches the last coordinate.
            var markerToRemove = MapControl.Markers
                .OfType<GMapMarker>()
                .LastOrDefault(m =>
                    m.Shape is System.Windows.Shapes.Ellipse &&
                    m.Position.Lat.Equals(lastCoordinate.Lat) &&
                    m.Position.Lng.Equals(lastCoordinate.Lng));

            if (markerToRemove != null)
            {
                MapControl.Markers.Remove(markerToRemove);
                Console.WriteLine($"Removed marker at: {lastCoordinate.Lat}, {lastCoordinate.Lng}");
            }
            else
            {
                Console.WriteLine("Marker not found for the last coordinate.");
            }
        }

        private void ZoomIn()
        {
            if (ZoomLevel < 18)
            {
                ZoomLevel++;
                Console.WriteLine($"ZoomIn: ZoomLevel = {ZoomLevel}");
            }
        }

        private void ZoomOut()
        {
            if (ZoomLevel > 2)
            {
                ZoomLevel--;
                Console.WriteLine($"ZoomOut: ZoomLevel = {ZoomLevel}");
            }
        }
    }

    public class CenterPointsAndName
    {
        public PointLatLng Points { get; set; }
        public string Name { get; set; }
    }
}