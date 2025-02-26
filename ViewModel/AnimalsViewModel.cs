using System;
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
        private int _zoomLevel = 13; // Nivelul inițial de zoom
        private GMapProvider _mapProvider = GoogleMapProvider.Instance;
        
        // List to store marker coordinates
        private List<PointLatLng> _markerCoordinates = new List<PointLatLng>();
        private PointLatLng _polygonCentroid;
        
        // Eveniment pentru notificarea schimbărilor de zoom
        public event Action<int> ZoomChanged;

        public GMapControl MapControl { get; set; }
        
        // Proprietăți bindable
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
                    // Update the map's zoom if it's out of sync (e.g., when using ZoomIn/ZoomOut commands)
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

        // Comenzi pentru zoom
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }

        // Command pentru clic pe hartă
        public ICommand MapClickedCommand { get; }
        
        // Command pentru crearea poligonului
        public ICommand CreatePolygonCommand { get; }


        public AnimalsViewModel()
        {
            MapCenter = new PointLatLng(44.4268, 26.1025); // București
            MapClickedCommand = new RelayCommand<PointLatLng>(OnMapClicked);

            // Inițializare comenzi pentru zoom
            ZoomInCommand = new RelayCommand(ZoomIn);
            ZoomOutCommand = new RelayCommand(ZoomOut);
            
            // Command pentru crearea poligonului
            CreatePolygonCommand = new RelayCommand(CreatePolygon);
            
            MapControl = new GMapControl
            {
                MapProvider = GMap.NET.MapProviders.OpenStreetMapProvider.Instance,
                Position = new PointLatLng(44.4268, 26.1025), // București
                MinZoom = 2,
                MaxZoom = 18,
                Zoom = 13,
                MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter,
                CanDragMap = true,
                DragButton = System.Windows.Input.MouseButton.Left
            };
            
            // Subscribe to the OnPositionChanged event
            MapControl.OnPositionChanged += MapControl_OnPositionChanged;

            // Handle right-click event
            MapControl.MouseRightButtonDown += MapControl_MouseRightButtonDown;
            AddPolygonToMap();
        }
        
        private void MapControl_OnPositionChanged(PointLatLng point)
        {
            // Sync ViewModel's ZoomLevel with the map's current zoom
            ZoomLevel = (int)MapControl.Zoom;
        }
        
        private void MapControl_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
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
        
        private void CreatePolygon()
        {
            if (_markerCoordinates.Count < 3)
            {
                // You need at least 3 points to create a polygon
                System.Diagnostics.Debug.WriteLine("Not enough markers to create a polygon.");
                return;
            }
            
            SaveCoordinates(_markerCoordinates);

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
            
            // Add text to the center of the polygon
            AddTextToPolygon(polygon, "Polygon Label");

            
            // Clears only the markers
            ClearMarkers();
        }
        
        private void AddTextToPolygon(GMapPolygon polygon, string text)
        {
            PointLatLng centroid = CalculateCentroid(polygon.Points);

            // Create text element
            TextBlock textBlock = new TextBlock
            {
                Text = text,
                Foreground = Brushes.Black,
                FontSize = 12, // Base size
                FontWeight = FontWeights.Bold,
                Background = Brushes.Transparent,
                Padding = new Thickness(2)
            };

            // Scale transform for dynamic resizing
            ScaleTransform scaleTransform = new ScaleTransform();
            textBlock.RenderTransform = scaleTransform;

            // Create text marker
            GMapMarker textMarker = new GMapMarker(centroid)
            {
                Shape = textBlock
            };

            // Add marker to the map
            MapControl.Markers.Add(textMarker);

            // Update text size when zoom changes
            ZoomChanged += (zoom) => UpdateTextScale(textBlock, scaleTransform, zoom);
    
            // Apply initial scaling
            UpdateTextScale(textBlock, scaleTransform, ZoomLevel);
        }

        
        // Method to update text size based on zoom level
        private void UpdateTextScale(TextBlock textBlock, ScaleTransform scaleTransform, int zoomLevel)
        {
            
                double scale = Math.Max(0.1, zoomLevel / 18.0); // Ensures text gets smaller when zooming out
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
                        textBlock.Visibility = Visibility.Collapsed; // Hide labels
                    }
                    else
                    {
                        textBlock.Visibility = Visibility.Visible; // Show labels
                        double scale = Math.Max(0.3, ZoomLevel / 18.0); // Adjust scale
                        ((ScaleTransform)textBlock.RenderTransform).ScaleX = scale;
                        ((ScaleTransform)textBlock.RenderTransform).ScaleY = scale;
                    }
                }
            }
        }
        


        
        private void ClearMarkers()
        {
            // List to store markers to be removed
            var markersToRemove = new List<GMapMarker>();

            // Iterate through the markers and identify which ones are point markers (not polygons or text)
            foreach (var marker in MapControl.Markers)
            {
                // Only remove small circle markers (skip polygons and text markers)
                if (marker.Shape is System.Windows.Shapes.Ellipse)
                {
                    markersToRemove.Add(marker);
                }
            }

            // Remove the identified markers
            foreach (var marker in markersToRemove)
            {
                MapControl.Markers.Remove(marker);
            }

            // Clear the list of marker coordinates
            _markerCoordinates.Clear();
        }
        
        private void SaveCoordinates(List<PointLatLng> coordinates)
        {
            // Save the coordinates to a database or file
            // Example: Log the coordinates to the console
            foreach (var point in coordinates)
            {
               // Console.WriteLine($"Saved Coordinate: Lat = {point.Lat}, Lng = {point.Lng}");
            }

            // TODO: Add logic to save to a database or file
        }


        private void OnMapClicked(PointLatLng point)
        {
            // Aici poți adăuga logica pentru desenare (ex: adaugă un marker)
            Console.WriteLine($"Clic la: {point.Lat}, {point.Lng}");
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
            if (ZoomLevel < 18) // Nivelul maxim de zoom
            {
                ZoomLevel++;
                Console.WriteLine($"ZoomIn: ZoomLevel = {ZoomLevel}");
            }
        }

        private void ZoomOut()
        {
            if (ZoomLevel > 2) // Nivelul minim de zoom
            {
                ZoomLevel--;
                Console.WriteLine($"ZoomOut: ZoomLevel = {ZoomLevel}");
            }
        }
        
        
    }
}