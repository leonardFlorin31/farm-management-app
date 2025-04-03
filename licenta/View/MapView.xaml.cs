using System.Windows.Controls;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using licenta.ViewModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ControlzEx.Theming;
using licenta.ViewModel;

namespace licenta.View
{
    public partial class MapView : UserControl
    {
        private ResizeAdornerSides resizeAdorner;
        private Point _startPoint;
        private bool _isDragging = false;
        public MapView()
        {
            InitializeComponent();
            Console.WriteLine("AnimalView initialized");
            
            var currentTheme = ThemeManager.Current.DetectTheme(Application.Current);
            
            // Schimbă tema aplicației la Light.Blue
            ThemeManager.Current.ChangeTheme(Application.Current, "Light.Green");
            
            Loaded += MapView_Loaded;
        }
        
        private void MapView_Loaded(object sender, RoutedEventArgs e)
        {
            // Obține adorner layer-ul pentru Border-ul tău
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(ParcelDetailsBorder);
            if (adornerLayer != null)
            {
                resizeAdorner = new ResizeAdornerSides(ParcelDetailsBorder);
                adornerLayer.Add(resizeAdorner);
            }
        }
        
        private void ParcelDetailsBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Salvăm poziția de start și capturăm mouse-ul
            _startPoint = e.GetPosition(null);
            _isDragging = true;
            ParcelDetailsBorder.CaptureMouse();
        }

        private void ParcelDetailsBorder_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point currentPoint = e.GetPosition(null);
                Vector offset = currentPoint - _startPoint;
        
                // Actualizăm valorile TranslateTransform
                borderTranslateTransform.X += offset.X;
                borderTranslateTransform.Y += offset.Y;
        
                _startPoint = currentPoint;
            }
        }

        private void ParcelDetailsBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            ParcelDetailsBorder.ReleaseMouseCapture();
        }
    }
}