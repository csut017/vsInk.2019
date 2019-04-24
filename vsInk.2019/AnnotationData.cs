using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Newtonsoft.Json;

namespace vsInk
{
    public class AnnotationData
    {
        public AnnotationData()
        {
            this.Anchors = new Dictionary<AnchorPointUse, AnchorPointData>();
        }

        public AnnotationData(Annotation annotation)
        {
            this.Type = annotation.Type;
            var stroke = annotation.OriginalStroke;
            this.InkData = stroke.Original.Serialise();
            this.Segments = stroke.Segments.Select(s => new SegmentData(s)).ToArray();
            this.Anchors = Enum.GetValues(typeof(AnchorPointUse))
                .Cast<AnchorPointUse>()
                .Select(ap => stroke[ap])
                .Where(s => s != null)
                .ToDictionary(s => s.Usage, s => new AnchorPointData(s));
        }

        [JsonProperty("anchors")]
        public IDictionary<AnchorPointUse, AnchorPointData> Anchors { get; private set; }

        [JsonProperty("ink")]
        public string InkData { get; set; }

        [JsonProperty("segments")]
        public SegmentData[] Segments { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        public Annotation ToModel(GlobalSettings settings, IWpfTextView textView, long id)
        {
            var snapshot = textView.TextSnapshot;
            var stroke = StrokeExtensions.Deserialise(this.InkData);
            var anchors = this.Anchors
                .Select(ap => new AnchorPoint(
                    ap.Key,
                    AnchorPointDefinition.Get(ap.Key).Calculation,
                    snapshot.CreateTrackingPoint(ap.Value.Position, PointTrackingMode.Negative),
                    new Point(ap.Value.Offset.X, ap.Value.Offset.Y)))
                .ToArray();
            var segments = this.Segments.Select(s => s.ToModel(textView)).ToArray();
            var annotationStroke = new AnnotationStroke(stroke, anchors, segments);
            var annotation = new Annotation(annotationStroke, annotationStroke, settings.InkDisplayMode, id);
            annotation.Type = this.Type;
            return annotation;
        }
    }
}
