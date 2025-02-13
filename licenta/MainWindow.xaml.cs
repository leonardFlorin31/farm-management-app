using System.Text;
using System.Windows;
using System.Windows.Controls;
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
}