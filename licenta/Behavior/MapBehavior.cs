using System.Windows;
using System.Windows.Input;
using GMap.NET.WindowsPresentation;
using Microsoft.Xaml.Behaviors;

namespace licenta.Behavior;

public class MapBehavior : Behavior<GMapControl>
{
    // Definirea DependencyProperty pentru Command
    public static readonly DependencyProperty MapClickedCommandProperty =
        DependencyProperty.Register(
            nameof(MapClickedCommand),
            typeof(ICommand),
            typeof(MapBehavior));

    // Proprietatea pentru Command
    public ICommand MapClickedCommand
    {
        get => (ICommand)GetValue(MapClickedCommandProperty);
        set => SetValue(MapClickedCommandProperty, value);
    }

    // Atasarea behavior-ului la control
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.MouseLeftButtonDown += OnMapClicked;
    }

    // Detasarea behavior-ului de la control
    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.MouseLeftButtonDown -= OnMapClicked;
    }

    // Metoda pentru gestionarea evenimentului de clic
    private void OnMapClicked(object sender, MouseButtonEventArgs e)
    {
        var map = AssociatedObject;
        var mousePosition = e.GetPosition(map);
        var latLng = map.FromLocalToLatLng((int)mousePosition.X, (int)mousePosition.Y);

        // Trimite coordonatele către ViewModel
        if (MapClickedCommand?.CanExecute(latLng) == true)
        {
            MapClickedCommand.Execute(latLng);
        }
    }
}