using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using Dx29.Data;

namespace Dx29.Services
{
    public class NCRAnnotationService
    {
        public NCRAnnotationService(HttpClient httpClient, IConfiguration configuration)
        {
            HttpClient = httpClient;
            IsEnabled = configuration["NCRAnnotation:IsEnabled"] == "True";
        }

        public HttpClient HttpClient { get; }
        public bool IsEnabled { get; }

        public async Task<DocAnnotations> AnnotateSegmentsAsync(TextSegments segments)
        {
            if (IsEnabled)
            {
                var ncrAnnotations = await NCRAnnotateSegmentsAsync(segments);
                return NCRParseAnnotations(ncrAnnotations, segments);
            }
            else
            {
                return new DocAnnotations() { Analyzer = "NCR" };
            }
        }

        private async Task<IList<NCRAnnotation>> NCRAnnotateSegmentsAsync(TextSegments segments)
        {
            var ncrDocs = new List<NCRAnnotation>();
            var annDocs = segments.AsNCRDocuments();

            foreach (var splitDocs in annDocs.SplitDocuments(25))
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var docs = await NCRAnnotateSegmentsAsync(splitDocs);
                ncrDocs.AddRange(docs);
                stopwatch.Stop(); Console.WriteLine("NCR - Annotate {0} segments in {1:00.00} secs.", splitDocs.documents.Count, stopwatch.Elapsed.TotalSeconds);
            }

            return ncrDocs;
        }

        private async Task<IList<NCRAnnotation>> NCRAnnotateSegmentsAsync(NCRDocuments docs)
        {
            var json = await HttpClient.POSTAsync("annotate_batch", docs.documents);
            // TODO: Fix NCR bug
            json = json.Replace("\"phens\":{}", "\"phens\":[]");
            return json.Deserialize<IList<NCRAnnotation>>();
        }

        #region NCRParseAnnotations
        private DocAnnotations NCRParseAnnotations(IList<NCRAnnotation> annotations, TextSegments segments)
        {
            var mapping = new Dictionary<string, TextSegment>();
            if (segments.Language_source != segments.Language_target)
            {
                foreach (var segment in segments.Segments)
                {
                    mapping[segment.Id] = segment;
                }
            }
            var docAnnotations = new DocAnnotations() { Analyzer = "NCR" };
            foreach (var annotation in annotations)
            {
                var segAnnotations = new SegAnnotations(annotation.Id, annotation.Text);
                if (mapping.ContainsKey(annotation.Id))
                {
                    var seg = mapping[annotation.Id];
                    segAnnotations.Source = new SourceText(segments.Language_source, seg.Source);
                }
                foreach (var ann in NCRParseAnnotation(annotation))
                {
                    segAnnotations.Annotations.Add(ann);
                }
                docAnnotations.Segments.Add(segAnnotations);
            }
            return docAnnotations;
        }

        private IEnumerable<Annotation> NCRParseAnnotation(NCRAnnotation annotation)
        {
            int id = 1;
            int pos = 0;
            var text = annotation.Text;
            for (int index = 0; index < annotation.Phens.Count; index++)
            {
                var ann = annotation.Phens[index];
                int x0 = ann.Characters[0];
                int x1 = ann.Characters[1];
                int dx = x1 - x0;

                if (x0 > pos)
                {
                    yield return Annotation.CreateBlank(id++.ToString(), text.Substring(pos, x0 - pos), pos, x0 - pos);
                }
                yield return Annotation.Create(id++.ToString(), text.Substring(x0, dx), x0, dx, "Symptom", ann.Probability).AddLink("HPO", ann.Id);
                pos = x1;
            }
            if (pos < text.Length)
            {
                yield return Annotation.CreateBlank(id++.ToString(), text.Substring(pos), pos, text.Length - pos);
            }
        }
        #endregion
    }
}
