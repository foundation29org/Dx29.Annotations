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
    public class TAHAnnotationService
    {
        public TAHAnnotationService(HttpClient httpClient, IConfiguration configuration)
        {
            HttpClient = httpClient;
            UrlPath = configuration["TAHAnnotation:Path"];
            Authorization = configuration["TAHAnnotation:Authorization"];
            BlackList = GetBlackList(configuration["TAHAnnotation:BlackList"]);
            IsEnabled = configuration["TAHAnnotation:IsEnabled"] == "True";
            HttpCommon = new HttpClient();
        }

        public HttpClient HttpClient { get; }
        public HttpClient HttpCommon { get; }
        public string UrlPath { get; }
        public string Authorization { get; }
        public IList<string> BlackList { get; set; }
        public bool IsEnabled { get; }

        public async Task<DocAnnotations> AnnotateSegmentsAsync(TextSegments segments, string userId = null)
        {
            if (IsEnabled)
            {
                if (AvoidExecution(userId))
                {
                    return new DocAnnotations() { Analyzer = "TextAnalytics" };
                }
                var tahAnnotations = await TAHAnnotateSegmentsAsync(segments);
                return TAHParseAnnotations(tahAnnotations, segments);
            }
            return new DocAnnotations() { Analyzer = "TextAnalytics" };
        }

        private async Task<TAHAnnotationDocs> TAHAnnotateSegmentsAsync(TextSegments segments)
        {
            var tahDocs = new TAHAnnotationDocs();
            var annDocs = segments.AsTAHDocuments(100);

            foreach (var splitDocs in annDocs.SplitDocuments())
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var docs = await TAHAnnotateSegmentsAsync(splitDocs);
                tahDocs.Documents.AddRange(docs);
                stopwatch.Stop(); Console.WriteLine("TAH - Annotate {0} segments in {1:00.00} secs.", splitDocs.documents.Count, stopwatch.Elapsed.TotalSeconds);
            }

            return tahDocs;
        }

        private async Task<IList<TAHAnnotationDoc>> TAHAnnotateSegmentsAsync(TAHDocuments docs)
        {
            string error = null;
            IList<TAHAnnotationDoc> anns = null;
            for (int n = 0; n < 3; n++)
            {
                if (n > 0)
                {
                    await Task.Delay(1000 + 2000 * n);
                }
                (anns, error) = await TryTAHAnnotateSegmentsAsync(docs);
                if (anns != null)
                {
                    return anns;
                }
                Console.WriteLine("Try {0}. Error: {1}", n + 1, error);
            }
            throw new InvalidOperationException(error);
        }

        private async Task<(IList<TAHAnnotationDoc>, string)> TryTAHAnnotateSegmentsAsync(TAHDocuments docs)
        {
            try
            {
                var res = await HttpClient.SendRequestAsync(UrlPath, HttpMethod.Post, docs);
                if (res.IsSuccessStatusCode)
                {
                    var url = res.Headers.GetValues("operation-location").FirstOrDefault();

                    while (true)
                    {
                        await Task.Delay(1000);
                        var response = await HttpCommon.GETAsync<TAHAnnotationResults>(url, ("Ocp-Apim-Subscription-Key", Authorization));
                        switch (response.Status)
                        {
                            case "succeeded":
                                return (response.Results.Documents, null);
                            case "notStarted":
                            case "running":
                                break;
                            default:
                                return (null, $"{response.Status}. {response.Errors.Serialize()}");
                        }
                    }
                }
                else
                {
                    using (var reader = new StreamReader(res.Content.ReadAsStream()))
                    {
                        return (null, $"{res.ReasonPhrase}. {reader.ReadToEnd()}");
                    }
                }
            }
            catch (Exception ex)
            {
                return (null, ex.ToString());
            }
        }

        #region TAHParseAnnotations
        private DocAnnotations TAHParseAnnotations(TAHAnnotationDocs tahAnnotations, TextSegments segments)
        {
            var mapping = new Dictionary<string, TextSegment>();
            foreach (var segment in segments.Segments)
            {
                mapping[segment.Id] = segment;
            }

            var docAnnotations = new DocAnnotations() { Analyzer = "TextAnalytics" };
            foreach (var doc in tahAnnotations.Documents)
            {
                var seg = mapping[doc.Id];
                var text = seg.Target;
                var segAnnotations = new SegAnnotations(doc.Id, text);
                if (segments.Language_source != segments.Language_target)
                {
                    segAnnotations.Source = new SourceText(segments.Language_source, seg.Source);
                }
                foreach (var ann in ParseTADocument(doc, text))
                {
                    segAnnotations.Annotations.Add(ann);
                }
                docAnnotations.Segments.Add(segAnnotations);
            }
            return docAnnotations;
        }

        private IEnumerable<Annotation> ParseTADocument(TAHAnnotationDoc doc, string text)
        {
            int id = 0;
            int pos = 0;
            for (int index = 0; index < doc.Entities.Count; index++)
            {
                var ann = doc.Entities[index];
                int x0 = ann.Offset;
                int x1 = ann.Offset + ann.Length;
                int dx = ann.Length;

                if (x0 > pos)
                {
                    yield return Annotation.CreateBlank(id++.ToString(), text.Substring(pos, x0 - pos), pos, x0 - pos);
                }
                var annotation = Annotation.Create(id++.ToString(), text.Substring(x0, dx), x0, dx, ann.Category, ann.ConfidenceScore);
                annotation.IsNegated = ann.IsNegated;
                if (ann.Links != null)
                {
                    foreach (var link in ann.Links)
                    {
                        annotation.AddLink(link.DataSource, link.Id);
                    }
                }
                yield return annotation;
                pos = x1;
            }
            if (pos < text.Length)
            {
                yield return Annotation.CreateBlank(id++.ToString(), text.Substring(pos), pos, text.Length - pos);
            }
        }
        #endregion

        #region AvoidExecution
        private bool AvoidExecution(string userId)
        {
            if (userId != null)
            {
                userId = userId.ToLower();
                foreach (var item in BlackList)
                {
                    if (item == "*" || item == userId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private IList<string> GetBlackList(string users)
        {
            if (!String.IsNullOrWhiteSpace(users))
            {
                return users.ToLower().Split(',').Select(r => r.Trim()).ToArray();
            }
            return new string[] { };
        }
        #endregion
    }
}
