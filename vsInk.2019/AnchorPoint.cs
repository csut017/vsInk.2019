using System;
using System.Windows;
using System.Windows.Ink;
using Microsoft.VisualStudio.Text;

namespace vsInk
{
    public class AnchorPoint
    {
        private readonly Func<Stroke, Point> calculation;

        public AnchorPoint(AnchorPointUse usage, Func<Stroke, Point> calculation, ITrackingPoint trackingPoint, Point offset)
        {
            this.Usage = usage;
            this.calculation = calculation;
            this.TrackingPoint = trackingPoint;
            this.Offset = offset;
        }

        public Point Offset { get; private set; }

        public ITrackingPoint TrackingPoint { get; private set; }

        public AnchorPointUse Usage { get; private set; }

        public Point CalculatePosition(Stroke stroke)
        {
            return this.calculation(stroke);
        }
    }
}
