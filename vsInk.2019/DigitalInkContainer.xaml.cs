using System.Windows;
using System.Windows.Controls;

namespace vsInk
{
    /// <summary>
    /// Interaction logic for DigitalInkContainer.xaml
    /// </summary>
    public partial class DigitalInkContainer : UserControl
    {
        public DigitalInkContainer()
        {
            InitializeComponent();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            this.InkCanvas.Width = sizeInfo.NewSize.Width;
            this.InkCanvas.Height = sizeInfo.NewSize.Height;
            base.OnRenderSizeChanged(sizeInfo);
        }
    }
}
