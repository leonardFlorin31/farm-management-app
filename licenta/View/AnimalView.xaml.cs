using System.Windows.Controls;
using GMap.NET;
using licenta.ViewModel;

namespace licenta.View;

public partial class AnimalView : UserControl
{
    public AnimalView()
    {
        InitializeComponent();
        var viewModel = new AnimalsViewModel();
        DataContext = new AnimalsViewModel();
        
        // Inițializează harta
        MapControl.MapProvider = GMap.NET.MapProviders.OpenStreetMapProvider.Instance;
        MapControl.Position = new PointLatLng(44.4268, 26.1025); // București
        MapControl.MinZoom = 2;
        MapControl.MaxZoom = 18;
        MapControl.Zoom = 13;
        MapControl.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
        MapControl.CanDragMap = true;
        MapControl.DragButton = System.Windows.Input.MouseButton.Left;
        
        // Ascultă evenimentul de schimbare a zoom-ului
        viewModel.ZoomChanged += OnZoomChanged;
    }
    
    private void OnZoomChanged(int zoomLevel)
    {
        MapControl.Zoom = zoomLevel; // Actualizează zoom-ul hărții
    }
}