using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;

namespace vsInk
{
    [Guid("0AD46D03-1433-4568-AFEC-57E50E42D718")]
    public class ConfigurationWindow
         : ToolWindowPane
    {
        private readonly ConfigurationWindowControl control = new ConfigurationWindowControl();

        public ConfigurationWindow()
            : base(null)
        {
            this.Caption = "Configure vsInk";
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;
            base.Content = this.control;
        }

        protected override void Initialize()
        {
            base.Initialize();
            var componentModel = (IComponentModel)this.GetService(typeof(SComponentModel));
            var model = componentModel.GetService<GlobalSettings>();
            this.control.DataContext = new ConfigurationWindowModel(model);
        }

        private class ConfigurationWindowModel
            : INotifyPropertyChanged
        {
            private readonly GlobalSettings settings;
            private bool advancedSettingsVisible;

            public ConfigurationWindowModel(GlobalSettings settings)
            {
                this.settings = settings;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public InkDisplayMode[] AvailableInkDisplayModes
            {
                get { return new[] { InkDisplayMode.Original }; }
            }

            public string[] AvailableAnnotationRendererNames
            {
                get { return this.settings.AvailableAnnotationRendererNames; }
            }

            public string AnnotationRendererName
            {
                get { return this.settings.AnnotationRendererName; }
                set
                {
                    this.settings.AnnotationRendererName = value;
                    this.settings.NotifySettingsChanged();
                    this.FirePropertyChanged();
                }
            }

            public string ExtensionColour
            {
                get { return this.settings.ExtensionColour; }
                set
                {
                    this.settings.ExtensionColour = value;
                    this.settings.NotifySettingsChanged();
                    this.FirePropertyChanged();
                }
            }

            public InkDisplayMode InkDisplayMode
            {
                get { return this.settings.InkDisplayMode; }
                set
                {
                    this.settings.InkDisplayMode = value;
                    this.settings.NotifySettingsChanged();
                    this.FirePropertyChanged();
                }
            }

            public bool ShowAnnotationBoundaries
            {
                get { return this.settings.ShowAnnotationBoundaries; }
                set
                {
                    this.settings.ShowAnnotationBoundaries = value;
                    this.settings.NotifySettingsChanged();
                    this.FirePropertyChanged();
                }
            }

            public bool ShowLineBoundaries
            {
                get { return this.settings.ShowLineBoundaries; }
                set
                {
                    this.settings.ShowLineBoundaries = value;
                    this.settings.NotifySettingsChanged();
                    this.FirePropertyChanged();
                }
            }

            public bool ShowSegmentBoundaries
            {
                get { return this.settings.ShowSegmentBoundaries; }
                set
                {
                    this.settings.ShowSegmentBoundaries = value;
                    this.settings.NotifySettingsChanged();
                    this.FirePropertyChanged();
                }
            }

            public string[] AvailableExtensionColours
            {
                get
                {
                    return new[]
                        {
                        "Original",
                        "Red",
                        "Green",
                        "Blue"
                    };
                }
            }

            public bool AdvancedSettings
            {
                get { return this.advancedSettingsVisible; }
                set
                {
                    this.advancedSettingsVisible = value;
                    this.FirePropertyChanged();
                    this.FirePropertyChanged("AdvancedSettingsVisibility");
                }
            }

            public Visibility AdvancedSettingsVisibility
            {
                get { return this.advancedSettingsVisible ? Visibility.Visible : Visibility.Collapsed; }
            }

            public StitchType[] AvailableStitchTypes
            {
                get
                {
                    return Enum.GetValues(typeof(StitchType))
                        .Cast<StitchType>()
                        .ToArray();
                }
            }

            public StitchType StitchType
            {
                get { return this.settings.StitchType; }
                set
                {
                    this.settings.StitchType = value;
                    this.settings.NotifySettingsChanged();
                    this.FirePropertyChanged();
                }
            }

            private void FirePropertyChanged([CallerMemberName] string property = null)
            {
                if (this.PropertyChanged == null)
                {
                    return;
                }

                var args = new PropertyChangedEventArgs(property);
                this.PropertyChanged(this, args);
            }
        }
    }
}
