using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

using Dx29.Data;
using Dx29.Jobs;
using Dx29.Services;

namespace Dx29.Annotations
{
    public partial class AnnotationService
    {
        public AnnotationService(DocConverterService docConverterService, DocTranslatorService docTranslatorService,
                                 NCRAnnotationService ncrAnnotationService, TAHAnnotationService tahAnnotationService,
                                 FileStorageClient2 fileStorageClient, ResourceGroupService resourceGroupService,
                                 SignalRService signalRService)
        {
            DocConverterService = docConverterService;
            DocTranslatorService = docTranslatorService;
            NCRAnnotationService = ncrAnnotationService;
            TAHAnnotationService = tahAnnotationService;
            FileStorageClient = fileStorageClient;
            ResourceGroupService = resourceGroupService;
            SignalRService = signalRService;
        }

        public DocConverterService DocConverterService { get; }
        public DocTranslatorService DocTranslatorService { get; }
        public NCRAnnotationService NCRAnnotationService { get; }
        public TAHAnnotationService TAHAnnotationService { get; }
        public FileStorageClient2 FileStorageClient { get; }
        public ResourceGroupService ResourceGroupService { get; }
        public SignalRService SignalRService { get; }

        public async Task<(string, string)> PrepareAsync(JobStorage jobStorage)
        {
            // Prepare working directory
            string folder = $"/app/working/{jobStorage.Token}";
            string output = $"{folder}/output";
            Directory.CreateDirectory(output);

            // Download input files
            await jobStorage.DownloadInputFolderAsync(folder);

            return (folder, output);
        }

        public async Task<Result> ExecuteAsync(JobInfo jobInfo, IJobStorage jobStorage, string inputFolder, string name = "document.bin")
        {
            Exception exception = null;
            DocAnnotations[] annotations = null;

            try
            {
                string filename = Path.Combine(inputFolder, name);

                if ((new FileInfo(filename)).Length > 3)
                {
                    // Convert to text
                    string txt = await ConvertAsync(filename);
                    await jobStorage.UploadOutputAsync("document.txt", txt);
                    await jobStorage.UpdateStatusAsync("Converted");

                    // Detect language
                    string lan = await DetectLanguageAsync(txt);
                    await jobStorage.UpdateStatusAsync("Language", lan);

                    // Segment text
                    var segs = await SegmentTextAsync(txt, lan);
                    await jobStorage.UpdateStatusAsync("Segmented");

                    // Translate segments
                    segs = await TranslateTextAsync(segs, lan);
                    await jobStorage.UploadOutputAsync("document.json", segs);
                    await jobStorage.UpdateStatusAsync("Translated");

                    // Get UserId
                    string userId = TryGetUserId(jobInfo);

                    // Task
                    var tahTask = TAHAnnotationAsync(segs, userId);
                    var ncrTask = NCRAnnotationAsync(segs);

                    // Wait tasks
                    Task.WaitAll(tahTask, ncrTask);

                    // TAH Annotations
                    var tahAnns = await tahTask;
                    await jobStorage.UploadOutputAsync("document.tah.json", tahAnns);
                    await jobStorage.UpdateStatusAsync("TextAnalytics");

                    // NCR Annotations
                    var ncrAnns = await ncrTask;
                    await jobStorage.UploadOutputAsync("document.ncr.json", ncrAnns);
                    await jobStorage.UpdateStatusAsync("NCR");

                    // Aggregate Annotations
                    annotations = new DocAnnotations[] { tahAnns, ncrAnns };
                    await jobStorage.UploadOutputAsync("annotations.json", annotations);
                }
                else
                {
                    var tah = new DocAnnotations { Analyzer = "TextAnalytics" };
                    var ncr = new DocAnnotations { Analyzer = "NCR" };
                    annotations = new DocAnnotations[] { tah, ncr };
                    await jobStorage.UploadOutputAsync("document.txt", "");
                    await jobStorage.UploadOutputAsync("document.json", new TextSegments { Language_source = "en", Language_target = "en", Segments = new List<TextSegment>() });
                    await jobStorage.UploadOutputAsync("document.tah.json", tah);
                    await jobStorage.UploadOutputAsync("document.ncr.json", ncr);
                    await jobStorage.UploadOutputAsync("annotations.json", annotations);
                    await jobStorage.UpdateStatusAsync("Zero");
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            try
            {
                // Update MedicalCase
                var reportInfo = ReportInfo.FromArgs(jobInfo.Args);
                if (reportInfo != null)
                {
                    if (exception == null)
                    {
                        double threshold = jobInfo.GetThreshold();
                        await UploadAnnotationsAsync(reportInfo, annotations);
                        await UpdateMedicalCaseAsync(reportInfo.UserId, reportInfo.CaseId, reportInfo.ReportId, threshold, annotations);
                        await SendNotificationAsync(reportInfo);
                    }
                    else
                    {
                        await UpdateMedicalCaseAsync(reportInfo.UserId, reportInfo.CaseId, reportInfo.ReportId, exception);
                        await SendNotificationAsync(reportInfo, exception);
                    }
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            return exception == null ? Result.Ok() : Result.Failed(exception.Message, exception.ToString());
        }

        private string TryGetUserId(JobInfo jobInfo)
        {
            try
            {
                var reportInfo = ReportInfo.FromArgs(jobInfo.Args);
                return reportInfo.UserId;
            }
            catch { }
            return null;
        }

        private async Task<string> ConvertAsync(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                var str = await DocConverterService.ConvertAsync(stream);
                if (String.IsNullOrEmpty(str))
                {
                    stream.Position = 0;
                    str = await DocConverterService.ConvertAsync(stream, strategy: "OcrOnly");
                }
                return str;
            }
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
            await Task.CompletedTask;
            return new DocAnnotations() { Analyzer = "NCR" };
        }

        private async Task<DocAnnotations> TAHAnnotationAsync(TextSegments segments, string userId)
        {
            return await TAHAnnotationService.AnnotateSegmentsAsync(segments, userId);
        }
    }
}
