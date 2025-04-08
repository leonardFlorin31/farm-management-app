using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;

namespace licenta.ViewModel;

public class ExpensesViewModel : ViewModelBase
{
    private readonly MapViewModel _mapViewModel;
    public SeriesCollection MonthlyProfitLoss { get; set; }
    public SeriesCollection ExpensePercentages { get; set; }
    public string[] Months { get; } = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames;
    public Func<double, string> AmountFormatter { get; } = value => value.ToString("N0") + " RON";
    public ExpensesViewModel(MapViewModel mapViewModel)
    {
        _mapViewModel = mapViewModel ?? throw new ArgumentNullException(nameof(mapViewModel));
        LoadDemoData();
    }
    
     private void LoadDemoData()
    {
        // Date Demo - Profit/Pierdere pe Lună
        var monthlyValues = new List<double> 
        { 
            15000, -5000, 20000, 3000, -2000, 18000, 
            9000, -3000, 12000, 7000, -4000, 25000 
        };

        var columnSeries = new ColumnSeries
        {
            Title = "Profit/Pierdere",
            Values = new ChartValues<double>(monthlyValues),
            Fill = Brushes.Transparent,
            StrokeThickness = 2,
            DataLabels = true
        };

        // Setare culori dinamice
        foreach (var value in monthlyValues)
        {
            columnSeries.Fill = value >= 0 
                ? new SolidColorBrush(Color.FromRgb(76, 175, 80)) // Verde
                : new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Roșu
        }

        MonthlyProfitLoss = new SeriesCollection { columnSeries };

        // Date Demo - Procente Cheltuieli
        ExpensePercentages = new SeriesCollection
        {
            new PieSeries
            {
                Title = "Semințe",
                Values = new ChartValues<decimal> { 40 },
                Fill = Brushes.DodgerBlue,
                DataLabels = true
            },
            new PieSeries
            {
                Title = "Îngrășăminte",
                Values = new ChartValues<decimal> { 25 },
                Fill = Brushes.Orange,
                DataLabels = true
            },
            new PieSeries
            {
                Title = "Lucrări",
                Values = new ChartValues<decimal> { 20 },
                Fill = Brushes.LightGreen,
                DataLabels = true
            },
            new PieSeries
            {
                Title = "Altele",
                Values = new ChartValues<decimal> { 15 },
                Fill = Brushes.Violet,
                DataLabels = true
            }
        };
    }
    

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
