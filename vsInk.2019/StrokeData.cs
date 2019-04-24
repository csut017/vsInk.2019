using Newtonsoft.Json;

namespace vsInk
{
    public class StrokeData
    {
        public StrokeData()
        {
        }

        public StrokeData(LineSegmentStroke s)
        {
            this.Order = s.Order;
            this.Offset = new PointData(s.Offset);
            this.InkData = s.Original.Serialise();
        }

        [JsonProperty("ink")]
        public string InkData { get; set; }

        [JsonProperty("offset")]
        public PointData Offset { get; set; }

        [JsonProperty("order")]
        public int Order { get; set; }
    }
}
