using System;
using System.Linq;
using System.Windows;
using System.Windows.Ink;

namespace vsInk
{
    public class AnchorPointDefinition
    {
        private static Lazy<AnchorPointDefinition[]> definitions = new Lazy<AnchorPointDefinition[]>(GenerateDefinitions);

        private AnchorPointDefinition(AnchorPointUse usage, Func<Stroke, Point> calculation)
        {
            this.Usage = usage;
            this.Calculation = calculation;
        }

        public static AnchorPointDefinition[] All
        {
            get { return definitions.Value; }
        }

        public Func<Stroke, Point> Calculation { get; private set; }

        public AnchorPointUse Usage { get; private set; }

        public static AnchorPointDefinition Get(AnchorPointUse usage)
        {
            return definitions.Value.FirstOrDefault(d => d.Usage == usage);
        }

        private static AnchorPointDefinition[] GenerateDefinitions()
        {
            return new[]
                {
                    new AnchorPointDefinition(AnchorPointUse.FirstPoint, s => s.StylusPoints.First().ToPoint()),
                    new AnchorPointDefinition(AnchorPointUse.TopPoint, s => s.GetBounds().TopLeft),
                    new AnchorPointDefinition(AnchorPointUse.BottomPoint, s => s.GetBounds().BottomLeft),
                    new AnchorPointDefinition(AnchorPointUse.MiddlePoint, s =>
                        {
                            var bounds = s.GetBounds();
                            var point = new Point(bounds.Left, bounds.Top + bounds.Height / 2);
                            return point;
                        })
                };
        }
    }
}
