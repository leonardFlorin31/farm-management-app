using System.Windows;
using System.Windows.Controls;
using ControlzEx.Theming;

namespace licenta.View;

public partial class TaskView : UserControl
{
    public TaskView()
    {
        InitializeComponent();
        ThemeManager.Current.ChangeTheme(Application.Current, "Light.Green");
    }
}