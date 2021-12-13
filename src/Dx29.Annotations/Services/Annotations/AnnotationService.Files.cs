using System;
using System.Threading.Tasks;

using Dx29.Data;

namespace Dx29.Annotations
{
    partial class AnnotationService
    {
        private async Task UploadAnnotationsAsync(ReportInfo reportInfo, DocAnnotations[] annotations)
        {
            string path = $"medical-reports/{reportInfo.ReportId}/annotations.json";
            await FileStorageClient.UploadFileAsync(reportInfo.UserId, reportInfo.CaseId, path, annotations);
        }
    }
}
