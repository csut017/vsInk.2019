using System;
using System.ComponentModel.Composition;
using System.Linq;

namespace vsInk
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class GlobalSettings
    {
        private readonly AnnotationRenderer[] renderers =
            {
                new SinglePointAnnotationRenderer(),
                new TwoPointStretchAnnotationRenderer(),
                new SimpleSplitAnnotationRenderer(),
                new JoinSplitAnnotationRenderer()
            };


        public GlobalSettings()
        {
            this.AnnotationRendererName = this.renderers.First().Name;
        }

        public string AnnotationRendererName { get; set; }

        public bool IsEnabled { get; set; } 

        public InkDisplayMode InkDisplayMode { get; set; } = InkDisplayMode.Original;

        public Mode CurrentMode { get; set; } = Mode.Pen;

        public bool ShowAnnotationBoundaries { get; set; }

        public bool ShowLineBoundaries { get; set; }

        public bool ShowSegmentBoundaries { get; set; }

        public bool UseOriginalColor
        {
            get { return this.ExtensionColour == "Original"; }
        }

        public string ExtensionColour { get; set; } = "Original";

        public StitchType StitchType { get; set; } = StitchType.Solid;

        public AnnotationRenderer AnnotationRenderer
        {
            get { return this.renderers.Single(r => r.Name == this.AnnotationRendererName); }
        }

        public event EventHandler SettingsChanged;

        public void NotifySettingsChanged()
        {
            this.SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        public string[] AvailableAnnotationRendererNames
        {
            get { return this.renderers.Select(r => r.Name).ToArray(); }
        }

        public enum Mode
        {
            Pen,
            Erase
        }
    }
}
