using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;


namespace licenta.ViewModel
{
    public class AnimalsViewModel : ViewModelBase
    {
        private PointLatLng _mapCenter;
        private int _zoomLevel = 13;
        private GMapProvider _mapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
        private List<PointLatLng> _markerCoordinates = new List<PointLatLng>();
        private PointLatLng _polygonCentroid;
        
        public event Action<int> ZoomChanged;
        public GMapControl MapControl { get; set; }
        
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

        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        public ICommand MapClickedCommand { get; }
        public ICommand CreatePolygonCommand { get; }

        // Service for communicating with the server
        private readonly PolygonService _polygonService;

        public AnimalsViewModel()
        {
            MapCenter = new PointLatLng(44.4268, 26.1025);
            MapClickedCommand = new RelayCommand<PointLatLng>(OnMapClicked);
            ZoomInCommand = new RelayCommand(ZoomIn);
            ZoomOutCommand = new RelayCommand(ZoomOut);
            CreatePolygonCommand = new RelayCommand(CreatePolygon);

            // Initialize the map control
            MapControl = new GMapControl
            {
                MapProvider = GMap.NET.MapProviders.OpenStreetMapProvider.Instance,
                Position = new PointLatLng(44.4268, 26.1025),
                MinZoom = 2,
                MaxZoom = 18,
                Zoom = 13,
                MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter,
                CanDragMap = true,
                DragButton = MouseButton.Left
            };

            MapControl.OnPositionChanged += MapControl_OnPositionChanged;
            MapControl.MouseRightButtonDown += MapControl_MouseRightButtonDown;
            AddPolygonToMap();

            // Initialize the polygon service (update the BaseAddress as needed)
            _polygonService = new PolygonService(new HttpClient { BaseAddress = new Uri("http://localhost:7088/") });
        }
        
        private void MapControl_OnPositionChanged(PointLatLng point)
        {
            ZoomLevel = (int)MapControl.Zoom;
        }
        
        private void MapControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var point = MapControl.FromLocalToLatLng((int)e.GetPosition(MapControl).X, (int)e.GetPosition(MapControl).Y);
            AddMarker(point);
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
        
        private void AddPolygonToMap()
        {
            List<PointLatLng> points = new List<PointLatLng>
            {
                new PointLatLng(44.4268, 26.1025),
                new PointLatLng(44.4300, 26.1100),
                new PointLatLng(44.4200, 26.1100),
                new PointLatLng(44.4268, 26.1025)
            };

            GMapPolygon polygon = new GMapPolygon(points)
            {
                Shape = new System.Windows.Shapes.Polygon
                {
                    Stroke = Brushes.Red,
                    Fill = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)),
                    StrokeThickness = 2
                }
            };

            MapControl.Markers.Add(polygon);
        }
        
        // Updated CreatePolygon method that sends data to the server
        private async void CreatePolygon()
        {
            if (_markerCoordinates.Count < 3)
            {
                Console.WriteLine("Not enough markers to create a polygon.");
                return;
            }

            // Build the request using the marker coordinates
            var createPolygonRequest = new CreatePolygonRequest
            {
                // Replace with the actual user ID in your app
                UserId = Guid.NewGuid(),
                Name = "Polygon Label",
                Points = _markerCoordinates.Select(p => new PointRequest
                {
                    Latitude = (decimal)p.Lat,
                    Longitude = (decimal)p.Lng
                }).ToList()
            };

            try
            {
                var polygonResponse = await _polygonService.CreatePolygonAsync(createPolygonRequest);
                Console.WriteLine("Polygon created with id: " + polygonResponse.PolygonId);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating polygon: " + ex.Message);
            }

            // Add the polygon to the map for visualization
            GMapPolygon polygon = new GMapPolygon(_markerCoordinates)
            {
                Shape = new System.Windows.Shapes.Polygon
                {
                    Stroke = Brushes.Red,
                    Fill = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)),
                    StrokeThickness = 2
                }
            };

            MapControl.Markers.Add(polygon);
            AddTextToPolygon(polygon, "Polygon Label");

            // Clear the temporary markers
            ClearMarkers();
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
        
        private void OnMapClicked(PointLatLng point)
        {
            Console.WriteLine($"Click at: {point.Lat}, {point.Lng}");
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
