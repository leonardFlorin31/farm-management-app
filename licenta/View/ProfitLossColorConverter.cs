using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace licenta.View;

public class ProfitLossColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var val = (double)value;
        return val >= 0 
            ? new LinearGradientBrush(Colors.LightGreen, Colors.DarkGreen, 90)
            : new LinearGradientBrush(Colors.OrangeRed, Colors.DarkRed, 90);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}