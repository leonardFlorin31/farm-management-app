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
        public static string _currentRole = MainViewModel.CurrentRole.RoleName;

        private PointLatLng _mapCenter;
        private int _zoomLevel = 13; // Initial zoom level
        private GMapProvider _mapProvider = GoogleSatelliteMapProvider.Instance;
        public int _mapTypeCounter = 0;
        private GMapMarker _draggedMarker;
        private PointLatLng _dragStartPoint;
        private int _polygonMarkerCounter = 0;
        private GMapMarker _selectedMarker = null;
        private GMapPolygon _editablePolygon = null;
        private List<GMapMarker> _controlMarkers = new List<GMapMarker>();
        private GMapMarker _draggedControlMarker = null;
        private List<EditablePolygon> _allPolygons = new List<EditablePolygon>();

        private EditablePolygon _currentEditablePolygon; // Poligonul editat în momentul curent
        private List<PointLatLng> _initialPolygonCoordinates;

// Poligonul selectat în momentul curent
        private EditablePolygon _selectedPolygon;


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
        public Guid _currentUserId; // Will be set after login
        private string _currentUsername = LoginViewModel.UsernameForUse.Username;
        private List<string> _polygonNames = new List<string> { };

        private Stack<IUndoableCommand> _undoStack = new Stack<IUndoableCommand>();
        private Stack<IUndoableCommand> _redoStack = new Stack<IUndoableCommand>();

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
        private List<PointLatLng> _editingPolygonCoordinates;
        private EditablePolygon _currentlyModifyingPolygon;

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

        public ICommand UndoCommand { get; }

        public ICommand RedoCommand { get; }

        public MapViewModel()
        {
           
            
            if (_currentRole != "Contabil")
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

                RedoCommand = new RelayCommand(Redo);
                UndoCommand = new RelayCommand(Undo);
                MapControl.MouseRightButtonDown += MapControl_MouseRightButtonDown;

                MapControl.MouseMove += MapControl_MouseMove;
                MapControl.MouseLeftButtonUp += MapControl_MouseLeftButtonUp;
            }
            else
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
            }

           

            
            
            
        }

        private void OnResizeCorner(DragDeltaEventArgs e)
        {
            // Actualizează ambele dimensiuni
            OnResizeHorizontal(e);
            OnResizeVertical(e);
        }

        public void Undo()
        {
            if (_undoStack.Any())
            {
                var command = _undoStack.Pop();
                command.Unexecute();
                _redoStack.Push(command);
            }
            else
            {
                MessageBox.Show("Nu mai există acțiuni de revenit.");
            }
        }

        public void Redo()
        {
            if (_redoStack.Any())
            {
                var command = _redoStack.Pop();
                command.Execute();
                _undoStack.Push(command);
            }
            else
            {
                MessageBox.Show("Nu mai există acțiuni pentru redo.");
            }
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
            ParcelDetailsVisibility = ParcelDetailsVisibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
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
                // Verifică dacă _currentUserId este setat
                if (_currentUserId == Guid.Empty)
                {
                    MessageBox.Show("User ID is not available.");
                    return;
                }

                var client = new HttpClient();
                // Preluăm poligoanele pentru utilizatorul curent
                var response = await client.GetAsync($"http://localhost:5035/api/Polygons?userId={_currentUserId}");

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Failed to fetch polygons. Status code: {response.StatusCode}");
                    return;
                }

                var polygonsJson = await response.Content.ReadAsStringAsync();

                // Opțiuni pentru deserializare fără probleme de casing
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var polygonDtos = JsonSerializer.Deserialize<List<PolygonDto>>(polygonsJson, options);

                if (polygonDtos == null || polygonDtos.Count == 0)
                {
                    Console.WriteLine("No polygons found for the current user.");
                    return;
                }

                // Parcurgem fiecare polygon primit și îl transformăm într-un poligon editabil
                foreach (var dto in polygonDtos)
                {
                    _polygons.Add(dto);
                    // Creăm obiectul EditablePolygon din DTO
                    var editablePolygon = new EditablePolygon
                    {
                        Name = dto.Name,
                        Coordinates = dto.Points
                            .OrderBy(p => p.Order)
                            .Select(p => new PointLatLng((double)p.Latitude, (double)p.Longitude))
                            .ToList()
                    };

                    // Creăm forma poligonului (GMapPolygon) pentru afișare pe hartă
                    editablePolygon.Polygon = new GMapPolygon(editablePolygon.Coordinates)
                    {
                        Shape = new System.Windows.Shapes.Polygon
                        {
                            Stroke = Brushes.Red,
                            Fill = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)),
                            StrokeThickness = 2
                        },
                        Tag = editablePolygon.Name // Utilizat ulterior pentru identificare
                    };

                    // Adăugăm poligonul pe hartă
                    MapControl.Markers.Add(editablePolygon.Polygon);

                    // Adăugăm markerele de control pentru fiecare punct (pentru a permite editarea)
                    AddControlMarkersForPolygon(editablePolygon);
                    AddTextToPolygon(editablePolygon.Polygon, editablePolygon.Name);

                    // Rețin poligonul editabil în lista de poligoane globale pentru modificări ulterioare
                    _allPolygons.Add(editablePolygon);

                    _polygonNames.Add(editablePolygon.Name);
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

        public class UpdatePolygonRequest
        {
            public Guid UserId { get; set; }
            public Guid Id { get; set; }
            public List<PointRequest> Points { get; set; }
            public string Name { get; set; }
        }

        public class PointRequest
        {
            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
            public int Order { get; set; }
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
            GMapMarker marker = new GMapMarker(point);
            marker.Shape = new System.Windows.Shapes.Ellipse
                { Width = 10, Height = 10, Fill = Brushes.Blue }; // Use a simple shape for now
            marker.Offset = new Point(-5, -5);
            MapControl.Markers.Add(marker);
            _polygonMarkerCounter++;

            if (marker.Shape != null) // Ensure the shape exists
            {
                marker.Shape.MouseLeftButtonDown += (sender, e) => MarkerShape_OnMouseLeftButtonDown(marker, e);
            }
        }

        private void MarkerShape_OnMouseLeftButtonDown(GMapMarker marker, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _draggedMarker = marker;
                System.Windows.Point mousePosition = e.GetPosition(MapControl);
                _dragStartPoint = MapControl.FromLocalToLatLng((int)mousePosition.X, (int)mousePosition.Y);
                Mouse.Capture(MapControl); // Capture mouse for the map
                e.Handled = true;
            }

            _selectedMarker = marker;

            // Dacă markerul aparține unui poligon, salvează coordonatele inițiale
            foreach (var polygon in _allPolygons)
            {
                if (polygon.ControlMarkers.Contains(marker))
                {
                    _initialPolygonCoordinates = new List<PointLatLng>(polygon.Coordinates);
                    _currentlyModifyingPolygon = polygon; // Setează poligonul curent de modificat
                    break;
                }
            }

            Console.WriteLine($"Marker selected at: {marker.Position.Lat}, {marker.Position.Lng}");
        }


        private void MapControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (_draggedMarker != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Point p = e.GetPosition(MapControl);
                PointLatLng newLatLng = MapControl.FromLocalToLatLng((int)p.X, (int)p.Y);
                _draggedMarker.Position = newLatLng;

                int markerIndex = MapControl.Markers.IndexOf(_draggedMarker) -
                                  (MapControl.Markers.Count - _polygonMarkerCounter);

                if (markerIndex >= 0 && markerIndex < _markerCoordinates.Count)
                {
                    _markerCoordinates[markerIndex] = newLatLng;
                }

                bool isEditingMarker = false;
                foreach (var polygon in _allPolygons)
                {
                    if (polygon.ControlMarkers.Contains(_draggedMarker))
                    {
                        isEditingMarker = true;
                        break;
                    }
                }

                // Actualizează _markerCoordinates doar dacă nu editezi un marker existent
                if (!isEditingMarker && markerIndex >= 0 && markerIndex < _markerCoordinates.Count)
                {
                    _markerCoordinates[markerIndex] = newLatLng;
                }

                foreach (var polygon in _allPolygons)
                {
                    int controlIndex = polygon.ControlMarkers.IndexOf(_draggedMarker);
                    if (controlIndex != -1)
                    {
                        // Actualizează coordonatele poligonului
                        polygon.Coordinates[controlIndex] = newLatLng;

                        // Set the currently modifying polygon
                        _currentlyModifyingPolygon = polygon;

                        // Șterge vechiul poligon de pe hartă
                        MapControl.Markers.Remove(polygon.Polygon);

                        // Crează un poligon nou cu punctele actualizate
                        polygon.Polygon = new GMapPolygon(polygon.Coordinates)
                        {
                            Shape = new System.Windows.Shapes.Polygon
                            {
                                Stroke = Brushes.Red,
                                Fill = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)),
                                StrokeThickness = 2
                            },
                            Tag = polygon.Name
                        };


                        // Adaugă noul poligon pe hartă
                        MapControl.Markers.Add(polygon.Polygon);
                        MapControl.InvalidateVisual();
                        break;
                    }
                }
            }

            MapControl.InvalidateVisual();
        }

        private void UpdatePolygonShape()
        {
            // Șterge poligonul vechi de pe hartă
            if (_editablePolygon != null)
            {
                MapControl.Markers.Remove(_editablePolygon);
            }

            // Crează un poligon nou cu punctele actualizate
            _editablePolygon = new GMapPolygon(_editingPolygonCoordinates)
            {
                Shape = new System.Windows.Shapes.Polygon
                {
                    Stroke = Brushes.Red,
                    Fill = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)),
                    StrokeThickness = 2
                },
                Tag = "EditablePolygon"
            };

            // Adaugă noul poligon pe hartă
            MapControl.Markers.Add(_editablePolygon);
            MapControl.InvalidateVisual(); // Forțează actualizarea vizuală
        }

        private async void MapControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_draggedMarker != null)
            {
                _draggedMarker = null;
                Mouse.Capture(null); // Eliberăm captura mouse-ului
                e.Handled = true;
            }

            // Obține poziția mouse-ului în coordonate geografice
            var mousePosition = e.GetPosition(MapControl);
            PointLatLng position = MapControl.FromLocalToLatLng((int)mousePosition.X, (int)mousePosition.Y);

            if (_currentlyModifyingPolygon != null)
            {
                var newCoordinates = new List<PointLatLng>(_currentlyModifyingPolygon.Coordinates);
                bool changed = !Enumerable.SequenceEqual(_initialPolygonCoordinates, newCoordinates,
                    new PointLatLngComparer());

                if (changed)
                {
                    // Actualizează markerii de control înainte de a salva starea
                    for (int i = 0; i < _currentlyModifyingPolygon.ControlMarkers.Count; i++)
                    {
                        _currentlyModifyingPolygon.ControlMarkers[i].Position = newCoordinates[i];
                    }

                    var moveCommand = new MovePolygonCommand(_currentlyModifyingPolygon,
                        _initialPolygonCoordinates, newCoordinates, MapControl, AddTextToPolygon, _currentUserId);
                    _undoStack.Push(moveCommand);
                    _redoStack.Clear();
                }

                using (var client = new HttpClient())
                {
                    // 1. Cerere GET pentru a obține id-ul poligonului după nume și _currentUserId
                    var getUrl =
                        $"https://localhost:7088/api/Polygons/id-by-name?polygonName={_currentlyModifyingPolygon.Name}&userId={_currentUserId}";
                    var getResponse = await client.GetAsync(getUrl);
                    if (getResponse.IsSuccessStatusCode)
                    {
                        var jsonResponse = await getResponse.Content.ReadAsStringAsync();
                        // Presupunem că răspunsul este de forma: { "Id": "..." }
                        var polygonIdResponse = JsonSerializer.Deserialize<PolygonIdResponse>(jsonResponse);
                        if (polygonIdResponse != null)
                        {
                            Guid polygonId = polygonIdResponse.Id;

                            // 2. Construim request-ul de update pentru poligon
                            var updateRequest = new UpdatePolygonRequest
                            {
                                Name = _currentlyModifyingPolygon.Name,
                                Points = _currentlyModifyingPolygon.Coordinates
                                    .Select((p, index) => new PointRequest
                                    {
                                        Latitude = (decimal)p.Lat,
                                        Longitude = (decimal)p.Lng,
                                        Order = index
                                    }).ToList()
                            };

                            AddTextToPolygon(new GMapPolygon(_currentlyModifyingPolygon.Coordinates),
                                _currentlyModifyingPolygon.Name);

                            var jsonContent = new StringContent(JsonSerializer.Serialize(updateRequest), Encoding.UTF8,
                                "application/json");
                            var putUrl = $"https://localhost:7088/api/Polygons/{polygonId}?userId={_currentUserId}";
                            var putResponse = await client.PutAsync(putUrl, jsonContent);
                            if (putResponse.IsSuccessStatusCode)
                            {
                                // Update reușit. Poți adăuga codul de notificare sau UI aici.
                                Console.WriteLine(
                                    $"Polygon {_currentlyModifyingPolygon.Name} was successfully updated.");
                            }
                            else
                            {
                                Console.WriteLine("Failed to update polygon.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("failed to deserialize");
                        }
                    }
                    else
                    {
                        Console.WriteLine("poligonul nu a fost gasit");
                    }
                }

                _currentlyModifyingPolygon = null; // Reset după trimitere
            }


            // Verifică dacă poziția este în interiorul vreunui poligon
            // foreach (var polygon in _allPolygons)
            // {
            //     if (IsPointInPolygon(position, polygon.Coordinates))
            //     {
            //         _currentEditablePolygon = polygon; // Setează-l ca poligon curent
            //         break;
            //     }
            // }
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

                // foreach (var point in points)
                // {
                //     AddMarker(point);
                // }

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

        // Returnează true dacă există suprapunere.
        private bool CheckPolygonIntersection(EditablePolygon polygon)
        {
            // Factorul de scalare folosit în operațiile Clipper
            double scale = 1000000.0;

            // Construiți poligonul curent cu Clipper (Path64) pe baza coordonatelor scalate
            var currentPathD = new PathD();
            foreach (var pt in polygon.Coordinates)
            {
                currentPathD.Add(new PointD(pt.Lng * scale, pt.Lat * scale));
            }

            // Închidem poligonul, dacă nu este deja închis
            if (currentPathD.First() != currentPathD.Last())
            {
                currentPathD.Add(currentPathD.First());
            }

            var currentPath64 = Clipper.Path64(currentPathD);

            // Parcurgem toate celelalte poligoane (excludem poligonul curent)
            foreach (var otherPolygon in _allPolygons)
            {
                if (otherPolygon == polygon)
                    continue;

                var otherPathD = new PathD();
                foreach (var pt in otherPolygon.Coordinates)
                {
                    otherPathD.Add(new PointD(pt.Lng * scale, pt.Lat * scale));
                }

                if (otherPathD.First() != otherPathD.Last())
                {
                    otherPathD.Add(otherPathD.First());
                }

                var otherPath64 = Clipper.Path64(otherPathD);

                // Realizăm operația de intersecție între poligonul curent și cel existent
                Paths64 intersectionResult = Clipper.Intersect(
                    new Paths64 { currentPath64 },
                    new Paths64 { otherPath64 },
                    FillRule.NonZero
                );

                // Dacă rezultatul intersecției nu este gol, înseamnă că există suprapunere
                if (intersectionResult != null && intersectionResult.Any())
                {
                    return true;
                }
            }

            return false;
        }

        public class PointLatLngComparer : IEqualityComparer<PointLatLng>
        {
            private const double Tolerance = 1e-6;

            public bool Equals(PointLatLng a, PointLatLng b)
            {
                if (a == null && b == null) return true;
                if (a == null || b == null) return false;
                return Math.Abs(a.Lat - b.Lat) < Tolerance && Math.Abs(a.Lng - b.Lng) < Tolerance;
            }

            public int GetHashCode(PointLatLng obj)
            {
                return obj.Lat.GetHashCode() ^ obj.Lng.GetHashCode();
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

                var jsonContent =
                    new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"https://localhost:7088/api/Polygons?userId={_currentUserId}",
                    jsonContent);

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

                // Adăugăm poligonul nou pe hartă și îl reținem ca poligon editabil
                // Crează un nou obiect EditablePolygon
                var newPolygon = new EditablePolygon
                {
                    Name = PolygonName,
                    Coordinates = new List<PointLatLng>(_markerCoordinates) // Copiază coordonatele
                };

                // if (CheckPolygonIntersection(newPolygon))
                // {
                //   
                //     MessageBox.Show($"Poligonul '{newPolygon.Name}' se intersectează cu alt poligon.");
                //     return;
                // }

                _allPolygons.Add(newPolygon);
                //_polygons.Add(newPolygon);


                // Adaugă poligonul pe hartă
                newPolygon.Polygon = new GMapPolygon(newPolygon.Coordinates)
                {
                    Shape = new System.Windows.Shapes.Polygon
                    {
                        Stroke = Brushes.Red,
                        Fill = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)),
                        StrokeThickness = 2
                    },
                    Tag = PolygonName
                };
                //MapControl.Markers.Add(newPolygon.Polygon);

                // CREAREA PUNCTELOR DE CONTROL: pentru fiecare colț al poligonului, adăugăm un marker transparent
                // Salvează în lista globală

                _currentEditablePolygon = newPolygon; // Setează-l ca poligon curent

                // Adăugăm poligonul nou pe hartă
                //AddClipperPolygonToMap(newPolygonPathDForDisplay, createdPolygon.Name);

                _editingPolygonCoordinates = new List<PointLatLng>(_markerCoordinates);

                AddControlMarkersForPolygon(newPolygon);
                AddTextToPolygon(newPolygon.Polygon, newPolygon.Name);

                // Creăm comanda pentru adăugarea poligonului
                var command = new UndoRedoPolygonCommand(newPolygon, _allPolygons, MapControl, _polygonNames, _currentUserId);
                // Executăm comanda
                command.Execute();
                // Adăugăm comanda în stiva de undo
                _undoStack.Push(command);
                // Ștergem redoStack-ul deoarece s-a efectuat o acțiune nouă
                _redoStack.Clear();

                ClearMarkers();
                PolygonName = string.Empty;
                _polygonMarkerCounter = 0;

                PolygonsUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la crearea poligonului: {ex.Message}");
            }
        }

        private void AddControlMarkersForPolygon(EditablePolygon polygon)
        {
            // Șterge markerii existenți (dacă există)
            foreach (var marker in polygon.ControlMarkers)
            {
                MapControl.Markers.Remove(marker);
            }

            polygon.ControlMarkers.Clear();

            foreach (var point in polygon.Coordinates)
            {
                GMapMarker controlMarker = new GMapMarker(point)
                {
                    Shape = new System.Windows.Shapes.Ellipse
                    {
                        Width = 10,
                        Height = 10,
                        Stroke = Brushes.Gold,
                        Fill = Brushes.Transparent
                    },
                    Offset = new Point(-5, -5),
                    Tag = Guid.NewGuid()
                };

                // Eveniment pentru drag (cu variabilele corecte)
                controlMarker.Shape.MouseLeftButtonDown += (sender, e) =>
                {
                    _draggedMarker = controlMarker;
                    _selectedPolygon = polygon; // Setează poligonul selectat
                    MarkerShape_OnMouseLeftButtonDown(controlMarker, e);
                };

                MapControl.Markers.Add(controlMarker);
                polygon.ControlMarkers.Add(controlMarker);
            }
        }

        private bool IsPointInPolygon(PointLatLng point, List<PointLatLng> polygon)
        {
            // Algoritm simplu pentru a verifica dacă un punct este în interiorul unui poligon
            bool inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if (((polygon[i].Lat > point.Lat) != (polygon[j].Lat > point.Lat) &&
                     (point.Lng < (polygon[j].Lng - polygon[i].Lng) * (point.Lat - polygon[i].Lat) /
                         (polygon[j].Lat - polygon[i].Lat) + polygon[i].Lng)))
                {
                    inside = !inside;
                }
            }

            return inside;
        }


        private void AddClipperPolygonToMap(PathD clipperPolygon, string polygonName)
        {
            // Convertește PathD la o listă de PointLatLng (asigură-te că ordinea coordonatelor e corectă)
            var points = clipperPolygon
                .Select(p =>
                    new PointLatLng((double)p.y, (double)p.x)) // reține: în Clipper2, de obicei se folosește (X, Y)
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


            // foreach (var point in points)
            // {
            //     AddMarker(point);
            // }
        }


        // Method to delete a polygon from the server
        public async void DeletePolygon()
        {
            try
            {
                var pName = PolygonName;
                var client = new HttpClient();

                var response =
                    await client.DeleteAsync($"https://localhost:7088/api/Polygons/{pName}?userId={_currentUserId}");

                if (response.IsSuccessStatusCode)
                {
                    PolygonsUpdated?.Invoke();

                    // Debugging: Get control markers before deletion
                    var controlMarkersBeforeDeletion = new List<GMapMarker>();
                    var polygonToDelete = _allPolygons.FirstOrDefault(p => p.Name == pName);
                    if (polygonToDelete != null)
                    {
                        foreach (var marker in MapControl.Markers.Where(m =>
                                     polygonToDelete.Coordinates.Any(coord =>
                                         coord.Lat == m.Position.Lat && coord.Lng == m.Position.Lng)))
                        {
                            // Further check if it's a control marker (Ellipse shape)
                            if (marker.Shape is System.Windows.Shapes.Ellipse)
                            {
                                controlMarkersBeforeDeletion.Add(marker);
                                Console.WriteLine(
                                    $"Before Deletion - Found Control Marker at: {marker.Position}, Tag: {marker.Tag}");
                            }
                        }
                    }

                    // Create and execute the delete command
                    var deleteCommand = new DeletePolygonCommand(pName, _allPolygons, MapControl, _polygonNames, AddControlMarkersForPolygon, _currentUserId);
                    deleteCommand.Execute();
                    _undoStack.Push(deleteCommand);
                    _redoStack.Clear(); // Clear redo stack as a new action occurred

                    // Debugging: Check control markers after deletion
                    Console.WriteLine("After Deletion - Checking for remaining control markers:");
                    foreach (var markerBefore in controlMarkersBeforeDeletion)
                    {
                        if (MapControl.Markers.Contains(markerBefore))
                        {
                            Console.WriteLine(
                                $"After Deletion - Control Marker STILL PRESENT at: {markerBefore.Position}, Tag: {markerBefore.Tag}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Failed to delete polygon from the server.");
                }

                PolygonName = string.Empty;
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

            // Remove the control markers
            // Trebuie să găsim poligonul editabil corespunzător
            var editablePolygonToRemove = _allPolygons.FirstOrDefault(p => p.Name == polygonName);

            if (editablePolygonToRemove != null)
            {
                // Iterăm prin lista de markere de control a poligonului și le eliminăm de pe hartă
                foreach (var controlMarker in
                         editablePolygonToRemove.ControlMarkers
                             .ToList()) // Folosim ToList() pentru a evita erori de modificare a colecției în timpul iterării
                {
                    MapControl.Markers.Remove(controlMarker);
                }

                editablePolygonToRemove.ControlMarkers
                    .Clear(); // Optional: Clear the list of control markers from the EditablePolygon object
                _allPolygons.Remove(
                    editablePolygonToRemove); // Optional: Remove the EditablePolygon object if it's no longer needed
            }

            CenterPointNames.Remove(polygonName);

            _polygonNames.Remove(polygonName);
        }

        private void AddTextToPolygon(GMapPolygon polygon, string text)
        {
            string textMarkerTag = $"TextMarker_{text}";

            // Elimină markerii text existenți cu același tag specific
            var markersToRemove = MapControl.Markers
                .Where(m => m.Tag != null && m.Tag is string && m.Tag.ToString() == textMarkerTag)
                .ToList();
            foreach (var marker in markersToRemove)
            {
                MapControl.Markers.Remove(marker);
            }

            var pointsToRemove = _centerPoints.Where(cp => cp.Name == text).ToList();

            foreach (var centerPoint in pointsToRemove)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    _centerPoints.Remove(centerPoint);
                    CenterPointNames.Remove(text);
                });
            }

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
            Panel.SetZIndex(textBlock, Int32.MaxValue);

            ScaleTransform scaleTransform = new ScaleTransform();
            textBlock.RenderTransform = scaleTransform;

            GMapMarker textMarker = new GMapMarker(centroid)
            {
                Shape = textBlock,
                Tag = textMarkerTag
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
                    .ConfigureAwait(true);

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
                    .ConfigureAwait(true);

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
                if (marker.Shape is System.Windows.Shapes.Ellipse ellipse)
                {
                    if (ellipse.Stroke != Brushes.Gold)
                    {
                        markersToRemove.Add(marker);
                    }
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
            // Dacă există un marker selectat prin click stânga, șterge-l
            if (_selectedMarker != null)
            {
                // Calculăm indexul relativ pentru a actualiza și _markerCoordinates
                int draggedMarkerIndexInAllMarkers = MapControl.Markers.IndexOf(_selectedMarker);
                int relativeIndex = draggedMarkerIndexInAllMarkers - (MapControl.Markers.Count - _polygonMarkerCounter);

                if (relativeIndex >= 0 && relativeIndex < _markerCoordinates.Count)
                {
                    _markerCoordinates.RemoveAt(relativeIndex);
                }

                MapControl.Markers.Remove(_selectedMarker);
                Console.WriteLine(
                    $"Removed selected marker at: {_selectedMarker.Position.Lat}, {_selectedMarker.Position.Lng}");
                _selectedMarker = null; // Resetează selecția
                return;
            }

            // Dacă nu există marker selectat, ștergem ultimul marker adăugat
            if (_markerCoordinates.Count == 0)
            {
                Console.WriteLine("No markers to remove.");
                return;
            }

            var lastCoordinate = _markerCoordinates.Last();
            _markerCoordinates.RemoveAt(_markerCoordinates.Count - 1);

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

    public class PolygonIdResponse
    {
        [JsonPropertyName("id")] public Guid Id { get; set; }
    }

    public class CenterPointsAndName
    {
        public PointLatLng Points { get; set; }
        public string Name { get; set; }
    }

    public class EditablePolygon
    {
        public GMapPolygon Polygon { get; set; }
        public List<GMapMarker> ControlMarkers { get; set; } = new List<GMapMarker>();
        public List<PointLatLng> Coordinates { get; set; } = new List<PointLatLng>();
        public string Name { get; set; }
    }

    public interface IUndoableCommand
    {
        void Execute();
        void Unexecute();
    }
    
    public class UpdatePolygonRequest
    {
        public Guid UserId { get; set; }
        public Guid Id { get; set; }
        public List<PointRequest> Points { get; set; }
        public string Name { get; set; }
    }

    public class UndoRedoPolygonCommand : IUndoableCommand
{
    private readonly EditablePolygon _polygon;
    private readonly List<EditablePolygon> _allPolygons;
    private readonly GMapControl _mapControl;
    private readonly List<string> _polygonNames;
    private readonly Guid _currentUserId;

    public UndoRedoPolygonCommand(
        EditablePolygon polygon,
        List<EditablePolygon> allPolygons,
        GMapControl mapControl,
        List<string> polygonNames,
        Guid currentUserId)
    {
        _polygon = polygon;
        _allPolygons = allPolygons;
        _mapControl = mapControl;
        _polygonNames = polygonNames;
        _currentUserId = currentUserId;
    }

    // Adaugă poligonul în colecție și pe hartă
    public void Execute()
    {
        _allPolygons.Add(_polygon);
        _polygonNames.Add(_polygon.Name);
        _mapControl.Markers.Add(_polygon.Polygon);
        foreach (var marker in _polygon.ControlMarkers)
        {
            _mapControl.Markers.Add(marker);
        }
    }

    // Șterge poligonul din colecție și de pe hartă, și îl șterge de pe server
    public async void Unexecute()
    {
        _allPolygons.Remove(_polygon);
        _polygonNames.Remove(_polygon.Name);
        _mapControl.Markers.Remove(_polygon.Polygon);
        foreach (var marker in _polygon.ControlMarkers)
        {
            _mapControl.Markers.Remove(marker);
        }

        // Șterge poligonul de pe server (deoarece a fost creat în CreatePolygon)
        using (var client = new HttpClient())
        {
            var getUrl =
                $"https://localhost:7088/api/Polygons/id-by-name?polygonName={_polygon.Name}&userId={_currentUserId}";
            var getResponse = await client.GetAsync(getUrl);
            if (getResponse.IsSuccessStatusCode)
            {
                var jsonResponse = await getResponse.Content.ReadAsStringAsync();
                var polygonIdResponse = JsonSerializer.Deserialize<PolygonIdResponse>(jsonResponse);
                if (polygonIdResponse != null)
                {
                    Guid polygonId = polygonIdResponse.Id;
                    var deleteUrl = $"https://localhost:7088/api/Polygons/{polygonId}?userId={_currentUserId}";
                    var deleteResponse = await client.DeleteAsync(deleteUrl);
                    if (deleteResponse.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Poligonul '{_polygon.Name}' a fost șters de pe server în Unexecute.");
                    }
                    else
                    {
                        Console.WriteLine($"Ștergerea poligonului '{_polygon.Name}' de pe server a eșuat în Unexecute. Status code: {deleteResponse.StatusCode}");
                    }
                }
                else
                {
                    Console.WriteLine("Deserializarea răspunsului cu ID-ul poligonului a eșuat în Unexecute.");
                }
            }
            else
            {
                Console.WriteLine($"Obținerea ID-ului poligonului '{_polygon.Name}' de pe server a eșuat în Unexecute.");
            }
        }
    }
}

    public class MovePolygonCommand : IUndoableCommand
    {
    
        private readonly EditablePolygon _polygon;
        private readonly List<PointLatLng> _oldCoordinates;
        private readonly List<PointLatLng> _newCoordinates;
        private readonly GMapControl _mapControl;
        private readonly Action<GMapPolygon , string > _addTextToPolygon; // Reference to the method
        private Guid _currentUserId;
        

        public MovePolygonCommand(EditablePolygon polygon, List<PointLatLng> oldCoordinates,
            List<PointLatLng> newCoordinates, GMapControl mapControl,  Action<GMapPolygon , string> addTextToPolygonn, Guid currentUserId)
        {
            _polygon = polygon;
            _oldCoordinates = new List<PointLatLng>(oldCoordinates);
            _newCoordinates = new List<PointLatLng>(newCoordinates);
            _mapControl = mapControl;
            _addTextToPolygon = addTextToPolygonn;
            _currentUserId = currentUserId;
        }

        public void Execute()
        {
            ApplyCoordinates(_newCoordinates);
        }

        public void Unexecute()
        {
            ApplyCoordinates(_oldCoordinates);
        }

        private async void ApplyCoordinates(List<PointLatLng> coordinates)
        {
            // Actualizează coordonatele poligonului
            _polygon.Coordinates = new List<PointLatLng>(coordinates);

            // Actualizează pozițiile markerelor de control
            for (int i = 0; i < _polygon.ControlMarkers.Count; i++)
            {
                _polygon.ControlMarkers[i].Position = coordinates[i];
            }

            // Înlătură vechiul poligon de pe hartă
            _mapControl.Markers.Remove(_polygon.Polygon);

            // Crează un nou poligon cu coordonatele actualizate
            _polygon.Polygon = new GMapPolygon(_polygon.Coordinates)
            {
                Shape = new System.Windows.Shapes.Polygon
                {
                    Stroke = Brushes.Red,
                    Fill = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)),
                    StrokeThickness = 2
                },
                Tag = _polygon.Name
            };

            // Adaugă noul poligon pe hartă
            _mapControl.Markers.Add(_polygon.Polygon);
            
             // Actualizează poligonul pe server
        using (var client = new HttpClient())
        {
            var getUrl =
                $"https://localhost:7088/api/Polygons/id-by-name?polygonName={_polygon.Name}&userId={_currentUserId}";
            var getResponse = await client.GetAsync(getUrl);
            if (getResponse.IsSuccessStatusCode)
            {
                var jsonResponse = await getResponse.Content.ReadAsStringAsync();
                var polygonIdResponse = JsonSerializer.Deserialize<PolygonIdResponse>(jsonResponse);
                if (polygonIdResponse != null)
                {
                    Guid polygonId = polygonIdResponse.Id;

                    var updateRequest = new UpdatePolygonRequest
                    {
                        Name = _polygon.Name,
                        Points = _polygon.Coordinates
                            .Select((p, index) => new PointRequest
                            {
                                Latitude = (decimal)p.Lat,
                                Longitude = (decimal)p.Lng,
                                Order = index
                            }).ToList()
                    };

                    _addTextToPolygon(new GMapPolygon(_polygon.Coordinates),
                        _polygon.Name);

                    var jsonContent = new StringContent(JsonSerializer.Serialize(updateRequest), Encoding.UTF8,
                        "application/json");
                    var putUrl = $"https://localhost:7088/api/Polygons/{polygonId}?userId={_currentUserId}";
                    var putResponse = await client.PutAsync(putUrl, jsonContent);
                    if (putResponse.IsSuccessStatusCode)
                    {
                        Console.WriteLine(
                            $"Polygon {_polygon.Name} was successfully updated on the server.");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to update polygon {_polygon.Name} on the server.");
                    }
                }
                else
                {
                    Console.WriteLine("Failed to deserialize polygon ID response.");
                }
            }
            else
            {
                Console.WriteLine($"Failed to get polygon ID for {_polygon.Name} from the server.");
            }
        }

            
            _mapControl.InvalidateVisual();
        }
    }

   public class DeletePolygonCommand : IUndoableCommand
{
    private EditablePolygon _polygonToDelete;
    private readonly List<EditablePolygon> _allPolygons;
    private readonly GMapControl _mapControl;
    private readonly List<string> _polygonNames;
    private readonly string _polygonName;
    private int _polygonIndexInAllPolygons = -1;
    private int _polygonNameIndex = -1;
    private List<PointLatLng> _controlPointCoordinates = new List<PointLatLng>(); // Store control point coordinates for undo
    private GMapPolygon _polygonMarker;
    private GMapMarker _textMarker;
    private readonly Action<EditablePolygon> _addControlMarkersForPolygon; // Reference to the method
    private Guid _currentUserId;
    private List<GMapMarker> _controlMarkersToRemove = new List<GMapMarker>(); // Stocăm markerii de control

    public DeletePolygonCommand(string polygonName, List<EditablePolygon> allPolygons, GMapControl mapControl, List<string> polygonNames, Action<EditablePolygon> addControlMarkersForPolygon, Guid currentUserId)
    {
        _polygonName = polygonName;
        _allPolygons = allPolygons;
        _mapControl = mapControl;
        _polygonNames = polygonNames;
        _addControlMarkersForPolygon = addControlMarkersForPolygon;
        _currentUserId = currentUserId;

        _polygonToDelete = _allPolygons.FirstOrDefault(p => p.Name == polygonName);
        if (_polygonToDelete != null)
        {
            _polygonIndexInAllPolygons = _allPolygons.IndexOf(_polygonToDelete);
            _polygonMarker = _polygonToDelete.Polygon;
            _controlPointCoordinates.AddRange(_polygonToDelete.Coordinates); // Store coordinates for undo
            _controlMarkersToRemove.AddRange(_polygonToDelete.ControlMarkers); // Stocăm markerii de control
        }
        _polygonNameIndex = _polygonNames.IndexOf(polygonName);

        // Find the text marker
        _textMarker = _mapControl.Markers.FirstOrDefault(m => m.Shape is Button tb && tb.Content?.ToString() == polygonName);
    }

    public async void Execute()
{
    Console.WriteLine($"Executing DeletePolygonCommand for: {_polygonName}");
    try
    {

        if (_polygonToDelete != null)
        {
            using (var client = new HttpClient())
            {
                // Assuming your EditablePolygon class has a property to store the server ID (e.g., ServerId)
                // and your API endpoint for deleting a polygon by ID is something like /api/Polygons/{polygonId}
                
                var response =
                    await client.DeleteAsync($"https://localhost:7088/api/Polygons/{_polygonToDelete.Name}?userId={_currentUserId}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Polygon '{_polygonName}' ( {_polygonToDelete.Name}) was successfully deleted from the server.");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"Warning: Polygon '{_polygonName}' ({_polygonToDelete.Name}) not found on the server during deletion.");
                }
                else
                {
                    Console.WriteLine($"Failed to delete polygon '{_polygonName}' ( {_polygonToDelete.Name}) from the server. Status code: {response.StatusCode}");
                    // Optionally, you might want to handle this error differently, 
                    // perhaps by preventing the local deletion if the server deletion fails.
                }
            }
        }
        else
        {
            Console.WriteLine($"Warning: _polygonToDelete is null during Redo, cannot delete from server.");
        }
        
        // Find the polygon marker on the map using its tag
        var polygonMarkerToRemove = _mapControl.Markers.OfType<GMapPolygon>()
            .FirstOrDefault(p => p.Tag?.ToString() == _polygonName);

        if (polygonMarkerToRemove != null)
        {
            Console.WriteLine(
                $"Removing polygon marker: {polygonMarkerToRemove.Tag} from map (Contains: {_mapControl.Markers.Contains(polygonMarkerToRemove)}).");
            // Remove polygon marker from map
            if (_mapControl.Markers.Contains(polygonMarkerToRemove))
            {
                _mapControl.Markers.Remove(polygonMarkerToRemove);
            }
        }
        else
        {
            Console.WriteLine($"Warning: Polygon marker '{_polygonName}' not found on the map during Redo.");
        }

        // Identify and remove control markers for this polygon
        var controlMarkersToRemove = _mapControl.Markers
            .Where(m => m.Shape is System.Windows.Shapes.Ellipse &&
                        _polygonToDelete.Coordinates.Any(coord =>
                            Math.Abs(coord.Lat - m.Position.Lat) < 1e-7 && Math.Abs(coord.Lng - m.Position.Lng) < 1e-7))
            .ToList();

        Console.WriteLine($"Found {controlMarkersToRemove.Count} control markers on the map to remove.");
        foreach (var marker in controlMarkersToRemove)
        {
            Console.WriteLine(
                $"Removing control marker with Tag: {marker.Tag} at: {marker.Position} (Contains: {_mapControl.Markers.Contains(marker)}).");
            if (_mapControl.Markers.Contains(marker))
            {
                _mapControl.Markers.Remove(marker);
            }
        }

        _polygonToDelete.ControlMarkers.Clear(); // Clear the potentially outdated list

        // Remove polygon from collections
        Console.WriteLine(
            $"Removing polygon '{_polygonName}' from _allPolygons (Contains: {_allPolygons.Contains(_polygonToDelete)}).");
        if (_polygonIndexInAllPolygons != -1 && _allPolygons.Contains(_polygonToDelete))
        {
            _allPolygons.RemoveAt(_polygonIndexInAllPolygons);
            _polygonToDelete = null; // Set to null after removal
        }
        else
        {
            Console.WriteLine(
                $"Warning: Index of polygon '{_polygonName}' not found in _allPolygons or polygon not present.");
        }

        Console.WriteLine(
            $"Removing polygon name '{_polygonName}' from _polygonNames (Contains: {_polygonNames.Contains(_polygonName)}).");
        if (_polygonNameIndex != -1 && _polygonNames.Contains(_polygonName))
        {
            _polygonNames.RemoveAt(_polygonNameIndex);
        }
        else
        {
            Console.WriteLine(
                $"Warning: Index of polygon name '{_polygonName}' not found in _polygonNames or name not present.");
        }

        // Remove text marker
        var textMarkerToRemove = _mapControl.Markers.OfType<GMapMarker>()
            .FirstOrDefault(m => m.Shape is Button tb && tb.Content?.ToString() == _polygonName);
        if (textMarkerToRemove != null)
        {
            Console.WriteLine(
                $"Removing text marker for '{_polygonName}' (Contains: {_mapControl.Markers.Contains(textMarkerToRemove)}).");
            if (_mapControl.Markers.Contains(textMarkerToRemove))
            {
                _mapControl.Markers.Remove(textMarkerToRemove);
            }
        }
        else
        {
            Console.WriteLine($"Warning: Text marker for '{_polygonName}' not found on the map during Redo.");
        }

        // Force map refresh
        _mapControl.InvalidateVisual();
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}

    public async void Unexecute()
    {
        Console.WriteLine($"Unexecuting DeletePolygonCommand for: {_polygonName}");
        try
        {
            if (_polygonToDelete == null)
            {
                // Recreate the EditablePolygon object
                _polygonToDelete = new EditablePolygon
                {
                    Name = _polygonName,
                    Coordinates = new List<PointLatLng>(_controlPointCoordinates),
                    Polygon = new GMapPolygon(_controlPointCoordinates)
                    {
                        Shape = new System.Windows.Shapes.Polygon
                        {
                            Stroke = Brushes.Red,
                            Fill = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)),
                            StrokeThickness = 2
                        },
                        Tag = _polygonName
                    },
                    ControlMarkers = new List<GMapMarker>()
                };

                using (var client = new HttpClient())
                {
                    var createRequest = new CreatePolygonRequest
                    {
                        Name = _polygonToDelete.Name,
                        UserId = _currentUserId,
                        Points = _polygonToDelete.Coordinates
                            .Select((p, index) => new PointRequest
                            {
                                Latitude = (decimal)p.Lat,
                                Longitude = (decimal)p.Lng,
                                Order = index
                            }).ToList()
                    };

                    var jsonContent = new StringContent(JsonSerializer.Serialize(createRequest), Encoding.UTF8,
                        "application/json");
                    var response = await client.PostAsync("https://localhost:7088/api/Polygons", jsonContent);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Polygon '{_polygonName}' was successfully re-created on the server.");
                        // Optionally, you might need to fetch the new ID if needed
                    }
                    else
                    {
                        Console.WriteLine(
                            $"Failed to re-create polygon '{_polygonName}' on the server. Status code: {response.StatusCode}");
                    }
                }
            }

            // Add polygon back to collections
            if (_polygonIndexInAllPolygons != -1 && !_allPolygons.Contains(_polygonToDelete))
            {
                Console.WriteLine(
                    $"Inserting '{_polygonName}' back into _allPolygons at index {_polygonIndexInAllPolygons}.");
                _allPolygons.Insert(_polygonIndexInAllPolygons, _polygonToDelete);
            }

            if (_polygonNameIndex != -1 && !_polygonNames.Contains(_polygonName))
            {
                Console.WriteLine($"Inserting '{_polygonName}' back into _polygonNames at index {_polygonNameIndex}.");
                _polygonNames.Insert(_polygonNameIndex, _polygonName);
            }

            // Add polygon marker back to map
            if (_polygonMarker != null && !_mapControl.Markers.Contains(_polygonMarker))
            {
                Console.WriteLine($"Adding polygon marker '{_polygonMarker.Tag}' back to map.");
                _mapControl.Markers.Add(_polygonMarker);
            }
            else if (_polygonMarker == null)
            {
                Console.WriteLine($"Warning: _polygonMarker is null in Unexecute.");
                _polygonMarker = _polygonToDelete.Polygon; // Ensure polygon marker is set
                if (_polygonMarker != null && !_mapControl.Markers.Contains(_polygonMarker))
                {
                    Console.WriteLine(
                        $"Trying to add reconstructed polygon marker '{_polygonMarker.Tag}' back to map.");
                    _mapControl.Markers.Add(_polygonMarker);
                }
            }

            // Găsește poligonul vizual pe hartă (poate fi redundant, dar încercăm)
            var existingPolygonMarker = _mapControl.Markers.OfType<GMapPolygon>()
                .FirstOrDefault(p => p.Tag?.ToString() == _polygonName);

            // Dacă există deja un poligon vizual cu acest nume, înlătură-l (pentru a evita duplicate)
            if (existingPolygonMarker != null)
            {
                Console.WriteLine($"Removing existing polygon marker '{_polygonName}' before re-adding in Unexecute.");
                _mapControl.Markers.Remove(existingPolygonMarker);
            }

            // Crează un nou poligon vizual cu coordonatele restaurate
            var restoredPolygonMarker = new GMapPolygon(_polygonToDelete.Coordinates)
            {
                Shape = new System.Windows.Shapes.Polygon
                {
                    Stroke = Brushes.Red,
                    Fill = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)),
                    StrokeThickness = 2
                },
                Tag = _polygonToDelete.Name
            };
            _polygonToDelete.Polygon = restoredPolygonMarker; // Actualizează referința

            // Adaugă noul poligon vizual pe hartă
            Console.WriteLine($"Adding restored polygon marker '{_polygonToDelete.Name}' back to map in Unexecute.");
            _mapControl.Markers.Add(restoredPolygonMarker);

            // Re-add control markers using the provided method
            Console.WriteLine($"Re-adding control markers for '{_polygonName}' to ensure interactivity.");
            _addControlMarkersForPolygon(_polygonToDelete);

            // Add text marker back to map
            if (_textMarker != null && !_mapControl.Markers.Contains(_textMarker))
            {
                Console.WriteLine($"Adding text marker for '{_polygonName}' back to map.");
                _mapControl.Markers.Add(_textMarker);
            }
            else if (_textMarker == null)
            {
                Console.WriteLine($"Warning: _textMarker is null in Unexecute.");
                // Potentially try to find and recreate the text marker if needed
            }

            _mapControl.InvalidateVisual();
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    
}
}