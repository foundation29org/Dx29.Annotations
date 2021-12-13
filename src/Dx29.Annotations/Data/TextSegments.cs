using System;
using System.Linq;
using System.Collections.Generic;

namespace Dx29.Data
{
    public class TextSegments
    {
        const int MAX_LENGTH = 999;

        public string Language_source { get; set; }
        public string Language_target { get; set; }
        public IList<TextSegment> Segments { get; set; }

        public TextSegments()
        {
            Segments = new List<TextSegment>();
        }

        public void JoinSegmentsLegacy()
        {
            Segments = JoinSegmentsLegacy(Segments).ToArray();
        }

        public void JoinSegments()
        {
            Segments = JoinSegments(Segments).ToArray();
        }

        private IEnumerable<TextSegment> JoinSegmentsLegacy(IList<TextSegment> segments)
        {
            var lastSegment = segments[0];
            if (lastSegment.Source.EndsWith(".") && lastSegment.Source.Length > 64)
            {
                yield return lastSegment;
                lastSegment = null;
            }
            for (int n = 1; n < segments.Count; n++)
            {
                lastSegment = JoinSegments(lastSegment, segments[n]);
                if (lastSegment.Source.EndsWith(".") && lastSegment.Source.Length > 64)
                {
                    yield return lastSegment;
                    lastSegment = null;
                }
            }
            if (lastSegment != null) yield return lastSegment;
        }

        private IEnumerable<TextSegment> JoinSegments(IList<TextSegment> segments)
        {
            var lastSegment = segments[0];
            if (lastSegment.Source.Length > MAX_LENGTH)
            {
                yield return lastSegment;
                lastSegment = null;
            }
            for (int n = 1; n < segments.Count; n++)
            {
                var nextSegment = segments[n];
                var joinSegment = JoinSegments(lastSegment, nextSegment);
                if (joinSegment.Source.Length > MAX_LENGTH)
                {
                    if (lastSegment != null)
                    {
                        yield return lastSegment;
                    }
                    lastSegment = nextSegment;
                }
                else
                {
                    lastSegment = joinSegment;
                }
            }
            if (lastSegment != null) yield return lastSegment;
        }

        private TextSegment JoinSegments(TextSegment segment1, TextSegment segment2)
        {
            if (segment1 == null) return segment2;
            var textSegment = new TextSegment
            {
                Id = segment1.Id,
                Source = segment1.Source + " " + segment2.Source
            };
            return textSegment;
        }
    }

    public class TextSegment
    {
        public string Id { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
    }
}
