using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace licenta;
using System.Runtime.InteropServices;
using System.Runtime;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
    }

    [DllImport("user32.dll")]
    public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
    
    private void PnlControlBar_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        WindowInteropHelper helper = new WindowInteropHelper(this);
        SendMessage(helper.Handle, 161, 2, 0);
    }

    private void BtnClose_OnClick(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void BtnMinimize_OnClick(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void BtnMaximize_OnClick(object sender, RoutedEventArgs e)
    {
        if(this.WindowState==WindowState.Normal)
           this.WindowState = WindowState.Maximized; 
        else this.WindowState = WindowState.Normal;
    }
    
    // private void GridSplitter_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
    // {
    //     var gridSplitter = sender as GridSplitter;
    //     if (gridSplitter != null)
    //     {
    //         var newWidth = gridSplitter.HorizontalOffset;
    //         if (newWidth < 200) newWidth = 200;
    //         if (newWidth > 300) newWidth = 300;
    //         gridSplitter.HorizontalOffset = newWidth;
    //     }
    // }
    private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        var newWidth = NavMenuColumn.ActualWidth + e.HorizontalChange;
    
        // Apply constraints
        if (newWidth < NavMenuColumn.MinWidth)
            newWidth = NavMenuColumn.MinWidth;
        else if (newWidth > NavMenuColumn.MaxWidth)
            newWidth = NavMenuColumn.MaxWidth;

        NavMenuColumn.Width = new GridLength(newWidth);
    }
}