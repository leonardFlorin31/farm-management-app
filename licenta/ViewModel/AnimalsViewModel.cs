using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using licenta.Repositories;

namespace licenta.ViewModel
{
    public class AnimalsViewModel : ViewModelBase
    {
        private PointLatLng _mapCenter;
        private int _zoomLevel = 13; // Initial zoom level
        private GMapProvider _mapProvider = GoogleMapProvider.Instance;

        // List to store marker coordinates
        private List<PointLatLng> _markerCoordinates = new List<PointLatLng>();
        private PointLatLng _polygonCentroid;

        // Event for zoom change notifications
        public event Action<int> ZoomChanged;

        // Fields for server interaction
        private readonly HttpClient _httpClient = new HttpClient();
        private string _apiBaseUrl = "https://localhost:7088/api"; // Replace with your API URL
        private Guid _currentUserId; // Will be set after login
        private string _currentUsername = LoginViewModel.UsernameForUse.Username;

        public GMapControl MapControl { get; set; }

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

        public AnimalsViewModel()
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

            MapControl = new GMapControl
            {
                MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance,
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
                    MessageBox.Show("No polygons found for the current user.");
                    return;
                }

                // Add each polygon to the map
                foreach (var polygon in polygons)
                {
                    AddPolygonToMap(polygon);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading polygons: {ex.Message}");
            }
        }

        // Updated DTO classes with JSON property mapping
        public class PolygonDto
        {
            [JsonPropertyName("polygonId")]
            public Guid Id { get; set; }
            
            [JsonPropertyName("polygonName")]
            public string Name { get; set; }
            
            [JsonPropertyName("createdByUserId")]
            public Guid CreatedByUserId { get; set; }
            
            [JsonPropertyName("createdDate")]
            public DateTime CreatedDate { get; set; }
            
            [JsonPropertyName("points")]
            public List<PointDto> Points { get; set; }
        }

        public class PointDto
        {
            [JsonPropertyName("pointId")]
            public Guid PointId { get; set; }
            
            [JsonPropertyName("latitude")]
            public decimal Latitude { get; set; }
            
            [JsonPropertyName("longitude")]
            public decimal Longitude { get; set; }
            
            [JsonPropertyName("order")]
            public int Order { get; set; }
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
            var point = MapControl.FromLocalToLatLng((int)e.GetPosition(MapControl).X, (int)e.GetPosition(MapControl).Y);

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

                var points = polygon.Points.ConvertAll(p => new PointLatLng((double)p.Latitude, (double)p.Longitude));

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
                    Tag = polygon.Id // Store polygon ID for reference
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
                MessageBox.Show("You need at least 3 points to create a polygon.");
                return;
            }

            try
            {
                if (_currentUserId == Guid.Empty)
                {
                    MessageBox.Show("User ID is not available.");
                    return;
                }

                var client = new HttpClient();

                var request = new CreatePolygonRequest
                {
                    UserId = _currentUserId,
                    Name = "TEstttttt", // You can bind this to a property for user input
                    Points = _markerCoordinates.ConvertAll(p => new PointRequest
                    {
                        Latitude = (decimal)p.Lat,
                        Longitude = (decimal)p.Lng
                    })
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"https://localhost:7088/api/Polygons?userId={_currentUserId}", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Failed to create polygon. Status code: {response.StatusCode}");
                    return;
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                // Use case-insensitive options to match JSON properties
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var createdPolygon = JsonSerializer.Deserialize<PolygonDto>(responseContent, options);

                if (createdPolygon == null || createdPolygon.Id == Guid.Empty)
                {
                    MessageBox.Show("Failed to create polygon on the server.");
                    return;
                }

                AddPolygonToMap(createdPolygon);

                ClearMarkers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating polygon: {ex.Message}");
            }
        }

        // Method to delete a polygon from the server
        public async void DeletePolygon(Guid polygonId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"Polygon/{polygonId}?userId={_currentUserId}");

                if (response.IsSuccessStatusCode)
                {
                    RemovePolygonFromMap(polygonId);
                }
                else
                {
                    MessageBox.Show("Failed to delete polygon from the server.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting polygon: {ex.Message}");
            }
        }

        // Method to remove a polygon from the map
        private void RemovePolygonFromMap(Guid polygonId)
        {
            var toRemove = MapControl.Markers
                .FirstOrDefault(m => m is GMapPolygon polygon && polygon.Tag is Guid id && id == polygonId);

            if (toRemove != null)
            {
                MapControl.Markers.Remove(toRemove);
            }
        }

        private void AddTextToPolygon(GMapPolygon polygon, string text)
        {
            PointLatLng centroid = CalculateCentroid(polygon.Points);

            TextBlock textBlock = new TextBlock
            {
                Text = text,
                Foreground = Brushes.Black,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Background = Brushes.Transparent,
                Padding = new Thickness(2)
            };

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

        // Update text size based on zoom level
        private void UpdateTextScale(TextBlock textBlock, ScaleTransform scaleTransform, int zoomLevel)
        {
            double scale = Math.Max(0.1, zoomLevel / 18.0);
            scaleTransform.ScaleX = scale;
            scaleTransform.ScaleY = scale;
        }

        private void UpdateLabelVisibility()
        {
            foreach (var marker in MapControl.Markers)
            {
                if (marker.Shape is TextBlock textBlock)
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

        private PointLatLng CalculateCentroid(List<PointLatLng> points)
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
}
