using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace vsInk
{
    public abstract class SingleStrokeAnnotationRenderer
        : AnnotationRenderer
    {
        protected void DisplayStroke(Annotation annotation, InkCanvas canvas, Point? topLeft, AnnotationStroke strokeToRender, AnchorPointUse usage)
        {
            // Only if the annotation is visible:
            if (topLeft.HasValue)
            {
                // Move the annotation to the new position
                var transform = new Matrix();
                var location = strokeToRender.CalculateLocation(usage);
                transform.Translate(
                    topLeft.Value.X - location.Value.X,
                    topLeft.Value.Y - location.Value.Y);
                strokeToRender.Current.Transform(transform, true);

                // Make sure the annotation is visible
                canvas.Strokes.Add(strokeToRender.Current);
                strokeToRender.IsVisible = true;
            }
            else
            {
                strokeToRender.IsVisible = false;
            }
        }
    }
}
