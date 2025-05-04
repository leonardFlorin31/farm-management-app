using System.Windows;
using System.Windows.Controls;
using ControlzEx.Theming;

namespace licenta.View;

public partial class ExpensesView : UserControl
{
    public ExpensesView()
    {
        InitializeComponent();
        
        ThemeManager.Current.ChangeTheme(Application.Current, "Light.Green");
    }
}