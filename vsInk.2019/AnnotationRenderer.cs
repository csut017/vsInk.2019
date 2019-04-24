using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.Text.Editor;

namespace vsInk
{
    public abstract class AnnotationRenderer
    {
        private DigitalInkContainer container;

        public abstract string Name { get; }

        protected InkCanvas InkCanvas { get; private set; }

        protected GlobalSettings Settings { get; private set; }

        protected IWpfTextView TextView { get; private set; }

        public bool PrepareToRender(GlobalSettings settings, DigitalInkContainer renderContainer, IWpfTextView textView)
        {
            this.container = renderContainer;
            this.Settings = settings;
            this.InkCanvas = renderContainer.InkCanvas;
            this.TextView = textView;
            return (textView != null) && (textView.TextViewModel != null);
        }

        public abstract void Render(Annotation annotation);

        public void ShowAnnotationBoundary(Annotation annotation)
        {
            var strokeToRender = annotation.CurrentStroke;
            if (!strokeToRender.IsVisible)
            {
                return;
            }

            var bounds = strokeToRender.Current.GetBounds();
            var rect = new Rectangle
            {
                Width = bounds.Width,
                Height = bounds.Height,
                Stroke = new SolidColorBrush(Color.FromArgb(128, 255, 128, 128)),
                StrokeThickness = 1
            };
            this.container.Underneath.Children.Add(rect);
            Canvas.SetLeft(rect, bounds.Left);
            Canvas.SetTop(rect, bounds.Top);
        }

        public void ShowLineBoundaries(Annotation annotation)
        {
            var strokeToRender = annotation.CurrentStroke;
            var brush = new SolidColorBrush(Color.FromArgb(128, 128, 255, 128));
            foreach (var line in strokeToRender.Segments)
            {
                if (!line.IsVisible)
                {
                    continue;
                }

                var bounds = line.GetOriginalBounds();
                var rect = new Rectangle
                {
                    Width = bounds.Width,
                    Height = bounds.Height,
                    Stroke = brush,
                    StrokeThickness = 1
                };
                this.container.Underneath.Children.Add(rect);
                Canvas.SetLeft(rect, bounds.Left);
                Canvas.SetTop(rect, bounds.Top);
            }
        }

        public void ShowSegmentBoundaries(Annotation annotation)
        {
            var strokeToRender = annotation.CurrentStroke;
            var brush = new SolidColorBrush(Color.FromArgb(128, 128, 128, 255));
            foreach (var segment in strokeToRender.Segments.SelectMany(l => l.Strokes.Select(s => new { Line = l, Stroke = s.Current })))
            {
                if (!segment.Line.IsVisible)
                {
                    continue;
                }

                var bounds = segment.Stroke.GetBounds();
                var rect = new Rectangle
                {
                    Width = bounds.Width,
                    Height = bounds.Height,
                    Stroke = brush,
                    StrokeThickness = 1
                };
                this.container.Underneath.Children.Add(rect);
                Canvas.SetLeft(rect, bounds.Left);
                Canvas.SetTop(rect, bounds.Top);
            }
        }

        protected DrawingAttributes GenerateDrawingAttributes(AnnotationStroke stroke)
        {
            if (this.Settings.UseOriginalColor)
            {
                return stroke.Original.DrawingAttributes;
            }

            var attributes = stroke.Original.DrawingAttributes.Clone();
            var convertedColour = ColorConverter.ConvertFromString(this.Settings.ExtensionColour);
            if (convertedColour != null)
            {
                attributes.Color = (Color)convertedColour;
            }

            return attributes;
        }
    }
}
