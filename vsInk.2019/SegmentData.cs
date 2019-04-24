using System.Linq;
using System.Windows;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Newtonsoft.Json;

namespace vsInk
{
    public class SegmentData
        : AnchorPointData
    {
        public SegmentData()
        {
        }

        public SegmentData(LineSegment segment)
        {
            this.Offset = new PointData(segment.Anchor.Offset);
            this.Position = segment.Anchor.TrackingPoint.GetPosition(segment.Anchor.TrackingPoint.TextBuffer.CurrentSnapshot);
            this.Strokes = segment.Strokes.Select(s => new StrokeData(s)).ToArray();
        }

        [JsonProperty("strokes")]
        public StrokeData[] Strokes { get; set; }

        public LineSegment ToModel(IWpfTextView textView)
        {
            var anchorPoint = new AnchorPoint(
                AnchorPointUse.MiddlePoint,
                AnchorPointDefinition.Get(AnchorPointUse.MiddlePoint).Calculation,
                textView.TextSnapshot.CreateTrackingPoint(this.Position, PointTrackingMode.Negative),
                new Point(this.Offset.X, this.Offset.Y));
            var strokes = this.Strokes
                .Select(s => new LineSegmentStroke(s.Order, StrokeExtensions.Deserialise(s.InkData))
                {
                    Offset = new Point(s.Offset.X, s.Offset.Y)
                })
                .ToArray();
            var segment = new LineSegment(anchorPoint, strokes);
            return segment;
        }
    }
}
