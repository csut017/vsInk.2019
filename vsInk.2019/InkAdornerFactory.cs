using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace vsInk
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class InkAdornerFactory
        : IWpfTextViewCreationListener
    {
        public const string LayerName = "InkAnnotationLayer";

        // Disable "Field is never assigned to..." and "Field is never used" compiler's warnings. Justification: the field is used by MEF.
#pragma warning disable 649, 169

        /// <summary>
        /// Defines the adornment layer for the scarlet adornment. This layer is ordered
        /// after the selection layer in the Z-order
        /// </summary>
        [Export(typeof(AdornmentLayerDefinition))]
        [Name(LayerName)]
        [Order(After = PredefinedAdornmentLayers.Caret)]
        [TextViewRole(PredefinedTextViewRoles.Document)]
        private AdornmentLayerDefinition editorAdornmentLayer;

#pragma warning restore 649, 169

        private readonly GlobalSettings settings;
        private readonly AnnotationStoreFactory storeFactory;

        [ImportingConstructor]
        public InkAdornerFactory(GlobalSettings settings, AnnotationStoreFactory storeFactory)
        {
            this.settings = settings;
            this.storeFactory = storeFactory;
        }

        /// <summary>
        /// Instantiates a InkAdorner manager when a textView is created.
        /// </summary>
        /// <param name="textView">The <see cref="IWpfTextView"/> upon which the adornment should be placed</param>
        public void TextViewCreated(IWpfTextView textView)
        {
            // The adorment will get wired to the text view events
            new InkAdorner(textView, this.settings, this.storeFactory);
        }
    }
}
