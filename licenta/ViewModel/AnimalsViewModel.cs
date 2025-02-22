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
        private GMapProvider _mapProvider = OpenStreetMapProvider.Instance;

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

        public AnimalsViewModel()
        {
            MapCenter = new PointLatLng(44.4268, 26.1025); // București
            MapClickedCommand = new RelayCommand<PointLatLng>(OnMapClicked);

            // Inițializare comenzi pentru zoom
            ZoomInCommand = new RelayCommand(ZoomIn);
            ZoomOutCommand = new RelayCommand(ZoomOut);
            
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

            AddPolygonToMap();
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