using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;

namespace vsInk
{
    public class SimpleSplitAnnotationRenderer
        : SegmentAnnotationRenderer
    {
        public override string Name
        {
            get { return "Simple Line Split"; }
        }

        public override void Render(Annotation annotation)
        {
            var strokeToRender = annotation.CurrentStroke;
            this.ResetAllSegments(strokeToRender);
            foreach (var segment in strokeToRender.Segments)
            {
                var firstStroke = segment.Strokes.FirstOrDefault();
                Point? topLeft = null;
                if (firstStroke != null)
                {
                    var point = segment.Anchor.TrackingPoint.GetPoint(this.TextView.TextSnapshot);
                    if (this.TextView.TextViewModel.IsPointInVisualBuffer(point, PositionAffinity.Successor))
                    {
                        // Calculate the new position of the anchor point
                        var line = this.TextView.TextViewLines.GetTextViewLineContainingBufferPosition(point);
                        if (line != null)
                        {
                            topLeft = new Point(
                                segment.Anchor.Offset.X - this.TextView.ViewportLeft,
                                segment.Anchor.Offset.Y + line.Top - this.TextView.ViewportTop);
                        }
                    }
                }

                if (topLeft.HasValue)
                {
                    // Move the strokes to their new positions
                    foreach (var stroke in segment.Strokes)
                    {
                        var transform = new Matrix();
                        var location = segment.Anchor.CalculatePosition(stroke.Original);
                        transform.Translate(
                            topLeft.Value.X - location.X + stroke.Offset.X,
                            topLeft.Value.Y - location.Y + stroke.Offset.Y);
                        stroke.Original.Transform(transform, true);
                    }

                    // Make sure the annotation is visible
                    this.AddStrokesForSegment(segment, this.InkCanvas);
                }
            }
        }
    }
}