using System.Windows;
using System.Windows.Controls;
using ControlzEx.Theming;

namespace licenta.View;

public partial class ParcelView : UserControl
{
    public ParcelView()
    {
        InitializeComponent();
        Console.WriteLine("ParcelView initialized");
        var currentTheme = ThemeManager.Current.DetectTheme(Application.Current);

        // Schimbă tema aplicației la Light.Blue
        ThemeManager.Current.ChangeTheme(Application.Current, "Light.Green");
    }
}