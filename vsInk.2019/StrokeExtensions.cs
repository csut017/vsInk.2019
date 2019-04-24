using System;
using System.IO;
using System.Linq;
using System.Windows.Ink;

namespace vsInk
{
    public static class StrokeExtensions
    {
        public static Stroke Deserialise(string data)
        {
            using (var stream = new MemoryStream(Convert.FromBase64String(data)))
            {
                var collection = new StrokeCollection(stream);
                return collection.First();
            }
        }

        public static string Serialise(this Stroke stroke)
        {
            using (var stream = new MemoryStream())
            {
                var collection = new StrokeCollection(new[] { stroke });
                collection.Save(stream);
                return Convert.ToBase64String(stream.ToArray());
            }
        }
    }
}
