using System.Globalization;
using System.Windows;
using System.Windows.Data;
using GMap.NET;

namespace licenta.View;

public class CentroidToMarginConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is PointLatLng centroid)
        {
            // Convert centroid coordinates to margin (adjust as needed)
            double left = centroid.Lng; // Adjust based on your map's coordinate system
            double top = centroid.Lat;  // Adjust based on your map's coordinate system
            return new Thickness(left, top, 0, 0);
        }
        return new Thickness(0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}