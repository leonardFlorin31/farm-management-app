using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

public class ResizeAdornerSides : Adorner
{
    private VisualCollection visualChildren;
    private Thumb leftThumb, rightThumb, topThumb, bottomThumb;
    private double thumbThickness = 20; // grosimea mânerelelor

    public ResizeAdornerSides(UIElement adornedElement) : base(adornedElement)
    {
        visualChildren = new VisualCollection(this);

        // Creează thumb-uri transparente
        leftThumb = BuildTransparentThumb();
        rightThumb = BuildTransparentThumb();
        topThumb = BuildTransparentThumb();
        bottomThumb = BuildTransparentThumb();

        // Atașează evenimentele de redimensionare
        leftThumb.DragDelta += LeftThumb_DragDelta;
        rightThumb.DragDelta += RightThumb_DragDelta;
        topThumb.DragDelta += TopThumb_DragDelta;
        bottomThumb.DragDelta += BottomThumb_DragDelta;
    }

    private Thumb BuildTransparentThumb()
    {
        var thumb = new Thumb
        {
            Background = Brushes.Transparent,
            Opacity = 0, // complet transparent
            Cursor = Cursors.SizeAll // cursor generic; se poate schimba în funcție de direcție
        };
        visualChildren.Add(thumb);
        return thumb;
    }
    
    private double scaleFactor = 5.0; // Factorul de încetinire

    // Redimensionare: doar lățimea, ajustând stânga
    private void LeftThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (AdornedElement is FrameworkElement fe)
        {
            double newWidth = fe.Width - (e.HorizontalChange / scaleFactor);
            if (newWidth >= 100 && newWidth <= 600) // limite min/max, ajustabile
                fe.Width = newWidth;
        }
    }

    // Redimensionare: doar lățimea, ajustând dreapta
    private void RightThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (AdornedElement is FrameworkElement fe)
        {
            double newWidth = fe.Width + (e.HorizontalChange / scaleFactor);
            if (newWidth >= 100 && newWidth <= 600)
                fe.Width = newWidth;
        }
    }

    // Redimensionare: doar înălțimea, partea de sus
    

// TopThumb: când tragi în sus, acumulează înălțimea
    private void TopThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (AdornedElement is FrameworkElement fe)
        {
            // Adaugă modificarea verticală împărțită la scaleFactor
            double newHeight = fe.Height - (e.VerticalChange / scaleFactor);
            if (newHeight >= 100 && newHeight <= 600)
                fe.Height = newHeight;
        }
    }

// BottomThumb: când tragi în jos, scade înălțimea
    private void BottomThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (AdornedElement is FrameworkElement fe)
        {
            // Scade modificarea verticală împărțită la scaleFactor
            double newHeight = fe.Height + (e.VerticalChange / scaleFactor);
            if (newHeight >= 100 && newHeight <= 600)
                fe.Height = newHeight;
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        // Determine the real width/height of the adorned element:
        //  - If the user explicitly set Width/Height (and they're finite), use them.
        //  - Otherwise, fall back to the layout system's finalSize.
        double adornerWidth = (!double.IsNaN(AdornedElement.RenderSize.Width) &&
                               !double.IsInfinity(AdornedElement.RenderSize.Width))
            ? AdornedElement.RenderSize.Width
            : finalSize.Width;

        double adornerHeight = (!double.IsNaN(AdornedElement.RenderSize.Height) &&
                                !double.IsInfinity(AdornedElement.RenderSize.Height))
            ? AdornedElement.RenderSize.Height
            : finalSize.Height;

        // Arrange the four thumbs around the edges
        leftThumb.Arrange(new Rect(-thumbThickness / 2, 0, thumbThickness, adornerHeight));
        rightThumb.Arrange(new Rect(adornerWidth - thumbThickness / 2, 0, thumbThickness, adornerHeight));
        topThumb.Arrange(new Rect(0, -thumbThickness / 2, adornerWidth, thumbThickness));
        bottomThumb.Arrange(new Rect(0, adornerHeight - thumbThickness / 2, adornerWidth, thumbThickness));

        return finalSize;
        
        //versiunea dinainte de crash ul random
        //
        // if (!(AdornedElement is FrameworkElement fe))
        //     return finalSize;
        //
        // double adornerWidth = fe.Width;
        // double adornerHeight = fe.Height;
        //
        // // Arrange pentru thumb‑ul din stânga:
        // // se întinde pe toată înălțimea adorner‑ului
        // leftThumb.Arrange(new Rect(-thumbThickness / 2, 0, thumbThickness, adornerHeight));
        //
        // // Thumb-ul din dreapta:
        // rightThumb.Arrange(new Rect(adornerWidth - thumbThickness / 2, 0, thumbThickness, adornerHeight));
        //
        // // Thumb-ul de sus:
        // topThumb.Arrange(new Rect(0, -thumbThickness / 2, adornerWidth, thumbThickness));
        //
        // // Thumb-ul de jos:
        // bottomThumb.Arrange(new Rect(0, adornerHeight - thumbThickness / 2, adornerWidth, thumbThickness));
        //
        // return finalSize;
    }

    protected override int VisualChildrenCount => visualChildren.Count;

    protected override Visual GetVisualChild(int index)
    {
        return visualChildren[index];
    }
}