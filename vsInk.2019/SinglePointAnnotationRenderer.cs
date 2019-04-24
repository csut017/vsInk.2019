using System.Windows;
using System.Windows.Ink;
using Microsoft.VisualStudio.Text;

namespace vsInk
{
    public class SinglePointAnnotationRenderer
        : SingleStrokeAnnotationRenderer
    {
        public override string Name
        {
            get { return "Single Point"; }
        }

        public override void Render(Annotation annotation)
        {
            // Try and find a tracking point is visible within the viewport
            var strokeToRender = annotation.CurrentStroke;
            Point? topLeft = null;
            var usage = AnchorPointUse.FirstPoint;
            var selectionOrder = new[] { AnchorPointUse.FirstPoint, AnchorPointUse.TopPoint, AnchorPointUse.BottomPoint };
            for (var loop = 0; (loop < selectionOrder.Length) && !topLeft.HasValue; loop++)
            {
                usage = selectionOrder[loop];
                var anchorPoint = strokeToRender[usage];
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
                        topLeft = new Point(
                            anchorPoint.Offset.X - this.TextView.ViewportLeft,
                            anchorPoint.Offset.Y + line.Top - this.TextView.ViewportTop);
                    }
                }
            }

            strokeToRender.Current = new Stroke(strokeToRender.Original.StylusPoints, strokeToRender.Original.DrawingAttributes);
            strokeToRender.Current.AddPropertyData(Annotation.AnnotationID, annotation.Id);
            this.DisplayStroke(annotation, this.InkCanvas, topLeft, strokeToRender, usage);
        }
    }
}
