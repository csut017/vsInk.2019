using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vsInk
{
    public class AnnotationStoreChangedEventArgs
        : EventArgs
    {
        public AnnotationStoreChangedEventArgs(InkAdorner source, ChangeType changeType)
        {
            this.Source = source;
            this.ChangeType = changeType;
        }

        public ChangeType ChangeType { get; private set; }

        public InkAdorner Source { get; private set; }
    }
}
