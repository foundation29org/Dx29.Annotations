using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Dx29.Data;
using Dx29.Annotations;

namespace Dx29.Services
{
    public class DocTranslatorService
    {
#if  DEBUG
        const string TRANSLATE_QUERYSTRING = "api-version=3.0";
        const string TRANSLATE_QUERYSTRING_BACKUP = "api-version=3.0";
#else
        const string TRANSLATE_QUERYSTRING = "api-version=3.0&category=<CATEGORY-TOKEN>";
        const string TRANSLATE_QUERYSTRING_BACKUP = "api-version=3.0";
#endif
        const int PARTS_LENGTH = 5;

        public DocTranslatorService(IHttpClientFactory clientFactory)
        {
            HttpClient = clientFactory.CreateClient("CognitiveServices");
            HttpClientSegs = clientFactory.CreateClient("Segmentation");
        }

        public HttpClient HttpClient { get; }
        public HttpClient HttpClientSegs { get; }

        public async Task<(string, double)> DetectLanguageAsync(string text)
        {
            var obj = new[] { new { Text = ExtractSampleText(text) } };
            var lan = await DetectLanguageAsync(obj);
            if (lan.Score < 0.95)
            {
                lan = await DetectLanguageAsync(new[] { new { Text = text } });
            }
            return (lan.Language, lan.Score);
        }
        private async Task<LangDetection> DetectLanguageAsync(object obj)
        {
            var langDetection = await HttpClient.POSTAsync<LangDetection[]>("Detect?api-version=3.0", obj);
            return langDetection[0];
        }

        public string ExtractSampleText(string text)
        {
            if (text.Length > 512)
            {
                var words = text.Split(' ');
                int index = words.Length / 3;
                string sample = "";
                for (int n = 0; n < 128; n++)
                {
                    if (index + n >= words.Length) break;
                    sample += words[index + n] + " ";
                }
                return sample.Trim();
            }
            return text;
        }

        public async Task<TextSegments> SegmentTextAsync(string text, string lan)
        {
            var obj = new { Text = text };

            var segments = await HttpClientSegs.POSTAsync<TextSegments>($"document/segmentation?lan={lan}", obj);
            segments.JoinSegments();
            return segments;
        }

        public async Task<TextSegments> TranslateSegmentsAsync(TextSegments segments, string sourceLanguage = null, string targetLanguage = "en")
        {
            segments.Language_source = sourceLanguage;
            segments.Language_target = targetLanguage;
            if (RequiresTranslation(segments))
            {
                await TranslateAsync(HttpClient, segments);
            }
            return segments;
        }

        private static bool RequiresTranslation(TextSegments segments)
        {
            if (segments.Language_source == segments.Language_target)
            {
                foreach (var seg in segments.Segments)
                {
                    seg.Target = seg.Source;
                }
                return false;
            }
            return true;
        }

        private static async Task TranslateAsync(HttpClient http, TextSegments segments)
        {
            for (int n = 0; n < Math.Ceiling((double)segments.Segments.Count / PARTS_LENGTH); n++)
            {
                var part = segments.Segments.Skip(n * PARTS_LENGTH).Take(PARTS_LENGTH).ToArray();
                var body = part.Select(r => new { Text = r.Source }).ToArray();

                Console.WriteLine(body.Select(r => r.Text.Length).Sum());

                var resp = await TranslateAsync(http, body, "en");
                for (int ix = 0; ix < part.Length; ix++)
                {
                    part[ix].Target = resp[ix].Translations.First().Text;
                }
            }
        }

        private static async Task<TranslateResponse> TranslateAsync(HttpClient http, object body, string lang)
        {
            for (int n = 0; n < 3; n++)
            {
                try
                {
                    await Task.Delay(1000);
                    return await http.POSTAsync<TranslateResponse>($"translate?to={lang}&{TRANSLATE_QUERYSTRING}", body);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error translating. Retry {0}. {1}", n, ex.Message);
                }
                await Task.Delay((n + 1) * 5000);
            }
            return await http.POSTAsync<TranslateResponse>($"translate?to={lang}&{TRANSLATE_QUERYSTRING_BACKUP}", body);
        }
    }
}
