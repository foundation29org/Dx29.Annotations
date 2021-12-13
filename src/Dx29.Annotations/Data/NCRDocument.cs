using System;
using System.Linq;
using System.Collections.Generic;

namespace Dx29.Data
{
    public class NCRDocuments
    {
        public IList<NCRDocument> documents { get; set; }
    }

    public class NCRDocument
    {
        public string id { get; set; }
        public string text { get; set; }
    }

    static public class NCRDocumentExtensions
    {
        static public NCRDocuments AsNCRDocuments(this TextSegments segs)
        {
            var docs = segs.Segments.Select(r => new NCRDocument
            {
                id = r.Id,
                text = r.Target
            }).ToList();
            return new NCRDocuments { documents = docs };
        }

        static public IEnumerable<NCRDocuments> SplitDocuments(this NCRDocuments docs, int size = 10)
        {
            int count = docs.documents.Count / size;
            if (docs.documents.Count % size > 0) count++;
            for (int n = 0; n < count; n++)
            {
                yield return new NCRDocuments { documents = docs.documents.Skip(n * size).Take(size).ToList() };
            }
        }

        static public string Stats(this NCRDocuments docs)
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
