using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace vsInk
{
    public class DocumentData
    {
        public DocumentData()
        {
            this.Annotations = new List<AnnotationData>();
        }

        public DocumentData(IEnumerable<Annotation> annotations)
        {
            this.Annotations = annotations.Select(a => new AnnotationData(a)).ToList();
        }

        [JsonProperty("annotations")]
        public IList<AnnotationData> Annotations { get; private set; }
    }
}
