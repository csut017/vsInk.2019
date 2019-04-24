using System.Windows;
using Newtonsoft.Json;

namespace vsInk
{
    public class PointData
    {
        public PointData()
        {
        }

        public PointData(Point point)
        {
            this.X = point.X;
            this.Y = point.Y;
        }

        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }
    }
}
