using System;
using System.Linq;
using System.Collections.Generic;

namespace Dx29.Data
{
    public class TAHDocuments
    {
        public IList<TAHDocument> documents { get; set; }
    }

    public class TAHDocument
    {
        public string id { get; set; }
        public string language { get; set; }
        public string text { get; set; }
    }

    static public class TAHDocumentExtensions
    {
        static public TAHDocuments AsTAHDocuments(this TextSegments segs, int maxSegments = -1)
        {
            var segments = segs.Segments;
            if (maxSegments > 0)
            {
                segments = segments.Take(maxSegments).ToList();
            }
            var docs = segments.Select(r => new TAHDocument
            {
                id = r.Id,
                language = segs.Language_target,
                text = r.Target
            }).ToList();
            return new TAHDocuments { documents = docs };
        }

        static public IEnumerable<TAHDocuments> SplitDocuments(this TAHDocuments docs)
        {
            int count = docs.documents.Count / 10;
            if (docs.documents.Count % 10 > 0) count++;
            for (int n = 0; n < count; n++)
            {
                yield return new TAHDocuments { documents = docs.documents.Skip(n * 10).Take(10).ToList() };
            }
        }

        static public string Stats(this TAHDocuments docs)
        {
            var lens = docs.documents.Select(r => r.text.Length);
            return String.Format("Docs: {0}\tMin: {1}\tMax: {2}\tAvg: {3}\tTotal: {4}",
                    docs.documents.Count,
                    lens.Min(),
                    lens.Max(),
                    (int)lens.Average(),
                    lens.Sum()
                );
        }
    }
}
