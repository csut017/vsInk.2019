using System.Linq;
using System.Windows;
using System.Windows.Ink;

namespace vsInk
{
    public class AnnotationStroke
    {
        private readonly AnchorPoint[] anchorPoints;

        public AnnotationStroke(Stroke ink, AnchorPoint[] anchorPoints, LineSegment[] segments)
        {
            this.Original = ink;
            this.Current = ink;
            this.anchorPoints = anchorPoints;
            this.Segments = segments;
            this.Stitches = new StrokeCollection();
        }

        public Stroke Current { get; set; }

        public bool IsVisible { get; set; }

        public Stroke Original { get; private set; }

        public LineSegment[] Segments { get; private set; }

        public StrokeCollection Stitches { get; private set; }

        public AnchorPoint this[AnchorPointUse usage]
        {
            get { return this.anchorPoints.FirstOrDefault(ap => ap.Usage == usage); }
        }

        public Point? CalculateLocation(AnchorPointUse usage)
        {
            var anchorPoint = this[usage];
            return anchorPoint == null
                ? (Point?)null
                : anchorPoint.CalculatePosition(this.Current);
        }

        public bool ContainsStroke(Stroke stroke)
        {
            return stroke.Equals(this.Current)
                   || this.Segments.Any(s => s.ContainsStroke(stroke));
        }
    }
}
