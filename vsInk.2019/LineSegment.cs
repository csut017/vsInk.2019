using System.Linq;
using System.Windows;
using System.Windows.Ink;

namespace vsInk
{
    public class LineSegment
    {
        public LineSegment(AnchorPoint anchor, LineSegmentStroke[] strokes)
        {
            this.Anchor = anchor;
            this.Strokes = strokes;
            if (strokes.Any())
            {
                var first = CalculateMidPoint(strokes.First());
                foreach (var stroke in strokes)
                {
                    var bounds = CalculateMidPoint(stroke);
                    var offset = new Point(bounds.X - first.X, bounds.Y - first.Y);
                    stroke.Offset = offset;
                    stroke.Parent = this;
                }
            }
        }

        public AnchorPoint Anchor { get; private set; }

        public bool IsVisible { get; set; }

        public LineSegmentStroke[] Strokes { get; private set; }

        public Rect GetCurrentBounds()
        {
            var bounds = this.Strokes
                .Select(s => s.Current)
                .Aggregate(new Rect(double.MaxValue, double.MaxValue, 0, 0), this.UpdateBounds);
            return bounds;
        }

        public Rect GetOriginalBounds()
        {
            var bounds = this.Strokes
                .Select(s => s.Original)
                .Aggregate(new Rect(double.MaxValue, double.MaxValue, 0, 0), this.UpdateBounds);
            return bounds;
        }

        private static Point CalculateMidPoint(LineSegmentStroke stroke)
        {
            var bounds = stroke.Original.GetBounds();
            var midpoint = new Point(bounds.Left, bounds.Top + bounds.Height / 2);
            return midpoint;
        }

        private Rect UpdateBounds(Rect current, Stroke nextStroke)
        {
            var bounds = nextStroke.GetBounds();
            var left = current.Left < bounds.Left ? current.Left : bounds.Left;
            var top = current.Top < bounds.Top ? current.Top : bounds.Top;
            var newWidth = bounds.Right - left;
            var newHeight = bounds.Bottom - top;
            var width = current.Width > newWidth ? current.Width : newWidth;
            var height = current.Height > newHeight ? current.Height : newHeight;
            var newRect = new Rect(left, top, width, height);
            return newRect;
        }

        public bool ContainsStroke(Stroke stroke)
        {
            return this.Strokes.Any(l => l.ContainsStroke(stroke));
        }
    }
}
