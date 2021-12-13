using System;
using System.Collections.Generic;

namespace Dx29.Data
{
    public class TAHAnnotationResults
    {
        public string JobId { get; set; }
        public string Status { get; set; }
        public object[] Errors { get; set; }

        public TAHAnnotationResult Results { get; set; }
    }

    public class TAHAnnotationResult
    {
        public IList<TAHAnnotationDoc> Documents { get; set; }
        public object[] Errors { get; set; }
        public string ModelVersion { get; set; }
    }

    public class TAHAnnotationDocs
    {
        public TAHAnnotationDocs()
        {
            Documents = new List<TAHAnnotationDoc>();
        }

        public List<TAHAnnotationDoc> Documents { get; set; }
    }

    public class TAHAnnotationDoc
    {
        public string Id { get; set; }
        public IList<TAHAnnotation> Entities { get; set; }
    }

    public class TAHAnnotation
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }

        public string Category { get; set; }
        public double ConfidenceScore { get; set; }

        public bool IsNegated { get; set; }
        public IList<TAHAnnotationLink> Links { get; set; }
    }

    public class TAHAnnotationLink
    {
        public TAHAnnotationLink() { }
        public TAHAnnotationLink(string dataSource, string id)
        {
            DataSource = dataSource;
            Id = id;
        }

        public string DataSource { get; set; }
        public string Id { get; set; }
    }
}
