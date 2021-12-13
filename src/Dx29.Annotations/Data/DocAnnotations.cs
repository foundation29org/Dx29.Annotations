using System;
using System.Collections.Generic;

namespace Dx29.Data
{
    public class DocAnnotations
    {
        public DocAnnotations()
        {
            Segments = new List<SegAnnotations>();
        }

        public string Analyzer { get; set; }
        public IList<SegAnnotations> Segments { get; set; }
    }

    public class SegAnnotations
    {
        public SegAnnotations()
        {
            Annotations = new List<Annotation>();
            Language = "en";
        }
        public SegAnnotations(string id, string text) : this()
        {
            Id = id;
            Text = text;
        }

        public string Id { get; set; }
        public string Language { get; set; }
        public string Text { get; set; }
        public SourceText Source { get; set; }
        public IList<Annotation> Annotations { get; set; }
    }

    public class SourceText
    {
        public SourceText() { }
        public SourceText(string language, string text)
        {
            Language = language;
            Text = text;
        }

        public string Language { get; set; }
        public string Text { get; set; }
    }

    public class Annotation
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }
        public string Category { get; set; }
        public double ConfidenceScore { get; set; }
        public bool IsNegated { get; set; }
        public bool IsDiscarded { get; set; }
        public IList<AnnotationLink> Links { get; set; }

        public Annotation AddLink(string dataSource, string id)
        {
            Links ??= new List<AnnotationLink>();
            Links.Add(new AnnotationLink(dataSource, id));
            return this;
        }

        static public Annotation Create(string id, string text, int offset, int length, string category, double confidenceScore, bool isNegated = false, bool isDiscarded = false)
        {
            return new Annotation
            {
                Id = id,
                Text = text,
                Offset = offset,
                Length = length,
                Category = category,
                ConfidenceScore = confidenceScore,
                IsNegated = isNegated,
                IsDiscarded = isDiscarded
            };
        }
        static public Annotation CreateBlank(string id, string text, int offset, int length)
        {
            return Create(id, text, offset, length, category: null, confidenceScore: 0.0, isNegated: false, isDiscarded: true);
        }
    }

    public class AnnotationLink
    {
        public AnnotationLink() { }
        public AnnotationLink(string dataSource, string id)
        {
            DataSource = dataSource;
            Id = id;
        }

        public string DataSource { get; set; }
        public string Id { get; set; }
    }
}
