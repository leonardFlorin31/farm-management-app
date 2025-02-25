using System;
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

        public int ZoomLevel
        {
            get => _zoomLevel;
            set
            {
                if (Set(ref _zoomLevel, value))
                {
                    ZoomChanged?.Invoke(value); // Notifică schimbarea de zoom
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

            // Handle right-click event
            MapControl.MouseRightButtonDown += MapControl_MouseRightButtonDown;
            AddPolygonToMap();
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
            
            ClearMarkers();
        }
        
        private void ClearMarkers()
        {
            // List to store markers to be removed
            var markersToRemove = new List<GMapMarker>();

            // Iterate through the markers and identify which ones are markers (not polygons)
            foreach (var marker in MapControl.Markers)
            {
                if (marker is GMapMarker && !(marker is GMapPolygon))
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
                Console.WriteLine($"Saved Coordinate: Lat = {point.Lat}, Lng = {point.Lng}");
            }

            // TODO: Add logic to save to a database or file
        }


        private void OnMapClicked(PointLatLng point)
        {
            // Aici poți adăuga logica pentru desenare (ex: adaugă un marker)
            System.Diagnostics.Debug.WriteLine($"Clic la: {point.Lat}, {point.Lng}");
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