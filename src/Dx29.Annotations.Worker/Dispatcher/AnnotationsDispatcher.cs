using System;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;

using Dx29.Jobs;
using Dx29.Services;

namespace Dx29.Annotations
{
    public class AnnotationsDispatcher : JobDispatcher
    {
        public AnnotationsDispatcher(AnnotationService annotationService, ServiceBus serviceBus, BlobStorage storage, ILogger<AnnotationsDispatcher> logger) : base(serviceBus, storage, logger)
        {
            AnnotationService = annotationService;
        }

        public AnnotationService AnnotationService { get; }

        public override string JobName => "Annotations";

        protected override async Task<bool> OnMessageAsync(JobStorage jobStorage, Message message, JobInfo jobInfo)
        {
            try
            {
                // Get JobStatus
                var jobStatus = await jobStorage.GetJobStatusAsync();
                if (jobStatus != null)
                {
                    switch (jobStatus.Status?.ToLower())
                    {
                        case "created":
                            break;
                        default:
                            return true;
                    }
                }

                // Update status: Preparing
                await UpdateStatusAsync(jobStorage, CommonStatus.Preparing);

                // Prepare
                (string folder, string output) = await AnnotationService.PrepareAsync(jobStorage);

                // Execute
                await UpdateStatusAsync(jobStorage, CommonStatus.Running);
                var result = await AnnotationService.ExecuteAsync(jobInfo, jobStorage, folder);

                // Update status by Result
                if (result.Success)
                {
                    await UpdateStatusAsync(jobStorage, result);
                }
                else
                {
                    await UpdateStatusAsync(jobStorage, result, AnnotationsErrors.GetErrorCode(result));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("OnMessageAsync Exception: {exception}", ex);

                // Log exception
                jobStorage.Logger.Error(ex);

                // Update status: Failed
                await UpdateStatusAsync(jobStorage, ex);
            }

            // Complete
            return true;
        }
    }
}
