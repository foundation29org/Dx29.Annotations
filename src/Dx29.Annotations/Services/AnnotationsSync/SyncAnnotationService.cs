using System;
using System.Threading.Tasks;

using Dx29.Data;
using Dx29.Services;

namespace Dx29.Annotations
{
    public class SyncAnnotationService
    {
        public SyncAnnotationService(DocTranslatorService docTranslatorService, NCRAnnotationService ncrAnnotationService, TAHAnnotationService tahAnnotationService)
        {
            DocTranslatorService = docTranslatorService;
            NCRAnnotationService = ncrAnnotationService;
            TAHAnnotationService = tahAnnotationService;
        }

        public DocTranslatorService DocTranslatorService { get; }
        public NCRAnnotationService NCRAnnotationService { get; }
        public TAHAnnotationService TAHAnnotationService { get; }

        public async Task<DocAnnotations[]> ExecuteAsync(string text)
        {
            var lan = await DetectLanguageAsync(text);

            var segs = new TextSegments() { Language_source = lan, Language_target = "en" };
            segs.Segments.Add(new TextSegment { Id = "S001", Source = text, Target = text });

            segs = await TranslateTextAsync(segs, lan);

            var annsNCR = await NCRAnnotationAsync(segs);
            var annsTAH = await TAHAnnotationAsync(segs);
            var annotations = new DocAnnotations[] { annsNCR, annsTAH };

            return annotations;
        }

        private async Task<string> DetectLanguageAsync(string text)
        {
            (string lan, _) = await DocTranslatorService.DetectLanguageAsync(text);
            return lan;
        }

        private async Task<TextSegments> SegmentTextAsync(string text, string lan)
        {
            return await DocTranslatorService.SegmentTextAsync(text, lan);
        }

        private async Task<TextSegments> TranslateTextAsync(TextSegments segments, string sourceLanguage)
        {
            return await DocTranslatorService.TranslateSegmentsAsync(segments, sourceLanguage);
        }

        private async Task<DocAnnotations> NCRAnnotationAsync(TextSegments segments)
        {
            return await NCRAnnotationService.AnnotateSegmentsAsync(segments);
        }

        private async Task<DocAnnotations> TAHAnnotationAsync(TextSegments segments)
        {
            return await TAHAnnotationService.AnnotateSegmentsAsync(segments);
        }
    }
}
