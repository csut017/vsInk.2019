using System.Windows;
using System.Windows.Ink;

namespace vsInk
{
    public class LineSegmentStroke
    {
        public LineSegmentStroke(int order, Stroke stroke)
        {
            this.Order = order;
            this.Original = stroke;
            this.Current = stroke;
        }

        public Stroke Current { get; set; }

        public Point Offset { get; set; }

        public int Order { get; private set; }

        public Stroke Original { get; private set; }

        public LineSegment Parent { get; set; }

        public bool ContainsStroke(Stroke stroke)
        {
            return this.Current == stroke;
        }
    }
}
