using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using System.Collections.Generic;

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
            15000, -10000, 20000, 3000, -2000, 18000,
            9000, -3000, 12000, 7000, -4000, 25000
        };

        var positiveValues = new ChartValues<double>();
        var negativeValues = new ChartValues<double>();

        for (int i = 0; i < monthlyValues.Count; i++)
        {
            if (monthlyValues[i] >= 0)
            {
                positiveValues.Add(monthlyValues[i]);
            }
            else
            {
                negativeValues.Add(monthlyValues[i]);
            }
        }

        MonthlyProfitLoss = new SeriesCollection
        {
            new ColumnSeries
            {
                Title = "Profit",
                Values = positiveValues,
                Fill = new SolidColorBrush(Color.FromRgb(76, 175, 80)), // Verde
                StrokeThickness = 2,
                DataLabels = true,
                LabelPoint = (chartPoint) => chartPoint.Y > 0 ? AmountFormatter(chartPoint.Y) : "" // Am schimbat chartPoint.Value în chartPoint.Y
            },
            new ColumnSeries
            {
                Title = "Pierdere",
                Values = negativeValues,
                Fill = new SolidColorBrush(Color.FromRgb(244, 67, 54)), // Roșu
                StrokeThickness = 2,
                DataLabels = true,
                LabelPoint = (chartPoint) => chartPoint.Y < 0 ? AmountFormatter(chartPoint.Y) : "" // Am schimbat chartPoint.Value în chartPoint.Y
            }
        };

        // Date Demo - Procente Cheltuieli (rămâne neschimbat)
        ExpensePercentages = new SeriesCollection
        {
            new PieSeries
            {
                Title = "Semințe",
                Values = new ChartValues<double> { 40 },
                Fill = Brushes.DodgerBlue,
                DataLabels = true,
                LabelPoint = chartPoint => string.Format("{0} ({1:P})", chartPoint.SeriesView.Title, chartPoint.Participation)
            },
            new PieSeries
            {
                Title = "Îngrășăminte",
                Values = new ChartValues<double> { 25 },
                Fill = Brushes.Orange,
                DataLabels = true,
                LabelPoint = chartPoint => string.Format("{0} ({1:P})", chartPoint.SeriesView.Title, chartPoint.Participation)
            },
            new PieSeries
            {
                Title = "Lucrări",
                Values = new ChartValues<double> { 20 },
                Fill = Brushes.LightGreen,
                DataLabels = true,
                LabelPoint = chartPoint => string.Format("{0} ({1:P})", chartPoint.SeriesView.Title, chartPoint.Participation)
            },
            new PieSeries
            {
                Title = "Altele",
                Values = new ChartValues<double> { 15 },
                Fill = Brushes.Violet,
                DataLabels = true,
                LabelPoint = chartPoint => string.Format("{0} ({1:P})", chartPoint.SeriesView.Title, chartPoint.Participation)
            }
        };
    }
    
}