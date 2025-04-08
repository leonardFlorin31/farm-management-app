using System.Globalization;
using System.Windows.Data;

namespace licenta.View;

public class ChartLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return $"{value:P}"; // Formatează ca procent
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}