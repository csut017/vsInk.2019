using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace vsInk
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AnnotationStoreFactory
    {
        private readonly Dictionary<string, AnnotationStore> stores = new Dictionary<string, AnnotationStore>();
        private readonly GlobalSettings settings;

        [ImportingConstructor]
        public AnnotationStoreFactory(GlobalSettings settings)
        {
            this.settings = settings;
        }

        public void Clear()
        {
            this.stores.Clear();
        }

        public AnnotationStore RetrieveOrCreate(ITextView view)
        {
            if (!view.TextBuffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument textDocument))
            {
                return new AnnotationStore(null);
            }

            if (!this.stores.TryGetValue(textDocument.FilePath, out AnnotationStore store))
            {
                store = new AnnotationStore(textDocument);
                this.stores[textDocument.FilePath] = store;
            }

            return store;
        }
    }
}
