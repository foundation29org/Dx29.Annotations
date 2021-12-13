using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Dx29.Data;
using Dx29.Jobs;
using Dx29.Services;
using Dx29.Tools;

namespace Dx29.Annotations
{
    public class AnnotationsClient : JobClient
    {
        public AnnotationsClient(ServiceBus serviceBus, BlobStorage blobStorage, string token) : base(serviceBus, blobStorage, token)
        {
        }

        static public AnnotationsClient CreateNew(ServiceBus serviceBus, BlobStorage blobStorage)
        {
            return new AnnotationsClient(serviceBus, blobStorage, IDGenerator.GenerateToken());
        }

        public override string JobName => "Annotations";

        public async Task<JobInfo> InitializeAsync(ReportInfo caseInfo = null)
        {
            var jobInfo = CreateJobInfo("Annotate", ("analyzers", "all"));
            if (caseInfo != null)
            {
                jobInfo.Args["userId"] = caseInfo.UserId;
                jobInfo.Args["caseId"] = caseInfo.CaseId;
                jobInfo.Args["fileId"] = caseInfo.ReportId;
            }
            await InitializeAsync(jobInfo);

            return jobInfo;
        }

        public async Task<AnnotationsResults> GetResultsAsync()
        {
            return new AnnotationsResults
            {
                Annotations = await Storage.DownloadOutputObjectAsync<IList<DocAnnotations>>("annotations.json")
            };
        }
    }
}
