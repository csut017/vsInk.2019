using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Newtonsoft.Json;

namespace vsInk
{
    /// <summary>
    /// Adornment class that draws a square box in the top right hand corner of the viewport
    /// </summary>
    public sealed class InkAdorner
    {
        public const string InkLayerTag = "inkLayer";

        private readonly IAdornmentLayer adornmentLayer;
        private readonly GlobalSettings settings;
        private readonly IWpfTextView textView;
        private AnnotationStore annotations;
        private DigitalInkContainer container;
        private AnnotationRenderer annotationRenderer;

        public InkAdorner(IWpfTextView view, GlobalSettings settings, AnnotationStoreFactory storeFactory)
        {
            this.settings = settings;
            this.settings.SettingsChanged += this.OnSettingsChanged;
            this.annotationRenderer = this.settings.AnnotationRenderer;
            this.textView = view;
            this.textView.ViewportHeightChanged += (o, e) =>
            {
                this.OnSizeChanged();
            };
            this.textView.ViewportWidthChanged += (o, e) => this.OnSizeChanged();
            this.textView.LayoutChanged += (o, e) => this.RenderAllAnnotations();
            this.CreateInkCanvas();
            this.adornmentLayer = this.textView.GetAdornmentLayer(InkAdornerFactory.LayerName);
            this.AssociateWithDocument(view, storeFactory);
        }

        private void CreateInkCanvas()
        {
            var width = this.textView.ViewportWidth;
            if (width < 100)
            {
                width = 100;
            }

            var height = this.textView.ViewportHeight;
            if (height < 100)
            {
                height = 100;
            }

            this.container = new DigitalInkContainer
            {
                Width = width,
                Height = height,
                Background = new SolidColorBrush(new Color { A = 0, B = 168, G = 168, R = 168 }),
                Visibility = this.settings.IsEnabled ? Visibility.Visible : Visibility.Collapsed,
                InkCanvas = { EditingModeInverted = InkCanvasEditingMode.EraseByStroke }
            };
            this.container.InkCanvas.StrokeCollected += this.OnStrokeCollected;
            this.container.InkCanvas.StrokeErasing += this.OnStrokeErased;
            this.SetEditMode();
        }

        private void AssociateWithDocument(ITextView view, AnnotationStoreFactory storeFactory)
        {
            this.annotations = storeFactory.RetrieveOrCreate(view);
            if (!this.annotations.IsLoaded) this.annotations.Load(this.textView, this.settings);
            this.annotations.Changed += (o, e) =>
            {
                if ((e.ChangeType == ChangeType.Remove) || !ReferenceEquals(this, e.Source)) this.RenderAllAnnotations();
            };
        }

        private void MarkAsDirty()
        {
            if (!this.textView.TextBuffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument textDocument))
            {
                return;
            }

            textDocument.UpdateDirtyState(true, DateTime.Now);
        }

        private void SetEditMode()
        {
            switch (this.settings.CurrentMode)
            {
                case GlobalSettings.Mode.Pen:
                    this.container.InkCanvas.EditingMode = InkCanvasEditingMode.Ink;
                    break;

                case GlobalSettings.Mode.Erase:
                    this.container.InkCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
                    break;
            }
        }

        private void OnStrokeErased(object sender, InkCanvasStrokeErasingEventArgs e)
        {
            this.annotations.Remove(e.Stroke, this);
            this.MarkAsDirty();
        }

        private void OnStrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            // Step 1: Generate all the anchor points
            var original = new AnnotationStroke(e.Stroke, this.GenerateAnchorPoints(e.Stroke), this.GenerateSegments(e.Stroke));
            var cleanedStroke = e.Stroke;       // Allow for stroke cleaning future
            var cleaned = new AnnotationStroke(cleanedStroke, this.GenerateAnchorPoints(cleanedStroke), this.GenerateSegments(cleanedStroke));

            // Step 2: Store the annotation for future use
            var annotation = this.annotations.Add(original,
                cleaned,
                this.settings.InkDisplayMode, 
                this);

