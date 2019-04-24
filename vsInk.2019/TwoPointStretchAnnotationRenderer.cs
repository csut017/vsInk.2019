using System.Linq;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using Microsoft.VisualStudio.Text;

namespace vsInk
{
    public class TwoPointStretchAnnotationRenderer
        : SingleStrokeAnnotationRenderer
    {
        public override string Name
        {
            get { return "Double Point Stretch"; }
        }

        public override void Render(Annotation annotation)
        {
            // Calculate all the needed locations
            var strokeToRender = annotation.CurrentStroke;
            var selectionOrder = new[] { AnchorPointUse.FirstPoint, AnchorPointUse.TopPoint, AnchorPointUse.BottomPoint };
            var points = selectionOrder.ToDictionary(ap => ap, ap => (Point?)null);
            foreach (var usageToFind in selectionOrder)
            {
                var anchorPoint = strokeToRender[usageToFind];
                if (anchorPoint == null)
                {
                    continue;
                }

                var point = anchorPoint.TrackingPoint.GetPoint(this.TextView.TextSnapshot);
                if (this.TextView.TextViewModel.IsPointInVisualBuffer(point, PositionAffinity.Successor))
                {
                    // Calculate the new position of the anchor point
                    var line = this.TextView.TextViewLines.GetTextViewLineContainingBufferPosition(point);
                    if (line != null)
                    {
                        points[usageToFind] = new Point(
                            anchorPoint.Offset.X - this.TextView.ViewportLeft,
                            anchorPoint.Offset.Y + line.Top - this.TextView.ViewportTop);
                    }
                }
            }

            // Stretch the stroke if all the conditions are met
            if (points[AnchorPointUse.TopPoint].HasValue && points[AnchorPointUse.BottomPoint].HasValue)
            {
                var top = points[AnchorPointUse.TopPoint].Value.Y;
                var bottom = points[AnchorPointUse.BottomPoint].Value.Y;
                var originalBounds = strokeToRender.Original.GetBounds();
                var scale = (bottom - top) / originalBounds.Height;
                var scaledPoints = new StylusPointCollection(strokeToRender.Original.StylusPoints.Count);
                foreach (var point in strokeToRender.Original.StylusPoints)
                {
                    scaledPoints.Add(new StylusPoint(point.X, (point.Y - top) * scale + top));
                }

                strokeToRender.Current = new Stroke(scaledPoints, this.GenerateDrawingAttributes(strokeToRender));
                strokeToRender.Current.AddPropertyData(Annotation.AnnotationID, annotation.Id);
            }

            // Get which positioning usage we are using
            var usage = selectionOrder.FirstOrDefault(ap => points[ap].HasValue);
            this.DisplayStroke(annotation, this.InkCanvas, points[usage], strokeToRender, usage);
        }
    }
}
