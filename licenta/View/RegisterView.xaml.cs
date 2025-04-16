using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace licenta.View;

public partial class RegisterView : Window
{
    public RegisterView()
    {
        InitializeComponent();
        Console.WriteLine("RegisterView initialized");
    }
    
    private void Windows_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }
}