            // Step 3: Re-render the stroke, this ensures the right stroke is displayed when we are in a mode other than original
            this.container.InkCanvas.Strokes.Remove(e.Stroke);
            this.annotationRenderer.PrepareToRender(this.settings, this.container, this.textView);
            this.RenderAnnotation(annotation);

            // Step 4: Make sure we can save
            this.MarkAsDirty();
        }

        private AnchorPoint[] GenerateAnchorPoints(Stroke stroke)
        {
            return AnchorPointDefinition.All
                .Select(definition => GenerateAnchorPoint(stroke, definition))
                .ToArray();
        }

        private AnchorPoint GenerateAnchorPoint(Stroke stroke, AnchorPointDefinition definition, Point? calculationOffset = null)
        {
            // Step 1: Get the location of the anchor in the viewport - include the adjustment for the viewport offset
            var location = definition.Calculation(stroke);
            location = calculationOffset.HasValue
                ? new Point(location.X + textView.ViewportLeft + calculationOffset.Value.X, location.Y + textView.ViewportTop + calculationOffset.Value.Y)
                : new Point(location.X + textView.ViewportLeft, location.Y + textView.ViewportTop);

            // Step 2: Get the line that the stroke is associated with
            var line = this.textView.TextViewLines.GetTextViewLineContainingYCoordinate(location.Y)
                ?? textView.TextViewLines.LastVisibleLine;

            // Step 3: Get the tracking point and offset
            var trackingPoint = line.Snapshot.CreateTrackingPoint(line.Start.Position, PointTrackingMode.Positive);
            var offset = new Point(location.X, location.Y - line.TextTop);

            // Step 4: Store it in the list since we can't do a partial initialisation of a params array
            var anchorPoint = new AnchorPoint(definition.Usage, definition.Calculation, trackingPoint, offset);
            return anchorPoint;
        }

        private LineSegment[] GenerateSegments(Stroke stroke)
        {
            var offset = this.textView.ViewportTop;
            var bounds = stroke.GetBounds();
            var firstLine = this.textView.TextViewLines.GetTextViewLineContainingYCoordinate(bounds.Top + offset);
            var firstoffset = -1;
            if (firstLine == null)
            {
                firstLine = textView.TextViewLines.FirstVisibleLine;
                firstoffset = 0;
            }

            var lastOffset = 1;
            var lastLine = this.textView.TextViewLines.GetTextViewLineContainingYCoordinate(bounds.Bottom + offset);
            if (lastLine == null)
            {
                lastLine = textView.TextViewLines.LastVisibleLine;
                lastOffset = 0;
            }

            var firstLineNumber = firstLine.Extent.Start.GetContainingLine().LineNumber + firstoffset;
            firstLineNumber = firstLineNumber < 0 ? 0 : firstLineNumber;
            var lastLineNumber = lastLine.Extent.Start.GetContainingLine().LineNumber + lastOffset;
            var lines = new List<LineDimensions>();
            var lastBottom = double.MinValue;
            for (var loop = firstLineNumber; loop < lastLineNumber; loop++)
            {
                var textLine = this.textView.TextSnapshot.GetLineFromLineNumber(loop);
                var line = this.textView.TextViewLines.GetTextViewLineContainingBufferPosition(textLine.Start);
                lines.Add(new LineDimensions(loop, lastBottom - offset, line.Bottom - offset));
                lastBottom = line.Bottom;
            }

            lines.Add(new LineDimensions(lastLineNumber, lastBottom + offset, double.MaxValue));
            var lastLineUsed = -1;
            var outputLines = lines.ToDictionary(l => l.Number, l => new List<LineSegmentStroke>());
            var order = 0;
            var points = new StylusPointCollection();
            var lastPoint = stroke.StylusPoints.First();
            foreach (var point in stroke.StylusPoints)
            {
                var containingLine = lines.FirstOrDefault(l => (point.Y >= l.Top) && (point.Y < l.Bottom));
                if (lastLineUsed != containingLine.Number)
                {
                    if (lastLineUsed != -1)
                    {
                        // Calculate the intersection point and add it to both strokes
                        var isTopDown = point.Y > lastPoint.Y;
                        var useTopLine = ((lastPoint.Y <= containingLine.Top) && (point.Y >= containingLine.Top))
                            || ((lastPoint.Y >= containingLine.Top) && (point.Y <= containingLine.Top));
                        var useBottomLine = ((lastPoint.Y <= containingLine.Bottom) && (point.Y >= containingLine.Bottom))
                            || ((lastPoint.Y >= containingLine.Bottom) && (point.Y <= containingLine.Bottom));
                        var yPos = isTopDown
                            ? (useTopLine ? containingLine.Top : containingLine.Bottom)
                            : (useBottomLine ? containingLine.Bottom : containingLine.Top);
                        var yFull = point.Y - lastPoint.Y;
                        var xFull = point.X - lastPoint.X;
                        var yPart = yPos - lastPoint.Y;
                        var ratio = Math.Abs(yFull) < 0.001 ? 0.0 : yPart / yFull;
                        var xPart = ratio * xFull;
                        var xPos = lastPoint.X + xPart;
                        points.Add(new StylusPoint(xPos, yPos));
                        outputLines[lastLineUsed].Add(new LineSegmentStroke(order++, new Stroke(points, stroke.DrawingAttributes)));
                        points = new StylusPointCollection { new StylusPoint(xPos, yPos) };
                    }

                    lastLineUsed = containingLine.Number;
                }

                points.Add(new StylusPoint(point.X, point.Y));
                lastPoint = point;
            }

            outputLines[lastLineUsed].Add(new LineSegmentStroke(order, new Stroke(points, stroke.DrawingAttributes)));
            var definition = AnchorPointDefinition.Get(AnchorPointUse.MiddlePoint);
            return outputLines.Values
                .Where(s => s.Any())
                .Select(s => new LineSegment(this.GenerateAnchorPoint(s.First().Original, definition), s.ToArray()))
                .ToArray();
        }

        private void OnSizeChanged()
        {
            if (this.container == null)
            {
                this.CreateInkCanvas();
            }

            this.adornmentLayer.RemoveAdornmentsByTag(InkLayerTag);
            this.container.Width = this.textView.ViewportWidth;
            this.container.Height = this.textView.ViewportHeight;
            Canvas.SetTop(this.container, this.textView.ViewportTop);
            Canvas.SetLeft(this.container, this.textView.ViewportLeft);
            this.adornmentLayer.AddAdornment(
                AdornmentPositioningBehavior.ViewportRelative,
                null,
                InkLayerTag,
                this.container,
                null);
        }

        private void RenderAllAnnotations()
        {
            if (!this.annotationRenderer.PrepareToRender(this.settings, this.container, this.textView))
            {
                return;
            }

            this.container.InkCanvas.Strokes.Clear();
            this.container.Underneath.Children.Clear();
            foreach (var annotation in this.annotations)
            {
                this.RenderAnnotation(annotation);
            }
        }

        private void RenderAnnotation(Annotation annotation)
        {
            this.annotationRenderer.Render(annotation);
            if (this.settings.ShowAnnotationBoundaries)
            {
                this.annotationRenderer.ShowAnnotationBoundary(annotation);
            }

            if (this.settings.ShowLineBoundaries)
            {
                this.annotationRenderer.ShowLineBoundaries(annotation);
            }

            if (this.settings.ShowSegmentBoundaries)
            {
                this.annotationRenderer.ShowSegmentBoundaries(annotation);
            }
        }

        private void OnSettingsChanged(object sender, EventArgs e)
        {
            if (this.container == null)
            {
                return;
            }

            this.container.Visibility = this.settings.IsEnabled ? Visibility.Visible : Visibility.Collapsed;
            this.SetEditMode();
            foreach (var annotation in this.annotations)
            {
                annotation.ChangeDisplay(this.settings.InkDisplayMode);
            }

            this.annotationRenderer = this.settings.AnnotationRenderer;
            this.RenderAllAnnotations();

        }

        private struct LineDimensions
        {
            public LineDimensions(int number, double top, double bottom)
                : this()
            {
                Number = number;
                Top = top;
                Bottom = bottom;
            }

            public double Bottom { get; private set; }

            public int Number { get; private set; }

            public double Top { get; private set; }
        }
    }
}
