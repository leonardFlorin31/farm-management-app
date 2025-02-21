using System;
using System.Windows.Input;
using GMap.NET;
using GMap.NET.MapProviders;

namespace licenta.ViewModel
{
    public class AnimalsViewModel : ViewModelBase
    {
        private PointLatLng _mapCenter;
        private int _zoomLevel = 13; // Nivelul inițial de zoom
        private GMapProvider _mapProvider = OpenStreetMapProvider.Instance;

        // Eveniment pentru notificarea schimbărilor de zoom
        public event Action<int> ZoomChanged;

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