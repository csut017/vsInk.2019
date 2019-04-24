using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;

namespace vsInk
{
    public abstract class SegmentAnnotationRenderer
        : AnnotationRenderer
    {
        protected void AddStrokesForSegment(LineSegment segment, InkCanvas canvas)
        {
            if (segment.IsVisible)
            {
                return;
            }

            segment.IsVisible = true;
            foreach (var stroke in segment.Strokes)
            {
                canvas.Strokes.Add(stroke.Current);
            }
        }

        protected StrokeCollection GenerateStitch(StylusPointCollection points, DrawingAttributes attributes)
        {
            var strokes = new StrokeCollection();
            switch (this.Settings.StitchType)
            {
                case StitchType.Solid:
                    strokes.Add(new Stroke(points, attributes));
                    break;

                case StitchType.Dashed:
                    strokes.Add(GenerateDashedLineFromPoints(points, attributes, 5));
                    break;
            }

            return strokes;
        }

        protected void ResetAllSegments(AnnotationStroke stroke)
        {
            foreach (var segment in stroke.Segments)
            {
                segment.IsVisible = false;
                foreach (var segmentStroke in segment.Strokes)
                {
                    segmentStroke.Current = segmentStroke.Original;
                }
            }
        }

        private static void AddDash(
            DrawingAttributes attributes,
            bool steep,
            double y,
            double x,
            StylusPoint startPoint,
            ICollection<Stroke> dashes)
        {
            var endPoint = steep ? new StylusPoint(y, x) : new StylusPoint(x, y);
            var dashPoints = new StylusPointCollection { startPoint, endPoint };
            dashes.Add(new Stroke(dashPoints, attributes));
        }

        private static StrokeCollection GenerateDashedLineFromPoints(
            IReadOnlyList<StylusPoint> points,
            DrawingAttributes attributes,
            int dashLength)
        {
            var dashes = new StrokeCollection();
            var length = points.Count;
            var adding = true;
            var startPoint = new StylusPoint();
            var steep = false;
            double x = 0;
            double y = 0;
            for (var index = 1; index < length; index++)
            {
                var first = points[index - 1];
                var second = points[index];
                steep = Math.Abs(second.Y - first.Y) > Math.Abs(second.X - first.X);
                var x0 = first.X;
                var y0 = first.Y;
                var x1 = second.X;
                var y1 = second.Y;
                if (steep)
                {
                    Swap(ref x0, ref y0);
                    Swap(ref x1, ref y1);
                }

                if (x0 > x1)
                {
                    Swap(ref x0, ref x1);
                    Swap(ref y0, ref y1);
                }

                var dx = x1 - x0;
                var dy = Math.Abs(y1 - y0);
                var error = dx / 2;
                var yStep = (y0 < y1) ? 1 : -1;
                y = y0;
                var count = 0;
                for (x = x0; x < x1; x++)
                {
                    if (++count == dashLength)
                    {
                        count = 0;
                        adding = !adding;
                        if (adding)
                        {
                            AddDash(attributes, steep, y, x, startPoint, dashes);
                        }
                        else
                        {
                            startPoint = steep ? new StylusPoint(y, x) : new StylusPoint(x, y);
                        }
                    }

                    error -= dy;
                    if (!(error < 0))
                    {
                        continue;
                    }

                    y += yStep;
                    error += dx;
                }
            }

            if (!adding)
            {
                AddDash(attributes, steep, y, x, startPoint, dashes);
            }

            return dashes;
        }

        private static void Swap(ref double first, ref double second)
        {
            var temp = first;
            first = second;
            second = temp;
        }
    }
}