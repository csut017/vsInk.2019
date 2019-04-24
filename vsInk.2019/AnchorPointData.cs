using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace vsInk
{
    public class AnchorPointData
    {
        public AnchorPointData()
        {
        }

        public AnchorPointData(AnchorPoint anchorPoint)
        {
            this.Offset = new PointData(anchorPoint.Offset);
            this.Position = anchorPoint.TrackingPoint.GetPosition(anchorPoint.TrackingPoint.TextBuffer.CurrentSnapshot);
        }

        [JsonProperty("offset")]
        public PointData Offset { get; set; }

        [JsonProperty("position")]
        public int Position { get; set; }
    }
}
