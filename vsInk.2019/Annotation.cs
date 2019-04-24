using System;
using System.Windows.Ink;

namespace vsInk
{
    public class Annotation
    {
        public static readonly Guid AnnotationID = new Guid("9CBDD119-F378-4D36-B3DB-46C3574E8C47");

        public Annotation(AnnotationStroke originalStroke, AnnotationStroke cleanedStroke, InkDisplayMode displayMode, long id)
        {
            this.OriginalStroke = originalStroke;
            this.CleanedStroke = cleanedStroke;
            this.Id = id;
            this.CurrentStroke = originalStroke;
            this.ChangeDisplay(displayMode);
        }

        public AnnotationStroke CleanedStroke { get; private set; }

        public long Id { get; }

        public AnnotationStroke CurrentStroke { get; private set; }

        public AnnotationStroke OriginalStroke { get; private set; }

        public AnnotationStroke PreviousStroke { get; private set; }

        public string Type { get; set; }

        public void ChangeDisplay(InkDisplayMode inkDisplayMode)
        {
            this.PreviousStroke = this.CurrentStroke;
            switch (inkDisplayMode)
            {
                case InkDisplayMode.Cleaned:
                    this.CurrentStroke = this.CleanedStroke;
                    break;

                default:
                    this.CurrentStroke = this.OriginalStroke;
                    break;
            }
        }

        public bool ContainsStroke(Stroke stroke)
        {
            return this.CurrentStroke.ContainsStroke(stroke);
        }
    }
}
